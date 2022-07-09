using Microsoft.Extensions.Hosting;
using Orleans;
using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans.DataStructures.Array;

namespace Examples.Client
{

    public class ArrayExamples : IHostedService
    {
        private readonly IHostApplicationLifetime appLife;
        private readonly IClusterClient client;

        public ArrayExamples(
            IHostApplicationLifetime appLife, IClusterClient client)
        {
            this.appLife = appLife;
            this.client = client;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // since this is an event handler, the lambda's async void is acceptable
            appLife.ApplicationStarted.Register(async () => await ExecuteAsync(cancellationToken));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;


        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var array = client.GetGrain<IArrayGrain<MyData>>("test");

                    await array.AddAsync(new MyData("A"));

                    await array.AddAsync(new MyData("B"));


                    var count = await array.CountAsync();

                    await NormalForLoop(array, count);

                    await foreach (var (index, item) in array.ToProxy())
                    {
                        Console.WriteLine($"array[{index}] = \"{item.Value}\"");
                    }

                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            appLife.StopApplication();


        }

        private static async Task NormalForLoop(IArrayGrain<MyData> array, long count)
        {
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine($"array[{i}] = \"{(await array.GetAsync(i)).Value}\"");
            }
        }
    }
}
