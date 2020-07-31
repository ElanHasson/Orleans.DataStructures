using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.DataStructures;
using System;
using System.Threading.Tasks;

namespace Examples.Client
{
    public static class AddClusterClientExtension
    {
        public static async Task<IHostBuilder> AddOrleansClusterClientAsync(this IHostBuilder builder)
        {
            Console.WriteLine("Starting AddOrleansClusterClient");

            // Nasty bit of code to "borrow" a copy of HostBulderContext so we can use it with await
            // instead of followng the normal ConfigureServices lambda pattern, like this:
            //
            // builder.ConfigureServices(async (context, services) => 
            // {
            //      var client = await GetConnectedClient(hostContext); 
            //      services.AddSingleton(client);
            // });
            //
            // An async lambda is async void so there is Task returned that can be awaited.

            HostBuilderContext hostContext = null;
            builder.ConfigureServices((context, services) => hostContext = context);
            var client = await GetConnectedClient(hostContext);
            builder.ConfigureServices(services => services.AddSingleton(client));
            return builder;
        }

        private static async Task<IClusterClient> GetConnectedClient(HostBuilderContext context)
        {
            // TODO get from context.config
            int remainingAttempts = 3;
            int retryDelaySeconds = 15;

            IClusterClient client = null;
            while (remainingAttempts-- > 0 && client is null)
            {
                Console.WriteLine($"Trying to connect, {remainingAttempts} attempts remaining.");
                try
                {
                    client = await TryConnect(context);
                }
                catch
                {
                    if (remainingAttempts == 0) throw;
                    await Task.Delay(retryDelaySeconds * 1000);
                }
            }
            Console.WriteLine($"Is client reference null? {client is null}");
            return client;
        }

        private static async Task<IClusterClient> TryConnect(HostBuilderContext context)
        {
            IClusterClient client = null;

            try
            {
                var builder = new ClientBuilder();

                // TODO get from context.config
                builder.UseLocalhostClustering();

                // TODO needs config delegates to customize parts
                builder.ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(IArrayGrain<>).Assembly).WithReferences();
                });

                client = builder.Build();

                // causes host builder to run hosted services???
                Console.WriteLine("Awaiting connect request.");
                await client.Connect();
            }
            catch
            {
                Console.WriteLine("Exception caught.");
                client?.Dispose();
                throw;
            }

            return client;
        }
    }
}