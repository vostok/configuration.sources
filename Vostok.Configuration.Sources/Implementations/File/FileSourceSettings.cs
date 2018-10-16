using System;
using System.Text;

namespace Vostok.Configuration.Sources.Implementations.File
{
    public class FileSourceSettings
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public TimeSpan FileWatcherPeriod { get; set; } = TimeSpan.FromSeconds(5);
    }
}