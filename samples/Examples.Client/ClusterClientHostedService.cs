using Microsoft.Extensions.Hosting;
using Orleans;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.Client
{
    public class ClusterClientHostedService : IHostedService
    {
        private readonly IClusterClient client;

        public ClusterClientHostedService(IClusterClient client)
        {
            this.client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Cluster client service started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Cluster client service stopping.");
            await client?.Close();
            client?.Dispose();
        }
    }
}