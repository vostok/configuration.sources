using System;
using System.IO;
using System.Text;

namespace Vostok.Configuration.Sources.File
{
    internal class FileSystem : IFileSystem
    {
        public bool Exists(string filePath) => System.IO.File.Exists(filePath);

        public TextReader OpenFile(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding encoding)
        {
            var stream = new FileStream(filePath, fileMode, fileAccess, fileShare);
            return new StreamReader(stream, encoding);
        }

        public IDisposable WatchFileSystem(string path, string filter, FileSystemEventHandler handler)
        {
            var fileWatcher = new FileSystemWatcher(path, filter)
            {
                InternalBufferSize = 8192,
            };

            fileWatcher.Created += handler;
            fileWatcher.Deleted += handler;
            fileWatcher.Changed += handler;
            fileWatcher.Renamed += (sender, args) => handler(sender, args);

            fileWatcher.EnableRaisingEvents = true;

            return fileWatcher;
        }
    }
}