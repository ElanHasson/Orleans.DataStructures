using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleans.DataStructures.HashRing
{
    public class NodeGrain : Grain, INodeGrain
    {
        private readonly ILogger<NodeGrain> logger;
        private readonly IPersistentState<SortedList<string, object>> node;

        public NodeGrain(ILogger<NodeGrain> logger,
            [PersistentState("node", "nodeStorage")]
            IPersistentState<SortedList<string, object>> node)
        {
            this.logger = logger;
            this.node = node;
        }

        public override async Task OnActivateAsync()
        {
            await Initialize();
        }

        private Task Initialize()
        {
            if (this.node.RecordExists)
            {
                return Task.CompletedTask;
            }

            this.node.State = new SortedList<string, object>();
            return Task.CompletedTask;
        }

        public async Task<bool> AddObject<T>(string key, object @object)
        {
            if (!this.node.State.TryAdd(key, @object))
            {
                return false;
            }

            await this.node.WriteStateAsync();
            return true;
        }

        public Task<T> GetObject<T>(string key) => this.node.State.TryGetValue(key, out var value) ? Task.FromResult((T)value) : default;

        [AlwaysInterleave]
        public async Task GetAll(Guid responseStreamId)
        {
            
                var streamProvider = GetStreamProvider("nodeResponse");
                var stream = streamProvider.GetStream<KeyValuePair<string, object>>(responseStreamId, "default");

                foreach (var @object in this.node.State)
                {
                    await stream.OnNextAsync(@object);
                }

        }
    }
}