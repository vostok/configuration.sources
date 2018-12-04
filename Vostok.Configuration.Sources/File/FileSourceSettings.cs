using System;
using System.Text;

namespace Vostok.Configuration.Sources.File
{
    public class FileSourceSettings
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public TimeSpan FileWatcherPeriod { get; set; } = TimeSpan.FromSeconds(5);

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
                return ((Encoding != null ? Encoding.GetHashCode() : 0) * 397) ^ FileWatcherPeriod.GetHashCode();
            }
        }

        protected bool Equals(FileSourceSettings other) => Equals(Encoding, other.Encoding) && FileWatcherPeriod.Equals(other.FileWatcherPeriod);
    }
}