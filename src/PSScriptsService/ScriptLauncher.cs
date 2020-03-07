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
using System.Threading;
using System.Text;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSScriptsService.ServiceLib;

namespace PSScriptsService {
    class ScriptLauncher {
        private string scriptPath;

        public ScriptLauncher(string path) {
            scriptPath = path;
        }

        object lockObject = new object();

        // This method is called by the timer delegate.
        public void RunScript(Object stateInfo) {
            if (Monitor.TryEnter(lockObject)) {
                try {
                    AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

                    StringBuilder stringBuilder = new StringBuilder();

                    Runspace rs = RunspaceFactory.CreateRunspace();
                    rs.Open();

                    PowerShell ps = PowerShell.Create();
                    ps.Runspace = rs;

                    string scriptPolicy = "Unrestricted";
                    if ((bool)Globals.settings.SignedScripts)
                        scriptPolicy = "AllSigned";

                    ps.AddScript("Set-ExecutionPolicy -Scope Process -ExecutionPolicy " + scriptPolicy);
                    ps.Invoke();

                    ps.AddScript(string.Format("Get-AuthenticodeSignature \"{0}\"", scriptPath));

                    foreach (PSObject result in ps.Invoke())  {
                        if ((bool)Globals.settings.SignedScripts) {
                            if (((Signature)result.BaseObject).Status != SignatureStatus.Valid) {
                                SrvLog.Write("Script " + Directory.GetParent(scriptPath).Name + " Signature Error! Correct, and restart the service.");

                                //signal the waiting thread
                                autoEvent.Set();

                                break;
                            }
                        }

                        SrvLog.Write("Invoking: " + scriptPath);

                        Command myCommand = new Command(scriptPath);

                        // Pass -Automated switch and -CuttrentDateTimeUtc, as UTC ISO 8601 string
                        myCommand.Parameters.Add(new CommandParameter("Automated", true));
                        myCommand.Parameters.Add(new CommandParameter("CurrentDateTimeUtc", DateTime.UtcNow.ToString("o")));

                        ps.Commands.Commands.Add(myCommand);
                        ps.Invoke();

                        /*foreach (PSObject obj in ps.Invoke())
                        {
                            SrvLog.Write(obj.ToString(), logPath);
                        }*/


                        //signal the waiting thread
                        //autoEvent.Set();
                    }
                }
                catch (Exception ex) {
                    SrvLog.Write(ex.Message.ToString());
                }
                finally {
                    Monitor.Exit(lockObject);
                }
            }
        }
    }
}
