using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources.File;
using Vostok.Configuration.Sources.Helpers;
using Vostok.Configuration.Sources.Tests.Helpers;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    public class SingleFileWatcher_Tests
    {
        private string settingsPath;
        private IFileSystem fileSystem;
        private string fileContent = "";

        [SetUp]
        public void SetUp()
        {
            settingsPath = @"C:\settings";
            fileSystem = Substitute.For<IFileSystem>();
        }

        [Test]
        public void Should_push_null_content_when_file_not_exists()
        {
            fileSystem.Exists(settingsPath).Returns(false);
            var watcher = new SingleFileWatcher(settingsPath, new FileSourceSettings(), fileSystem);

            watcher.WaitFirstValue(1.Seconds()).Should().Be((null, null));
        }
        
        [Test]
        public void Should_push_current_file_content_when_file_exists()
        {
            SetupFileExists(settingsPath, "settings");
            var watcher = CreateFileWatcher();

            watcher.WaitFirstValue(1.Seconds()).Should().Be(("settings", null));
        }

        [Test]
        public void Should_push_error_when_failed_to_read_file()
        {
            var error = new IOException();
            SetupReadingError(settingsPath, error);

            var watcher = CreateFileWatcher();

            watcher.WaitFirstValue(1.Seconds()).Should().Be((null, error));
        }

        [Test]
        public void Should_repeat_last_value_for_new_subscribers([Values]bool simulateReadingError)
        {
            var content = "settings";
            var error = new IOException();
            (string, Exception) expectedValue;

            if (simulateReadingError)
            {
                SetupReadingError(settingsPath, error);
                expectedValue = (null, error);
            }
            else
            {
                SetupFileExists(settingsPath, content);
                expectedValue = (content, null);
            }

            var watcher = CreateFileWatcher();

            watcher.WaitFirstValue(1.Seconds()).Should().Be(expectedValue);
            watcher.WaitFirstValue(1.Seconds()).Should().Be(expectedValue);
        }

        [Test]
        public void Should_receive_file_updates_from_fileSystem()
        {
            FileSystemEventHandler handler = null;
            fileSystem
                .When(fs => fs.WatchFileSystem(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FileSystemEventHandler>()))
                .Do(callInfo => handler = callInfo.ArgAt<FileSystemEventHandler>(2));
            
            SetupFileExists(settingsPath, "settings1");

            var watcher = CreateFileWatcher(10.Seconds());

            handler.Should().NotBeNull();
            
            var observer = new TestScheduler().CreateObserver<(string, Exception)>();
            using (watcher.Subscribe(observer))
            {
                Action assertion1 = () => observer.GetValues().Should().Equal(("settings1", null));
                assertion1.ShouldPassIn(100.Milliseconds());

                fileContent = "settings2";
                handler(null, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(settingsPath), Path.GetFileName(settingsPath)));

                Action assertion2 = () => observer.GetValues().Should().Equal(("settings1", null), ("settings2", null));
                assertion2.ShouldPassIn(100.Milliseconds());
            }
        }

        [Test]
        public void Should_periodically_check_file_updates()
        {
            SetupFileExists(settingsPath, "settings1");

            var watcher = CreateFileWatcher(50.Milliseconds());

            var observer = new TestScheduler().CreateObserver<(string, Exception)>();
            using (watcher.Subscribe(observer))
            {
                Action assertion1 = () => observer.GetValues().Distinct().Should().Equal(("settings1", null));
                assertion1.ShouldPassIn(100.Milliseconds());

                fileContent = "settings2";

                Action assertion2 = () => observer.GetValues().Distinct().Should().Equal(("settings1", null), ("settings2", null));
                assertion2.ShouldPassIn(100.Milliseconds());
            }
        }

        private SingleFileWatcher CreateFileWatcher(TimeSpan? fileWatcherPeriod = null)
        {
            var settings = new FileSourceSettings();
            if (fileWatcherPeriod != null)
                settings.FileWatcherPeriod = fileWatcherPeriod.Value;
            return new SingleFileWatcher(settingsPath, settings, fileSystem);
        }

        private void SetupFileExists(string filePath, string initialContent)
        {
            fileSystem.Exists(filePath)
                .Returns(true);
            fileContent = initialContent;
            fileSystem.OpenFile(filePath, Arg.Any<FileMode>(), Arg.Any<FileAccess>(), Arg.Any<FileShare>(), Arg.Any<Encoding>())
                .Returns(callInfo => new StringReader(fileContent));
        }

        private void SetupReadingError(string filePath, Exception error)
        {
            fileSystem.Exists(filePath)
                .Returns(true);
            fileSystem.OpenFile(filePath, Arg.Any<FileMode>(), Arg.Any<FileAccess>(), Arg.Any<FileShare>(), Arg.Any<Encoding>())
                .Throws(error);
        }
    }
}