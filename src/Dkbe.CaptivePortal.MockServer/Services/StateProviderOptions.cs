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
