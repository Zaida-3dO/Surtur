using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Surtur_Core {
    //TODO Recheck file after a while(after a read error
    //timeout
    //Move from source to source
    //Scan
    public class Watcher {
        public DirectoryHandler DH;
        string _foundFile;
        public string FoundFile { get { return _foundFile; } set {
                _foundFile = value;
                SyncedPage.FilePath= value;
            } }
        string _handledType;
        public string Format;
        public string HandledType {
            get { return _handledType; }
            set {
                _handledType = value;
                SyncedPage.FileType = value;
            }
        }
        public List<string> StorageButtons;
        public StorageInfo CurrentlyWatched;
        public bool IsBusy { get { return busy; } }
        bool busy;
        //TODO comments
        public bool TriggerOnRename;
        ISurtur SyncedPage;
        Form SyncedPageForm;
        string SavePath;
        string SaveDir;
        List<FileSystemWatcher> Dlist;
        string _destination;
        public string Destination {
            get { return _destination; }
            set {
                _destination = value;
                SyncedPage.FileDestination = value;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher" /> class.
        /// </summary>
        /// <param name="DH">The directory handler we are working with.</param>
        /// <param name="SP">The page to synchronize with(Just pass in 'this' from the expected form).</param>
        public Watcher(string SavePath, ISurtur SP) {
            SyncedPage = SP;
            TriggerOnRename = false;
            SyncedPageForm = SP.Form;
            StorageButtons = new List<string>();
            this.SavePath = SavePath;
            Reload();
            Save();
            busy = false;
            SaveDir = Path.GetDirectoryName(SavePath);
            FileSystemWatcher fileSystemWatcher1 = new FileSystemWatcher(SaveDir) {
                EnableRaisingEvents = true,
                SynchronizingObject = SyncedPageForm
            };
            fileSystemWatcher1.Changed += new FileSystemEventHandler(FileSystemWatcherHandleSave);
            fileSystemWatcher1.Created += new FileSystemEventHandler(FileSystemWatcherHandleSave);
            fileSystemWatcher1.Renamed += new RenamedEventHandler(FileSystemWatcherHandleSave);
        }

        public void RemoveFromList(string toBeRemoved) {

        }
        public void AddToList(string toBeAdded) {

        }
        public void AddToList(string toBeAdded, long delayInMs) {

        }
        public void NavigateTo(StorageInfo si) {
            SI_Click(si);
        }
        public void RemoveFromList(List<string> toBeRemoved) {

        }
        public void AddToList(List<string> toBeAdded) {

        }

        /// <summary>
        /// Files system watcher to handle save.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        public void FileSystemWatcherHandleSave(object sender, FileSystemEventArgs e) {
            if (e.FullPath.Equals(SavePath))
                Reload();
        }
        /// <summary>
        /// Changes the working directory handler.
        /// </summary>
        /// <param name="DH">The new directory handler.</param>
        public void RebaseDH(DirectoryHandler DH) {
            this.DH = DH;
            Init_Watchers();
        }
        void Save() {
            try {
                DH.Save(SavePath + ".temp");
                DH.Save(SavePath);
            } catch  {
                //TODO logerror
            }
        }
       string Pop() {
            string ret =  DH.Pop();
            SyncedPage.UpdateQueue(DH.Queue);
            return ret;
        }
        void Push(string newPath) {
            DH.Push(newPath);
            SyncedPage.UpdateQueue(DH.Queue);
        }
        void HandleQueue() {
            string FullPath =Pop();
            string Type = Path.GetExtension(FullPath);
            if (Type.StartsWith("."))
                Type = Type.Substring(1);
            Format = Type;
            if (DH.AllHandledTypes.Contains(Type)) {
                if (DH.GetHandle(Type).NeedsPrompt) {
                    SyncedPageForm.Show();
                    SyncedPageForm.BringToFront();
                    FoundFile = FullPath;
                    SI_Click(DH.GetHandle(Type));
                } else {
                    //If it doesn't need prompt
                    MoveFile(FullPath, DH.GetHandle(Type).DefaultPath);
                }
            } else {
                FolderBrowserDialog fbd = new FolderBrowserDialog {
                    Description = Type + " Sort. Organize your Folders, where would you like to store ." + Type + " files. (To ignore these files, or change your settings Open Surtur by clicking the icon in your Tray.)",
                    ShowNewFolderButton = true
                };
                if (!string.IsNullOrWhiteSpace(DH.RecentlySelectedPath))
                    fbd.SelectedPath = DH.RecentlySelectedPath;
                
                //TODO C# sounds
                if ((fbd.ShowDialog() == DialogResult.OK)) {
                    DH.RecentlySelectedPath = fbd.SelectedPath;
                    DH.SetHandler(Type, new StorageInfoBuilder()
                                            .SetDefaultPath(fbd.SelectedPath)
                                            .NeedsPrompting(false)
                                            .SetHandledName(Type)
                                            .SetParent(null)
                                            .Build());
                    SyncedPage.Notification("Moving " + Type, "To change, Click here, Files of type " + Type + " will now be moved to " + fbd.SelectedPath, ToolTipIcon.Info);
                    Push(FullPath);
                    HandleQueue();
                }
                if (DH.Queue.Count > 0)
                    HandleQueue();
                else
                    busy = false;
            }
            
        }
        void SI_Click(StorageInfo si) {
            StorageButtons = new List<string>();
            Destination = si.DefaultPath;
            HandledType = si.Handlee;
            CurrentlyWatched = si;
            if (si.NeedsPrompt)
                foreach (string sub in si.AllHandledTypes) {
                    StorageButtons.Add(sub);
                }
            SyncedPage.RefreshView();
          
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



        void Init_Watchers() {
            Dlist = new List<FileSystemWatcher>();
            foreach (string path in DH.AllWatchedPaths) {

                FileSystemWatcher fsw = new FileSystemWatcher(path) {
                    EnableRaisingEvents = true,
                    SynchronizingObject = SyncedPageForm
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
            Init_Watchers();

        }
        void FileSystemWatcher1_Created(object sender, FileSystemEventArgs e) {
         //    SyncedPageForm.Show();
         //   Push(e.FullPath);
            HandleFileChanges(e.Name, e.FullPath);
        }
        void FileSystemWatcher1_Renamed(object sender, RenamedEventArgs e) {
            if (TriggerOnRename)
                HandleFileChanges(e.Name, e.FullPath);
        }
        bool IsFolder(string Parth) {
            return (Path.GetExtension(Parth) == "");
        }
        void HandleFileChanges(string Name, string FullPath) {
            if(IsFolder(FullPath)) return;
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
                Push(FullPath);
            lock (this) {
                if (!busy) {
                    busy = true;
                    //If not busy?
                    HandleQueue();
                }
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
            SyncedPage.Notification("Ignored File", FoundFile + " added to ignore List", ToolTipIcon.Info);
            if (DH.Queue.Count > 0)
                HandleQueue();
            else
                busy = false;
        }
        public void IgnorePath(string FoundFile) {
            DH.AddIgnorePath(FoundFile);
            LogTransfers(FoundFile + " added to ignore List");
            SyncedPage.Notification("Ignored File", FoundFile + " added to ignore List", ToolTipIcon.Info);
            if (DH.Queue.Count > 0)
                HandleQueue();
            else
                busy = false;
        }
        public void MoveFile() {
            MoveFile(FoundFile, Destination);
        }
        public void MoveFile(string FileToMove) {
            MoveFile(FileToMove, Destination);
        }
        public void MoveFile(string FileToMove,string Destination) {
            try {
                int dup=0;
                if(File.Exists(Destination + "\\" + Path.GetFileName(FileToMove))) {
                    dup++;
                    while(File.Exists(Destination + "\\" + Path.GetFileName(FileToMove) + "(" + dup + ")")) {
                        dup++;
                    }
                }
                string DestinationName = Path.GetFileNameWithoutExtension(FileToMove) + ((dup > 0) ? "(" + dup + ")" : "") + Path.GetExtension(FileToMove);
                (new FileInfo(Destination + "\\" + DestinationName)).Directory.Create();
                File.Move(FileToMove, Destination + "\\" + DestinationName);
                SyncedPage.Notification("Auto-Moved " + Path.GetFileName(FileToMove), " Succesfully Moved " + Path.GetFileName(FileToMove) + " to " + DestinationName, ToolTipIcon.Info);
                LogTransfers(Path.GetFileName(FileToMove), FileToMove, DestinationName);
            } catch (Exception ex) {
                LogTransfers("An Error Occured: when Moving " + FileToMove + ":" + ex.Message);
                SyncedPage.Notification("Error Moving " + Path.GetFileName(FileToMove), ex.Message,ToolTipIcon.Error);
                if (File.Exists(FileToMove))
                    AddToList(FileToMove, 60000);
            }




            //If file exists

            if (DH.Queue.Count > 0)
                HandleQueue();
            else
                busy = false;
        }
    }
}



