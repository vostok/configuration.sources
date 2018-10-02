using NUnit.Framework;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.Tests.Commons
{
    public class Sources_Test
    {
        [TearDown]
        public void TearDown() => SettingsFileWatcher.ClearCache();
    }
}