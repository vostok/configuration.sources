using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Combined;

namespace Vostok.Configuration.Sources.Tests
{
    internal class CombinedSource_Tests
    {
        private ISettingsNode[] settingsNodes;
        private TestConfigurationSource[] sources;

        [SetUp]
        public void SetUp()
        {
            settingsNodes = Enumerable.Range(0, 3).Select(_ => Substitute.For<ISettingsNode>()).ToArray();
            sources = settingsNodes.Select(settings => new TestConfigurationSource(settings)).ToArray();
        }
        
        [Test]
        public void Should_throw_exception_when_no_sources()
        {
            new Action(() => new CombinedSource(null))
                .Should().Throw<ArgumentException>();

            new Action(() => new CombinedSource(new IConfigurationSource[0]))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Should_merge_settings_correctly()
        {
            var merged01 = Substitute.For<ISettingsNode>();
            settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Returns(merged01);

            var merged012 = Substitute.For<ISettingsNode>();
            merged01.Merge(settingsNodes[2], Arg.Any<SettingsMergeOptions>()).Returns(merged012);

            var source = new CombinedSource(sources);

            source.Get().Should().Be(merged012);

            settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Received();
            merged01.Merge(settingsNodes[2], Arg.Any<SettingsMergeOptions>()).Received();
        }

        [Test]
        public void Should_merge_exceptions_into_aggregate_exception()
        {
            var source = new CombinedSource(sources);
            
            Exception[] errors = {new IOException(), new FormatException()};
            sources[0].RawSource.PushNewConfiguration(null, errors[0]);
            sources[1].RawSource.PushNewConfiguration(null, errors[1]);

            source.Observe().ToEnumerable().First()
                .error.Should().BeEquivalentTo(new AggregateException(errors));
        }

        [Test]
        public void Should_reflect_underlying_sources_updates()
        {
            var source = new CombinedSource(sources.Take(2).ToArray());
            source.Get();

            settingsNodes[0] = Substitute.For<ISettingsNode>();

            var merged01 = Substitute.For<ISettingsNode>();
            settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Returns(merged01);
            
            sources[0].RawSource.PushNewConfiguration(settingsNodes[0]);

            source.Get().Should().Be(merged01);
        }
    }
}