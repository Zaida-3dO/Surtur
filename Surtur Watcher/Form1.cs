using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Surtur_Core;
using System.IO;
using Surtur;

namespace Surtur_Watcher {
    //TODO handle downloaded folders
    //select folder go to last selected
    public partial class Form1 : Form {
        DirectoryHandler DH;
        public Form1() {
            InitializeComponent();
            Reload();
        }
        List<FileSystemWatcher> Dlist;
        void Init() {
            for (int i= 0;i < Dlist.Count;i++) {
                Dlist[i] = null;
            }
            Dlist.Clear();
            foreach(string path in DH.AllWatchedPaths) {

                FileSystemWatcher fsw = new FileSystemWatcher(path) {
                    EnableRaisingEvents = true,
                    SynchronizingObject = this
                };
                fsw.Created += new FileSystemEventHandler(FileSystemWatcher1_Created);
                Dlist.Add(fsw);
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.Hide();
            ShowInTaskbar = false;
            notifyIcon1.Visible = true;
            
            notifyIcon1.BalloonTipText = "Surtur is Now active and will help sort your files automatically.";
            notifyIcon1.BalloonTipTitle = "Surtur(File Sorter)";
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            AddScanDir CP = new AddScanDir(DH);
            CP.Show();
        }

        private void FileSystemWatcher1_Created(object sender,FileSystemEventArgs e) {
            foreach (string ignore in DH.AllIgnoredPaths) {
                if (e.FullPath.StartsWith(ignore)) {
                    TypeName.Text = "Skipped" + e.Name;
                    return;
                }
            }
            string Type = Path.GetExtension(e.Name);
            if (Type.StartsWith("."))
                Type = Type.Substring(1);
            if (DH.AllIgnoredTypes.Contains(Type)) {
                TypeName.Text = "ignore " + e.Name;
                LogTransfers("Ignored " + e.Name + " due to file type");
                return;
            }
            if(DH.AllHandledTypes.Contains(Type))
            if (DH.GetHandle(Type).NeedsPrompt) {
                    PromptCall(Type,e);
            } else {
                    string from = e.FullPath;
                    string to = DH.GetHandle(Type).DefaultPath +"\\"+ e.Name;
                    try {
                        File.Move(from,to);
                        notifyIcon1.ShowBalloonTip(1000, "Auto-Moved " + e.Name, " Succesfully Moved "+e.Name+" to " + to,ToolTipIcon.Info);
                        LogTransfers(e.Name,from,to);
                    } catch (Exception ex){
                        LogTransfers("An Error Occured: " + ex.Message + " when Moving " + from);
                    }
                    
                }

        }
        void PromptCall(String type, FileSystemEventArgs e) {
            this.Show();
            TypeName.Text = e.Name;
            foreach (string sub in DH.GetHandle(type).AllHandledTypes) {
                flowLayoutPanel1.Controls.Add(new Button {
                    Text = sub,
                    Height = 50,
                    AutoSize = true                    
                });
            }
        }
        void LogTransfers(string file, string from, string to) {
            File.AppendAllText(@"C:\ProgramData\surtur\Transfers.log","["+DateTime.Now.ToString()+"]"+ " Succesfully moved " + file + " from " + from + " to " + to+"\r\n");
        }
        void LogTransfers(string logText) {
            File.AppendAllText(@"C:\ProgramData\surtur\Transfers.log", "[" + DateTime.Now.ToString() + "]" + " "+ logText+"\r\n");
        }

        void Reload() {
            DH = DirectoryHandler.Load(@"C:\ProgramData\surtur\Sorter.srtr");
            Dlist = new List<FileSystemWatcher>();
            Init();
        }

        private void LiveToolStripMenuItem_Click(object sender, EventArgs e) {
            AddScanDir CP = new AddScanDir(DH);
            CP.Show();
            WindowState = FormWindowState.Normal;
        }

        private void FileSystemWatcher1_Renamed(object sender, RenamedEventArgs e) {
            if (e.Name.Equals("Sorter.srtr")) {
                Reload();
            }
        }

        private void ExitToolStripMenuItem1_Click(object sender, EventArgs e) {
            if(MessageBox.Show("Are you Sure you want to close surtur", "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes){
                this.Close();
            }
            
        }
    }
}
