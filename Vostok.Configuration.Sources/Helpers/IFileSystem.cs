using System.IO;
using System.Text;

namespace Vostok.Configuration.Sources.Helpers
{
    internal interface IFileSystem
    {
        bool Exists(string filePath);

        TextReader OpenFile(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding encoding);

        void WatchFileSystem(string path, string filter, FileSystemEventHandler handler);
    }
}