using Autofac;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;
using NetMQ;
using IContainer = Autofac.IContainer;
using Module = Autofac.Module;

namespace Waters.Control.Client
{
    /// <summary>
    /// Typhoon Factory Implementation
    /// </summary>
    public class TyphoonFactory
    {
        protected static TyphoonFactory instance;
        protected TyphoonClientConfiguration configuration;
        protected IContainer container;
        protected Module typhoonClientModule;
        protected readonly static Module DefaultTyphoonClientModule = new TyphoonClientModule();

        public const string DefaultEndPointUri = "tcp://127.0.0.1:7777";

        public static TyphoonFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Create();
                }

                return instance;
            }
        }

        public static TyphoonFactory Create()
        {
            return Create(DefaultTyphoonClientModule, new TyphoonClientConfiguration()
            {
                EndPointUri = DefaultEndPointUri,
                StartupTyphoon = false
            });
        }

        public static TyphoonFactory Create(TyphoonClientConfiguration configuration)
        {
            return Create(DefaultTyphoonClientModule, configuration);
        }

        public static TyphoonFactory Create(Module typhoonClientModule)
        {
            return Create(typhoonClientModule, new TyphoonClientConfiguration()
            {
                EndPointUri = DefaultEndPointUri,
                StartupTyphoon = true
            });
        }

        public static TyphoonFactory Create(Module typhoonClientModule, TyphoonClientConfiguration configuration)
        {
            instance = new TyphoonFactory(typhoonClientModule, configuration);

            return Instance;
        }

        protected TyphoonFactory(Module typhoonClientModule, TyphoonClientConfiguration configuration)
        {
            this.typhoonClientModule = typhoonClientModule;
            this.configuration = configuration;
            CreateContainer(typhoonClientModule, configuration);
        }

        private void CreateContainer(Module typhoonClientModule, TyphoonClientConfiguration configuration)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(configuration);

            RegisterModules(builder);

            this.container = builder.Build();
        }

        protected virtual void RegisterModules(ContainerBuilder builder)
        {
            builder.RegisterModule(typhoonClientModule);
        }

        public virtual void Reset()
        {
            Destroy();

            // recreate the container and TyphoonFactory
            CreateContainer(typhoonClientModule, configuration);
        }

        public virtual void Destroy()
        {
            if (container != null)
            {
                // dispose of all object associated with the current container and TyphoonFactory
                container.Dispose();
                container = null;
            }

            NetMQConfig.Cleanup(true);
        }

        #region Factory Properties
        public ISystemManager SystemManager => container.Resolve<ISystemManager>();

        public IMethodRunner MethodRunner => container.Resolve<IMethodRunner>();

        public IHardwareControl HardwareControl => container.Resolve<IHardwareControl>();

        //public IInstrumentParameterStorage InstrumentParameterStorage => container.Resolve<IInstrumentParameterStorage>();

        //public IInstrumentMonitor InstrumentMonitor => container.Resolve<IInstrumentMonitor>();

        //public IInstrumentSetup InstrumentSetup => container.Resolve<IInstrumentSetup>();

        public IKeyValueStore KeyValueStore => container.Resolve<IKeyValueStore>();

        public ISystemMonitor SystemMonitor => container.Resolve<ISystemMonitor>();

        public ITyphoonStarter TyphoonStarter => container.Resolve<ITyphoonStarter>();

        public ITyphoonRestarter TyphoonRestarter => container.Resolve<ITyphoonRestarter>();

        //public ICommandFactory CommandFactory => container.Resolve<ICommandFactory>();

        //public IManifestProvider ManifestProvider => container.Resolve<IManifestProvider>();

        //public IInstrumentSetupReportProvider InstrumentSetupReportProvider => container.Resolve<IInstrumentSetupReportProvider>();

        //public IInstrumentSetupCalibrationDataProvider InstrumentSetupCalibrationDataProvider => container.Resolve<IInstrumentSetupCalibrationDataProvider>();

        //public IInstrumentInfoProvider InstrumentInfoProvider => container.Resolve<IInstrumentInfoProvider>();

        public IClientAccess ClientAccess => container.Resolve<IClientAccess>();

        //public ISystemAccess GetSystemAccess()
        //{
        //    return container.Resolve<ISystemAccess>();
        //}

        public ISystemManager GetSystemManager()
        {
            return container.Resolve<ISystemManager>();
        }

        //public IHealthSystem HealthSystem => container.Resolve<IHealthSystem>();

        //public MessagingKeyValueStore GetNewMessagingKeyValueStore()
        //{
        //    return new MessagingKeyValueStore(container.Resolve<IClientAccess>(), container.Resolve<IClientConnector>());
        //}

        //public ITyphoonCompatibility TyphoonCompatibility => container.Resolve<ITyphoonCompatibility>();

        //public IClientManager ClientManager => container.Resolve<IClientManager>();

        #endregion Factory Properties
    }
}
