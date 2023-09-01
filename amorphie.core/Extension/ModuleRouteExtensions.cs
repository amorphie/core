using amorphie.core.Module.minimal_api;
using AutoMapper;
using System.Reflection;

namespace amorphie.core.Extension
{
    public static class ModuleRouteExtensions
    {
        public static void AddRoutes(this WebApplication webApplication, IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            assemblies = assemblies.Distinct().ToArray();


            var interfaceTypes = new[] { typeof(IBaseRoute) };

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes().Where(a => !a.IsAbstract))
                {
                    if (!interfaceTypes.Any(t => t.IsAssignableFrom(type))) continue;

                    var interfaces = type.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        if (!interfaceTypes.Any(t => t == @interface))
                            continue;

                        Activator.CreateInstance(type, webApplication);
                    }
                }
            }
        }
        public static void AddRoutes(this WebApplication webApplication) => webApplication.AddRoutes(AppDomain.CurrentDomain.GetAssemblies());
    }
}

