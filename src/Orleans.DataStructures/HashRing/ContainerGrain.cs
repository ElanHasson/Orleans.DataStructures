using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.DataStructures.ConsistentHashing;
using Orleans.Runtime;

namespace Orleans.DataStructures.HashRing
{
    public class ContainerGrain : Grain, IContainerGrain
    {
        private const long InitialNodeCount = 4;
        private readonly ILogger<ContainerGrain> logger;
        private readonly IPersistentState<SortedList<uint, Node>> ring;
        private ConsistentHash<Node> consistentHash;

        public ContainerGrain(ILogger<ContainerGrain> logger,
            [PersistentState("ring", "ringStorage")]
            IPersistentState<SortedList<uint, Node>> ring)
        {
            this.logger = logger;
            this.ring = ring;
        }

        public async Task<bool> AddObject<T>(string key, T @object)
        {
            var node = this.consistentHash.GetNode(key);
            
            return await node.NodeGrain.AddObject<T>(key, @object);
        }

        public async Task<T> GetObject<T>(string key)
        {
            var node = this.consistentHash.GetNode(key);
            return await node.NodeGrain.GetObject<T>(key);
        }

        private string GetNodeKey(string nodeToAddObjectTo)
        {
            return $"{this.GetGrainIdentity()}-{nodeToAddObjectTo}";
        }

        public override Task OnActivateAsync()
        {
            this.Initialize();

            this.consistentHash = new ConsistentHash<Node>(this.ring.State.Values);
            return Task.CompletedTask;
        }

        private void Initialize()
        {
            if (this.ring.RecordExists)
            {
                return;
            }

            this.ring.State = new SortedList<uint, Node>();

            for (var i = 0; i < InitialNodeCount; i++)
            {
                var key = $"node{i}";
                var node = new Node(key, (uint)ConsistentHash<Node>.BetterHash(key),this.GrainFactory.GetGrain<INodeGrain>(this.GetNodeKey(key)));
                this.ring.State.Add(node.NodeHash, node);
            }
        }
        
        
        [AlwaysInterleave]
        public async Task<ResponseStream> GetAll(Guid responseStreamId)
        {
            var streamProvider = GetStreamProvider("nodeResponse");
            var stream = streamProvider.GetStream<KeyValuePair<string, object>>(responseStreamId, "default");

            var nodeTasks = this.ring.State.Select(n => n.Value.NodeGrain.GetAll(responseStreamId));
            await Task.WhenAll(nodeTasks.ToArray());
            await stream.OnCompletedAsync();
            return new ResponseStream(responseStreamId);
        }
    }
}