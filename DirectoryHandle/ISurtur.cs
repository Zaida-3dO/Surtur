using System.Collections.Generic;
using System.Windows.Forms;

namespace Surtur_Core {
    public interface ISurtur {
        Form Form {
            get;
        }
        void ShowSelectPath();
        void HideSelectPath();
        void Notification(string Title, string Text, ToolTipIcon img);
        void RefreshView();
        string FilePath {
            set;
        }
        string FileType {
            set;
        }
        string FileDestination {
            set;
        }
        void UpdateQueue(List<string> Queue);
    }
}
