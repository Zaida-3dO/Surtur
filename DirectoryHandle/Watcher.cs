using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Media;
namespace Surtur_Core { 
    public class Watcher {
        public DirectoryHandler DH;
        string _foundFile;
        public string FoundFile { get { return _foundFile; } set {
                _foundFile = value;
                SyncedPage.FilePath= value;
            }
        }
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
        public void NotBusy() {
            busy = false;
        }
        bool busy;
        //TODO v2 comments evrywhere, in app and in code
        public bool TriggerOnRename;
        public ISurtur SyncedPage;
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
            _foundFile = "";
            SyncedPage = SP;
            TriggerOnRename = false;
            SyncedPageForm = SP.Form;
            StorageButtons = new List<string>();
            this.SavePath = SavePath;
            SaveDir = Path.GetDirectoryName(SavePath);
            Reload();
            Save();
            busy = false;
          
            FileSystemWatcher fileSystemWatcher1 = new FileSystemWatcher(SaveDir) {
                EnableRaisingEvents = true,
                SynchronizingObject = SyncedPageForm
            };
            fileSystemWatcher1.Changed += new FileSystemEventHandler(FileSystemWatcherHandleSave);
            fileSystemWatcher1.Created += new FileSystemEventHandler(FileSystemWatcherHandleSave);
            fileSystemWatcher1.Renamed += new RenamedEventHandler(FileSystemWatcherHandleSave);
           
        }
        /// <summary>
        /// Starts running if anything is in the queue.
        /// </summary>
        public void StartQueue() {
            if (DH.Queue.Count > 0) {
                busy = true;
                HandleQueue();
            }
        }
        public void RemoveFromList(string toBeRemoved) {

        }
        //TODO v2 implement all methods
     //TODO v2 pass in list
        public void NavigateTo(StorageInfo si) {
            SI_Click(si);
        }
        public void RemoveFromList(List<string> toBeRemoved) {

        }
      

