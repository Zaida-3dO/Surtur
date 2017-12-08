using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using Surtur_Core;
using System.IO;
using Surtur;

namespace Surtur_Watcher {
 
    public partial class Form1 : Form {
        
        public Form1() {
            InitializeComponent();
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
            this.Hide();
        }

      
        void HandleQueue() {
            string FullPath = DH.Pop();
            string Name = Path.GetFileName(FullPath);
            string Type = Path.GetExtension(Name);
            if (Type.StartsWith("."))
                Type = Type.Substring(1);
            if (DH.AllHandledTypes.Contains(Type)) {
                if (DH.GetHandle(Type).NeedsPrompt) {
                    this.Show();
                    this.BringToFront();
                    this.WindowState = FormWindowState.Normal;
                  /* if (DH.Queue().Count > 0) {
                        checkedListBox1.Items.Clear();
                        foreach (string QPath in DH.Queue()) {
                            string QP;
                            if (QPath.StartsWith("."))
                                QP = QPath.Substring(1);
                            else
                                QP = QPath;
                            if (QP.Equals(Type))
                                checkedListBox1.Items.Add(QPath);
                        }
                        checkBox2.Visible = true;
                        checkedListBox1.Visible = true;
                    } else {
                        checkBox2.Visible = false;
                        checkedListBox1.Visible = false;
                    }*/
                    TypeName.Text = FullPath;
                    //PromptCall(e);
                    SI_Click(DH.GetHandle(Type));
                } else {
                    string from = FullPath;
                    string to = DH.GetHandle(Type).DefaultPath + "\\" + Name;
                    try {
                        File.Move(from, to);
                        notifyIcon1.ShowBalloonTip(1000, "Auto-Moved " + Name, " Succesfully Moved " + Name + " to " + to, ToolTipIcon.Info);
                        LogTransfers(Name, from, to);
                    } catch (Exception ex) {
                        LogTransfers("An Error Occured: when Moving " + from + ":" + ex.Message);
                       // DH.Push(from);
                    }

                }
            } else {
                FolderBrowserDialog fbd = new FolderBrowserDialog {
                    Description= Type+" Sort. Organize your Folders, where would you like to store ."+Type+" files. (To ignore these files, or change your settings Open Surtur by clicking the icon in your Tray.)",
                    ShowNewFolderButton = true
                };
                if (!string.IsNullOrWhiteSpace(DH.RecentlySelectedPath))
                    fbd.SelectedPath = DH.RecentlySelectedPath;
                if ((fbd.ShowDialog() == DialogResult.OK)) {
                    DH.RecentlySelectedPath = fbd.SelectedPath;
                    DH.SetHandler(Type, new StorageInfoBuilder()
                                            .SetDefaultPath(fbd.SelectedPath)
                                            .NeedsPrompting(false)
                                            .SetHandledName(Type)
                                            .SetParent(null)
                                            .Build());
                    notifyIcon1.ShowBalloonTip(1000, "Moving " + Type, "To change, Click here, Files of type " + Type + " will now be moved to " + fbd.SelectedPath, ToolTipIcon.Info);
                    DH.Push(FullPath);
                    HandleQueue();
                    //Save();
                }
            }
            if (DH.Queue().Count > 0)
                HandleQueue();
            else
                busy = false;
        }
        void PromptCall(FileSystemEventArgs e) {
            flowLayoutPanel1.Controls.Clear();
            string type = Path.GetExtension(e.Name);
            if (type.StartsWith("."))
                type = type.Substring(1);

            Type.Text = type;
            movePath.Text = DH.GetHandle(type).DefaultPath;
            foreach (string sub in DH.GetHandle(type).AllHandledTypes) {
                Button btn = new Button {
                    Text = sub,
                    Height = 50,
                    AutoSize = true
                };
                btn.Click += new EventHandler((o, a) => {
                    SI_Click(DH.GetHandle(type).GetHandle(sub));
                });
                flowLayoutPanel1.Controls.Add(btn);
            }
            Button btn2 = new Button {
                Text = "+",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Height = 50,
                AutoSize = true
            };
            btn2.Click += new EventHandler((o, a) => {
                string newType = Interaction.InputBox("Enter a file Type to Add under " + type, "Add Type", "");
                string newPath = "";
                if (!string.IsNullOrWhiteSpace(newType)) {
                    FolderBrowserDialog fbd = new FolderBrowserDialog {
                        Description = "Select Path to Save this type to",
                        ShowNewFolderButton = true
                    };
                    fbd.ShowDialog();
                    newPath = fbd.SelectedPath;
                }
                if (!string.IsNullOrWhiteSpace(newPath)) {
                    DH.GetHandle(type).NeedsPrompt = true;
                    DH.GetHandle(type).SetHandler(newType, new StorageInfoBuilder()
                                        .SetDefaultPath(newPath)
                                        .NeedsPrompting(false)
                                        .SetHandledName(newType)
                                        .SetParent(null)
                                        .Build());
                    Button btn3 = new Button {
                        Text = newType,
                        Height = 50,
                        AutoSize = true
                    };
                    btn3.Click += new EventHandler((od, ad) => {
                        SI_Click(DH.GetHandle(type).GetHandle(newType));
                    });
                    flowLayoutPanel1.Controls.Remove(btn2);
                    flowLayoutPanel1.Controls.Add(btn3);
                    flowLayoutPanel1.Controls.Add(btn2);

                    Save();
                }
            });
            flowLayoutPanel1.Controls.Add(btn2);
        }
        void Save() {
            DH.Save(@"C:\ProgramData\surtur\Sorter.srtr.temp");
            DH.Save(@"C:\ProgramData\surtur\Sorter.srtr");
        }
      

