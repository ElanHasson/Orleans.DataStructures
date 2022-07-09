using Orleans;
using Orleans.DataStructures.HashRing;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.DataStructures.LinkedList;

public class DoublyLinkedListGrain<TNodeGrain> : Grain, IDoublyLinkedListGrain<TNodeGrain>
where TNodeGrain : class, new()
{
    private readonly IClusterClient clusterClient;
    private readonly ITransactionalState<DoublyLinkedListMetadata<TNodeGrain>> metadataState;

    public DoublyLinkedListGrain(IClusterClient clusterClient, [TransactionalState("dll", "dllStorage")] ITransactionalState<DoublyLinkedListMetadata<TNodeGrain>> metadataState)
    {
        this.clusterClient = clusterClient;
        this.metadataState = metadataState;
    }
    public Task<DoublyLinkedListMetadata<TNodeGrain>> GetMetadata()
    {
        return this.metadataState.PerformRead(x => x);
    }

    public async Task Push(string id, TNodeGrain data)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(data);

        var newNode = this.clusterClient.GetGrain<IDoublyLinkedListNodeGrain<TNodeGrain>>($"{this.GetPrimaryKeyString()}-{id}");

        var metadata = await this.metadataState.PerformRead(x => x);



        if (metadata.Head is not null)
        {
            await newNode.Set(nextNode: metadata.Head, previousNode: null, data);

            await metadata.Head.SetPrevious(newNode);

            await this.metadataState.PerformUpdate(x =>
            {

                x.Head = newNode;
                x.Count++;
            });
            return;
        }

        // List is empty
        await newNode.Set(nextNode: null, previousNode: null, data);
        await this.metadataState.PerformUpdate(x =>
        {

            x.Head = newNode;
            x.Tail = newNode;
            x.Count++;
        });
    }

    public Task InsertAfter(string previousNodeId, string id, TNodeGrain data)
    {
        ArgumentNullException.ThrowIfNull(previousNodeId);

        var previousNode = this.clusterClient.GetGrain<IDoublyLinkedListNodeGrain<TNodeGrain>>($"{this.GetPrimaryKeyString()}-{previousNodeId}");

        return this.InsertAfter(previousNode, id, data);
    }

    public async Task InsertAfter(IDoublyLinkedListNodeGrain<TNodeGrain> previousNode, string id, TNodeGrain data)
    {
        ArgumentNullException.ThrowIfNull(previousNode);
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(data);

        var newNode = this.clusterClient.GetGrain<IDoublyLinkedListNodeGrain<TNodeGrain>>($"{this.GetPrimaryKeyString()}-{id}");

        var previousNodeNext = await previousNode.GetNext();

        if (previousNodeNext is null)
        {
            await this.Append(id, data);
            return;
        }

        await newNode.Set(nextNode: previousNodeNext, previousNode: previousNode, data);

        await previousNode.SetNext(newNode);
        await previousNodeNext.SetPrevious(newNode);



        await this.metadataState.PerformUpdate(x =>
        {
            x.Count++;

            // the head is updated
            if (previousNodeNext is null && previousNode is not null)
            {
                x.Head = newNode;
                return;
            }

            // the tail is updated
            if (previousNodeNext is not null && previousNodeNext is null)
            {
                x.Tail = newNode;
                return;
            }

        });
    }

    public Task InsertBefore(string nextNodeId, string id, TNodeGrain data)
    {
        ArgumentNullException.ThrowIfNull(nextNodeId);

        var nextNode = this.clusterClient.GetGrain<IDoublyLinkedListNodeGrain<TNodeGrain>>($"{this.GetPrimaryKeyString()}-{nextNodeId}");

        return this.InsertBefore(nextNode, id, data);
    }

    public async Task InsertBefore(IDoublyLinkedListNodeGrain<TNodeGrain> nextNode, string id, TNodeGrain data)
    {
        ArgumentNullException.ThrowIfNull(nextNode);
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(data);

     var nextNodePrevious = await nextNode.GetPrevious();

        // We're at head, so make this a Push operation
        if (nextNodePrevious is null)
        {
            await this.Push(id, data);
            return;
        }
        var newNode = this.clusterClient.GetGrain<IDoublyLinkedListNodeGrain<TNodeGrain>>($"{this.GetPrimaryKeyString()}-{id}");


        await newNode.Set(nextNode: nextNode, previousNode: nextNodePrevious, data);

        await nextNode.SetPrevious(newNode);
        await nextNodePrevious.SetNext(newNode);



        await this.metadataState.PerformUpdate(x =>
        {
            x.Count++;
            // the head is updated
            if (nextNodePrevious is null && nextNode is not null)
            {
                x.Head = newNode;
                return;
            }

            // the tail is updated
            if (nextNodePrevious is not null && nextNode is null)
            {
                x.Tail = newNode;
                return;
            }
        });
    }

    public async Task Append(string id, TNodeGrain data)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(data);

        var newNode = this.clusterClient.GetGrain<IDoublyLinkedListNodeGrain<TNodeGrain>>($"{this.GetPrimaryKeyString()}-{id}");

        var metadata = await this.metadataState.PerformRead(x => x);

        // If list is empty
        if (metadata.Head is null)
        {
            await newNode.Set(nextNode: null, previousNode: null, data);
            await this.metadataState.PerformUpdate(x =>
            {
                x.Head = newNode;
                x.Tail = newNode;
                x.Count++;
            });
            return;
        }


        await metadata.Tail!.SetNext(newNode);

        await newNode.Set(nextNode: null, previousNode: metadata.Tail, data);

        await this.metadataState.PerformUpdate(x =>
        {
            x.Tail = newNode;
            x.Count++;
        });
    }


    public Task Delete(string id)
    {
        ArgumentNullException.ThrowIfNull(id);


        var nodeToDelete = this.clusterClient.GetGrain<IDoublyLinkedListNodeGrain<TNodeGrain>>($"{this.GetPrimaryKeyString()}-{id}");
        return this.Delete(nodeToDelete);
    }

    public async Task Delete(IDoublyLinkedListNodeGrain<TNodeGrain> nodeToDelete)
    {
        ArgumentNullException.ThrowIfNull(nodeToDelete);

        var node = await nodeToDelete.Get();
        // node does not exist as part of the list
        if (node is null)
        {
            return;
        }

        // We are deleting the head
        if (node.Previous is null && node.Next is not null)
        {
            await node.Next.SetPrevious(null);
        }



        // We are deleting the tail
        if (node.Previous is not null && node.Next is null)
        {
            await node.Previous.SetNext(null);
        }

        // We are deleting a node in the middle
        if (node.Previous is not null && node.Next is not null)
        {
            await node.Previous.SetNext(node.Next);
            await node.Next.SetPrevious(node.Previous);
        }

        await nodeToDelete.Delete();

        await this.metadataState.PerformUpdate(x =>
        {
            x.Count--;

            // the head is being deleted
            if (node.Previous is null && node.Next is not null)
            {
                x.Head = node.Next;
                return;
            }

            // the tail is being deleted
            if (node.Previous is not null && node.Next is null)
            {
                x.Tail = node.Previous;
                return;
            }

            // List is empty
            if (node.Previous is null && node.Next is null)
            {
                x.Head = null;
                x.Tail = null;
            }

        });
    }
}

