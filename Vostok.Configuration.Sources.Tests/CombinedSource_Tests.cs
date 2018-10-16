using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources.Implementations.Combined;

namespace Vostok.Configuration.Sources.Tests
{
    internal class CombinedSource_Tests
    {
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