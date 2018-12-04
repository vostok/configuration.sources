using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.Helpers
{
    internal interface IFileSystem
    {
        bool Exists(string filePath);

        [CanBeNull] // CR(krait): Why CanBeNull?
        TextReader OpenFile(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding encoding);

        void WatchFileSystem(string path, string filter, FileSystemEventHandler handler);
    }
}