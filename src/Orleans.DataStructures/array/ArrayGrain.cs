using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.DataStructures.Array
{
    public class ArrayGrain<T> : Grain, IArrayGrain<T>
    {
        private readonly IPersistentState<ArrayState> arrayState;

        public ArrayGrain([PersistentState("arrayState")] IPersistentState<ArrayState> arrayState)
        {
            this.arrayState = arrayState;
         }

        public async Task<long> AddAsync(T value)
        {
            long newLength = this.arrayState.State.Length++;
            await GetItemReference(newLength).Set((T)value);

            return newLength;
        }

        public async Task<T> GetAsync(long index)
        {
            return await GetItemReference(index).Get();
        }

        public async Task<long> CountAsync()
        {
            return await Task.FromResult(this.arrayState.State.Length);
        }

        private IArrayItemGrain<T> GetItemReference(long index)
        {
            return GrainFactory.GetGrain<IArrayItemGrain<T>>(index, this.GetPrimaryKeyString());
        }
      

    }
}
