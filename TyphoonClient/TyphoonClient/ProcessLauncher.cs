////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2013-2016 Waters Corporation.
//
// Starts the Typhoon process
//
////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
namespace Waters.Control.Client
{
    /// <summary>
    ///
    /// </summary>
    public interface IProcessLauncher
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <param name="workingFolder"></param>
        void Start(string command, string args, string workingFolder);
    }

    internal class ProcessLauncher : IProcessLauncher
    {
        public void Start(string command, string args, string workingFolder)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = args,
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workingFolder
                }
            };

            process.Start();
        }
    }
}
