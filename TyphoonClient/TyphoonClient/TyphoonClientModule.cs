using Autofac;
using System.Threading.Tasks;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;

namespace Waters.Control.Client
{
    class TyphoonClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<SystemManager>()
                .AsSelf()
                .As<ISystemManager>()
                .SingleInstance();

            //builder.RegisterType<SystemAccess>()
            //    .AsSelf()
            //    .As<ISystemAccess>()
            //    .SingleInstance();

            builder.RegisterType<MethodRunner>()
                .AsSelf()
                .As<IMethodRunner>()
                .SingleInstance();

            builder.RegisterType<ClientAccess>()
                .AsSelf()
                .As<IClientAccess>()
                .SingleInstance();

            builder.RegisterType<HardwareControl>()
                .AsSelf()
                .As<IHardwareControl>()
                .SingleInstance();

            //builder.RegisterType<InstrumentParameterStorage>()
            //    .AsSelf()
            //    .As<IInstrumentParameterStorage>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentMonitor>()
            //    .AsSelf()
            //    .As<IInstrumentMonitor>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentSetup>()
            //    .AsSelf()
            //    .As<IInstrumentSetup>()
            //    .SingleInstance();

            builder.RegisterType<MessagingKeyValueStore>()
                .AsSelf()
                .As<KeyValueStore>()
                .As<IKeyValueStore>()
                .SingleInstance();

            builder.RegisterInstance<TaskScheduler>(TaskScheduler.Default)
                .AsSelf()
                .As<TaskScheduler>()
                .SingleInstance();

            builder.RegisterType<SystemMonitor>()
                .AsSelf()
                .As<ISystemMonitor>()
                .SingleInstance();

            builder.RegisterType<TyphoonRestarter>()
                .AsSelf()
                .As<ITyphoonRestarter>()
                .SingleInstance();

            builder.RegisterType<ClientConnector>()
                .AsSelf()
                .As<IClientConnector>()
                .SingleInstance();

            builder.RegisterType<ConnectionManager>()
                .AsSelf()
                .As<IConnectionManager>()
                .SingleInstance();

            //builder.RegisterType<CommandFactory>()
            //    .AsSelf()
            //    .As<ICommandFactory>()
            //    .SingleInstance();

            //builder.RegisterType<ManifestProvider>()
            //    .AsSelf()
            //    .As<IManifestProvider>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentSetupReportProvider>()
            //    .AsSelf()
            //    .As<IInstrumentSetupReportProvider>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentSetupCalibrationDataProvider>()
            //    .AsSelf()
            //    .As<IInstrumentSetupCalibrationDataProvider>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentInfoProvider>()
            //    .AsSelf()
            //    .As<IInstrumentInfoProvider>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentMonitor>()
            //    .AsSelf()
            //    .As<IInstrumentMonitor>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentSetup>()
            //    .AsSelf()
            //    .As<IInstrumentSetup>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentSetupCalibrationDataProvider>()
            //    .AsSelf()
            //    .As<IInstrumentSetupCalibrationDataProvider>()
            //    .SingleInstance();

            //builder.RegisterType<InstrumentSetupReportProvider>()
            //    .AsSelf()
            //    .As<IInstrumentSetupReportProvider>()
            //    .SingleInstance();

            //builder.RegisterType<ManifestProvider>()
            //    .AsSelf()
            //    .As<IManifestProvider>()
            //    .SingleInstance();

            builder.RegisterType<TyphoonStarter>()
                .AsSelf()
                .As<ITyphoonStarter>()
                .SingleInstance();

            //builder.RegisterType<CalibrationResultsMonitor>()
            //    .AsSelf()
            //    .SingleInstance();

            //builder.RegisterType<TyphoonStarter>()
            //    .AsSelf()
            //    .As<ITyphoonStarter>()
            //    .SingleInstance();

            builder.RegisterType<ProcessLauncher>()
                .AsSelf()
                .As<IProcessLauncher>()
                .SingleInstance();

            builder.RegisterType<SystemManagerLocator>()
                .AsSelf()
                .As<ISystemManagerLocator>()
                .SingleInstance();

            //builder.RegisterType<HealthSystem>()
            //    .AsSelf()
            //    .As<IHealthSystem>()
            //    .SingleInstance();

            //builder.RegisterType<TyphoonCompatibility>()
            //    .As<ITyphoonCompatibility>()
            //    .SingleInstance();

            builder.RegisterType<ConnectionTester>()
                .As<IConnectionTester>()
                .SingleInstance();

            builder.RegisterType<ClientManager>()
                .As<IClientManager>()
                .SingleInstance();
        }
    }
}
