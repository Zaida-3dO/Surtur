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

namespace Surtur_Watcher {
    public partial class Scan : Form {
        Watcher surtur;
        Form blackground;
        public Scan(Watcher surtur,Form blackground) {
            this.surtur = surtur;
            this.blackground = blackground;
          
            InitializeComponent();
        }

        private void Button3_Click(object sender, EventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog {
                Description = "Add a path to be Scanned",
                ShowNewFolderButton = false
            };
            if (!string.IsNullOrWhiteSpace(surtur.DH.RecentlySelectedPath))
                fbd.SelectedPath = surtur.DH.RecentlySelectedPath;
            if (!(fbd.ShowDialog(this) == DialogResult.OK)) return;
                surtur.DH.RecentlySelectedPath = fbd.SelectedPath;
            if (string.IsNullOrWhiteSpace(fbd.SelectedPath) || listBox1.Items.Contains(fbd.SelectedPath))
                return;
            listBox1.Items.Add(fbd.SelectedPath);
        }

        private void Button6_Click(object sender, EventArgs e) {
            if (SelectedWatchPath == null || string.IsNullOrEmpty(SelectedWatchPath))
                return;
            if (!AreYouSure(SelectedWatchPath, "Watched Directories"))
                return;
            listBox1.Items.Remove(SelectedWatchPath);
        }

        private void Button1_Click(object sender, EventArgs e) {
            if (radioButton1.Checked) {
                Launch();
            } else if (radioButton3.Checked) {
                OpenFileDialog fbd = new OpenFileDialog {
                    InitialDirectory = @"C:\ProgramData\surtur"
                };
                if (!(fbd.ShowDialog(this) == DialogResult.OK)) return;
                string SelectedPath = fbd.FileName;
                try{
                    DirectoryHandler.Load(SelectedPath);
                    surtur = ( new Form1(SelectedPath)).surtur;
                    Launch();
                } catch {
                    MessageBox.Show(this, "The file at " + SelectedPath + " is not a valid surtur save file");
                    return;
                }
            }else if(radioButton2.Checked){
                if (File.Exists(@"C:\ProgramData\surtur\" + textBox1.Text + "scn.srtr")) {
                    MessageBox.Show(this, "Name already used, change name");
                    return;
                } else {
                    surtur = (new Form1(@"C:\ProgramData\surtur\" + textBox1.Text + "scn.srtr")).surtur;
                    Launch();
                }
            }
            
           
        }
        void Launch() {
            List<string> allowedDirectories = new List<string>();
            List<string> ignoredDirectories = new List<string>();
            Hide();
            blackground.Hide();
            List<string> scannedFiles = new List<string>();
            if (checkBox2.Checked) {
                foreach (string directory in listBox1.Items) {
                    allowedDirectories.Add(directory);
                }
                foreach (string path in listBox1.Items) {
                    //TODO unauthorizwed acces to files and co.
                    foreach (string File in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
                        try {
                            if (!allowedDirectories.Contains(Path.GetDirectoryName(File))) {
                                bool ignored = false;
                                string parent = File;
                                while (Path.GetDirectoryName(parent) != null) {
                                    parent = Path.GetDirectoryName(parent);
                                    if (ignoredDirectories.Contains(parent)) {
                                        ignored = true;
                                        break;
                                    }
                                }
                                if (!ignored) {
                                    blackground.Show();
                                    if (MessageBox.Show(blackground, "Sort Files within this folder: " + Path.GetDirectoryName(File) + "?", "Sort " + Path.GetFileName(Path.GetDirectoryName(File)) + " folder?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes) {
                                        
                                        if (MessageBox.Show(blackground, "Sort FOLDERS within this folder: " + Path.GetDirectoryName(File) + "?", "Sort " + Path.GetFileName(Path.GetDirectoryName(File)) + " folder?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes) {
                                            allowedDirectories.Add(Path.GetDirectoryName(File));
                                        } else {
                                            foreach (string FileInFold in Directory.EnumerateFiles(Path.GetDirectoryName(File))) {
                                                scannedFiles.Add(FileInFold);
                                            }
                                            ignoredDirectories.Add(Path.GetDirectoryName(File));
                                        }
                                    } else {
                                        ignoredDirectories.Add(Path.GetDirectoryName(File));
                                    }
                                    blackground.Hide();
                                }
                            } else {
                                scannedFiles.Add(File);
                            }
                        }catch(Exception ex) {
                            surtur.LogTransfers(ex.Message);
                        }
                    }
                }
            } else {
                foreach (string path in listBox1.Items) {
                    foreach (string File in Directory.EnumerateFiles(path)) {
                        scannedFiles.Add(File);
                    }
                }
            }

           
            surtur.HandleFileChanges(scannedFiles);
        }

        private void Scan_Load(object sender, EventArgs e) {
            CenterToScreen();
            radioButton1.Checked = true;
            foreach(string Directory in surtur.DH.AllWatchedPaths) {
                listBox1.Items.Add(Directory);
            }
        }
        string SelectedWatchPath;
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox1.SelectedItem == null)
                SelectedWatchPath = "";
            else
                SelectedWatchPath = listBox1.SelectedItem.ToString();
        }
        bool AreYouSure(string deleted, string from) {
            DialogResult ans = MessageBox.Show(this,"Are you sure you want to delete " + deleted + " from " + from, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            return (ans == DialogResult.Yes);
        }

        private void Button2_Click(object sender, EventArgs e) {
            blackground.Close();
            this.Close();
        }
    }
}
