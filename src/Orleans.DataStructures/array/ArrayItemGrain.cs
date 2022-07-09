using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.DataStructures.Array
{
    public class ArrayItemGrain<T>: Grain, IArrayItemGrain<T>
    {
        private readonly IPersistentState<T> item;

        public ArrayItemGrain([PersistentState("item")] IPersistentState<T> item)
        {
            this.item = item;
        }

        public Task Set(T value)
        {
            this.item.State = value;
            return Task.CompletedTask;
        }

        public Task<T> Get()
        {
            return Task.FromResult(this.item.State);
        }
    }
}