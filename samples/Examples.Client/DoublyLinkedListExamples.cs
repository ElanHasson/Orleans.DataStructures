using Microsoft.Extensions.Hosting;
using Orleans;
using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans.DataStructures.Array;
using Orleans.DataStructures.LinkedList;

namespace Examples.Client
{

    public class DoublyLinkedListExamples : IHostedService
    {
        private readonly IHostApplicationLifetime appLife;
        private readonly IClusterClient client;

        public DoublyLinkedListExamples(
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
                    var dll = client.GetGrain<IDoublyLinkedListGrain<MyData>>(DateTime.Now.ToString("G"));
                    await dll.Append("2", new MyData("Second"));
                    await DumpList(dll);

                    await dll.Push("1", new MyData("First"));
                    await DumpList(dll);

                    await dll.Append("3", new MyData("Third"));
                    await DumpList(dll);

                    await dll.Append("5", new MyData("Fifth"));
                    await DumpList(dll);

                    await dll.InsertAfter("3", "4", new MyData("Fourth"));
                    await DumpList(dll);

                    await dll.InsertAfter("2", "bad", new MyData("nope"));
                    await DumpList(dll);

                    await dll.Delete("bad");
                    await DumpList(dll);


                    await dll.Delete("1");
                    await DumpList(dll);

                    await dll.Push("1", new MyData("First"));
                    await DumpList(dll);

                    await dll.Delete("5");
                    await DumpList(dll);

                    await dll.InsertAfter("4", "5", new MyData("Fifth"));
                    await DumpList(dll);


                    await dll.Delete("3");
                    await DumpList(dll);


                    await dll.InsertAfter("5", "6", new MyData("Sixth"));
                    await DumpList(dll);

                    await dll.InsertAfter("1", "1.5", new MyData("One and One Half"));
                    await DumpList(dll);


                    await dll.InsertAfter("6", "7", new MyData("Seventh"));
                    await DumpList(dll);

                    await dll.InsertBefore("1", "0", new MyData("Zero"));
                    await DumpList(dll);


                    await dll.InsertBefore("4", "3", new MyData("Third"));
                    await DumpList(dll);

                    await dll.InsertBefore("7", "6.5", new MyData("6 1/2"));
                    await DumpList(dll);

                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task DumpList(IDoublyLinkedListGrain<MyData> dll)
        {
            var md = await dll.GetMetadata();
            var next = md.Head;
            Console.WriteLine($"Count: {md.Count}");
            while (next is not null)
            {
                var data = await next.Get();
                Console.Write($"{data.Data.Value}");
                next = await next.GetNext();
                if (next is null)
                {
                    Console.WriteLine();
                    break;
                }
                Console.Write("-->");
            }


            next = md.Tail;
            while (next is not null)
            {
                var data = await next.Get();
                Console.Write($"{data.Data.Value}");
                next = await next.GetPrevious();
                if (next is null)
                {
                    Console.WriteLine();
                    break;
                }
                Console.Write("<--");
            }

        }
    }
}
