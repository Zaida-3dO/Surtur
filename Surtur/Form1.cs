using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Surtur_Core;
using System.Windows.Forms;
using Microsoft.VisualBasic;
namespace Surtur {
    public partial class AddScanDir : Form {
        DirectoryHandler DH;
        TreeNode SelectedNode;
        String SelectedWatchPath;
        String SelectedIgnorePath;
        String SelectedIgnoreType;
        public AddScanDir(DirectoryHandler DH) {
            this.DH = DH;
            
            InitializeComponent();
            Fresh();
        }
        void Fresh() {
            foreach(string a in DH.AllWatchedPaths) {
                listBox1.Items.Add(a);
            }
            foreach (string a in DH.AllIgnoredPaths) {
                listBox2.Items.Add(a);
            }
            foreach (string a in DH.AllIgnoredTypes) {
                listBox3.Items.Add(a);
            }
            RefreshTree();
        }
        void RefreshTree() {
            SelectedNode = null;
            HandlerTree.Nodes.Clear();
            foreach(string type in DH.AllHandledTypes) {
                HandlerTree.Nodes.Add(type,type);
                StorageInfo si = DH.GetHandle(type);
                if (si.NeedsPrompt) {
                    Unwrap(type, si);
                }
            }
        }
        void Unwrap(string node,StorageInfo si) {
            foreach(string type in si.AllHandledTypes) {
                string path = node + "\\" + type;
                AddSubNode(node, type);
                StorageInfo sii = si.GetHandle(type);
                if (sii.NeedsPrompt)
                    Unwrap(path, sii);
            }
        }

        private void Button1_Click(object sender, EventArgs e) {
            //Add Buton
            string present = "";
            if (string.IsNullOrWhiteSpace(NewName.Text) || string.IsNullOrWhiteSpace(NewPath.Text))
                return;
            string key = NewName.Text;
            string defPath = NewPath.Text;
            NewName.Text = "";
            NewPath.Text = "";
            if (SelectedNode!=null)
                 present = SelectedNode.FullPath;
            else {
                DH.SetHandler(key, new StorageInfoBuilder()
                                                .SetDefaultPath(defPath)
                                                .NeedsPrompting(false)
                                                .SetHandledName(key)
                                                .SetParent(null)
                                                .Build());
                RefreshTree();
                Save();
                return;
            }
            string[] nodes = present.Split(new char[] { '\\' });
            bool first = true;
            StorageInfo si=DH.GetHandle(nodes[0]); 
            foreach (string node in nodes) {
                if (first)
                    first = false;
                else {
                    si = si.GetHandle(node);
                }


            }
            si.NeedsPrompt = true;
            si.SetHandler(key, new StorageInfoBuilder()
                                        .SetDefaultPath(defPath)
                                        .NeedsPrompting(false)
                                        .SetHandledName(key)
                                        .SetParent(si)
                                        .Build());
            RefreshTree();
            Save();
        }
        void AddSubNode(string before,string newNode) {
            string[] nodes = before.Split(new char[] { '\\' });
            List<string> nodeList = nodes.ToList();
            nodeList.RemoveAt(0);
            if (nodeList.Count == 0) {
                TreeNode[] happy = HandlerTree.Nodes.Find(nodes[0], false);
                HandlerTree.Nodes.Find(nodes[0],false)[0].Nodes.Add(newNode,newNode);
            } else {
                TreeNode a = GetLeaf(HandlerTree.Nodes.Find(nodes[0], false)[0], nodeList);
                a.Nodes.Add(newNode,newNode);
            }
        }
        TreeNode GetLeaf(TreeNode source,List<string> path) {
            if (path.Count == 1) {
                return source.Nodes.Find(path[0], false)[0];
            }
            string first = path[0];
            path.RemoveAt(0);
            return GetLeaf(source.Nodes.Find(first, false)[0], path);
        }
        private void Button2_Click(object sender, EventArgs e) {
            //Remove button
            if (SelectedNode != null) {
                if (!AreYouSure(SelectedNode.FullPath+( (SelectedNode.Nodes.Count > 0)?" and all it's SubTypes":""), "Handled Types"))
                    return;
                string fullPath = SelectedNode.FullPath;
                string[] nodes = fullPath.Split(new char[] { '\\' });
               // HandlerTree.Nodes.Remove(SelectedNode);
                if (nodes.Length == 1) {
                    DH.RemoveHandler(nodes[0]);
                } else {
                    bool first = true;
                    StorageInfo temp = DH.GetHandle(nodes[0]);
                    StorageInfo si=null;
                    foreach (string node in nodes) {
                        if (first)
                            first = false;
                        else {
                            si = temp;
                            temp = si.GetHandle(node);
                        }


                    }
                    si.RemoveHandler(nodes[nodes.Length - 1]);
                }
                SelectedNode = null;
                RefreshTree();
                Save();
            }
            

        }

