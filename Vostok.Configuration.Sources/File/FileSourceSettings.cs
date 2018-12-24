using System;
using System.Text;

namespace Vostok.Configuration.Sources.File
{
    public class FileSourceSettings
    {
        public FileSourceSettings(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public TimeSpan FileWatcherPeriod { get; set; } = TimeSpan.FromSeconds(5);

        #region Equality

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((FileSourceSettings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FilePath != null ? FilePath.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Encoding != null ? Encoding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FileWatcherPeriod.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(FileSourceSettings other) => 
            string.Equals(FilePath, other.FilePath) && Equals(Encoding, other.Encoding) && FileWatcherPeriod.Equals(other.FileWatcherPeriod);

        #endregion
    }
}