using System;
using System.Reactive.Subjects;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Sources.Tests.Helpers;
using Vostok.Configuration.Sources.Watchers;
using Exception = System.Exception;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class WatcherCache_Tests
    {
        private WatcherCache<string, string> watcherCache;
        private IWatcherFactory<string, string> watcherFactory;
        private string settings;

        [SetUp]
        public void SetUp()
        {
            watcherFactory = Substitute.For<IWatcherFactory<string, string>>();
            watcherCache = new WatcherCache<string, string>(watcherFactory);
            settings = "settings";
        }

        [Test]
        public void Should_cache_watchers_by_settings_while_watcher_has_subscribers()
        {
            var watcher = watcherCache.Watch(settings);
            using (watcher.Subscribe(new TestObserver<(string content, Exception error)>()))
            {
                watcherCache.Watch(settings)
                    .Should()
                    .BeSameAs(watcher);

                watcherFactory.Received(1).CreateWatcher(settings);
            }

            watcherCache.Watch(settings)
                .Should()
                .NotBe(watcher);
        }

        [Test]
        public void Should_not_cache_watchers_without_subscribers()
        {
            var watcher = watcherCache.Watch(settings);
            watcherCache.Watch(settings).Should().NotBe(watcher);
        }

        [Test]
        public void Should_subscribe_to_watcher_no_more_than_once()
        {
            var fileWatcher = Substitute.For<IObservable<(string, Exception)>>();
            var subscription = Substitute.For<IDisposable>();
            fileWatcher.Subscribe(null).ReturnsForAnyArgs(subscription);
            
            watcherFactory.CreateWatcher(settings).Returns(fileWatcher);

            using (watcherCache.Watch(settings).Subscribe(new TestObserver<(string, Exception)>()))
            using (watcherCache.Watch(settings).Subscribe(new TestObserver<(string, Exception)>()))
            {
                fileWatcher.ReceivedWithAnyArgs(1).Subscribe(null);
                subscription.DidNotReceive().Dispose();
            }

            subscription.Received(1).Dispose();
        }
        
        [Test]
        public void Should_replay_last_value_from_watcher_to_new_subscriber()
        {
            var fileWatcher = new Subject<(string, Exception)>();
            watcherFactory.CreateWatcher(settings).Returns(fileWatcher);
        
            var observer1 = new TestObserver<(string, Exception)>();
            var observer2 = new TestObserver<(string, Exception)>();
            
            using (watcherCache.Watch(settings).Subscribe(observer1))
            {
                fileWatcher.OnNext(("settings1", null));
                fileWatcher.OnNext(("settings2", null));
        
                observer1.Values.Should().Equal(("settings1", null), ("settings2", null));
                
                using (watcherCache.Watch(settings).Subscribe(observer2))
                {
                    observer2.Values.Should().Equal(("settings2", null));
                }
            }
        }
    }
}