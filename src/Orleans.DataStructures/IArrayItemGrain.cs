using System.Threading.Tasks;

namespace Orleans.DataStructures
{
    public interface IArrayItemGrain<T> : IGrainWithIntegerCompoundKey
    {
        Task<T> Get();
        Task Set(T value);
    }
}