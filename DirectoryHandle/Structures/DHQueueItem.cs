using System;

namespace Surtur_Core.Queue {
    [Serializable]
    public class QueueItem {
        //TODO v2 this file has been tried several times, go to folder?        
        /// <summary>
        /// Gets the Full Name of this Item.
        /// </summary>
        /// <value>
        /// The Full path and name o the item.
        /// </value>
        public string Name { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is removed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is removed; otherwise, <c>false</c>.
        /// </value>
        public bool IsRemoved { get; private set; }
        /// <summary>
        /// Gets the Number of times, the user has tried this item.
        /// </summary>
        /// <value>
        /// The number of tries.
        /// </value>
        public int TryCount { get; private set; }
        /// <summary>
        /// Tries to move this item.
        /// </summary>
        public void Try() {
            TryCount++;
        }
        /// <summary>
        /// Removes this intem from the queue.
        /// </summary>
        public void Remove() {
            IsRemoved = true;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueItem"/> class.
        /// </summary>
        /// <param name="name">The name of the File (full path).</param>
        public QueueItem(string name) {
            Name = name;
            TryCount = 0;
            IsRemoved = false;
        }
        public override bool Equals(object obj) {
            if (!(obj is QueueItem))
                return false;
            return (Name.Equals((obj as QueueItem).Name));

        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
