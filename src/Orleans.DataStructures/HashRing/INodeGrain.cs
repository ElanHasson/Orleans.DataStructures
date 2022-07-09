using System;
using System.Threading.Tasks;

namespace Orleans.DataStructures.HashRing
{
    public interface INodeGrain: IGrainWithStringKey
    {
        Task<bool> AddObject<T>(string key, object @object);
        Task<T> GetObject<T>(string key);
        Task GetAll(Guid responseStreamId);
    }
}