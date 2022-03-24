// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;

namespace Bff.InMemoryTests.TestFramework
{
    public class MockClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }
}
