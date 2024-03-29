﻿using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
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

            (res["DoTnET_RoOt"]?.Value).Should().NotBeNull();
        }
    }
}