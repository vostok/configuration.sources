using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class SingletonConfigurationSource_Tests
    {
        private Func<IConfigurationSource> constructor;
        private IObservable<(ISettingsNode, Exception)> observable;
        private IConfigurationSource implementation;

        [SetUp]
        public void SetUp()
        {
            SingletonConfigurationSource.ClearCache();
            observable = Substitute.For<IObservable<(ISettingsNode, Exception)>>();
            implementation = Substitute.For<IConfigurationSource>();
            implementation.Observe().Returns(observable);
            constructor = Substitute.For<Func<IConfigurationSource>>();
            constructor.Invoke().Returns(implementation);
        }
        
        [Test]
        public void Should_cache_implementation_when_same_key_and_type()
        {
            var source1 = new TestSourceA(constructor, "key");
            source1.Observe().Should().Be(observable);
    
            var source2 = new TestSourceA(constructor, "key");
            source2.Observe().Should().Be(observable);
    
            constructor.Received(1).Invoke();
        }
    
        [Test]
        public void Should_not_cache_implementation_when_same_key_and_different_types()
        {
            var source1 = new TestSourceA(constructor, "key");
            source1.Observe().Should().Be(observable);
    
            constructor.Invoke().Returns(Substitute.For<IConfigurationSource>());
    
            var source2 = new TestSourceB(constructor, "key");
            source2.Observe().Should().NotBe(observable);
    
            constructor.Received(2).Invoke();
        }
    
        [Test]
        public void Should_not_cache_implementation_when_different_keys()
        {
            var source1 = new TestSourceA(constructor, "key1");
            source1.Observe().Should().Be(observable);
    
            constructor.Invoke().Returns(Substitute.For<IConfigurationSource>());
    
            var source2 = new TestSourceA(constructor, "key2");
            source2.Observe().Should().NotBe(observable);
    
            constructor.Received(2).Invoke();
        }
        
        private class TestSourceA : SingletonConfigurationSource
        {
            public TestSourceA(Func<IConfigurationSource> constructor, object key)
                : base(key, constructor)
            {
            }
        }
        
        private class TestSourceB : SingletonConfigurationSource
        {
            public TestSourceB(Func<IConfigurationSource> constructor, object key)
                : base(key, constructor)
            {
            }
        }
    }
}