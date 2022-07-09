using System.Threading.Tasks;

namespace Orleans.DataStructures.Array
{
    public interface IArrayItemGrain<T> : IGrainWithIntegerCompoundKey
    {
        Task<T> Get();
        Task Set(T value);
    }
}