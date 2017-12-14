using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Surtur_Core {
    //Recheck file after a while(after a read error
    //timeout
    //Move from source to source
    //Scan
    public class Watcher {
        public DirectoryHandler DH;
        public string FoundFile;
        public string HandledType;
        public List<string> StorageButtons;
        public StorageInfo CurrentlyWatched;
        public List<string> QueueList;

        public string Destination;
        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher" /> class.
        /// </summary>
        /// <param name="DH">The directory handler we are working with.</param>
        /// <param name="SP">The page to synchronize with(Just pass in 'this' from the expected form).</param>
        public Watcher(string SavePath, Form SP) {
            SyncedPage = SP;
            StorageButtons = new List<string>();
            this.SavePath = SavePath;
            Reload();
            busy = false;
            SaveDir = Path.GetDirectoryName(SavePath);



            FileSystemWatcher fileSystemWatcher1 = new FileSystemWatcher(SaveDir) {
                EnableRaisingEvents = true,
                SynchronizingObject = SyncedPage
            };
            fileSystemWatcher1.Changed += new FileSystemEventHandler(FileSystemWatcher1_CreatedDH);
            fileSystemWatcher1.Created += new FileSystemEventHandler(FileSystemWatcher1_CreatedDH);
            fileSystemWatcher1.Renamed += new RenamedEventHandler(this.FileSystemWatcher1_RenamedDH);
        }

        public void RemoveFromList(string toBeRemoved) {

        }
        public void AddToList(string toBeAdded) {

        }
        public void AddToList(string toBeAdded, long delayInMs) {

        }
        public void NavigateTo(StorageInfo si) {

        }
        public void RemoveFromList(List<string> toBeRemoved) {

        }
        public void AddToList(List<string> toBeAdded) {

        }
        bool busy;
        bool TriggerOnRename;
        Form SyncedPage;
        string SavePath;
        string SaveDir;
        List<FileSystemWatcher> Dlist;

        void FileSystemWatcher1_CreatedDH(object sender, FileSystemEventArgs e) {
            if (e.FullPath.Equals(SavePath))
                HandleFileChanges(e.Name, e.FullPath);
        }
        private void FileSystemWatcher1_RenamedDH(object sender, RenamedEventArgs e) {
            if (e.FullPath.Equals(SavePath)) {
                Reload();
            }
        }
        /// <summary>
        /// Changes the working directory handler.
        /// </summary>
        /// <param name="DH">The new directory handler.</param>
        public void RebaseDH(DirectoryHandler DH) {
            this.DH = DH;
            Dlist = new List<FileSystemWatcher>();
            Init();
        }
        void Save() {
            try {
                DH.Save(SavePath + ".temp");
                DH.Save(SavePath);
            } catch (Exception ex) {
                //TODO logerror
            }
        }
        void Notification(string Title, String Text, ToolTipIcon img) {
            //  notifyIcon1.ShowBalloonTip(1000, Title, Text, img);
            MessageBox.Show(Text, Title);
        }
        void HandleQueue() {
            string FullPath = DH.Pop();
            string Name = Path.GetFileName(FullPath);
            string Type = Path.GetExtension(Name);
            if (Type.StartsWith("."))
                Type = Type.Substring(1);
            if (DH.AllHandledTypes.Contains(Type)) {
                if (DH.GetHandle(Type).NeedsPrompt) {
                    SyncedPage.Show();
                    SyncedPage.BringToFront();
                    SyncedPage.WindowState = FormWindowState.Normal;
                    FoundFile = FullPath;
                    SI_Click(DH.GetHandle(Type));
                } else {
                    string from = FullPath;
                    string to = DH.GetHandle(Type).DefaultPath + "\\" + Name;
                    try {
                        File.Move(from, to);
                        Notification("Auto-Moved " + Name, " Succesfully Moved " + Name + " to " + to, ToolTipIcon.Info);
                        LogTransfers(Name, from, to);
                    } catch (Exception ex) {
                        LogTransfers("An Error Occured: when Moving " + from + ":" + ex.Message);
                        AddToList(from, 60000);
                    }

                }
            } else {
                FolderBrowserDialog fbd = new FolderBrowserDialog {
                    Description = Type + " Sort. Organize your Folders, where would you like to store ." + Type + " files. (To ignore these files, or change your settings Open Surtur by clicking the icon in your Tray.)",
                    ShowNewFolderButton = true
                };
                if (!string.IsNullOrWhiteSpace(DH.RecentlySelectedPath))
                    fbd.SelectedPath = DH.RecentlySelectedPath;
                SyncedPage.BringToFront();
                bool sit = SyncedPage.ShowInTaskbar;
                SyncedPage.ShowInTaskbar = true;
                //TODO C# sounds
                if ((fbd.ShowDialog() == DialogResult.OK)) {
                    DH.RecentlySelectedPath = fbd.SelectedPath;
                    DH.SetHandler(Type, new StorageInfoBuilder()
                                            .SetDefaultPath(fbd.SelectedPath)
                                            .NeedsPrompting(false)
                                            .SetHandledName(Type)
                                            .SetParent(null)
                                            .Build());
                    Notification("Moving " + Type, "To change, Click here, Files of type " + Type + " will now be moved to " + fbd.SelectedPath, ToolTipIcon.Info);
                    DH.Push(FullPath);
                    HandleQueue();
                }
                SyncedPage.ShowInTaskbar = sit;
            }
            if (DH.Queue().Count > 0)
                HandleQueue();
            else
                busy = false;
        }
        void SI_Click(StorageInfo si) {
            StorageButtons = new List<string>();
            Destination = si.DefaultPath;
            HandledType = si.Handlee;

            if (si.NeedsPrompt)
                foreach (string sub in si.AllHandledTypes) {
                    StorageButtons.Add(sub);
                }
          
        }
        public void AddSubType(string Type, string Path) {
            CurrentlyWatched.NeedsPrompt = true;
            CurrentlyWatched.SetHandler(Type, new StorageInfoBuilder()
                                .SetDefaultPath(Path)
                                .NeedsPrompting(false)
                                .SetHandledName(Type)
                                .SetParent(CurrentlyWatched)
                                .Build());
            Save();
        }



        void Init() {
            foreach (string path in DH.AllWatchedPaths) {

                FileSystemWatcher fsw = new FileSystemWatcher(path) {
                    EnableRaisingEvents = true,
                    SynchronizingObject = SyncedPage
                };
                fsw.Created += new FileSystemEventHandler(FileSystemWatcher1_Created);
                fsw.Renamed += new RenamedEventHandler(FileSystemWatcher1_Renamed);
                Dlist.Add(fsw);
            }
        }
        void Reload() {
            try {
                DH = DirectoryHandler.Load(SavePath);
            } catch {
                try {
                    DH = DirectoryHandler.Load(SavePath + ".temp");
                } catch {
                    DH = new DirectoryHandler();
                }
            }
            Dlist = new List<FileSystemWatcher>();
            Init();

        }
        void FileSystemWatcher1_Created(object sender, FileSystemEventArgs e) {
            HandleFileChanges(e.Name, e.FullPath);
        }
        void FileSystemWatcher1_Renamed(object sender, RenamedEventArgs e) {
            if (TriggerOnRename)
                HandleFileChanges(e.Name, e.FullPath);
        }
        void HandleFileChanges(string Name, string FullPath) {
            //TODO if(folder) break;
            foreach (string ignore in DH.AllIgnoredPaths)
                if (FullPath.Equals(ignore))
                    return;
            string Type = Path.GetExtension(Name);
            if (Type.StartsWith("."))
                Type = Type.Substring(1);
            if (DH.AllIgnoredTypes.Contains(Type)) {
                LogTransfers("Ignored " + Name + " due to file type");
                return;
            }

            if (busy) {
                DH.Push(FullPath);
                //TODO code to add duplicate to listbox
                //TODO figure this async shii out
            } else {
                DH.Push(FullPath);
                busy = true;
                HandleQueue();

            }
        }
        void LogTransfers(string file, string from, string to) {
            File.AppendAllText(SaveDir + "\\Transfers.log", "[" + DateTime.Now.ToString() + "]" + " Succesfully moved " + file + " from " + from + " to " + to + "\r\n");
        }
        void LogTransfers(string logText) {
            File.AppendAllText(SaveDir + "\\Transfers.log", "[" + DateTime.Now.ToString() + "]" + " " + logText + "\r\n");
        }

        public void IgnorePath() {
            DH.AddIgnorePath(FoundFile);
            LogTransfers(FoundFile + " added to ignore List");
            if (DH.Queue().Count > 0)
                HandleQueue();
            else
                busy = false;
        }
        public void MoveFile() {
            LogTransfers(Path.GetFileName(FoundFile), FoundFile, Destination);
            File.Move(FoundFile, Destination + "\\" + Path.GetFileName(FoundFile));

            if (DH.Queue().Count > 0)
                HandleQueue();
            else
                busy = false;
        }
        public void MoveFile(string FileToMove) {
            LogTransfers(Path.GetFileName(FileToMove), FileToMove,Destination);
            File.Move(FileToMove, Destination + "\\" + Path.GetFileName(FileToMove));
        }
    }
}



