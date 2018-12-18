using System;
using System.Reactive.Subjects;
using System.Text;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Sources.File;
using Vostok.Configuration.Sources.Tests.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    public class SettingsFileWatcher_Tests
    {
        private const string Filename = @"C:\settings";

        [Test]
        public void Should_cache_watchers_by_fileName_and_settings_while_watcher_has_subscribers()
        {
            var watcher = SettingsFileWatcher.WatchFile(Filename, new FileSourceSettings {Encoding = Encoding.ASCII});
            using (watcher.Subscribe(new TestObserver<(string content, Exception error)>()))
            {
                SettingsFileWatcher.WatchFile(Filename, new FileSourceSettings {Encoding = Encoding.ASCII})
                    .Should()
                    .BeSameAs(watcher);
            }
            
            SettingsFileWatcher.WatchFile(Filename, new FileSourceSettings {Encoding = Encoding.ASCII})
                .Should()
                .NotBe(watcher);
        }

        [Test]
        public void Should_not_cache_watchers_without_subscribers()
        {
            var watcher = SettingsFileWatcher.WatchFile(Filename);
            SettingsFileWatcher.WatchFile(Filename).Should().NotBe(watcher);
        }
        
        [Test]
        public void Should_subscribe_to_watcher_no_more_than_once()
        {
            var singleFileWatcher = Substitute.For<IObservable<(string, Exception)>>();
            var subscription = Substitute.For<IDisposable>();
            singleFileWatcher.Subscribe(null).ReturnsForAnyArgs(subscription);
            
            using (SettingsFileWatcher.WatchFile(Filename, null, () => singleFileWatcher).Subscribe(new TestObserver<(string, Exception)>()))
            using (SettingsFileWatcher.WatchFile(Filename, null, () => singleFileWatcher).Subscribe(new TestObserver<(string, Exception)>()))
            {
                singleFileWatcher.ReceivedWithAnyArgs(1).Subscribe(null);
                subscription.DidNotReceive().Dispose();
            }
            subscription.Received(1).Dispose();
        }

        [Test]
        public void Should_replay_last_value_from_watcher_to_new_subscriber()
        {
            var singleFileWatcher = new Subject<(string, Exception)>();

            var observer1 = new TestObserver<(string, Exception)>();
            var observer2 = new TestObserver<(string, Exception)>();
            
            using (SettingsFileWatcher.WatchFile(Filename, null, () => singleFileWatcher).Subscribe(observer1))
            {
                singleFileWatcher.OnNext(("settings1", null));
                singleFileWatcher.OnNext(("settings2", null));

                observer1.Values.Should().Equal(("settings1", null), ("settings2", null));
                
                using (SettingsFileWatcher.WatchFile(Filename, null, () => singleFileWatcher).Subscribe(observer2))
                {
                    observer2.Values.Should().Equal(("settings2", null));
                }
            }
        }
    }
}