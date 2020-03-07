/**
 * @license
 * Internet Systems Consortium license
 *
 * Copyright (c) 2020 Maksym Sadovnychyy (MAKS-IT)
 * Website: https://maks-it.com
 * Email: commercial@maks-it.com
 *
 * Permission to use, copy, modify, and/or distribute this software for any purpose
 * with or without fee is hereby granted, provided that the above copyright notice
 * and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
 * REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
 * INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
 * OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
 * TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
 * THIS SOFTWARE.
 */

using System;
using System.ServiceProcess;
using System.Reflection;
using System.Configuration.Install;
using System.Linq;


namespace PSScriptsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static void Main(String[] args)
        {
            ServiceBase[] services = new ServiceBase[] {
                new PSScriptsService()
            };

            // UserInteractive mode
            if (Environment.UserInteractive)
            {
                if (System.Diagnostics.Debugger.IsAttached) {
                    // Debug mode
                    InteractiveService(services);
                }
                else {
                    // Service actions mode
                    InteractiveServiceArgs(args, services);
                }
            }
            else {
                // NonInteractive mode
                ServiceBase.Run(services);
            }
        }

        /// <summary>
        /// Interactive service
        /// </summary>
        /// 
        static void InteractiveService(ServiceBase[] services)
        {
            Console.WriteLine();
            Console.WriteLine("Start service in interactive mode.");
            Console.WriteLine();

            // Get the method to invoke on each service to start it
            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);

            // Start services loop
            foreach (ServiceBase service in services)
            {
                Console.Write("Starting {0} ... ", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.WriteLine("Started");
            }

            // Waiting the end
            Console.WriteLine();
            Console.WriteLine("Press any key to stop service and end process...");
            Console.ReadKey();
            Console.WriteLine();

            // Get the method to invoke on each service to stop it
            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);

            // Stop loop
            foreach (ServiceBase service in services)
            {
                Console.Write("Stopping {0} ... ", service.ServiceName);
                onStopMethod.Invoke(service, null);
                Console.WriteLine("Stopped");
            }

            Console.WriteLine();
            Console.WriteLine("All services are stopped.");

            // Waiting a key press to not return to VS directly
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.Write("---- Press any key to quit ----");
                Console.ReadKey();
            }
        }


        /// <summary>
        /// Interactive service arguments actions: install, uninstall, start and stop service
        /// </summary>
        /// 
        static void InteractiveServiceArgs(string[] args, ServiceBase[] services) {
            try
            {
                bool success = false;
                args.ToList().ForEach(cmd => {
                    switch (cmd.ToLower())
                    {
                        case "install":
                            {
                                ManagedInstallerClass.InstallHelper(new string[] { typeof(Program).Assembly.Location });
                                success = true;
                                break;
                            }
                        case "uninstall":
                            {
                                ManagedInstallerClass.InstallHelper(new string[] { "/u", typeof(Program).Assembly.Location });
                                success = true;
                                break;
                            }
                        case "start":
                            {
                                foreach (var service in services)
                                {
                                    ServiceController sc = new ServiceController(service.ServiceName);
                                    sc.Start();
                                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                                    success = true;
                                }
                                break;
                            }
                        case "stop":
                            {
                                foreach (var service in services)
                                {
                                    ServiceController sc = new ServiceController(service.ServiceName);
                                    sc.Stop();
                                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                                    success = true;
                                }
                                break;
                            }
                    }
                });

                if (!success)  {
                    Console.WriteLine("Usage : {0} [command] [command ...]", Environment.GetCommandLineArgs());
                    Console.WriteLine("Commands : ");
                    Console.WriteLine(" service.exe install : Install the service");
                    Console.WriteLine(" service.exe uninstall : Uninstall the service");
                }
            }
            catch (Exception ex)
            {
                var bkpColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error : {0}", ex.GetBaseException().Message);
                Console.ForegroundColor = bkpColor;
            }


        }

    }
}
