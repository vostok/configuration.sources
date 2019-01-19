using System;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.File
{
    /// <summary>
    /// Settings for <see cref="BaseFileSource"/>.
    /// </summary>
    [PublicAPI]
    public class FileSourceSettings
    {
        public FileSourceSettings([NotNull] string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// Path to the file to read settings from. This setting is required.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Encoding to use when reading the settings file.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// <para><see cref="BaseFileSource"/> updates settings from file each <see cref="FileWatcherPeriod"/>.</para>
        /// <para>Settings are also updated upon receiving a file changed event.</para>
        /// </summary>
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
                var hashCode = FilePath != null ? PathComparer.Instance.GetHashCode(FilePath) : 0;
                hashCode = (hashCode * 397) ^ (Encoding != null ? Encoding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FileWatcherPeriod.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(FileSourceSettings other) => 
            PathComparer.Instance.Equals(FilePath, other.FilePath) && 
            Equals(Encoding, other.Encoding) && 
            FileWatcherPeriod.Equals(other.FileWatcherPeriod);

        #endregion
    }
}