using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Constant;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class BaseConstantSource_Tests
    {
        [Test]
        public void Should_return_settings_from_getter()
        {
            var node = Substitute.For<ISettingsNode>();
            new TestConstantSource(() => node)
                .Get().Should().Be(node);
        }

        [Test]
        public void Should_rethrow_getter_exception()
        {
            Action get = () => new TestConstantSource(() => throw new FormatException()).Get();
            get.Should().Throw<FormatException>();
        }
        
        private class TestConstantSource : BaseConstantSource
        {
            public TestConstantSource(Func<ISettingsNode> settingsGetter)
                : base(settingsGetter)
            {
            }
        }
    }
}