public class DoublyLinkedListNodeGrain<TNodeGrain> : Grain, IDoublyLinkedListNodeGrain<TNodeGrain>
    where TNodeGrain : class, new()
{
    private readonly ITransactionalState<DoublyLinkedListNode<TNodeGrain>> node;

    public DoublyLinkedListNodeGrain([TransactionalState("dllNode", "dllStorage")] ITransactionalState<DoublyLinkedListNode<TNodeGrain>> node)
    {
        this.node = node;
    }

    public Task Set(IDoublyLinkedListNodeGrain<TNodeGrain>? nextNode, IDoublyLinkedListNodeGrain<TNodeGrain>? previousNode, TNodeGrain data)
    {
        return this.node.PerformUpdate(x =>
        {
            x.Next = nextNode;
            x.Previous = previousNode;
            x.Data = data;
            return Task.CompletedTask;
        });
    }

    public Task SetNext(IDoublyLinkedListNodeGrain<TNodeGrain>? nextNode)
    {
        return this.node.PerformUpdate(x =>
        {
            x.Next = nextNode;
            return Task.CompletedTask;
        });
    }

    public Task SetPrevious(IDoublyLinkedListNodeGrain<TNodeGrain>? previousNode)
    {
        return this.node.PerformUpdate(x =>
        {
            x.Previous = previousNode;
            return Task.CompletedTask;
        });
    }

    public Task<IDoublyLinkedListNodeGrain<TNodeGrain>?> GetPrevious()
    {
        return this.node.PerformRead(x => x.Previous);
    }

    public Task<IDoublyLinkedListNodeGrain<TNodeGrain>?> GetNext()
    {
        return this.node.PerformRead(x => x.Next);
    }

    public Task<DoublyLinkedListNode<TNodeGrain>> Get()
    {
        return this.node.PerformRead(x => x);
    }

    public Task Delete()
    {
        return this.node.PerformUpdate(x => x = default!);
    }
}

