namespace Kamaji
{
    using Microsoft.Extensions.DependencyInjection;
    using System;

    internal static class DI
    {
        internal static ServiceCollection ServiceCollection { get; } = new ServiceCollection();

        private static readonly Lazy<ServiceProvider> _serviceProvider = new Lazy<ServiceProvider>(() => ServiceCollection.BuildServiceProvider(), true);
        internal static ServiceProvider Provider => _serviceProvider.Value;
    }
}
