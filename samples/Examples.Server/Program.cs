using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Console;
using Orleans.DataStructures.Array;
using Examples.Client;

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
                        .UseTransactions()
                        .AddMemoryGrainStorageAsDefault()
                         .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                        .ConfigureApplicationParts(parts =>
                        {
                            parts.AddApplicationPart(typeof(ArrayGrain<>).Assembly).WithReferences();
                            parts.AddApplicationPart(typeof(MyData).Assembly).WithReferences();

                        });
                    siloBuilder.AddMemoryGrainStorage("ringStorage");
                    siloBuilder.AddMemoryGrainStorage("nodeStorage");
                    siloBuilder.AddMemoryGrainStorage("PubSubStore");
                    siloBuilder.AddMemoryGrainStorage("dllStorage");
                    siloBuilder.AddSimpleMessageStreamProvider("nodeResponse",
                        configurator => { configurator.FireAndForgetDelivery = true; });
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
