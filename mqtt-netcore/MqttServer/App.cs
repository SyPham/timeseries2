using Data.Enumerations;
using Data.Helpers;
using Data.Heplers;
using Data.Models;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace MqttServer
{
    public class App
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _user;
        private readonly MemoryCache DataLimitCacheMonth;
        private readonly IPasswordHasher<Data.Models.User> Hasher = new PasswordHasher<Data.Models.User>();
        public App(IConfiguration config, IUserRepository user)
        {
            _config = config;
            _user = user;
            DataLimitCacheMonth = MemoryCache.Default;
        }

        public async Task RunAsync()
        {
            await CreateMqttServer();
        }
        public async Task CreateMqttServer()
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = ReadConfiguration(currentPath);
            var optionsBuilder = new MqttServerOptionsBuilder()
                   .WithDefaultEndpoint()
                   .WithDefaultEndpointPort(config.Port)
                   .WithConnectionValidator(this.ValidateConnection)
                   .WithSubscriptionInterceptor(this.ValidateSubscription)
                   .WithApplicationMessageInterceptor(this.ValidatePublish);

            var mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e =>
            {
                MyConsole.Disconnected($"Client {e.ClientId} disconnected event fired.");
            });
            //mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e =>
            //{
            //    MyConsole.Connected($"Client {e.ClientId} connected event fired.");
            //});
            await mqttServer.StartAsync(optionsBuilder.Build());
            Console.ReadLine();
        }
        /// <summary>
        ///     Reads the configuration.
        /// </summary>
        /// <param name="currentPath">The current path.</param>
        /// <returns>A <see cref="Config" /> object.</returns>
        private static Config ReadConfiguration(string currentPath)
        {
            var filePath = $"{currentPath}\\config.json";

            Config config = null;

            // ReSharper disable once InvertIf
            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<Config>(json);
            }

            return config;
        }

        /// <summary> 
        ///     Logs the message from the MQTT subscription interceptor context. 
        /// </summary> 
        /// <param name="context">The MQTT subscription interceptor context.</param> 
        /// <param name="successful">A <see cref="bool"/> value indicating whether the subscription was successful or not.</param> 
        private static void LogMessage(MqttSubscriptionInterceptorContext context, bool successful)
        {
            if (context == null)
            {
                return;
            }

            MyConsole.Info(successful ? $"New subscription: ClientId = {context.ClientId}, TopicFilter = {context.TopicFilter}" : $"Subscription failed for clientId = {context.ClientId}, TopicFilter = {context.TopicFilter}");
        }

        /// <summary>
        ///     Logs the message from the MQTT message interceptor context.
        /// </summary>
        /// <param name="context">The MQTT message interceptor context.</param>
        private static void LogMessage(MqttApplicationMessageInterceptorContext context)
        {
            if (context == null)
            {
                return;
            }

            var payload = context.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(context.ApplicationMessage?.Payload);

            MyConsole.Info(
                $"Message: ClientId = {context.ClientId}, Topic = {context.ApplicationMessage?.Topic},"
                + $" Payload = {payload}, QoS = {context.ApplicationMessage?.QualityOfServiceLevel},"
                + $" Retain-Flag = {context.ApplicationMessage?.Retain}");
        }

      
        /// <summary>
        ///     Logs the message from the MQTT connection validation context.
        /// </summary>
        /// <param name="context">The MQTT connection validation context.</param>
        /// <param name="showPassword">A <see cref="bool" /> value indicating whether the password is written to the log or not.</param>
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

        /// <summary>
        ///     Checks whether a user has used the maximum of its publishing limit for the month or not.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="sizeInBytes">The message size in bytes.</param>
        /// <param name="monthlyByteLimit">The monthly byte limit.</param>
        /// <returns>A value indicating whether the user will be throttled or not.</returns>
        private bool IsUserThrottled(string clientId, long sizeInBytes, long monthlyByteLimit)
        {
            var foundUserInCache = DataLimitCacheMonth.GetCacheItem(clientId);

            if (foundUserInCache == null)
            {
                DataLimitCacheMonth.Add(clientId, sizeInBytes, DateTimeOffset.Now.EndOfCurrentMonth());

                if (sizeInBytes < monthlyByteLimit)
                {
                    return false;
                }

                MyConsole.Info($"The client with client id {clientId} is now locked until the end of this month because it already used its data limit.");
                return true;
            }

            try
            {
                var currentValue = Convert.ToInt64(foundUserInCache.Value);
                currentValue = checked(currentValue + sizeInBytes);
                DataLimitCacheMonth[clientId] = currentValue;

                if (currentValue >= monthlyByteLimit)
                {
                    MyConsole.Info($"The client with client id {clientId} is now locked until the end of this month because it already used its data limit.");
                    return true;
                }
            }
            catch (OverflowException)
            {
                MyConsole.Info("OverflowException thrown.");
                MyConsole.Info($"The client with client id {clientId} is now locked until the end of this month because it already used its data limit.");
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Validates the message publication.
        /// </summary>
        /// <param name="context">The context.</param>
        private async void ValidatePublish(MqttApplicationMessageInterceptorContext context)
        {
            var clientIdPrefix = await this.GetClientIdPrefix(context.ClientId);
            Data.Models.User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = context.SessionItems.TryGetValue(context.ClientId, out var currentUserObject);
                currentUser = currentUserObject as Data.Models.User;
            }
            else
            {
                userFound = context.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                currentUser = currentUserObject as Data.Models.User;
            }

            if (!userFound || currentUser == null)
            {
                context.AcceptPublish = false;
                return;
            }

            var topic = context.ApplicationMessage.Topic;

            if (currentUser.ThrottleUser)
            {
                var payload = context.ApplicationMessage?.Payload;

                if (payload != null)
                {
                    if (currentUser.MonthlyByteLimit != 0)
                    {
                        if (IsUserThrottled(context.ClientId, payload.Length, currentUser.MonthlyByteLimit))
                        {
                            context.AcceptPublish = false;
                            return;
                        }
                    }
                }
            }

            // Get blacklist
            var publishBlackList = await this._user.GetBlacklistItemsForUser(currentUser.Id, BlackListWhiteListType.Publish);
            var blacklist = publishBlackList?.ToList() ?? new List<BlackList>();

            // Get whitelist
            var publishWhitelist = await this._user.GetWhitelistItemsForUser(currentUser.Id, BlackListWhiteListType.Publish);
            var whitelist = publishWhitelist?.ToList() ?? new List<WhiteList>();

            // Check matches
            if (blacklist.Any(b => b.Value == topic))
            {
                context.AcceptPublish = false;
                return;
            }

            if (whitelist.Any(b => b.Value == topic))
            {
                context.AcceptPublish = true;
                LogMessage(context);
                return;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var forbiddenTopic in blacklist)
            {
                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                context.AcceptPublish = false;
                return;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var allowedTopic in whitelist)
            {
                var doesTopicMatch = TopicChecker.Regex(allowedTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                context.AcceptPublish = true;
                LogMessage(context);
                return;
            }

            context.AcceptPublish = false;
        }

        /// <summary>
        ///     Validates the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        private async void ValidateSubscription(MqttSubscriptionInterceptorContext context)
        {
            var clientIdPrefix = await this.GetClientIdPrefix(context.ClientId);
            Data.Models.User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = context.SessionItems.TryGetValue(context.ClientId, out var currentUserObject);
                currentUser = currentUserObject as Data.Models.User;
            }
            else
            {
                userFound = context.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                currentUser = currentUserObject as Data.Models.User;
            }

            if (!userFound || currentUser == null)
            {
                context.AcceptSubscription = false;
                LogMessage(context, false);
                return;
            }

            var topic = context.TopicFilter.Topic;

            // Get blacklist
            var publishBlackList = await this._user.GetBlacklistItemsForUser(currentUser.Id, BlackListWhiteListType.Publish);
            var blacklist = publishBlackList?.ToList() ?? new List<BlackList>();

            // Get whitelist
            var publishWhitelist = await this._user.GetWhitelistItemsForUser(currentUser.Id, BlackListWhiteListType.Publish);
            var whitelist = publishWhitelist?.ToList() ?? new List<WhiteList>();


            // Check matches
            if (blacklist.Any(b => b.Value == topic))
            {
                context.AcceptSubscription = false;
                LogMessage(context, false);
                return;
            }

            if (whitelist.Any(b => b.Value == topic))
            {
                context.AcceptSubscription = true;
                LogMessage(context, true);
                return;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var forbiddenTopic in blacklist)
            {
                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                context.AcceptSubscription = false;
                LogMessage(context, false);
                return;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var allowedTopic in whitelist)
            {
                var doesTopicMatch = TopicChecker.Regex(allowedTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                context.AcceptSubscription = true;
                LogMessage(context, true);
                return;
            }

            context.AcceptSubscription = false;
            LogMessage(context, false);
        }

        /// <summary>
        ///     Validates the connection.
        /// </summary>
        /// <param name="context">The context.</param>
        private  void ValidateConnection(MqttConnectionValidatorContext context)
        {
            var currentUser =  this._user.GetUserByName(context.Username).GetAwaiter().GetResult();

            if (currentUser == null)
            {
                context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(context, true);
                return;
            }

            if (context.Username != currentUser.Username)
            {
                context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(context, true);
                return;
            }

            var hashingResult = Hasher.VerifyHashedPassword(
                currentUser,
                currentUser.PasswordHash,
                context.Password);

            if (hashingResult == PasswordVerificationResult.Failed)
            {
                context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(context, true);
                return;
            }

            if (!currentUser.ValidateClientid)
            {
                context.ReasonCode = MqttConnectReasonCode.Success;
                context.SessionItems.Add(context.ClientId, currentUser);
                LogMessage(context, false);
                return;
            }

            if (string.IsNullOrWhiteSpace(currentUser.ClientidPrefix))
            {
                if (context.ClientId != currentUser.Clientid)
                {
                    context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    LogMessage(context, true);
                    return;
                }

                context.SessionItems.Add(currentUser.Clientid, currentUser);
            }
            else
            {
                context.SessionItems.Add(currentUser.ClientidPrefix, currentUser);
            }

            context.ReasonCode = MqttConnectReasonCode.Success;
            LogMessage(context, false);
        }

        /// <summary>
        ///     Gets the client id prefix for a client id if there is one or <c>null</c> else.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>The client id prefix for a client id if there is one or <c>null</c> else.</returns>
        private async Task<string> GetClientIdPrefix(string clientId)
        {
            var clientIdPrefixes = await this._user.GetAllClientIdPrefixes();
            return clientIdPrefixes.FirstOrDefault(clientId.StartsWith);
        }
    }
}
