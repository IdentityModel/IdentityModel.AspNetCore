using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore
{
    /// <summary>
    /// Service to provide a concurrent dictionary for synchronizing token endpoint requests
    /// </summary>
    public interface IAccessTokenRequestSynchronization
    {
        /// <summary>
        /// Concurrent dictionary as synchronization primitive
        /// </summary>
        public ConcurrentDictionary<string, Lazy<Task<string?>>> Dictionary { get; }
    }


    /// <inheritdoc />
    public interface IClientAccessTokenRequestSynchronization : IAccessTokenRequestSynchronization
    { }

    /// <inheritdoc />
    public interface IUserAccessTokenRequestSynchronization : IAccessTokenRequestSynchronization
    { }

    /// <summary>
    /// Default implementation for token request synchronization primitive
    /// </summary>
    public class AccessTokenRequestSynchronization : IClientAccessTokenRequestSynchronization, IUserAccessTokenRequestSynchronization
    {
        /// <inheritdoc />
        public ConcurrentDictionary<string, Lazy<Task<string?>>> Dictionary { get; } = new();
    }
}