public interface IDoublyLinkedListNodeGrain<TNodeGrain> : IGrainWithStringKey
where TNodeGrain : class, new()
{
    [Transaction(TransactionOption.CreateOrJoin)]
    Task Delete();

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<DoublyLinkedListNode<TNodeGrain>> Get();

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<IDoublyLinkedListNodeGrain<TNodeGrain>?> GetNext();

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<IDoublyLinkedListNodeGrain<TNodeGrain>?> GetPrevious();

    [Transaction(TransactionOption.CreateOrJoin)]
    Task Set(IDoublyLinkedListNodeGrain<TNodeGrain>? nextNode, IDoublyLinkedListNodeGrain<TNodeGrain>? previousNode, TNodeGrain data);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task SetNext(IDoublyLinkedListNodeGrain<TNodeGrain>? nextNode);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task SetPrevious(IDoublyLinkedListNodeGrain<TNodeGrain>? previousNode);
}

public interface IDoublyLinkedListGrain<TNodeGrain> : IGrainWithStringKey
where TNodeGrain : class, new()
{
    [Transaction(TransactionOption.CreateOrJoin)]
    Task Append(string id, TNodeGrain data);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task Delete(string id);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task Delete(IDoublyLinkedListNodeGrain<TNodeGrain> nodeToDelete);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<DoublyLinkedListMetadata<TNodeGrain>> GetMetadata();

    [Transaction(TransactionOption.CreateOrJoin)]
    Task InsertAfter(IDoublyLinkedListNodeGrain<TNodeGrain> previousNode, string id, TNodeGrain data);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task InsertAfter(string previousNodeId, string id, TNodeGrain data);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task InsertBefore(string nextNodeId, string id, TNodeGrain data);
    [Transaction(TransactionOption.CreateOrJoin)]
    Task InsertBefore(IDoublyLinkedListNodeGrain<TNodeGrain> nextNode, string id, TNodeGrain data);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task Push(string id, TNodeGrain data);
}

public class DoublyLinkedListNode<TNodeGrain>
where TNodeGrain : class, new()
{
    public TNodeGrain Data { get; set; }
    public IDoublyLinkedListNodeGrain<TNodeGrain>? Previous { get; set; }
    public IDoublyLinkedListNodeGrain<TNodeGrain>? Next { get; set; }
}

public class DoublyLinkedListMetadata<TNodeGrain>
where TNodeGrain : class, new()
{
    public long Count { get; set; }
    public IDoublyLinkedListNodeGrain<TNodeGrain>? Head { get; set; }
    public IDoublyLinkedListNodeGrain<TNodeGrain>? Tail { get; set; }
}
