using System;
using Surtur_Core;
using System.Windows.Forms;

namespace Surtur {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DirectoryHandler DH = LoadDH();
            Application.Run(new AddScanDir(DH));
        }
        static DirectoryHandler LoadDH() {
            try {
                return DirectoryHandler.Load(@"C:\ProgramData\surtur\Sorter.srtr");
            } catch {
                try {
                    return DirectoryHandler.Load(@"C:\ProgramData\surtur\Sorter.srtr.temp");
                } catch {
                    return new DirectoryHandler();
                }
            }
            
        }
    }
}
