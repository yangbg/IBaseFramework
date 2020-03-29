using System.Linq;
using System.Reflection;
using IBaseFramework.Base;
using Microsoft.Extensions.DependencyInjection;

namespace IBaseFramework.Ioc
{
    public static class ServiceCollectionExtend
    {
        public static void Registers(this IServiceCollection services, string startWithName, Assembly executingAssembly = null)
        {
            if (executingAssembly == null)
                executingAssembly = Assembly.GetExecutingAssembly();

            var assemblies = AssemblyFinder.Instance(startWithName).FindAll().Union(new[] { executingAssembly }).ToArray();

            if (!assemblies.Any())
                return;

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                if (types.Length < 1)
                {
                    continue;
                }

                var lifetimeTypes = types.Where(type => typeof(ILifetimeDependency).IsAssignableFrom(type) && !type.IsAbstract);
                foreach (var item in lifetimeTypes.Where(t => !t.IsInterface))
                {
                    var interFaces = item.GetInterfaces();
                    foreach (var interFace in interFaces)
                    {
                        services.AddScoped(interFace, item);
                    }
                }

                var singleInstanceTypes = types.Where(type => typeof(ISingleInstanceDependency).IsAssignableFrom(type) && !type.IsAbstract);
                foreach (var item in singleInstanceTypes)
                {
                    var interFaces = item.GetInterfaces();
                    foreach (var interFace in interFaces)
                    {
                        services.AddSingleton(interFace, item);
                    }
                }

                var everyTypes = types.Where(type => typeof(IDependency).IsAssignableFrom(type) && !type.IsAbstract);
                foreach (var item in everyTypes)
                {
                    var interFaces = item.GetInterfaces();
                    foreach (var interFace in interFaces)
                    {
                        services.AddTransient(interFace, item);
                    }
                }
            }
        }


    }
}