using Examples.Grains;
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
        static async Task Main()
        {
            using var client = await StartClientWithRetries();
            while (true)
            {
                try
                {
                    var array = client.GetGrain<IArrayGrain<MyData>>("test");

                    await array.AddAsync(new MyData("A"));

                    await array.AddAsync(new MyData("B"));


                    var count = await array.CountAsync();

                    for (int i = 0; i < count; i++)
                    {
                        Console.WriteLine($"array[{i}] = \"{(await array.GetAsync(i)).Value}\"");
                    }
                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            int attempt = 0;
            IClusterClient client;
            while (true)
            {
                try
                {
                    var builder = new ClientBuilder()
                        .UseLocalhostClustering()
                        .ConfigureApplicationParts(parts =>
                        {

                            parts.AddApplicationPart(typeof(ArrayGrain<>).Assembly).WithReferences();
                            parts.AddApplicationPart(typeof(MyData).Assembly).WithReferences();

                        })
                        .ConfigureLogging(logging => logging.AddConsole());
                    client = builder.Build();
                    await client.Connect();
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (Exception)
                {
                    attempt++;
                    Console.WriteLine(
                        $"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            return client;
        }
    }
}