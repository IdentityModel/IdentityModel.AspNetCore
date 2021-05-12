using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore
{
    public interface IAccessTokenRequestSynchronization
    {
        public ConcurrentDictionary<string, Lazy<Task<string>>> Dictionary { get; }
    }

    public interface IClientAccessTokenRequestSynchronization : IAccessTokenRequestSynchronization
    { }
    
    public interface IUserAccessTokenRequestSynchronization : IAccessTokenRequestSynchronization
    { }

    public class AccessTokenRequestSynchronization : IClientAccessTokenRequestSynchronization, IUserAccessTokenRequestSynchronization
    {
        public ConcurrentDictionary<string, Lazy<Task<string>>> Dictionary { get; } = new();
    }
}