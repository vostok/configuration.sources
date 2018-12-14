using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class GarbageCollectedCache_Tests
    {
        private GarbageCollectedCache<string, object> cache;

        [Test]
        public void Should_cache_items_when_not_garbage()
        {
            cache = new GarbageCollectedCache<string, object>(_ => false);
            
            GetOrAddNew("key").Should().BeSameAs(GetOrAddNew("key"));
        }

        [Test]
        public void Should_remove_garbage_items_from_cache()
        {
            object garbage = null;
            cache = new GarbageCollectedCache<string, object>(kv => kv.Value == garbage);
            
            var value1 = GetOrAddNew("key1");
            var value2 = GetOrAddNew("key2");

            GetOrAddNew("key1").Should().BeSameAs(value1);
            GetOrAddNew("key2").Should().BeSameAs(value2);

            garbage = value2;

            GetOrAddNew("key1").Should().BeSameAs(value1);
            GetOrAddNew("key2").Should().NotBe(value2);
        }

        private object GetOrAddNew(string key)
        {
            return cache.GetOrAdd(key, _ => new object());
        }
    }
}