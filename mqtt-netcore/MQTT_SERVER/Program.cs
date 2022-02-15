using Data.Repositories.Implementation;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MQTT_SERVER;
using MQTT_SERVER.Constants;
using MQTT_SERVER.Data;
using MQTT_SERVER.hepler;
using MQTT_SERVER.Models;
using MQTT_SERVER.Topics;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
namespace MQTTSERVER
{
    class Program
    {
        private const string SettingTopic = "/hello/setting";
        private const string SetingReplyTopic = "/hello/settingreply";
        private const string RPMTopic = "/hello/data";
        private const string RSSITopic = "/hello/rssi";
        private const int MQTTPort = 1885;

        private const string MONGO_URI = "mongodb://10.4.4.224:27017";
        private const string MONGO_DB = "rpm";
        private const string RAWDATA_SCHEMA = "rawdatas";
        private const string SETTING_SCHEMA = "settings";
        private IUserRepository userRepository;
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            //var mqttServer = await CreateMqttServerAsync();
            //await CreateMqttClientAsync();
            //MyConsole.Info("Press any key to exit.");
            //Console.ReadLine();
        }
        private static async Task<RawData> CreateRawDataAsync(IMongoCollection<RawData> mongoCollection, RawData rawDatas)
        {
            try
            {
                await mongoCollection.InsertOneAsync(rawDatas);
                MyConsole.Info("#### ### Create Raw Data Async Successfully!");

                return rawDatas;
            }
            catch (Exception ex)
            {
                MyConsole.Error("#### ###Create Raw Data failed" + ex.Message);

                throw;
            }

        }
        private static double TimeDifferent(DateTime lastDateTime)
        {
            var time = DateTime.Now - lastDateTime;

            MyConsole.Data("TimeDifferent" + time.TotalSeconds);

            return time.TotalSeconds;

        }
        private static void SetRPMRedis(IRedisClient redisClient, int sequence, DateTime createddatetime)
        {
            redisClient.SetEntryInHash("RPM", Constants.SEQUENCE, sequence.ToString());
            redisClient.SetEntryInHash("RPM", Constants.CREATED_DATE_TIME, createddatetime.ToString());
        }
        private static async Task SettingReplyPublish(IMqttClient mqttClient, Setting settings)
        {
            try
            {
                var data = new SettingReplyPublish()
                {
                    id = settings.machineID,
                    maxRPM = settings.standardRPM,
                    minRPM = settings.minRPM,
                    timer = settings.timer,
                    tp = "set"
                };
                var payload = JsonConvert.SerializeObject(data);
                var message = new MqttApplicationMessageBuilder()
                  .WithTopic(SetingReplyTopic)
                  .WithPayload(payload)
                  .Build();
                await mqttClient.PublishAsync(message);
                MyConsole.Info($"### SetingReplyTopic SENT MESSAGE {Encoding.UTF8.GetString(message.Payload)} TO SERVER ");
            }
            catch (Exception ex)
            {
                MyConsole.Error($"### SettingTopic SENT MESSAGE TO SERVER ERROR ###" + ex.Message);
            }
        }
        private static async Task<IMqttServer> CreateMqttServerAsync()
        {
            var appsettings = ReadAppSettingsConfiguration();
            // Configure MQTT server.
            var optionsBuilder = new MqttServerOptionsBuilder()
            .WithConnectionBacklog(100)
            .WithDefaultEndpointPort(appsettings.Port);

            var mqttServer = new MqttFactory().CreateMqttServer();
           
            mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e =>
            {
                MyConsole.Disconnected($"Client {e.ClientId} disconnected event fired.");
            });
            mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e =>
            {
                MyConsole.Connected($"Client {e.ClientId} connected event fired.");
            });
            await mqttServer.StartAsync(optionsBuilder.Build());
            return mqttServer;
          
        }
        private static async Task CreateMqttClientAsync()
        {
            var appsettings = ReadAppSettingsConfiguration();
            var connectionString = ReadConnectionStringConfiguration();
            // signalr
            HubConnection _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(appsettings.SignalrUri))
            .WithAutomaticReconnect(new RandomRetryPolicy())
            .Build();

            // mongodb
            MongoClient _mongoClient = new MongoClient(connectionString.MongoConnection);
            IMongoDatabase _db = _mongoClient.GetDatabase(connectionString.MongoDb);
            IMongoCollection<RawData> _rawDatasCollection = _db.GetCollection<RawData>(RAWDATA_SCHEMA);
            IMongoCollection<Setting> _settingsCollection = _db.GetCollection<Setting>(SETTING_SCHEMA);

            // redis
            RedisManagerPool _redisClient = new RedisManagerPool(appsettings.RedisHost);

            MqttFactory factory = new MqttFactory();
            // Create a new MQTT client.            
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
            .WithTcpServer(appsettings.Host, appsettings.Port)
            .Build();

            // Reconnecting event handler
            mqttClient.UseDisconnectedHandler(async e =>
            {
                MyConsole.Warning("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                    MyConsole.Error("### RECONNECTING FAILED ###");
                }
            });

            // Message received event handler
            // Consuming messages
            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                // RecevicedTopic(e);
                if (e.ApplicationMessage.Topic == RPMTopic)
                {
                    await RecevicedRPMTopic(e, _rawDatasCollection, _redisClient);
                    // emit ve client
                    await _connection.InvokeAsync(Constants.MESSAGE, true);

                }

                if (e.ApplicationMessage.Topic == SettingTopic)
                {
                    var payload = e.ApplicationMessage.Payload;
                    string result = Encoding.UTF8.GetString(payload);
                    MyConsole.Info("#### ### Someone ask for setting ###" + result);

                    var settingTopicData = JsonConvert.DeserializeObject<SettingTopic>(result);
                    var settingItem = _settingsCollection.AsQueryable<Setting>().Where(x => x.machineID == settingTopicData.id).FirstOrDefault();
                    await SettingReplyPublish(mqttClient, settingItem);
                }

            });
            // Connected event handler
            mqttClient.UseConnectedHandler(async e =>
            {
                // Subscribe to topics
                await mqttClient.SubscribeAsync(RPMTopic);
                await mqttClient.SubscribeAsync(RSSITopic);
                await mqttClient.SubscribeAsync(SettingTopic);

                // Subscribe all topics
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("#").Build());
                MyConsole.Data($"### SUBSCRIBED TOPICS ### :  ' {SettingTopic} ', ' {RPMTopic} ', ' {RSSITopic} '");
            });
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });
            // Try to connect to MQTT server
            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception exception)
            {
                Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }

        }

        /// <summary> 
        ///     Logs the message from the MQTT connection validation context. 
        /// </summary> 
        /// <param name="context">The MQTT connection validation context.</param> 
        /// <param name="showPassword">A <see cref="bool"/> value indicating whether the password is written to the log or not.</param> 
        private static void LogMessage(MqttConnectionValidatorContext context, bool showPassword)
        {
            if (context == null)
            {
                return;
            }

            if (showPassword)
            {
                MyConsole.Info(
                    $"New connection: ClientId = {context.ClientId}, Endpoint = {context.Endpoint},"
                    + $" Username = {context.Username}, Password = {context.Password},"
                    + $" CleanSession = {context.CleanSession}");
            }
            else
            {
                MyConsole.Info(
                    $"New connection: ClientId = {context.ClientId}, Endpoint = {context.Endpoint},"
                    + $" Username = {context.Username}, CleanSession = {context.CleanSession}");
            }
        }
        static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        private static async Task RecevicedRPMTopic(MqttApplicationMessageReceivedEventArgs e,
            IMongoCollection<RawData> rawDatasCollection,
            RedisManagerPool redisClient)
        {
            using (var client = redisClient.GetClient())
            {
                var payload = e.ApplicationMessage.Payload;
                string result = System.Text.Encoding.UTF8.GetString(payload);
                var rpmTopic = JsonConvert.DeserializeObject<RPMTopic>(result);

                MyConsole.Data($"RPM data = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

                var RPM = client.GetAllEntriesFromHash("RPM");
                // Neu chua co trong redis thi vao db tim roi set vao redis
                if (RPM.Count == 0)
                {
                    MyConsole.Warning("Chua co trong redis vao db tim cai latest");

                    var rawData = rawDatasCollection.AsQueryable().Where(x => x.machineID == rpmTopic.i).OrderByDescending(x => x.createddatetime).FirstOrDefault();
                    // Neu trong db cua co thi gan sequen bang 1 va current vao redis
                    if (rawData != null)
                    {
                        MyConsole.Warning("Tim duoc cai latest gan vao redis");

                        SetRPMRedis(client, rawData.sequence, rawData.createddatetime);
                    }
                    else
                    {
                        MyConsole.Warning("Khong duoc cai latest them moi vao sequence 1 va currentdate");

                        DateTime currentDate = DateTime.Now;
                        int sequence = 1;
                        await CreateRawDataAsync(rawDatasCollection, new RawData
                        {
                            machineID = rpmTopic.i,
                            RPM = rpmTopic.r,
                            duration = rpmTopic.d,
                            sequence = sequence,
                            createddatetime = currentDate
                        });
                        SetRPMRedis(client, sequence, currentDate);
                    }
                    MyConsole.Info($"RPM In Redis: {String.Join(",", client.GetAllEntriesFromHash("RPM").Values.ToList())}");
                }
                else
                {
                    var lastDatetime = RPM.FirstOrDefault(x => x.Key == Constants.CREATED_DATE_TIME).Value.ToSafetyString();
                    var sequence = RPM.FirstOrDefault(x => x.Key == Constants.SEQUENCE).Value.ToInt();
                    var datetime = Convert.ToDateTime(lastDatetime);
                    var timedifferent = TimeDifferent(datetime);
                    var sequencetempelseInRedis = sequence;
                    var sequencetempifInRedis = sequence + 1;

                    if (timedifferent > 10)
                    {
                        MyConsole.Info("#### ### roi vao if ###");
                        MyConsole.Info("#### ###Sequence trong if ###: " + sequencetempifInRedis);
                        DateTime currentDate = DateTime.Now;
                        var rawData = await CreateRawDataAsync(rawDatasCollection, new RawData
                        {
                            machineID = rpmTopic.i,
                            RPM = rpmTopic.r,
                            duration = rpmTopic.d,
                            sequence = sequencetempifInRedis,
                            createddatetime = currentDate
                        });
                        SetRPMRedis(client, sequencetempifInRedis, currentDate);
                    }
                    else
                    {
                        MyConsole.Info("#### ### roi vao if ###");
                        MyConsole.Info("#### ### Khuay luot moi ###");
                        MyConsole.Info("#### ###Sequence trong if ###: " + sequencetempelseInRedis);
                        DateTime currentDate = DateTime.Now;
                        var rawData = await CreateRawDataAsync(rawDatasCollection, new RawData
                        {
                            machineID = rpmTopic.i,
                            RPM = rpmTopic.r,
                            duration = rpmTopic.d,
                            sequence = sequencetempelseInRedis,
                            createddatetime = currentDate
                        });
                        SetRPMRedis(client, sequencetempelseInRedis, currentDate);
                    }
                }

            }
        }
        private static void RecevicedTopic(MqttApplicationMessageReceivedEventArgs e)
        {
            MyConsole.Info("### RECEIVED APPLICATION MESSAGE ###");
            MyConsole.Info($"+ Topic = {e.ApplicationMessage.Topic}");
            MyConsole.Info($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            MyConsole.Info($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            MyConsole.Info($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        }

  
        private static AppSettings ReadAppSettingsConfiguration()
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var filePath = $"{currentPath}\\appsettings.json";
            AppSettings config = null;
            // ReSharper disable once InvertIf
            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<AppSettings>(json);
            }
            return config;
        }
      
        private static ConnectionStrings ReadConnectionStringConfiguration()
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var filePath = $"{currentPath}\\appsettings.json";
            ConnectionStrings config = null;
            // ReSharper disable once InvertIf
            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<ConnectionStrings>(json);
            }

            return config;
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
           .ConfigureLogging(loggerFactory =>
           {
               var path = Directory.GetCurrentDirectory();
               loggerFactory.AddFile($"{path}\\Logs\\Log.txt");
           })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                var env = builderContext.HostingEnvironment;

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
           {

               IConfiguration configuration = hostContext.Configuration;
               AppSettings options = configuration.GetSection("AppSettings").Get<AppSettings>();
               services.AddSingleton(options);
               services.AddLogging();
               services.AddHostedService<App>();
           }).UseWindowsService();

    }
}
