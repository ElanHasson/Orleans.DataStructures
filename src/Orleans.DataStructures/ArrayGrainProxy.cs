using Orleans.CodeGeneration;
using System.Collections.Generic;
using System.Threading;

namespace Orleans.DataStructures
{
    public class ArrayGrainProxy<T>: IAsyncEnumerable<(long index, T item)>
    {
        private IArrayGrain<T> arrayGrainRef;

        public ArrayGrainProxy(IArrayGrain<T> arrayGrainRef)
        {
            this.arrayGrainRef = arrayGrainRef;
        }

        async IAsyncEnumerator<(long index, T item)> IAsyncEnumerable<(long index, T item)>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            for (long i = 0; i  < await this.arrayGrainRef.CountAsync(); i ++)
            {
                //TODO: Find a way to get grain client here and go directly to the item itself

                yield return (index: i, item: await this.arrayGrainRef.GetAsync(i));

            }
        }

    }
}