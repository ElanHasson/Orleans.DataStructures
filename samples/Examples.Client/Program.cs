using Examples.Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.DataStructures;
using Orleans.Runtime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Examples.Client
{
    class Program
    {
        
            static async Task Main(string[] args)
            {
                try
                {
                    var hostBuilder = Host.CreateDefaultBuilder(args);

                    await hostBuilder.AddOrleansClusterClientAsync();

                    hostBuilder.ConfigureServices(services =>
                    {
                        services.AddHostedService<ClusterClientHostedService>();

                        services.AddHostedService<ArrayExamples>();
                    });

                    await hostBuilder.RunConsoleAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n\nException:\n{ex}");
                }

                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("\n\nPress any key to exit.");
                    Console.ReadKey(true);
                }
            }
        }
}