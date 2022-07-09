using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Orleans.DataStructures.HashRing
{
    public static class IContainerGrainExtensions
    {
        public static async IAsyncEnumerable<T> Get<T>(this IClusterClient clusterClient,
            Func<Guid, Task<ResponseStream>> grainCall, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var responseStreamId = Guid.NewGuid();
            var stream = clusterClient.GetStreamProvider("nodeResponse")
                .GetStream<T>(responseStreamId, "default");
            var channel = Channel.CreateUnbounded<T>();
            var streamObserver = new ResponseStreamObserver<T>(async (item, streamSequenceToken) => await channel.Writer.WriteAsync(item, cancellationToken), onCompleted: () =>
            {
                 channel.Writer.Complete(); 
                 return Task.CompletedTask;
            }, exception =>
            {
                channel.Writer.Complete(exception);
                return Task.CompletedTask;
            });
            var handle = await stream.SubscribeAsync(streamObserver);
            
            var result = await grainCall(responseStreamId);

            
            while (await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (channel.Reader.TryRead(out var item))
                {
                    yield return item;
                }
            }
        }
    }
}