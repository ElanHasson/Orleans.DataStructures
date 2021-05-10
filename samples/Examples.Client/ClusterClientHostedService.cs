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

#pragma warning disable 1998
        public async Task StartAsync(CancellationToken cancellationToken)
#pragma warning restore 1998
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