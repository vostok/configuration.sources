using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Scoped;
using Vostok.Configuration.Sources.Tests.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class ScopedRawSource_Tests
    {
        private TestConfigurationSource testSource;

        [SetUp]
        public void SetUp()
        {
            testSource = new TestConfigurationSource();
        }
        
        [Test]
        public void Should_push_full_tree_when_no_scope()
        {
            var tree = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                ["value"] = new ValueNode("1"),
            });
            testSource.RawSource.PushNewConfiguration(tree);
            
            var source = new ScopedRawSource(testSource);
            var result = source.ObserveRaw().WaitFirstValue(100.Milliseconds());
            result.Should().Be((tree, null));
        }

        [Test]
        public void Should_scope_settings_when_no_errors()
        {
            var scopedSettings = new ValueNode("value");
            
            var settings = Substitute.For<ISettingsNode>();
            settings.ScopeTo("key").Returns(scopedSettings);
            
            var source = new ScopedRawSource(testSource, "key");
            
            testSource.RawSource.PushNewConfiguration(settings);

            source.ObserveRaw().WaitFirstValue(100.Milliseconds()).Should().Be((scopedSettings, null));
            settings.Received().ScopeTo("key");
        }

        [Test]
        public void Should_push_null_and_error_when_underlying_source_has_error()
        {
            var error = new IOException();
            var settings = Substitute.For<ISettingsNode>();
            
            testSource.RawSource.PushNewConfiguration(settings);
            testSource.RawSource.PushNewConfiguration(null, error);
            testSource.Observe().WaitFirstValue(100.Milliseconds()).Should().Be((settings, error));

            var source = new ScopedRawSource(testSource, "key");

            source.ObserveRaw().WaitFirstValue(100.Milliseconds()).Should().Be((null, error));
            settings.DidNotReceiveWithAnyArgs().ScopeTo();
        }

        [Test]
        public void Should_reflect_underlying_source_updates()
        {
            var source = new ScopedRawSource(testSource, "key");
            var value1 = new ValueNode("key", "value1");

            var observer = new TestObserver<(ISettingsNode, Exception)>();
            using (source.ObserveRaw().Subscribe(observer))
            {
                testSource.RawSource.PushNewConfiguration(new ObjectNode("root", new Dictionary<string, ISettingsNode>
                {
                    ["key"] = value1
                }));

                var value2 = new ValueNode("key", "value2");
                testSource.RawSource.PushNewConfiguration(new ObjectNode("root", new Dictionary<string, ISettingsNode>
                {
                    ["key"] = value2
                }));

                Action assertion = () => observer.Values.Should().Equal((value1, null), (value2, null));
                assertion.ShouldPassIn(1.Seconds());
            }
        }
    }
}