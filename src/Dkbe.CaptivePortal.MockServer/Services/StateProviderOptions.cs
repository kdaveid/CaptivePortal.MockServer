// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Dkbe.CaptivePortal.MockServer.Models;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace Dkbe.CaptivePortal.MockServer.Services
{
    public static class StateProviderSetup
    {
        public static IServiceCollection AddStateProvider(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            return collection.AddSingleton<IStateProvider, StateProvider>();
        }
    }
}
