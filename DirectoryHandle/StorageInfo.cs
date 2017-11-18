using System;
using System.Collections.Generic;
using System.Linq;

namespace Surtur_Core {
    [Serializable]
    public class StorageInfo {
        /// <summary>
        /// Gets the Storage info with this as it's child.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public StorageInfo Parent { get; }
        /// <summary>
        /// Gets or sets the Type beign handled
        /// </summary>
        /// <value>
        /// The handled Type.
        /// </value>
        public string Handlee { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this storage info Can Sort itself out
        /// </summary>
        /// <value>
        ///   <c>true</c> if [needs prompt]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsPrompt { get; set; }
        /// <summary>
        /// Gets or sets the default path.
        /// </summary>
        /// <value>
        /// The default path.
        /// </value>
        public string DefaultPath { get; set; }
        Dictionary<string, StorageInfo> children;
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageInfo" /> class.
        /// </summary>
        /// <param name="Prompt">Specifies if this SI needs prompting, or always stores in a specified Place.</param>
        /// <param name="defaultPath">The default path to send files to.</param>
        /// <param name="parent">The parent of this .</param>
        public StorageInfo(StorageInfoBuilder SIB) {
            NeedsPrompt = SIB.needsPrompt;
            DefaultPath = SIB.defaultPath;
            children = new Dictionary<string, StorageInfo>();
            Parent = SIB.Parent;
            Handlee = SIB.Handlee;
        }
        /// <summary>
        /// Sets the handler.
        /// </summary>
        /// <param name="Type">The Type.</param>
        /// <param name="SI">The storage info.</param>
        public void SetHandler(string Type, StorageInfo SI) {
            if (children.ContainsKey(Type))
                children[Type].DefaultPath = SI.DefaultPath;
            else
               children.Add(Type, SI);
        }
        /// <summary>
        /// Removes the handler.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <exception cref="NotSupportedException"></exception>
        public void RemoveHandler(string Type) {
            if (!children.ContainsKey(Type))
                throw new NotSupportedException(Type + " is not handled");
            children.Remove(Type);
        }
        /// <summary>
        /// returns All the handled types.
        /// </summary>
        /// <returns>All the file types handles by the SI</returns>
        public List<string> AllHandledTypes => children.Keys.ToList();
        /// <summary>
        /// Gets the handler of the type.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <returns>The Storage Handler of the Type</returns>
        /// <exception cref="NotSupportedException"></exception>
        public StorageInfo GetHandle(string Type) {
            if (!children.ContainsKey(Type))
                throw new NotSupportedException(Type + " is not handled");
            return children[Type];
        }
    }
    public class StorageInfoBuilder {
        public StorageInfo Parent;
        public string Handlee;
        public bool needsPrompt;
        public string defaultPath;
        bool set1 = false, set2 = false, set3 = false, set4 = false;
        /// <summary>
        /// Sets the default path.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <returns></returns>
        public StorageInfoBuilder SetDefaultPath(String Path) {
            defaultPath = Path;
            set1 = true;
            return this;
        }
        /// <summary>
        /// Specifies if the SI needs prompting while saving files, or saves to Default Path.
        /// </summary>
        /// <param name="needsPrompt">bool to specify if the SI needs prompting</param>
        /// <returns></returns>
        public StorageInfoBuilder NeedsPrompting(bool needsPrompt) {
            this.needsPrompt = needsPrompt;
            set2 = true;
            return this;
        }
        /// <summary>
        /// Sets the parent SI where this is Found.
        /// </summary>
        /// <param name="Parent">The parent SI.</param>
        /// <returns></returns>
        public StorageInfoBuilder SetParent(StorageInfo Parent) {
            this.Parent = Parent;
            set3 = true;
            return this;
        }
        /// <summary>
        /// Sets the name of the handled Type.
        /// </summary>
        /// <param name="Handlee">The handlee.</param>
        /// <returns></returns>
        public StorageInfoBuilder SetHandledName(String Handlee) {
            this.Handlee = Handlee;
            set4 = true;
            return this;
        }
        /// <summary>
        /// Builds an instance of <see cref="StorageInfo"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MissingFieldException">Not all fields of the Storage Info have been set</exception>
        public StorageInfo Build() {
            if (!(set1 && set2 && set3 && set4))
                throw new MissingFieldException("Not all fields of the Storage Info have been set");
            return new StorageInfo(this);
        }
    }
}
