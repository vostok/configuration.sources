using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources.File;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    public class SettingsFileWatcher_Tests
    {
        [Test]
        public void Should_cache_watchers_by_fileName_and_settings()
        {
            var filename = @"C:\settings";
            SettingsFileWatcher.WatchFile(filename, new FileSourceSettings {Encoding = Encoding.ASCII})
                .Should()
                .BeSameAs(SettingsFileWatcher.WatchFile(filename, new FileSourceSettings {Encoding = Encoding.ASCII}));
        }
    }
}