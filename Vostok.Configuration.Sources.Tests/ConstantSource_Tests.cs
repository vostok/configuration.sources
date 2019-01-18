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
    internal class ConstantSource_Tests
    {
        [Test]
        public void Should_push_preconfigured_settings()
        {
            var settings = Substitute.For<ISettingsNode>();
            new ConstantSource(settings)
                .Observe()
                .WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((settings, null));
        }
    }
}