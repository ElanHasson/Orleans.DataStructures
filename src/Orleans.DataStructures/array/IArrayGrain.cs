using System.Threading.Tasks;

namespace Orleans.DataStructures.Array
{
    public interface IArrayGrain<T> : IGrainWithStringKey
    {

        Task<long> AddAsync(T value);
        Task<long> CountAsync();
        Task<T> GetAsync(long index);

    }
}