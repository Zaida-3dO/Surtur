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
        DirectoryHandler DH;
        bool busy;
        bool TriggerOnRename;
        Form SyncedPage;
        List<FileSystemWatcher> Dlist;
        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher" /> class.
        /// </summary>
        /// <param name="DH">The directory handler we are working with.</param>
        /// <param name="SP">The page to synchronize with(Just pass in 'this' from the expected form).</param>
        public Watcher(DirectoryHandler DH, Form SP) {
            SyncedPage = SP;
            RebaseDH(DH);
            busy = false;
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
        void FileSystemWatcher1_Created(object sender, FileSystemEventArgs e) {
            HandleFileChanges(e.Name,e.FullPath);
        }
        void FileSystemWatcher1_Renamed(object sender,RenamedEventArgs e) {
            if(TriggerOnRename)
                HandleFileChanges(e.Name,e.FullPath);
        }
        void HandleFileChanges(string Name, string FullPath) {
            //if(folder) break;
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
            File.AppendAllText(@"C:\ProgramData\surtur\Transfers.log", "[" + DateTime.Now.ToString() + "]" + " Succesfully moved " + file + " from " + from + " to " + to + "\r\n");
        }
        void LogTransfers(string logText) {
            File.AppendAllText(@"C:\ProgramData\surtur\Transfers.log", "[" + DateTime.Now.ToString() + "]" + " " + logText + "\r\n");
        }
    }
}



