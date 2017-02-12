using Microsoft.Extensions.DependencyInjection;
using System;


namespace Dkbe.CaptivePortal.MockServer.Services
{
    public static class StateProviderSetup
    {
        public static IServiceCollection AddStateProvider(this IServiceCollection collection, Action<StateProviderOptions> setupAction)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            collection.Configure(setupAction);
            return collection.AddSingleton<IStateProvider, StateProvider>();
        }
    }

    public class StateProviderOptions
    {
        public string[] ZoneNames { get; set; }
    }
}
