// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Duende.Bff;

namespace Bff.InMemoryTests.TestFramework
{
    public class MockSessionRevocationService : ISessionRevocationService
    {
        public bool DeleteUserSessionsWasCalled { get; set; }
        public UserSessionsFilter DeleteUserSessionsFilter { get; set; }
        public Task RevokeSessionsAsync(UserSessionsFilter filter, CancellationToken cancellationToken)
        {
            DeleteUserSessionsWasCalled = true;
            DeleteUserSessionsFilter = filter;
            return Task.CompletedTask;
        }
    }
}
