namespace Kamaji.Data.Mongo
{
    using Microsoft.Extensions.DependencyInjection;
    using System;

    internal static class DI
    {
        private static readonly Lazy<ServiceProvider> _serviceProvider = new Lazy<ServiceProvider>(() => Startup.ServiceCollection.BuildServiceProvider(), true);
        internal static ServiceProvider Provider => _serviceProvider.Value;
    }
}