        void Reload() {
            try {
                DH =  DirectoryHandler.Load(@"C:\ProgramData\surtur\Sorter.srtr");
            } catch {
                try {
                    DH =  DirectoryHandler.Load(@"C:\ProgramData\surtur\Sorter.srtr.temp");
                } catch {
                    DH =  new DirectoryHandler();
                }
            }
            Dlist = new List<FileSystemWatcher>();
                Init();
          
        }

        private void LiveToolStripMenuItem_Click(object sender, EventArgs e) {
            AddScanDir CP = new AddScanDir(DH);
            CP.Show();
            this.Hide();
            WindowState = FormWindowState.Normal;
        }

        private void FileSystemWatcher1_Renamed(object sender, RenamedEventArgs e) {
            if (e.Name.Equals("Sorter.srtr")) {
                Reload();
            }
        }

        private void ExitToolStripMenuItem1_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Are you Sure you want to close surtur", "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                this.Close();
            }

        }

        private void Button1_Click(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                if (MessageBox.Show("You, clicked ignore,Are you Sure you want to Leave this file unsorted, you can always change later in the Menu", "Confirm Ignore", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                    DH.AddIgnorePath(TypeName.Text);
                    LogTransfers(TypeName.Text + " added to ignore List");
                    this.Hide();
                    checkedListBox1.Items.Clear();
                    checkedListBox1.Visible = false;
                    checkBox2.Visible = false;
                    if (DH.Queue().Count > 0)
                        HandleQueue();
                    else
                        busy = false;
                }
                return;
            }
            if (MessageBox.Show("Are you sure you want to Move "+ TypeName.Text+((checkedListBox1.CheckedItems.Count>0)?"and "+ checkedListBox1.CheckedItems.Count+" more":"")+" to "+movePath.Text, "Confirm Move", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                LogTransfers(Path.GetFileName(TypeName.Text), TypeName.Text, movePath.Text);
                File.Move(TypeName.Text, movePath.Text+"\\"+Path.GetFileName(TypeName.Text));
                if(checkedListBox1.CheckedItems.Count > 0) {
                    foreach (string pathTo in checkedListBox1.CheckedItems) {
                        LogTransfers(Path.GetFileName(pathTo), pathTo, movePath.Text);
                        File.Move(pathTo, movePath.Text + "\\" + Path.GetFileName(pathTo));
                    }
                }
                this.Hide();
                checkedListBox1.Items.Clear();
                checkedListBox1.Visible = false;
                checkBox2.Visible = false;
                if (DH.Queue().Count > 0)
                    HandleQueue();
                else
                    busy = false;
            }
        }
        void SI_Click(StorageInfo si) {
            flowLayoutPanel1.Controls.Clear();
            movePath.Text = si.DefaultPath;
            Type.Text = si.Handlee;
            if (si.Parent != null) {
                Button btn4 = new Button {
                    Text = "...",
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Height = 50,
                    AutoSize = true
                };
                btn4.Click += new EventHandler((o, a) => {
                    SI_Click(si.Parent);
                });
                flowLayoutPanel1.Controls.Add(btn4);
            }
            if (si.NeedsPrompt)
                foreach (string sub in si.AllHandledTypes) {
                    Button btn = new Button {
                        Text = sub,
                        Height = 50,
                        AutoSize = true
                    };
                    btn.Click += new EventHandler((o, a) => {
                        SI_Click(si.GetHandle(sub));
                    });
                    flowLayoutPanel1.Controls.Add(btn);
                }
            Button btn2 = new Button {
                Text = "+",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Height = 50,
                AutoSize = true
            };
            btn2.Click += new EventHandler((o, a) => {
                string newType = Interaction.InputBox("Enter a file Type to Add under " + si.Handlee, "Add Type", "");
                string newPath = "";
                if (!string.IsNullOrWhiteSpace(newType)) {
                    FolderBrowserDialog fbd = new FolderBrowserDialog {
                        Description = "Select Path to Save this type to",
                        ShowNewFolderButton = true
                    };
                    if (!string.IsNullOrWhiteSpace(DH.RecentlySelectedPath))
                        fbd.SelectedPath = DH.RecentlySelectedPath;
                    if (!(fbd.ShowDialog() == DialogResult.OK)) return;
                    DH.RecentlySelectedPath = fbd.SelectedPath;
                    newPath = fbd.SelectedPath;
                }
                if (!string.IsNullOrWhiteSpace(newPath)) {
                    si.NeedsPrompt = true;
                    si.SetHandler(newType, new StorageInfoBuilder()
                                        .SetDefaultPath(newPath)
                                        .NeedsPrompting(false)
                                        .SetHandledName(newType)
                                        .SetParent(si)
                                        .Build());
                    Button btn3 = new Button {
                        Text = newType,
                        Height = 50,
                        AutoSize = true
                    };
                    btn3.Click += new EventHandler((od, ad) => {
                        SI_Click(si.GetHandle(newType));
                    });
                    flowLayoutPanel1.Controls.Remove(btn2);
                    flowLayoutPanel1.Controls.Add(btn3);
                    flowLayoutPanel1.Controls.Add(btn2);

                    Save();
                }
            });
            flowLayoutPanel1.Controls.Add(btn2);
        }

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
    }
}
