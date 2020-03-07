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

using System.ServiceProcess;
using System.IO;
using System.Threading;
using PSScriptsService.ServiceLib;

namespace PSScriptsService
{
    public partial class PSScriptsService : ServiceBase
    {
        public Thread srvThread = null;
        public static Timer stateTimer = null;

        public PSScriptsService()
        {
            InitializeComponent();

            srvThread = new Thread(DoWork);
            srvThread.IsBackground = true;
        }

        protected override void OnStart(string[] args)
        {
            SrvLog.Write((string)(Globals.settings.ServiceName) + " started");
            srvThread.Start("PSScriptsService");
        }

        protected override void OnStop()
        {
            SrvLog.Write((string)(Globals.settings.ServiceName) + " stopped");
        }

        private static void DoWork(object data)
        {
            SrvLog.Write(string.Format("Thread Started: '{0}'", data.ToString()));

            string [] scriptsDir = Directory.GetDirectories((string)Globals.settings.ScriptsPath);
            if (scriptsDir.Length == 0) {
                SrvLog.Write(string.Format("Folder: '{0}' has no scripts", (string)Globals.settings.ScriptsPath));
            }

            foreach (string scriptDir in scriptsDir)
            {
                string scriptPath = Path.Combine(scriptDir, (string)Globals.settings.TargetScript);

                if (File.Exists(scriptPath))
                {
                    Thread scriptThread = new Thread(ScriptWorker);
                    scriptThread.IsBackground = true;
                    scriptThread.Start(scriptPath);
                }
                else
                {
                    SrvLog.Write("Script folder " + Directory.GetParent(scriptPath).Name + " doesn't contain  " + Path.GetFileName(scriptPath) + ". Ignored!");
                }
                
               
            }
        }

        private static void ScriptWorker(object data)
        {
            string scriptPath = (string)data;

            SrvLog.Write(string.Format("Script Started: '{0}'", Directory.GetParent(scriptPath).Name));

            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);

            TimerCallback ScriptLauncher = new ScriptLauncher(scriptPath).RunScript;
            stateTimer = new Timer(ScriptLauncher, autoEvent, 1000, 60000); //1 seconds delay and every 1 minute

            // When autoEvent signals, dispose of the timer.
            autoEvent.WaitOne();
            stateTimer.Dispose();
        }
    }

    


}