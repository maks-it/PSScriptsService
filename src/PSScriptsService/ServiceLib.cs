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
using System.IO;

namespace PSScriptsService.ServiceLib {
    public static class SrvLog {
        public static void Write(string message) {

            //build the log path string, test again, testfff
            string logPath = (string)Globals.settings.LogPath;

            if (File.Exists(logPath) && new FileInfo(logPath).Length / 1048576 > (int)Globals.settings.LogSize) 
                File.Delete(logPath);

            try {
                using (StreamWriter sw = new StreamWriter(logPath, true))
                    sw.Write(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + message + Environment.NewLine);
            }
            catch {
            }
        }
    }
}
