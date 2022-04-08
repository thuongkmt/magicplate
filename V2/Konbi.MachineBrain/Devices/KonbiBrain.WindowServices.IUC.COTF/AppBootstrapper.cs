using Autofac;
using Konbi.Common.Interfaces;
using KonbiBrain.Common.Services;
using KonbiBrain.Interfaces;
using KonbiBrain.WindowServices.IUC.COTF.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.COTF
{
    public class AppBootstrapper
    {
        public static readonly AppBootstrapper Current = new AppBootstrapper();
        public AppBootstrapper()
        {
            Initialize();

        }

        private IContainer container;

        protected void Initialize()
        {
            Configure();
        }
        protected void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<IucService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<IucHandler>().As(typeof(IIucHandler)).SingleInstance();
            builder.RegisterType<LogService>().As(typeof(IKonbiBrainLogService)).PropertiesAutowired().SingleInstance();
            builder.RegisterType<NsqMessageProducerService>().As(typeof(IMessageProducerService)).PropertiesAutowired().SingleInstance();
            builder.RegisterType<IucSerialPortInterface>().As(typeof(IIucDeviceService)).InstancePerDependency();

            container = builder.Build();
        }
        public object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ?
                container.Resolve(service) :
                container.ResolveNamed(key, service);

        }

        protected IEnumerable<object> GetAllInstances(Type service)
        {
            return container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected void BuildUp(object instance)
        {
            container.InjectProperties(instance);
        }
        ~AppBootstrapper()
        {
            container.Dispose();
        }
    }
}
