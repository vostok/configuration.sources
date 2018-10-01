using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Sources.Tests.Helper;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    public class CombinedSource_Tests: Sources_Test
    {
        private SingleFileWatcherSubstitute[] watchers;

        [Test]
        public void Should_throw_exception_if_no_sources()
        {
            new Action(() => new CombinedSource(null))
                .Should().Throw<ArgumentException>();

            new Action(() => new CombinedSource(new IConfigurationSource[0]))
                .Should().Throw<ArgumentException>();
        }
    }
}