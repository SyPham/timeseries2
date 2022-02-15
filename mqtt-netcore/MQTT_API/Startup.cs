using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using AutoMapper;
using MQTT_API.Heplers;
using MQTT_API.Heplers.AutoMapper;
using Data.Repositories.Interfaces;
using Data.Repositories.Implementation;
using Microsoft.AspNetCore.Identity;
using Data.Models;
using MQTT_API.Extensions;
using MQTTnet.AspNetCore.Extensions;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Data.Heplers;
using Data.Enumerations;
using Data.Helpers;
using System.Runtime.Caching;

namespace MQTT_API
{
    public class Startup
    {
        private DataAuthContext userRepository;

        public Startup(IConfiguration configuration)
        {
            
            Configuration = configuration;
        }
        private static readonly MemoryCache DataLimitCacheMonth = MemoryCache.Default;
        private static readonly IPasswordHasher<User> Hasher = new PasswordHasher<User>();
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var conn = Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.AddDbContext<DataContext>(option => option.UseSqlServer(
                    conn.DefaultConnection, x => x.MigrationsAssembly("Data")));

            services.AddControllers().AddNewtonsoftJson(options =>
               options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
           );
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                        .GetBytes(appSettings.Token)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Add identity stuff
            services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IBlackListRepository, BlackListRepository>();
            services.AddTransient<IWhiteListRepository, WhiteListRepository>();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins(appSettings.CorsPolicy) //register for client
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            //Auto Mapper
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IMapper>(sp =>
            {
                return new Mapper(AutoMapperConfig.RegisterMappings());
            });
            services.AddSingleton(AutoMapperConfig.RegisterMappings());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cicle Time Button System", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                     {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,

                            },
                            new List<string>()
                    }
                });
            });
            this.userRepository = new DataAuthContext();
            // Add MQTT stuff
            services.AddHostedMqttServer(
                builder => builder
#if DEBUG
                    .WithDefaultEndpoint().WithDefaultEndpointPort(appSettings.Port)
#else
                    .WithoutDefaultEndpoint()
#endif
                    .WithConnectionValidator(this.ValidateConnection).WithSubscriptionInterceptor(this.ValidateSubscription).WithApplicationMessageInterceptor(this.ValidatePublish));

            services.AddMqttConnectionHandler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mqtt");
            });
          
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        /// <summary>
        ///     Validates the message publication.
        /// </summary>
        /// <param name="context">The context.</param>
        private  void ValidatePublish(MqttApplicationMessageInterceptorContext context)
        {
            var clientIdPrefix =  this.GetClientIdPrefix(context.ClientId).GetAwaiter().GetResult();
            User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = context.SessionItems.TryGetValue(context.ClientId, out var currentUserObject);
                currentUser = currentUserObject as User;
            }
            else
            {
                userFound = context.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                currentUser = currentUserObject as User;
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
            var publishBlackList =  this.userRepository.BlackList.Where(x=> x.UserId ==currentUser.Id && x.Type == BlackListWhiteListType.Publish).ToList();
            var blacklist = publishBlackList?.ToList() ?? new List<BlackList>();

            // Get whitelist
            var publishWhitelist =  this.userRepository.WhiteList.Where(x => x.UserId == currentUser.Id && x.Type == BlackListWhiteListType.Publish).ToList();
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
        ///     Gets the client id prefix for a client id if there is one or <c>null</c> else.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>The client id prefix for a client id if there is one or <c>null</c> else.</returns>
        private async Task<string> GetClientIdPrefix(string clientId)
        {
            var clientIdPrefixes = await this.userRepository.Users.Select(x=> x.ClientidPrefix).ToListAsync();
            return clientIdPrefixes.FirstOrDefault(clientId.StartsWith);
        }
        /// <summary>
        ///     Validates the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        private async void ValidateSubscription(MqttSubscriptionInterceptorContext context)
        {
            var clientIdPrefix = await this.GetClientIdPrefix(context.ClientId);
            User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = context.SessionItems.TryGetValue(context.ClientId, out var currentUserObject);
                currentUser = currentUserObject as User;
            }
            else
            {
                userFound = context.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                currentUser = currentUserObject as User;
            }

            if (!userFound || currentUser == null)
            {
                context.AcceptSubscription = false;
                LogMessage(context, false);
                return;
            }

            var topic = context.TopicFilter.Topic;
            // Get blacklist
            var publishBlackList = await this.userRepository.BlackList.Where(x => x.UserId == currentUser.Id && x.Type == BlackListWhiteListType.Publish).ToListAsync();
            var blacklist = publishBlackList?.ToList() ?? new List<BlackList>();

            // Get whitelist
            var publishWhitelist = await this.userRepository.WhiteList.Where(x => x.UserId == currentUser.Id && x.Type == BlackListWhiteListType.Publish).ToListAsync();
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
        private async void ValidateConnection(MqttConnectionValidatorContext context)
        {
            var currentUser = await this.userRepository.Users.FirstOrDefaultAsync(x=> x.Username == context.Username).ConfigureAwait(false);

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
        /// <summary>
        ///     Logs the message from the MQTT subscription interceptor context.
        /// </summary>
        /// <param name="context">The MQTT subscription interceptor context.</param>
        /// <param name="successful">A <see cref="bool" /> value indicating whether the subscription was successful or not.</param>
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
        ///     Checks whether a user has used the maximum of its publishing limit for the month or not.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="sizeInBytes">The message size in bytes.</param>
        /// <param name="monthlyByteLimit">The monthly byte limit.</param>
        /// <returns>A value indicating whether the user will be throttled or not.</returns>
        private static bool IsUserThrottled(string clientId, long sizeInBytes, long monthlyByteLimit)
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

    }
}
