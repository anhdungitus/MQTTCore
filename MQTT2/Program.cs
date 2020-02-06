using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MQTTnet.AspNetCore;

namespace MQTT2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(o =>
                {
                    o.ListenAnyIP(1883, l => l.UseMqtt());
                    o.ListenAnyIP(5000); // default http pipeline
                })
                .UseStartup<Startup>()
                .Build();
    }
}