using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Scoped;
using Vostok.Configuration.Sources.Tests.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class ScopedSource_Tests
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
            var tree = new ObjectNode(new []{new ValueNode("1")});
            testSource.PushNewConfiguration(tree);
            
            var source = new ScopedSource(testSource);
            var result = source.Observe().WaitFirstValue(100.Milliseconds());
            result.Should().Be((tree, null));
        }

        [Test]
        public void Should_scope_settings([Values] bool hasError)
        {
            var error = hasError ? new IOException() : null;
            var scopedSettings = new ValueNode("value");
            
            var settings = Substitute.For<ISettingsNode>();
            settings.ScopeTo("key").Returns(scopedSettings);
            
            testSource.PushNewConfiguration(settings, error);
            
            var source = new ScopedSource(testSource, "key");
            
            source.Observe().WaitFirstValue(100.Milliseconds()).Should().Be((scopedSettings, error));
            settings.Received().ScopeTo("key");
        }

        [Test]
        public void Should_reflect_underlying_source_updates()
        {
            var source = new ScopedSource(testSource, "key");
            var value1 = new ValueNode("key", "value1");

            var observer = new TestObserver<(ISettingsNode, Exception)>();
            using (source.Observe().Subscribe(observer))
            {
                testSource.PushNewConfiguration(new ObjectNode("root", new[] {value1}));

                var value2 = new ValueNode("key", "value2");
                testSource.PushNewConfiguration(new ObjectNode("root", new[] {value2}));

                Action assertion = () => observer.Values.Should().Equal((value1, null), (value2, null));
                assertion.ShouldPassIn(1.Seconds());
            }
        }
    }
}