        /// <summary>
        /// Files system watcher to handle save.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        public void FileSystemWatcherHandleSave(object sender, FileSystemEventArgs e) {
            if (e.FullPath.Equals(SavePath)) {
                Reload();
         //       if (CurrentlyWatched != null) {
           //         CurrentlyWatched = DH.GetHandle(CurrentlyWatched.Handlee);
             //       SI_Click(CurrentlyWatched);
               // }
               // SyncedPage.RefreshView();
              //  SyncedPage.UpdateQueue(DH.Queue);
            }
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
                File.Delete(SavePath);
                File.Move(SavePath + ".temp",SavePath);
            } catch  {
                LogTransfers("Error Saving to " + SavePath);
            }
        }
       string Pop() {
            string ret =  DH.Pop().Name;
            SyncedPage.UpdateQueue(DH.Queue);
            Save();
            return ret;
        }
        void Push(string toBeAdded, long delayInMs) {
          //TODO v2 requeue
          //
        }
        public string CleanUpType(string type) {
            if (type.StartsWith("."))
                    type = type.Substring(1);
            type = type.ToLower();
            return type;
        }
        void Push(string newPath) {
            DH.Push(newPath);

            SyncedPage.UpdateQueue(DH.Queue);
            Save();
        }
        void Push(List<string> newPaths) {
            foreach(string newPath in newPaths)
              DH.Push(newPath);

            SyncedPage.UpdateQueue(DH.Queue);
            Save();
        }
        string Peek() {
            string ret = DH.Peek().Name;
            SyncedPage.UpdateQueue(DH.Queue);
            return ret;
        }
        void HandleQueue() {
            string FullPath =Peek();
            string Type = CleanUpType(Path.GetExtension(FullPath));
            if (DH.AllIgnoredTypes.Contains(Type)) {
                DH.RemoveFromQueue(FullPath);

                if (DH.Queue.Count > 0)
                    HandleQueue();
                else
                    busy = false;
            }
            Format = Type;
            FoundFile = FullPath;
            SyncedPage.UpdateQueue(DH.Queue);
            if (DH.AllHandledTypes.Contains(Type)) {
                if (DH.GetHandle(Type).NeedsPrompt) {
                    SyncedPageForm.Show();
                    SyncedPageForm.BringToFront();
                    SystemSounds.Hand.Play();
                    FoundFile = FullPath;
                    SI_Click(DH.GetHandle(Type));
                } else {
                    //If it doesn't need prompt
                    MoveFile(FullPath, DH.GetHandle(Type).DefaultPath);
                }
            } else {
                Tuple<bool, string, Dictionary<string, string>> newSI = SyncedPage.ShowSelectPath(Type);
                if ((newSI.Item1)) {
                    SyncedPage.HideSelectPath();
                    DH.RecentlySelectedPath = newSI.Item2;
                    StorageInfo si = new StorageInfoBuilder()
                                            .SetDefaultPath(newSI.Item2)
                                            .NeedsPrompting(false)
                                            .SetHandledName(Type)
                                            .SetParent(null)
                                            .Build();
                    DH.SetHandler(Type, si);
                    if (newSI.Item3.Count > 0) {
                        Dictionary<string, string> children = newSI.Item3;
                        si.NeedsPrompt = true;
                        foreach (string subtype in children.Keys) {
                            si.SetHandler(subtype, new StorageInfoBuilder()
                                                .SetDefaultPath(children[subtype])
                                                .NeedsPrompting(false)
                                                .SetHandledName(subtype)
                                                .SetParent(si)
                                                .Build());
                        }
                    }
                    SyncedPage.Notification("Moving " + Type, "To change, Click here, Files of type " + Type + " will now be moved to " + newSI.Item2, ToolTipIcon.Info);
                  
                    //HandleQueue();
                } else {
                    SyncedPage.HideSelectPath();
                    if (MessageBox.Show("Do you want to ignore future "+Type+" Files", "Ignore "+Type+"?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                        DH.AddIgnoreType(Type);
                    } else {
                        //TODO v2 add to unsorted (menu stuffs)
                    }
                }
                Save();
                DH.RemoveFromQueue(FullPath);
                
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
        bool ReviewCurrentlyWatched() {
            //Don't ask... seriously
            if (CurrentlyWatched.Parent == null) {
                if (DH.AllHandledTypes.Contains(CurrentlyWatched.Handlee)) {
                    CurrentlyWatched = DH.GetHandle(CurrentlyWatched.Handlee);
                    return true;
                } else {
                    return false;
                }
            } else {
                List<string> handles = new List<string> { CurrentlyWatched.Handlee };
                StorageInfo parent = CurrentlyWatched.Parent;
                while (parent.Parent != null) {
                    handles.Add(parent.Handlee);
                    parent = parent.Parent;
                }
                if (DH.AllHandledTypes.Contains(parent.Handlee)) {
                        StorageInfo child = DH.GetHandle(parent.Handlee);
                    handles.Reverse();
                    foreach (string handle in handles) {
                        if (child.AllHandledTypes.Contains(handle)) {
                            child = child.GetHandle(handle);
                        } else {
                            return false;
                        }
                    }
                    CurrentlyWatched = child;
                    } else {
                        return false;
                    }
                
            }
            return true;
        }
        public void AddSubType(string Type, string Path) {
            if (!ReviewCurrentlyWatched()) { SyncedPage.RefreshView(); return; } 
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
            HandleFileChanges(e.FullPath);
        }
        void FileSystemWatcher1_Renamed(object sender, RenamedEventArgs e) {
            if (TriggerOnRename)
                HandleFileChanges( e.FullPath);
        }
        bool IsFolder(string Parth) {
            return (Path.GetExtension(Parth) == "");
        }
        void HandleFileChanges(string FullPath) {
            string Name = Path.GetFileName(FullPath);
            if(IsFolder(FullPath)) return;
            foreach (string ignore in DH.AllIgnoredPaths)
                if (FullPath.Equals(ignore))
                    return;
            string Type = CleanUpType(Path.GetExtension(Name));
            if (DH.AllIgnoredTypes.Contains(Type)) {
                LogTransfers("Ignored " + Name + " due to file type");
                return;
            }
            if (File.Exists(FullPath)) {
                Push(FullPath);
                lock (this) {
                    if (!busy) {
                        busy = true;
                        //If not busy?
                        HandleQueue();
                    }
                }
            }
            
        }
        public void HandleFileChanges(List<string> FullPaths) {
            List<string> all = new List<string>();
            foreach(string FullPath in FullPaths) {
                string Name = Path.GetFileName(FullPath);
                if (IsFolder(FullPath)) continue;
                bool cont = false;
                foreach (string ignore in DH.AllIgnoredPaths)
                    if (FullPath.Equals(ignore))
                        cont = true;
                if (cont) continue;
                string Type =CleanUpType(Path.GetExtension(Name));
                if (DH.AllIgnoredTypes.Contains(Type)) {
                    LogTransfers("Ignored " + Name + " due to file type");
                    continue;
                }
                all.Add(FullPath);
            }
            Push(all);
            if (DH.Queue.Count > 0) {
                lock (this) {
                    if (!busy) {
                        busy = true;
                        //If not busy?
                        HandleQueue();
                    }
                }
            }
        }
        void LogTransfers(string file, string from, string to) {
            try {
                File.AppendAllText(SaveDir + "\\Transfers.log", "[" + DateTime.Now.ToString() + "]" + " Succesfully moved " + file + " from " + from + " to " + to + "\r\n");
            } catch {
                File.AppendAllText(SaveDir + "\\Transfers.log2", "[" + DateTime.Now.ToString() + "]" + " Succesfully moved " + file + " from " + from + " to " + to + "\r\n");
            }
        }
        public void LogTransfers(string logText) {
            try {
                File.AppendAllText(SaveDir + "\\Transfers.log", "[" + DateTime.Now.ToString() + "]" + " " + logText + "\r\n");
            } catch {
                File.AppendAllText(SaveDir + "\\Transfers2.log", "[" + DateTime.Now.ToString() + "]" + " " + logText + "\r\n");
            }
        }

        public void IgnorePath() {
            DH.AddIgnorePath(FoundFile);
            LogTransfers(FoundFile + " added to ignore List");
            SyncedPage.Notification("Ignored File", FoundFile + " added to ignore List", ToolTipIcon.Info);
            Save();
            if (DH.Queue.Count > 0)
                HandleQueue();
            else
                busy = false;
        }
        public void IgnorePath(string FoundFile) {
            IgnorePath(FoundFile, true);
        }
        public void IgnorePath(string FoundFile,bool Continue) {
            DH.AddIgnorePath(FoundFile);
            DH.RemoveFromQueue(FoundFile);
            LogTransfers(FoundFile + " added to ignore List");
            SyncedPage.Notification("Ignored File", FoundFile + " added to ignore List", ToolTipIcon.Info);
            Save();
            if (Continue)
                if (DH.Queue.Count > 0)
                    HandleQueue();
                else
                    busy = false;
        }
        public void MoveFile() {
            MoveFile(FoundFile, Destination);
        }
        public void MoveFile(string FileToMove){
            MoveFile(FileToMove, Destination);
        }
        public void MoveFile(string FileToMove,bool Continue) {
            MoveFile(FileToMove, Destination,Continue);
        }
        public void MoveFile(string FileToMove,string Destination) {
            MoveFile(FileToMove, Destination, true);
        }
        public void MoveFile(string FileToMove, string Destination,bool Continue) {
            if (Path.GetDirectoryName(FileToMove).Equals(Destination)) {
                DH.RemoveFromQueue(FileToMove);
                SyncedPage.TransferNotification(Path.GetFileName(FileToMove), " Destination is same as source", true);
                LogTransfers("Didn't Move " + Path.GetFileName(FileToMove) + " Destination is same as source");
                Save();
            } else {
                try {
                    int dup = 0;
                    DH.RemoveFromQueue(FileToMove);
                   
                    if (File.Exists(Destination + "\\" + Path.GetFileName(FileToMove))) {
                        long moveFilelength = new FileInfo(FileToMove).Length;
                        long existFilelength = new FileInfo(Destination + "\\" + Path.GetFileName(FileToMove)).Length;
                        if (existFilelength == moveFilelength) {
                            File.Delete(FoundFile);
                            SyncedPage.TransferNotification(Path.GetFileName(FileToMove), " File already exist in desitination", true);
                            LogTransfers("Files merged " + Path.GetFileName(FileToMove) + " File already exist in desitination");
                            Save();
                            if (Continue)
                                if (DH.Queue.Count > 0)
                                    HandleQueue();
                                else
                                    busy = false;
                            return;
                        } else {
                            dup++;
                            while (File.Exists(Destination + "\\" + Path.GetFileNameWithoutExtension(FileToMove) + ((dup > 0) ? "(" + dup + ")" : "") + Path.GetExtension(FileToMove))) {
                                existFilelength = new FileInfo(Destination + "\\" + Path.GetFileNameWithoutExtension(FileToMove) + ((dup > 0) ? "(" + dup + ")" : "") + Path.GetExtension(FileToMove)).Length;
                                if (existFilelength == moveFilelength) {
                                    File.Delete(FoundFile);
                                    SyncedPage.TransferNotification(Path.GetFileName(FileToMove), " File already exist in desitination", true);
                                    LogTransfers("Files merged " + Path.GetFileName(FileToMove) + " File already exist in desitination");
                                    Save();
                                    if (Continue)
                                        if (DH.Queue.Count > 0)
                                            HandleQueue();
                                        else
                                            busy = false;
                                    return;
                                } else {
                                    dup++;
                                }
                            }
                        }
                    }
                    string DestinationName = Path.GetFileNameWithoutExtension(FileToMove) + ((dup > 0) ? "(" + dup + ")" : "") + Path.GetExtension(FileToMove);
                    (new FileInfo(Destination + "\\" + DestinationName)).Directory.Create();
                    File.Move(FileToMove, Destination + "\\" + DestinationName);
                    SyncedPage.TransferNotification(Path.GetFileName(FileToMove), Destination + "\\" + DestinationName, true);
                        LogTransfers(Path.GetFileName(FileToMove), FileToMove, Destination+"\\"+DestinationName);
                } catch (Exception ex) {
                    LogTransfers("An Error Occured: when Moving " + FileToMove + ":" + ex.Message);
                    SyncedPage.TransferNotification(Path.GetFileName(FileToMove), Destination , false);
                    if (File.Exists(FileToMove))
                        Push(FileToMove, 60000);
                }
                Save();

            }
            if (Continue)
            if (DH.Queue.Count > 0)
                HandleQueue();
            else
                busy = false;
        }
    }
}



