////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2014-2016 Waters Corporation.
//
// Typhoon process starter
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using Waters.Control.Client.Interface;
using Waters.Control.Client.InternalInterface;

namespace Waters.Control.Client
{
    /// <summary>
    /// Responsible to setting up and start the typhoon process
    /// </summary>
    public class TyphoonStarter : ITyphoonStarter
    {
        //  private const string SystemManagerExe = "waters_systemmanager.exe";
        private const string SystemManagerArgs = "--flag:Simulation --epc-ip:127.0.0.1";
        private readonly TyphoonClientConfiguration configuration;
        private readonly IProcessLauncher processLauncher;
        private readonly ISystemManagerLocator locator;

        public TyphoonStarter(TyphoonClientConfiguration configuration, IProcessLauncher processStarter, ISystemManagerLocator locator)
        {
            this.configuration = configuration;
            this.processLauncher = processStarter;
            this.locator = locator;
        }

        /// <summary>
        /// Start the typhoon process
        /// </summary>
        public void Start()
        {
            try
            {
                string systemManagerFilename = locator.GetSystemManager();
                string workingFolder = Path.GetDirectoryName(systemManagerFilename);
                string args = string.Empty;

                if (configuration.UseSimulatedInstrument)
                    args = SystemManagerArgs;
                else
                    if (!string.IsNullOrEmpty(configuration.InstrumentNetworkIPAddress))
                        args = "--epc-ip:" + configuration.InstrumentNetworkIPAddress;

                if (!string.IsNullOrEmpty(configuration.LogFolder))
                    args += " --log-location:" + configuration.LogFolder;

                processLauncher.Start(systemManagerFilename, args, workingFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception starting Typhoon - {ex}");
            }
        }
    }
}
