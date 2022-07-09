using System;
using System.Threading.Tasks;

namespace Orleans.DataStructures.HashRing
{
    public interface IContainerGrain: IGrainWithStringKey
    {
        Task<bool> AddObject<T>(string key, T @object);
        Task<T> GetObject<T>(string key);
        Task<ResponseStream> GetAll(Guid responseStreamId);
    }
}