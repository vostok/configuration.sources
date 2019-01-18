using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Tests.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
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
        public void Should_merge_settings_nodes_correctly()
        {
            var merged01 = Substitute.For<ISettingsNode>();
            settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Returns(merged01);

            var merged012 = Substitute.For<ISettingsNode>();
            merged01.Merge(settingsNodes[2], Arg.Any<SettingsMergeOptions>()).Returns(merged012);

            var source = new CombinedSource(sources);

            source.Observe().WaitFirstValue(100.Milliseconds())
                .Should().Be((merged012, null));

            settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Received();
            merged01.Merge(settingsNodes[2], Arg.Any<SettingsMergeOptions>()).Received();
        }

        [Test]
        public void Should_merge_null_settings_correctly()
        {
            var merged12 = Substitute.For<ISettingsNode>();
            settingsNodes[1].Merge(settingsNodes[2], Arg.Any<SettingsMergeOptions>()).Returns(merged12);

            var source = new CombinedSource(sources);
            
            sources[0].PushNewConfiguration(null);

            source.Observe().WaitFirstValue(100.Milliseconds()).settings.Should().BeSameAs(merged12);
        }

        [Test]
        public void Should_push_single_exception_when_one_exception()
        {
            var source = new CombinedSource(sources);
            
            var error = new IOException();
            sources[0].PushNewConfiguration(null, error);
            sources[1].PushNewConfiguration(null);

            source.Observe().WaitFirstValue(100.Milliseconds()).error
                .Should().BeEquivalentTo(error);
        }

        [Test]
        public void Should_merge_exceptions_into_aggregate_exception_when_more_than_one_exception()
        {
            var source = new CombinedSource(sources);
            
            Exception[] errors = {new IOException(), new FormatException()};
            sources[0].PushNewConfiguration(null, errors[0]);
            sources[1].PushNewConfiguration(null, errors[1]);

            source.Observe().WaitFirstValue(100.Milliseconds()).error
                .Should().BeEquivalentTo(new AggregateException(errors));
        }

        [Test]
        public void Should_reflect_underlying_sources_updates()
        {
            var source = new CombinedSource(sources.Take(2).ToArray());

            var observer = new TestObserver<(ISettingsNode, Exception)>();
            using (source.Observe().Subscribe(observer))
            {
                Action assertion1 = () => observer.Values.Count.Should().Be(1);
                assertion1.ShouldPassIn(100.Milliseconds());
                
                settingsNodes[0] = Substitute.For<ISettingsNode>();

                var merged01 = Substitute.For<ISettingsNode>();
                settingsNodes[0].Merge(settingsNodes[1], Arg.Any<SettingsMergeOptions>()).Returns(merged01);
            
                sources[0].PushNewConfiguration(settingsNodes[0]);

                Action assertion2 = () =>
                {
                    var values = observer.Values;
                    values.Count.Should().Be(2);
                    values.Last().Should().Be((merged01, null));
                };
                assertion2.ShouldPassIn(100.Milliseconds());
            }
        }
    }
}