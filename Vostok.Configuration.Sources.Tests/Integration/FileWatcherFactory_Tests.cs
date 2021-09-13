using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Tests.Integration
{
    [TestFixture]
    public class FileWatcher_Tests
    {
        private string settingsPath;
        private IFileSystem fileSystem;
        private string fileContent = "";
        private IDisposable fsWatcher;
        private FileWatcherFactory factory;

        [SetUp]
        public void SetUp()
        {
            settingsPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\settings" : "/etc/settings/";
            fsWatcher = Substitute.For<IDisposable>();
            fileSystem = Substitute.For<IFileSystem>();
            fileSystem.WatchFileSystem("", "", null).ReturnsForAnyArgs(fsWatcher);
            factory = new FileWatcherFactory(fileSystem);
        }

        [Test]
        public void Should_push_null_content_when_file_not_exists()
        {
            fileSystem.Exists(settingsPath).Returns(false);
            var watcher = CreateFileWatcher();

            watcher.WaitFirstValue(1.Seconds()).Should().Be((null, null));
        }
        
        [Test]
        public void Should_push_current_file_content_when_file_exists()
        {
            SetupFileExists(settingsPath, "settings");
            var watcher = CreateFileWatcher();

            watcher.WaitFirstValue(1.Seconds()).Should().Be(("settings", null));
        }

        [TestCase("settings")]
        [TestCase("../settings.txt")]
        [TestCase("..\\settings.txt")]
        [TestCase("./settings.txt")]
        [TestCase(".\\settings.txt")]
        public void Should_change_relative_file_path_to_absolute(string filePath)
        {
            var expectedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            SetupFileExists(expectedFilePath, "some settings");
            
            var watcher = CreateFileWatcher(null, filePath);
            watcher.WaitFirstValue(1.Seconds());

            fileSystem.Received()
                .OpenFile(
                    expectedFilePath,
                    Arg.Any<FileMode>(),
                    Arg.Any<FileAccess>(),
                    Arg.Any<FileShare>(),
                    Arg.Any<Encoding>());
        }

        [TestCase("/etc/apt/apt.conf")]
        [TestCase("//etc/apt/apt.conf")]
        [TestCase("C:/settings/one.json", IncludePlatform = "Win")]
        [TestCase(@"C:\settings\one.json", IncludePlatform = "Win")]
        public void Should_not_change_absolute_file_path(string filePath)
        {
            SetupFileExists(filePath, "settings file content");
            
            var watcher = CreateFileWatcher(null, filePath);
            watcher.WaitFirstValue(1.Seconds());

            fileSystem.Received()
                .OpenFile(
                    filePath,
                    Arg.Any<FileMode>(),
                    Arg.Any<FileAccess>(),
                    Arg.Any<FileShare>(),
                    Arg.Any<Encoding>());
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

            var observer = new TestObserver<(string, Exception)>();
            using (watcher.Subscribe(observer))
            {
                handler.Should().NotBeNull();
                
                Action assertion1 = () => observer.Values.Should().Equal(("settings1", null));
                assertion1.ShouldPassIn(100.Milliseconds());

                fileContent = "settings2";
                handler(null, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(settingsPath), Path.GetFileName(settingsPath)));

                Action assertion2 = () => observer.Values.Should().Equal(("settings1", null), ("settings2", null));
                assertion2.ShouldPassIn(100.Milliseconds());
            }
        }

        [Test]
        public void Should_periodically_check_file_updates()
        {
            SetupFileExists(settingsPath, "settings1");

            var watcher = CreateFileWatcher(50.Milliseconds());

            var observer = new TestObserver<(string, Exception)>();
            using (watcher.Subscribe(observer))
            {
                Action assertion1 = () => observer.Values.Distinct().Should().Equal(("settings1", null));
                assertion1.ShouldPassIn(100.Milliseconds());

                fileContent = "settings2";

                Action assertion2 = () => observer.Values.Distinct().Should().Equal(("settings1", null), ("settings2", null));
                assertion2.ShouldPassIn(100.Milliseconds());
            }
        }

        [Test]
        public void Should_dispose_fsWatcher_after_unsubscription()
        {
            var watcher = CreateFileWatcher(50.Milliseconds());

            var observer = new TestObserver<(string, Exception)>();
            using (watcher.Subscribe(observer))
            {
                fsWatcher.DidNotReceive().Dispose();
            }
            fsWatcher.Received().Dispose();
        }

        [Test]
        public void Should_stop_worker_after_unsubscription()
        {
            var watcher = CreateFileWatcher(50.Milliseconds());

            var observer = new TestObserver<(string, Exception)>();
            using (watcher.Subscribe(observer))
            {
                Action assertion1 = () => fileSystem.Received(2).Exists(settingsPath);
                assertion1.ShouldPassIn(150.Milliseconds());
            }
            fileSystem.ClearReceivedCalls();
            Action assertion2 = () => fileSystem.DidNotReceive().Exists(settingsPath);
            assertion2.ShouldNotFailIn(100.Milliseconds());
        }

        private IObservable<(string value, Exception error)> CreateFileWatcher(TimeSpan? fileWatcherPeriod = null, string filePath = null)
        {
            var settings = new FileSourceSettings(filePath ?? settingsPath);
            if (fileWatcherPeriod != null)
                settings.FileWatcherPeriod = fileWatcherPeriod.Value;
            return factory.CreateWatcher(settings);
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