using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace AmbiMass.SpectrumStream.Utils.loader
{
    public static class PluginLoader
    {
        public static void RegisterTypeFromPlugin<TInterface>(
            this IServiceCollection services,
            string pluginPath,
            string implementationTypeName,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string? key = null )
        {
            if (!File.Exists(pluginPath))
                throw new FileNotFoundException($"Plugin not found at path: {pluginPath}");

            // Load the plugin assembly
            var assembly = Assembly.LoadFrom(pluginPath);

            // Try to find the implementation type
            var implementationType = assembly.GetTypes()
                .FirstOrDefault(t => t.FullName == implementationTypeName || t.Name == implementationTypeName);

            if (implementationType == null)
                throw new InvalidOperationException($"Type '{implementationTypeName}' not found in plugin.");

            if (!typeof(TInterface).IsAssignableFrom(implementationType))
                throw new InvalidOperationException($"Type '{implementationTypeName}' does not implement '{typeof(TInterface).FullName}'.");

            // Register the implementation
            ServiceDescriptor serviceDescriptor = null;

            if (string.IsNullOrWhiteSpace(key))
            {
                serviceDescriptor = new ServiceDescriptor(typeof(TInterface), implementationType, lifetime);
            }
            else
            {
                serviceDescriptor = new ServiceDescriptor(typeof(TInterface), key, implementationType, lifetime);
            }

            services.Add(serviceDescriptor);

        }

        public static void RegisterTypeFromPlugin<TInterface>(
            this IServiceCollection services,
            string implementationTypeInPlugin,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string? key = null )
        {
            var items = implementationTypeInPlugin.Split(';');

            var folder = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

            var assemblyName = $"{folder}/{items[0]}";

            services.RegisterTypeFromPlugin<TInterface>(assemblyName, items[1], lifetime, key );
        }
        public static void RegisterHostedServiceFromPlugin(
            this IServiceCollection services,
            string pluginPath,
            string implementationTypeName)
        {
            if (!File.Exists(pluginPath))
                throw new FileNotFoundException($"Plugin not found at path: {pluginPath}");

            var assembly = Assembly.LoadFrom(pluginPath);

            var implementationType = assembly.GetTypes()
                .FirstOrDefault(t => t.FullName == implementationTypeName || t.Name == implementationTypeName);

            if (implementationType == null)
                throw new InvalidOperationException($"Type '{implementationTypeName}' not found in plugin.");

            if (!typeof(IHostedService).IsAssignableFrom(implementationType))
                throw new InvalidOperationException($"Type '{implementationTypeName}' does not implement IHostedService.");

            // Use generic AddHostedService<> via reflection
            var addHostedServiceMethod = typeof(ServiceCollectionHostedServiceExtensions)
                .GetMethods()
                .First(m => m.Name == "AddHostedService" && m.IsGenericMethod);

            var genericMethod = addHostedServiceMethod.MakeGenericMethod(implementationType);
            genericMethod.Invoke(null, new object[] { services });
        }

        public static void RegisterHostedServiceFromPlugin(
                this IServiceCollection services,
                string implementationTypeInPlugin)
        {
            var items = implementationTypeInPlugin.Split(';');

            var folder = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

            var assemblyName = $"{folder}/{items[0]}";

            services.RegisterHostedServiceFromPlugin(assemblyName, items[1]);
        }

    }
}
