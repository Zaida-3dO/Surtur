using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using Surtur_Core;
using System.IO;
using Surtur;
using System.Media;
namespace Surtur_Watcher {
 
    public partial class Form1 : Form, ISurtur {
        //TODO History, undo, ignore, view details
        //TODO v2 Click on Notification?
        public Form1() {
            InitializeComponent();
            surtur = new Watcher(@"C:\ProgramData\surtur\Sorter.srtr", this);
            transfers = new List<Tuple<string, string, bool>>();
            surtur.StartQueue();
        }
        private void Form1_Load(object sender, EventArgs e) {
            this.Hide();
            ShowInTaskbar = false;
            Notification("Surtur(File Sorter)", "Surtur is Now active and will help sort your files automatically.", ToolTipIcon.Info);
            Program.ResetCrashCount();
        }


        #region  Implementing ISurtur
        Watcher surtur;
        public Form Form { get { return this; } }
        public void Notification(string Title, string Text, ToolTipIcon img) {
              notifyIcon1.ShowBalloonTip(1000, Title, Text, img);
        }
        List<Tuple<string, string, bool>> transfers;
        public void TransferNotification(string From, string To,bool Successfull) {
            if (!timer1.Enabled) {
                ShowTransferNotification(From, To, Successfull);
                timer1.Start();
            } else {
                Tuple<string, string, bool> hya = new Tuple<string, string, bool>(From, To, Successfull);
                transfers.Add(hya);
                timer1.Start();
            }

        }
        public void ShowTransferNotification(string From, string To, bool Successfull) {
            string title, text;
            if (Successfull) {
                title = "Auto-Moved" + From;
                text = "Succesfully Moved " + From + " to " + To;
                notifyIcon1.ShowBalloonTip(1000, title, text, ToolTipIcon.Info);
            } else {
                title = "Error Moving " + From;
                text = "Failed to move " + From + " to " + To + ". click here for more details";
                notifyIcon1.ShowBalloonTip(1000, title, text, ToolTipIcon.Error);
            }
        }
        public void ShowTransferNotifications(int pass, int fail) {
            notifyIcon1.ShowBalloonTip(1000, "Auto-Moved " + (pass + fail) + " Files", pass + " Moved Successfully, and " + fail + " Failed. Click here for more details",ToolTipIcon.Info);
        }
            public void ShowSelectPath() {
            BringToFront();
            SystemSounds.Hand.Play();
          //  ShowInTaskbar = true;
        }
        public void HideSelectPath() {
           // ShowInTaskbar = false;
        }
        public void UpdateQueue(List<string> QueueList) {
            FullPaths = new Dictionary<string, string>();
            List<string> marker = new List<string>();
            foreach (string marked in checkedListBox1.CheckedItems)
                marker.Add(marked);
            checkBox2.Checked = false;
          
            checkedListBox1.Items.Clear();
            if (QueueList.Count > 0) {
                foreach (string item in QueueList) {
                    if (item.Equals(surtur.FoundFile)) continue;
                    string FileType = surtur.CleanUpType(Path.GetExtension(item));
                    if (FileType.Equals(surtur.Format)) {
                        FullPaths.Add( Path.GetFileName(item), item);
                        if(marker.Contains(Path.GetFileName(item)))
                            checkedListBox1.Items.Add(Path.GetFileName(item),true);
                        else
                            checkedListBox1.Items.Add(Path.GetFileName(item));
                    }
                    //TODO PendingToolStripMenuItem. Make move, cancel, cancel and ignore
                }
            }
            checkBox2.Visible = checkedListBox1.Visible = (checkedListBox1.Items.Count > 0);
        }
        public string FilePath {
            set { TypeName.Text = value; }
        }
        public string FileType {
            set { Type.Text = value; }
        }
        public string FileDestination {
            set { movePath.Text = value; }
            
        }
        public void RefreshView() {
            flowLayoutPanel1.Controls.Clear();
            if (surtur.CurrentlyWatched.Parent != null) {
                Button btn = new Button {
                    Text = "...",
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Height = 50,
                    AutoSize = true
                };
                btn.Click += new EventHandler((o, a) => {
                    surtur.NavigateTo(surtur.CurrentlyWatched.Parent);
                    RefreshView();
                });
                flowLayoutPanel1.Controls.Add(btn);
            }
            foreach (string si in surtur.StorageButtons) {
                flowLayoutPanel1.Controls.Add(GenerateButton(si));
            }
            Button btn2 = new Button {
                Text = "+",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Height = 50,
                AutoSize = true
            };
            btn2.Click += new EventHandler((o, a) => {
                string newType = Interaction.InputBox("Enter a file Type to Add under " + surtur.CurrentlyWatched.Handlee, "Add Type", "");
                string newPath = "";
                if (!string.IsNullOrWhiteSpace(newType)) {
                    FolderBrowserDialog fbd = new FolderBrowserDialog {
                        Description = "Select Path to Save this type to",
                        ShowNewFolderButton = true
                    };
                    if (!string.IsNullOrWhiteSpace(surtur.DH.RecentlySelectedPath))
                        fbd.SelectedPath = surtur.DH.RecentlySelectedPath;
                    if (!(fbd.ShowDialog() == DialogResult.OK)) return;
                    surtur.DH.RecentlySelectedPath = fbd.SelectedPath;
                    newPath = fbd.SelectedPath;
                }
                if (!string.IsNullOrWhiteSpace(newPath)) {
                    surtur.AddSubType(newType, newPath);

                    flowLayoutPanel1.Controls.Remove(btn2);
                    flowLayoutPanel1.Controls.Add(GenerateButton(newType));
                    flowLayoutPanel1.Controls.Add(btn2);
                }
            });
            flowLayoutPanel1.Controls.Add(btn2);
        }
        Button GenerateButton(string sub) {
            Button btn = new Button {
                Text = sub,
                Height = 50,
                AutoSize = true
            };
            btn.Click += new EventHandler((o, a) => {
                surtur.NavigateTo(surtur.CurrentlyWatched.GetHandle(sub));
            });
            return btn;
        }
#endregion