        private void HandlerTree_AfterSelect(object sender, TreeViewEventArgs e) {
            SelectedNode = HandlerTree.SelectedNode;
            string[] nodes = SelectedNode.FullPath.Split(new char[] { '\\' });
            bool first = true;
            StorageInfo si = DH.GetHandle(nodes[0]);
            foreach (string node in nodes) {
                if (first)
                    first = false;
                else {
                    si = si.GetHandle(node);
                }
            }
            DefaultPath.Text = si.DefaultPath;
            TypeName.Text = si.Handlee;
        }
        void Save() {
            DH.Save(@"C:\ProgramData\Sorter.srtr.temp");
            File.Delete(@"C:\ProgramData\Sorter.srtr");
            File.Move(@"C:\ProgramData\Sorter.srtr.temp", @"C:\ProgramData\Sorter.srtr");
        }

        private void Button3_Click(object sender, EventArgs e) {
            //Add watch Path
            FolderBrowserDialog fbd = new FolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.UserProfile,
                Description = "Add a path to be Scanned",
                ShowNewFolderButton = false
            };
            fbd.ShowDialog();
            if (string.IsNullOrWhiteSpace(fbd.SelectedPath)||DH.AllWatchedPaths.Contains(fbd.SelectedPath))
                return;
            DH.AddPath(fbd.SelectedPath);
            listBox1.Items.Add(fbd.SelectedPath);
            Save();
        }

        private void Button6_Click(object sender, EventArgs e) {
            //Remove watch Path
            if (SelectedWatchPath == null||string.IsNullOrEmpty(SelectedWatchPath))
                return;
            if (!AreYouSure(SelectedWatchPath, "Watched Directories"))
                return;
            listBox1.Items.Remove(SelectedWatchPath);
            DH.RemovePath(SelectedWatchPath);
            Save();
        }
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox1.SelectedItem == null) 
                SelectedWatchPath = "";
            else
                SelectedWatchPath = listBox1.SelectedItem.ToString();
        }
        private void ListBox2_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox2.SelectedItem == null)
                SelectedIgnorePath = "";
            else
                SelectedIgnorePath = listBox2.SelectedItem.ToString();
        }
        private void ListBox3_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox3.SelectedItem == null)
                SelectedIgnoreType = "";
            else
                SelectedIgnoreType = listBox3.SelectedItem.ToString();
        }
        private void Button4_Click(object sender, EventArgs e) {
            //Remove ignore Path
            if (SelectedIgnorePath == null || string.IsNullOrEmpty(SelectedIgnorePath))
                return;
            if (!AreYouSure(SelectedIgnorePath, "Ignored Directories"))
                return;
            listBox2.Items.Remove(SelectedIgnorePath);
            DH.RemovePath(SelectedIgnorePath);
            Save();
        }

        private void Button5_Click(object sender, EventArgs e) {
            //Remove ignore type
            if (SelectedIgnorePath == null || string.IsNullOrEmpty(SelectedIgnoreType))
                return;
            if (!AreYouSure(SelectedIgnoreType, "Ignored Types"))
                return;
            listBox3.Items.Remove(SelectedIgnoreType);
            DH.RemovePath(SelectedIgnoreType);
            Save();
        }

        private void Button7_Click(object sender, EventArgs e) {
            //Add watch Path
            FolderBrowserDialog fbd = new FolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.UserProfile,
                Description = "Add a path to be Ignored",
                ShowNewFolderButton = false
            };
            fbd.ShowDialog();
            if (string.IsNullOrWhiteSpace(fbd.SelectedPath)||DH.AllIgnoredPaths.Contains(fbd.SelectedPath))
                return;
            DH.AddIgnorePath(fbd.SelectedPath);
            listBox2.Items.Add(fbd.SelectedPath);
            Save();
        }

        private void Button9_Click(object sender, EventArgs e) {
            //New Path
            FolderBrowserDialog fbd = new FolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.UserProfile,
                Description = "Select Path to Save this type to",
                ShowNewFolderButton = true
            };
            fbd.ShowDialog();
            NewPath.Text=fbd.SelectedPath;
        }

        bool AreYouSure(string deleted, string from) {
            DialogResult ans = MessageBox.Show( "Are you sure you want to delete "+deleted+" from " +from,"Confirm Delete",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2);
            return (ans == DialogResult.Yes);
        }

        private void Button8_Click(object sender, EventArgs e) {
            string ans = Interaction.InputBox("Enter a file Type to Ignore", "Ignore Type", ".txt");
            if (string.IsNullOrWhiteSpace(ans))
                return;
            if (ans.First() == '.') 
                ans = ans.Substring(1);
            if (DH.AllIgnoredTypes.Contains(ans)) return;
            listBox3.Items.Add(ans);
            DH.AddIgnoreType(ans);
        }
    }
}
