using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Environment;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class EnvironmentVariablesSource_Tests
    {
        [Test]
        public void Should_return_correct_values()
        {
            var evs = new EnvironmentVariablesSource();
            var res = evs.Observe().WaitFirstValue(100.Milliseconds()).settings;

            CheckResult(res);
        }

        private static void CheckResult(ISettingsNode settings)
        {
            var windows = new[] { PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE };
            if (windows.Contains(System.Environment.OSVersion.Platform))
            {
                settings["pAtH"].Value.Should().NotBeNull();
                settings["APPdata"].Value.Should().NotBeNull();
            }
            else if (System.Environment.OSVersion.Platform == PlatformID.Unix)
            {
                settings["pAtH"].Value.Should().NotBeNull();
                settings["sheLL"].Value.Should().NotBeNull();
            }
            else
                throw new NotImplementedException();
        }
    }
}