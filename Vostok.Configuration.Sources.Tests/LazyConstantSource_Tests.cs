using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Constant;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class LazyConstantSource_Tests
    {
        [Test]
        public void Should_push_settings_from_getter_when_getter_succeeded()
        {
            var settings = Substitute.For<ISettingsNode>();
            new LazyConstantSource(() => settings)
                .Observe()
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((settings, null));
        }

        [Test]
        public void Should_push_getter_exception_when_getter_failed()
        {
            var error = new FormatException();
            new LazyConstantSource(() => throw error)
                .Observe()
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((null, error));
        }

        [Test]
        public void Should_not_call_getter_twice()
        {
            var getter = Substitute.For<Func<ISettingsNode>>();
            var source = new LazyConstantSource(getter);

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

        [Test]
        public void Should_not_allow_null_settings_provider()
        {
            new Action(() => new LazyConstantSource(null))
                .Should().Throw<ArgumentNullException>();
        }
    }
}