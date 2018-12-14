using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
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
        public void Should_push_settings_from_getter_when_getter_succeeded()
        {
            var settings = Substitute.For<ISettingsNode>();
            new TestConstantSource(() => settings)
                .Observe()
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((settings, null));
        }

        [Test]
        public void Should_push_getter_exception_when_getter_failed()
        {
            var error = new FormatException();
            new TestConstantSource(() => throw error)
                .Observe()
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((null, error));
        }

        [Test]
        public void Should_not_call_getter_twice()
        {
            var getter = Substitute.For<Func<ISettingsNode>>();
            var source = new TestConstantSource(getter);

            source.Observe()
                .WaitFirstValue(100.Milliseconds())
                .settings
                .Should()
                .NotBeNull();
            
            source.Observe()
                .WaitFirstValue(100.Milliseconds())
                .settings
                .Should()
                .NotBeNull();

            getter.ReceivedCalls().Count().Should().Be(1);
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