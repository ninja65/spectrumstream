//
// Copyright � 2014 - 2016 Waters Corporation. All Rights Reserved.
//

using System;
using System.IO;
using System.Reflection;

namespace Waters.Control.Client.InternalInterface
{
    /// <summary>
    /// Finds the Typhoon system manager.
    /// </summary>
    public class SystemManagerLocator : ISystemManagerLocator
    {
        private const string SystemManagerExe = "waters_systemmanager.exe";

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemManagerLocator"/> class.
        /// </summary>
        public SystemManagerLocator()
        {
        }

        /// <summary>
        /// Get the fully qualified filename of the Typhoon system manager.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Thrown if the system manager executable folder is not found.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the system manager executable file is not found.</exception>
        /// <returns></returns>
        public string GetSystemManager()
        {
            var currentExePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string typhoonBinFolder = GetTyphoonInstallFolder(currentExePath);
            if (!Directory.Exists(typhoonBinFolder))
            {
                throw new DirectoryNotFoundException(typhoonBinFolder);
            }

            string systemManagerFilename = Path.Combine(typhoonBinFolder, SystemManagerExe);
            if (!File.Exists(systemManagerFilename))
            {
                throw new FileNotFoundException(systemManagerFilename);
            }

            return systemManagerFilename;
        }

        /// <summary>
        /// Get the Typhoon install folder.
        /// </summary>
        /// <remarks>
        /// Any exceptions, such as permissions, will be thrown to the caller.
        /// </remarks>
        /// <returns>Returns the folder name, based on the EnvironmentVariable</returns>
        public string GetTyphoonInstallFolder(string currentExecutingAssemblyFolder = "")
        {
            // Attempt to read the environment variable
            // then fallback to Typhoon/Bin subfolder
            var typhoonDir = Environment.GetEnvironmentVariable("TYPHOON_BIN_DIRECTORY");
            if (!String.IsNullOrWhiteSpace(typhoonDir))
            {
                return typhoonDir;
            }

            typhoonDir = currentExecutingAssemblyFolder;
            return Path.Combine(currentExecutingAssemblyFolder, @"Typhoon\Bin");
        }
    }
}
