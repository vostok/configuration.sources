using System;
using System.IO;
using System.Text;

namespace Vostok.Configuration.Sources.File
{
    internal interface IFileSystem
    {
        bool Exists(string filePath);

        TextReader OpenFile(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding encoding);

        IDisposable WatchFileSystem(string path, string filter, FileSystemEventHandler handler);
    }
}