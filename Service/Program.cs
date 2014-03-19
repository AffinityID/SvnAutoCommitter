using System;
using System.ServiceProcess;

namespace SvnAutoCommitter.Service {
    internal static class Program {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main() {
            var service = new SvnAutoCommitterService();
            if (Environment.UserInteractive) {
                service.Start();

                Console.WriteLine("Press Enter to terminate ...");
                Console.ReadLine();

                service.Stop();
            }
            else {
                ServiceBase.Run(service);
            }
        }
    }
}