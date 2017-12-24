using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Surtur_Watcher {
    static class Program {
        static int CrashCount=0;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Main2();
            //TODO v2 Start new Form?
            }
        static void Main2() {
            try {
                Application.Run(new Form1());
            } catch (Exception ex) {
                File.AppendAllText(@"C:\ProgramData\surtur" + "\\Transfers.log", "[" + DateTime.Now.ToString() + "]" + "Fatal Error " + ex.Message + " ... Restarting  \r\n");
                if (CrashCount++ < 5) {
                    Main2();
                } else {
                    MessageBox.Show("The App has encountered an error and needs to close", "Surtur Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

            }
        }
            public static void ResetCrashCount() {
            CrashCount = 0;
        }
    }
}
