using System;
using System.Linq;
using System.Reflection;
using Autofac;
using IBaseFramework.Base;

namespace IBaseFramework.Ioc
{
    public static class IocRegister
    {
        public static IContainer Container { get; private set; }

        public static event Action<ContainerBuilder> RegisterEventHandler;

        /// <summary> 注册依赖 </summary>
        /// <param name="builder"></param>
        /// <param name="startWithName"></param>
        /// <param name="executingAssembly"></param>
        public static void Register(this ContainerBuilder builder, string startWithName, Assembly executingAssembly = null)
        {
            RegisterHandle(builder, startWithName, executingAssembly);
        }

        /// <summary> 注册依赖 </summary>
        /// <param name="startWithName"></param>
        /// <param name="executingAssembly"></param>
        public static void Register(string startWithName, Assembly executingAssembly = null)
        {
            var builder = new ContainerBuilder();
            RegisterHandle(builder, startWithName, executingAssembly);
            Container = builder.Build();
        }

        /// <summary>
        /// 注册 Container
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterContainer(this ContainerBuilder builder)
        {
            builder.RegisterBuildCallback(container =>
            {
                Container = container;
            });
        }

        private static void RegisterHandle(ContainerBuilder builder, string startWithName, Assembly executingAssembly = null)
        {
            if (executingAssembly == null)
                executingAssembly = Assembly.GetExecutingAssembly();

            var assemblies = AssemblyFinder.Instance(startWithName).FindAll().Union(new[] { executingAssembly }).ToArray();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(ILifetimeDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                                           //.PropertiesAutowired()//属性注入
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(IRequestDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                                           //.PropertiesAutowired()//属性注入
                .InstancePerRequest();//保证生命周期基于请求

            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(ISingleInstanceDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                                           //.PropertiesAutowired()//属性注入
                .SingleInstance();//单例

            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(IDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                          //.PropertiesAutowired()//属性注入
                .AsImplementedInterfaces(); //接口服务

            RegisterEventHandler?.Invoke(builder);
        }
    }
}
