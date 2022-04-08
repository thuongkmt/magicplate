namespace Konbi.WindowServices.QRPayment
{
    using Autofac;
    using Konbi.WindowServices.QRPayment.Configuration;
    using KonbiBrain.Common.Services;
    using KonbiBrain.Interfaces;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;
    using System.Reflection;

    public static class AutofacConfig
    {
        private static readonly IServiceProvider ServiceProvider;

        public static IContainer Config()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Application>().AsSelf().SingleInstance();

            // Service assembly
            var assembly = typeof(Services.Type).Assembly;

            // Core
            builder.RegisterAssemblyTypes(assembly)
                .Where(type => ((
                type.Namespace != null && type.Namespace.EndsWith(nameof(Services.Core))) && type.IsClass))
                .AsSelf().SingleInstance();

            builder.RegisterType<NsqMessageProducerService>()
               .As<IMessageProducerService>()
               .PropertiesAutowired()
               .InstancePerLifetimeScope();



            var container = builder.Build();
            CurrentContainer = container;
            return container;
        }

        public static IContainer CurrentContainer { get; set; }

    }
}