        private void CheckBox2_CheckedChanged(object sender, EventArgs e) {
            if (checkBox2.Checked) {
                checkBox2.Text = "Uncheck All";
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    checkedListBox1.SetItemChecked(i, true);
            } else {
                checkBox2.Text = "Check All";
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    checkedListBox1.SetItemChecked(i, false);
            }
        }
        void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            CPToolStripMenuItem_Click(sender, e);
        }
        private void CPToolStripMenuItem_Click(object sender, EventArgs e) {
            AddScanDir CP = new AddScanDir(surtur.DH);
            CP.Show();
            surtur.NotBusy();
            this.Hide();
        }
        private void ExitToolStripMenuItem1_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Are you Sure you want to close surtur", "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                this.Close();
            }
        }

        private void Button1_Click(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                if (MessageBox.Show("You, clicked ignore,Are you Sure you want to Leave this file " + ((checkedListBox1.CheckedItems.Count > 0) ? " and " + checkedListBox1.CheckedItems.Count + " more" : "")+" unsorted, you can always change later in the Menu", "Confirm Ignore", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                    List<string> power = GetCheckedPaths(checkedListBox1.CheckedItems);
                    HideForm();
                    if (power.Count > 0) {
                        foreach (string pathTo in power) {
                            surtur.IgnorePath(pathTo, false);
                        }
                    }
                    surtur.IgnorePath();
                }
                return;
            }
            if (MessageBox.Show("Are you sure you want to Move " + surtur.FoundFile + ((checkedListBox1.CheckedItems.Count > 0) ? " and " + checkedListBox1.CheckedItems.Count + " more" : "") + " to " + surtur.Destination, "Confirm Move", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                
                List<string> power = GetCheckedPaths(checkedListBox1.CheckedItems);
                HideForm();
               
                if (power.Count > 0) {
                    foreach (string pathTo in power) {
                        surtur.MoveFile(pathTo,false);
                    }
                }
                surtur.MoveFile();
            }
        }
        void HideForm() {
            this.Hide();
            checkedListBox1.Items.Clear();
            checkedListBox1.Visible = false;
            checkBox2.Visible = false;
        }
        Dictionary<string,string> FullPaths;
        List<string> GetCheckedPaths(CheckedListBox.CheckedItemCollection names) {
            List<string> ret = new List<string>();
            foreach (string name in names)
                ret.Add(FullPaths[name]);
            return ret;
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e) {
            List<string> scannedFiles = new List<string>();
            foreach(string path in surtur.DH.AllWatchedPaths) {
                foreach(string File in Directory.EnumerateFiles(path)) {
                    scannedFiles.Add(File);
                }
            }
            surtur.HandleFileChanges(scannedFiles);
        }

        private void Timer1_Tick(object sender, EventArgs e) {
            
            if (transfers.Count <= 3) {
                foreach(Tuple<string,string,bool> trans in transfers) {
                    ShowTransferNotification(trans.Item1, trans.Item2, trans.Item3);
                }
            } else {
                int pass=0, fail=0;
                foreach (Tuple<string, string, bool> trans in transfers) {
                    if (trans.Item3)
                        pass++;
                    else
                        fail++;
                }
                ShowTransferNotifications(pass, fail);
            }
            transfers.Clear();
            timer1.Stop();
        }
    }
    
}
