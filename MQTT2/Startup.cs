using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet.AspNetCore;
using MQTTnet.Client.Receiving;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using System;
using System.Linq;
using System.Net;

namespace MQTT2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mqttServerOptions = new MqttServerOptionsBuilder()
                .WithoutDefaultEndpoint()
                .WithConnectionValidator(new Authenticators.HubAuthenticatorService(Configuration).Validate)
                .WithApplicationMessageInterceptor(c => { })
                .Build();

            services
                .AddHostedMqttServer(mqttServerOptions)
                .AddMqttConnectionHandler()
                .AddConnections();

            services.AddDbContext<HubDbContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("HubDatabase")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseConnections(c => c.MapConnectionHandler<MqttConnectionHandler>("/mqtt", options =>
            {
                options.WebSockets.SubProtocolSelector = MQTTnet.AspNetCore.ApplicationBuilderExtensions.SelectSubProtocol;
            }));

            app.UseMqttServer(server =>
            {
                server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(args =>
                {
                    using (var serviceScope = app.ApplicationServices.CreateScope())
                    {
                        var hubDbContext = serviceScope.ServiceProvider.GetService<HubDbContext>();
                        var clients = server.GetClientStatusAsync().Result;

                        var log = new WilinkHubLog
                        {
                            WilinkHubLogId = Guid.NewGuid(),
                            ClientId = args.ClientId,
                            LogLevel = (int)MqttNetLogLevel.Info,
                            ClientIpAddress = clients?.FirstOrDefault(s => s.ClientId == args.ClientId)?.Endpoint,
                            CreatedOnUtc = DateTime.UtcNow,
                            ShortMessage = "Client connected",
                            FullMessage = $"Client {args.ClientId} connected to broker",
                            BrokerIpAddress = GetIpAddress()
                        };
                        hubDbContext.WilinkHubLog.Add(log);
                        hubDbContext.SaveChanges();
                    }
                });

                server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(args =>
                {
                    using (var serviceScope = app.ApplicationServices.CreateScope())
                    {
                        var hubDbContext = serviceScope.ServiceProvider.GetService<HubDbContext>();
                        var clients = server.GetClientStatusAsync().Result;
                        var log = new WilinkHubLog
                        {
                            WilinkHubLogId = Guid.NewGuid(),
                            ClientId = args.ClientId,
                            LogLevel = (int)MqttNetLogLevel.Info,
                            ClientIpAddress = clients?.FirstOrDefault(s => s.ClientId == args.ClientId)?.Endpoint,
                            CreatedOnUtc = DateTime.UtcNow,
                            ShortMessage = "Client disconnected",
                            FullMessage = $"Client {args.ClientId} disconnected with type {args.DisconnectType}",
                            BrokerIpAddress = GetIpAddress()
                        };
                        hubDbContext.WilinkHubLog.Add(log);
                        hubDbContext.SaveChanges();
                    }
                });

                server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(args =>
                {
                    using (var serviceScope = app.ApplicationServices.CreateScope())
                    {
                        var hubDbContext = serviceScope.ServiceProvider.GetService<HubDbContext>();
                        // Seed the database.
                        var clients = server.GetClientStatusAsync().Result;
                        hubDbContext.WilinkHubLog.Add(new WilinkHubLog
                        {
                            WilinkHubLogId = Guid.NewGuid(),
                            ClientId = args.ClientId,
                            LogLevel = (int)MqttNetLogLevel.Info,
                            ClientIpAddress = clients?.FirstOrDefault(s => s.ClientId == args.ClientId)?.Endpoint,
                            CreatedOnUtc = DateTime.UtcNow,
                            ShortMessage = "Client subscribed",
                            FullMessage = $"Client subscribed to {args.TopicFilter.Topic}",
                            BrokerIpAddress = GetIpAddress()
                        });
                        hubDbContext.SaveChanges();
                    }
                });

                server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(args =>
                {
                    using (var serviceScope = app.ApplicationServices.CreateScope())
                    {
                        var hubDbContext = serviceScope.ServiceProvider.GetService<HubDbContext>();
                        // Seed the database.
                        var clients = server.GetClientStatusAsync().Result;
                        hubDbContext.WilinkHubLog.Add(new WilinkHubLog
                        {
                            WilinkHubLogId = Guid.NewGuid(),
                            ClientId = args.ClientId,
                            LogLevel = (int)MqttNetLogLevel.Info,
                            ClientIpAddress = clients?.FirstOrDefault(s => s.ClientId == args.ClientId)?.Endpoint,
                            CreatedOnUtc = DateTime.UtcNow,
                            ShortMessage = "Client unsubscribed",
                            FullMessage = $"Client unsubscribed to {args.TopicFilter}",
                            BrokerIpAddress = GetIpAddress()
                        });
                        hubDbContext.SaveChanges();
                    }
                });

                server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(args =>
                {
                    using (var serviceScope = app.ApplicationServices.CreateScope())
                    {
                        var hubDbContext = serviceScope.ServiceProvider.GetService<HubDbContext>();
                        // Seed the database.
                        var clients = server.GetClientStatusAsync().Result;
                        hubDbContext.WilinkHubLog.Add(new WilinkHubLog
                        {
                            WilinkHubLogId = Guid.NewGuid(),
                            ClientId = args.ClientId,
                            LogLevel = (int)MqttNetLogLevel.Info,
                            ClientIpAddress = clients?.FirstOrDefault(s => s.ClientId == args.ClientId)?.Endpoint,
                            CreatedOnUtc = DateTime.UtcNow,
                            ShortMessage = "Client published message",
                            FullMessage = $"Client {args.ClientId} published message",
                            BrokerIpAddress = GetIpAddress()
                        });
                        hubDbContext.SaveChanges();
                    }
                });
            });
        }

        public string GetIpAddress()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            if (ipHostInfo.AddressList != null && ipHostInfo.AddressList.Any())
            {
                var ipAddress = ipHostInfo.AddressList[0];

                return ipAddress?.MapToIPv4().ToString();
            }
            return "";
        }
    }
}