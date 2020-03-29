using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using IBaseFramework.Infrastructure;
using IBaseFramework.Ioc;
using IBaseFramework.Utility.Extension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FrameworkDemo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //.AddNewtonsoftJson(jsonOptions =>
            //{
            //    var settings = jsonOptions.SerializerSettings;

            //    settings.ContractResolver = new NullToEmptyResolver();
            //    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            //    //settings.NullValueHandling = NullValueHandling.Ignore;
            //    settings.DateFormatString = "yyyy/MM/dd HH:mm:ss";
            //    settings.Converters.Add(new LongJsonConverter());
            //}); 
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Add any Autofac modules or registrations.
            // This is called AFTER ConfigureServices so things you
            // register here OVERRIDE things registered in ConfigureServices.
            //
            // You must have the call to `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
            // when building the host or this won't be called.
            builder.RegisterModule(new AutofacModule());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }


    public class AutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));

            builder.Register("FrameworkDemo.");
            builder.RegisterContainer();
        }
    }
    public class NullToEmptyResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.ValueProvider = new NullToEmptyStringValueProvider(property.ValueProvider, property.PropertyType);

            return property;
        }
    }

    sealed class NullToEmptyStringValueProvider : IValueProvider
    {
        readonly IValueProvider _valueProvider;
        readonly Type _type;
        public NullToEmptyStringValueProvider(IValueProvider valueProvider, Type type)
        {
            _valueProvider = valueProvider;
            _type = type;
        }

        public object GetValue(object target)
        {
            if (_type == typeof(string))
            {
                return _valueProvider.GetValue(target) ?? string.Empty;
            }

            return _valueProvider.GetValue(target);
        }

        public void SetValue(object target, object value)
        {
            _valueProvider.SetValue(target, value);
        }
    }
}
