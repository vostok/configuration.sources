using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Tests
{
    internal class TestConfigurationSource : ConfigurationSourceAdapter
    {
        internal TestRawConfigurationSource RawSource => (TestRawConfigurationSource)rawSource;

        public TestConfigurationSource()
            : base(new TestRawConfigurationSource())
        {
        }

        public TestConfigurationSource(ISettingsNode settings, Exception error = null)
            : base(new TestRawConfigurationSource(settings, error))
        {
        }
    }
}