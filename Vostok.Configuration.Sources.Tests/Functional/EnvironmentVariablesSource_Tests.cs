using System;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Environment;

namespace Vostok.Configuration.Sources.Tests.Functional
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                settings["pAtH"].Value.Should().NotBeNull();
                settings["APPdata"].Value.Should().NotBeNull();
            }
            else
            {
                settings["pAtH"].Value.Should().NotBeNull();
                settings["sheLL"].Value.Should().NotBeNull();
            }
        }
    }
}