using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using Surtur_Core;
using System.IO;
using Surtur;

namespace Surtur_Watcher {
 
    public partial class Form1 : Form, ISurtur {
        public Form1() {
            InitializeComponent();
            surtur = new Watcher(@"C:\ProgramData\surtur\Sorter.srtr", this);
        }
        private void Form1_Load(object sender, EventArgs e) {
            this.Hide();
            ShowInTaskbar = false;
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = "Surtur is Now active and will help sort your files automatically.";
            notifyIcon1.BalloonTipTitle = "Surtur(File Sorter)";
            notifyIcon1.ShowBalloonTip(1000);
        }


        #region  Implementing ISurtur
        Watcher surtur;
        public Form Form { get { return this; } }
        public void Notification(string Title, String Text, ToolTipIcon img) {
              notifyIcon1.ShowBalloonTip(1000, Title, Text, img);
        }
       public void ShowSelectPath() {
            BringToFront();
            ShowInTaskbar = true;
        }
        public void HideSelectPath() {
            ShowInTaskbar = false;
        }
        public void UpdateQueue(List<string> QueueList) {
            //TODO clear check on load page
            checkBox2.Visible = checkedListBox1.Visible = (QueueList.Count > 0);
            checkedListBox1.Items.Clear();
            //TODO persistent checks after add
            if (QueueList.Count > 0) {
                foreach (string item in QueueList) {
                    string FileType = Path.GetExtension(item).Substring(1);
                    if (FileType.StartsWith("."))
                        FileType = FileType.Substring(1);
                    if (FileType.Equals(surtur.Format))
                        checkedListBox1.Items.Add(Path.GetFileName(item));                    PendingToolStripMenuItem.
                }
            }
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
            this.Hide();
        }
        private void ExitToolStripMenuItem1_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Are you Sure you want to close surtur", "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                this.Close();
            }
        }

        private void Button1_Click(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                if (MessageBox.Show("You, clicked ignore,Are you Sure you want to Leave this file unsorted, you can always change later in the Menu", "Confirm Ignore", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                    HideForm();
                    surtur.IgnorePath();
                    //recalls Handle on multiple ignore
                    //TODO Ignore multiple
                }
                return;
            }
            if (MessageBox.Show("Are you sure you want to Move " + surtur.FoundFile + ((checkedListBox1.CheckedItems.Count > 0) ? "and " + checkedListBox1.CheckedItems.Count + " more" : "") + " to " + surtur.Destination, "Confirm Move", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                CheckedListBox.CheckedItemCollection a = checkedListBox1.CheckedItems;
                List<string> power = new List<string>();
                foreach(string path in a) {
                    power.Add(path);
                }
                HideForm();
                surtur.MoveFile();
                if (power.Count > 0) {
                    foreach (string pathTo in power) {
                        surtur.MoveFile(pathTo);
                    }
                }
            }
            if (!surtur.IsBusy)
                HideForm();
        }
        void HideForm() {
            this.Hide();
            checkedListBox1.Items.Clear();
            checkedListBox1.Visible = false;
            checkBox2.Visible = false;
        }
    }
    
}
