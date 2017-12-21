using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Surtur_Core {
    [Serializable]
    public class DirectoryHandler {
        Dictionary<string, StorageInfo> _dirHandler;
        HashSet<string> _watchPath;
        HashSet<string> _ignoredPath;
        HashSet<string> _ignoredType;
        public string RecentlySelectedPath { get { return _recentPath; }set { if (Directory.Exists(value))_recentPath = value;  } }
        string _recentPath;
        ConcurrentQueue<string> _Queue;
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryHandler"/> class.
        /// </summary>
        public DirectoryHandler(){
            _dirHandler = new Dictionary<string, StorageInfo>();
            RecentlySelectedPath = "";
            _Queue = new ConcurrentQueue<string>();
            _watchPath = new HashSet<string>();
            _ignoredPath = new HashSet<string>();
            _ignoredType = new HashSet<string>();
        }
        /// <summary>
        /// Pushes the specified path into the Queue.
        /// </summary>
        /// <param name="enque">The item to enque.</param>
        public void Push(string enque) {
            if(!_Queue.Contains(enque))
                _Queue.Enqueue(enque);
        }
        /// <summary>
        /// Pops the path at the top of the Queue.
        /// </summary>
        /// <returns>The Path first in the Queue</returns>
        public string Pop() {
            _Queue.TryDequeue( out string ans);
            return ans;
        }
        /// <summary>
        /// A Queue of all items waiting to be handled
        /// </summary>
        /// <returns>A list of all paths in the queue</returns>
        public List<string> Queue {
           get { return _Queue.ToList(); }
        }
        /// <summary>
        /// Handles the specified type.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="SI">The storage detai;ls.</param>
        public void SetHandler(string Type,StorageInfo SI) {
            if (Type.StartsWith("."))
                Type = Type.Substring(1);
            if (_dirHandler.ContainsKey(Type)) 
                _dirHandler[Type].DefaultPath = SI.DefaultPath;
            else
                _dirHandler.Add(Type, SI);
        }
        /// <summary>
        /// Removes the handler.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <exception cref="NotSupportedException"></exception>
        public void RemoveHandler(string Type) {
            if (!_dirHandler.ContainsKey(Type))
                throw new NotSupportedException(Type + " is not handled");
            _dirHandler.Remove(Type);
        }
        /// <summary>
        /// Alls the handled types.
        /// </summary>
        /// <returns>All the file types handles by the DH</returns>
        public List<string> AllHandledTypes => _dirHandler.Keys.ToList();
        /// <summary>
        /// Gets the handler of the type.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <returns>The Storage Handler of the Type</returns>
        /// <exception cref="NotSupportedException"></exception>
        public StorageInfo GetHandle(string Type) {
            if(!_dirHandler.ContainsKey(Type))
                throw new NotSupportedException(Type + " is not handled");
            return _dirHandler[Type];
        }
        /// <summary>
        /// Starts watching the path.
        /// </summary>
        /// <param name="Path">The path.</param>
        public void AddPath(string Path) => _watchPath.Add(Path);
        /// <summary>
        /// Stops watching the path.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <exception cref="KeyNotFoundException">Cannot Find the specified Path</exception>
        public void RemovePath(string Path) {
            if (!_watchPath.Contains(Path))
                throw new KeyNotFoundException("Cannot Find the specified Path");
            _watchPath.Remove(Path);
        }
        /// <summary>
        /// Gets all watched paths.
        /// </summary>
        /// <value>
        /// All watched paths.
        /// </value>
        public List<string> AllWatchedPaths => _watchPath.ToList();
        /// <summary>
        /// Starts ignoring the path.
        /// </summary>
        /// <param name="Path">The path.</param>
        public void AddIgnorePath(string Path) => _ignoredPath.Add(Path);
        /// <summary>
        /// Stops ignoring the path.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <exception cref="KeyNotFoundException">Cannot Find the specified Path</exception>
        public void RemoveIgnorePath(string Path) {
            if (!_ignoredPath.Contains(Path))
                throw new KeyNotFoundException("Cannot Find the specified Path");
            _ignoredPath.Remove(Path);
        }
        /// <summary>
        /// Gets all ignored paths.
        /// </summary>
        /// <value>
        /// All ignored paths.
        /// </value>
        public List<string> AllIgnoredPaths => _ignoredPath.ToList();
        /// <summary>
        /// Starts ignoring the Type.
        /// </summary>
        /// <param name="Type">The Type.</param>
        public void AddIgnoreType(string Type) => _ignoredType.Add(Type);
        /// <summary>
        /// Stops ignoring the Type.
        /// </summary>
        /// <param name="Type">The Type.</param>
        /// <exception cref="KeyNotFoundException">Cannot Find the specified Type</exception>
        public void RemoveIgnoreType(string Type) {
            if (!_ignoredType.Contains(Type))
                throw new KeyNotFoundException("Cannot Find the specified Type");
            _ignoredType.Remove(Type);
        }
        /// <summary>
        /// Gets all ignored Types.
        /// </summary>
        /// <value>
        /// All ignored Types.
        /// </value>
        public List<string> AllIgnoredTypes => _ignoredType.ToList();
        /// <summary>
        /// Serializes and saves this instance of DH to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="FieldAccessException">Could not acquire Lock on "+path+" this is Likely because another app is using it</exception>
        /// <exception cref="Exception">Could not acquire Lock on "+path+"\n this is Likely because another app is using it
        /// or
        /// An error Occured, Could not save DH</exception>
        public void Save(string path) {
            (new FileInfo(path)).Directory.Create();
            try {
                using (FileStream fsout = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    new BinaryFormatter().Serialize(fsout, this);
                    fsout.Close();
                }
            } catch (Exception ex){
                throw new Exception("An error Occured, Could not save DH :"+path,ex);
            }
        }
        /// <summary>
        /// Loads the Serialized file stored at the specified path.
        /// </summary>
        /// <param name="pathToSerialized">The path to the serialized File.</param>
        /// <returns>An object of DH deserialized from the file</returns>
        /// <exception cref="FileNotFoundException">The file at the path doesn't contain a valid Serialized DH</exception>
        public static DirectoryHandler Load(string pathToSerialized) {
            DirectoryHandler l;
            try {
                using (FileStream fsin = new FileStream(pathToSerialized, FileMode.Open, FileAccess.Read, FileShare.None)) {
                    l = (DirectoryHandler)(new BinaryFormatter().Deserialize(fsin));
                    fsin.Close();
                }
            } catch (FileNotFoundException) {
                throw new FileNotFoundException("The file at the path doesn't contain a valid Serialized DH");
            }
            return l;
        }
    }
}
