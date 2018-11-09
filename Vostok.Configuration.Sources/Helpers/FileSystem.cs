using System.IO;
using System.Text;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class FileSystem : IFileSystem
    {
        public bool Exists(string filePath) => System.IO.File.Exists(filePath);

        public TextReader OpenFile(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding encoding)
        {
            var stream = new FileStream(filePath, fileMode, fileAccess, fileShare);
            return new StreamReader(stream, encoding);
        }

        public void WatchFileSystem(string path, string filter, FileSystemEventHandler handler)
        {
            var fileWatcher = new FileSystemWatcher(path, filter);
            fileWatcher.Changed += handler;
            fileWatcher.EnableRaisingEvents = true;
        }
    }
}