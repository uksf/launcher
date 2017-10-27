using System.ServiceProcess;

namespace ServerService {
    internal static class Program {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main() {
            ServiceBase[] servicesToRun = {new ServerService()};
            ServiceBase.Run(servicesToRun);
        }
    }
}