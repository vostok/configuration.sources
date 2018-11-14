using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Tests.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class CombinedRawSource_Tests
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
            new Action(() => new CombinedRawSource(null))
                .Should().Throw<ArgumentException>();

            new Action(() => new CombinedRawSource(new IConfigurationSource[0]))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Should_merge_settings_correctly()
        {
            var merged01 = Substitute.For<ISettingsNode>();
            settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Returns(merged01);

            var merged012 = Substitute.For<ISettingsNode>();
            merged01.Merge(settingsNodes[2], Arg.Any<SettingsMergeOptions>()).Returns(merged012);

            var source = new CombinedRawSource(sources);

            source.ObserveRaw().WaitFirstValue(100.Milliseconds())
                .Should().Be((merged012, null));

            settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Received();
            merged01.Merge(settingsNodes[2], Arg.Any<SettingsMergeOptions>()).Received();
        }

        [Test]
        public void Should_merge_exceptions_into_aggregate_exception()
        {
            var source = new CombinedRawSource(sources);
            
            Exception[] errors = {new IOException(), new FormatException()};
            sources[0].RawSource.PushNewConfiguration(null, errors[0]);
            sources[1].RawSource.PushNewConfiguration(null, errors[1]);

            source.ObserveRaw().WaitFirstValue(100.Milliseconds())
                .Should().BeEquivalentTo((null as ISettingsNode, new AggregateException(errors)));
        }

        [Test]
        public void Should_reflect_underlying_sources_updates()
        {
            var source = new CombinedRawSource(sources.Take(2).ToArray());

            var observer = new TestScheduler().CreateObserver<(ISettingsNode, Exception)>();
            using (source.ObserveRaw().Subscribe(observer))
            {
                Action assertion1 = () => observer.Messages.Count.Should().Be(1);
                assertion1.ShouldPassIn(100.Milliseconds());
                
                settingsNodes[0] = Substitute.For<ISettingsNode>();

                var merged01 = Substitute.For<ISettingsNode>();
                settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Returns(merged01);
            
                sources[0].RawSource.PushNewConfiguration(settingsNodes[0]);

                Action assertion2 = () =>
                {
                    var values = observer.GetValues().ToArray();
                    values.Length.Should().Be(2);
                    values.Last().Should().Be((merged01, null));
                };
                assertion2.ShouldPassIn(100.Milliseconds());
            }
        }
    }
}