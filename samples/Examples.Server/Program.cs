using Examples.Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.DataStructures;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Console;

namespace Examples.Server
{
    class Program
    {
        static async Task Main()
        {
            var host = CreateHost();
            await host.RunAsync();
        }
        private static IHost CreateHost()
        {
            return new HostBuilder()
                .UseOrleans((context, siloBuilder) =>
                {
                    siloBuilder
                        .UseLocalhostClustering()
                        .AddMemoryGrainStorageAsDefault()
                         .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                        .ConfigureApplicationParts(parts =>
                        {
                            parts.AddApplicationPart(typeof(ArrayGrain<>).Assembly).WithReferences();
                            parts.AddApplicationPart(typeof(ArrayItemGrain<>).Assembly).WithReferences();
                            parts.AddApplicationPart(typeof(MyData).Assembly).WithReferences();

                        });
                })
                .ConfigureServices(serviceCollection =>
                {

                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    
                    logging.AddConsole(options => options.FormatterName = ConsoleFormatterNames.Systemd);
                }).Build();
        }
    }
}
