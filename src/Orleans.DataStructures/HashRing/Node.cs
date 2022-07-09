namespace Orleans.DataStructures.HashRing
{
    public class Node
    {

        public Node(string nodeName, uint nodeHash, INodeGrain nodeGrain)
        {
            this.NodeHash = nodeHash;
            this.NodeName = nodeName;
            this.NodeGrain = nodeGrain;
        }

        public long ItemCount { get; set; }
        public string NodeName { get;  }
        public uint NodeHash { get; }
        public INodeGrain NodeGrain { get; }
    }
}