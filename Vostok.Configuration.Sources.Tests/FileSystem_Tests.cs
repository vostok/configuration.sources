using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class FileSystem_Tests
    {
        private FileSystem fileSystem;

        [SetUp]
        public void TestSetup()
        {
            fileSystem = new FileSystem();
        }
        
        [Test]
        public void WatchFileSystem_should_detect_file_creation()
        {
            using (var folder = new TemporaryFolder())
            {
                var file = "settings";
                var receivedEvents = 0;
                using (fileSystem.WatchFileSystem(folder.Name, file, (sender, args) => receivedEvents++))
                {
                    receivedEvents.Should().Be(0);
                    
                    System.IO.File.WriteAllText(folder.GetFileName(file), "xx");

                    new Action(() => receivedEvents.Should().BeGreaterThan(0))
                        .ShouldPassIn(5.Seconds());
                }
            }
        }

        [Test]
        public void WatchFileSystem_should_detect_file_deletion()
        {
            using (var folder = new TemporaryFolder())
            {
                var file = "settings";
                System.IO.File.WriteAllText(folder.GetFileName(file), "xx");
                var receivedEvents = 0;
                using (fileSystem.WatchFileSystem(folder.Name, file, (sender, args) => receivedEvents++))
                {
                    receivedEvents.Should().Be(0);
                    
                    System.IO.File.Delete(folder.GetFileName(file));

                    new Action(() => receivedEvents.Should().BeGreaterThan(0))
                        .ShouldPassIn(5.Seconds());
                }
            }
        }

        [Test]
        public void WatchFileSystem_should_detect_file_update()
        {
            using (var folder = new TemporaryFolder())
            {
                var file = "settings";
                System.IO.File.WriteAllText(folder.GetFileName(file), "xx");
                var receivedEvents = 0;
                using (fileSystem.WatchFileSystem(folder.Name, file, (sender, args) => receivedEvents++))
                {
                    receivedEvents.Should().Be(0);
                    
                    System.IO.File.WriteAllText(folder.GetFileName(file), "yy");

                    new Action(() => receivedEvents.Should().BeGreaterThan(0))
                        .ShouldPassIn(5.Seconds());
                }
            }
        }

        [Test]
        public void WatchFileSystem_should_not_throw_if_directory_doesnt_exist()
        {
            using (var folder = new TemporaryFolder())
            {
                Directory.Delete(folder.Name);

                Action createWatcher = () => fileSystem.WatchFileSystem(folder.Name, "*.*", (sender, args) => {});

                createWatcher.Should().NotThrow();

                Directory.CreateDirectory(folder.Name);
            }
        }

        [Test]
        public void WatchFileSystem_should_fire_event_on_parent_directory_creation()
        {
            using (var folder = new TemporaryFolder())
            {
                Directory.Delete(folder.Name);

                var fired = 0;

                Action createWatcher = () => fileSystem.WatchFileSystem(folder.Name, "*.*", (sender, args) => fired++);
                createWatcher.Should().NotThrow();
                fired.Should().Be(0);

                Directory.CreateDirectory(folder.Name);
                System.IO.File.WriteAllText(folder.GetFileName("settings.txt"), "newContents");

                Action check = () => fired.Should().Be(1);
                check.ShouldPassIn(TimeSpan.FromSeconds(20));
            }
        }

        [Test]
        public void WatchFileSystem_should_fire_event_on_fast_directory_deletion_and_creation()
        {
            using (var folder = new TemporaryFolder())
            {
                var filepath = folder.GetFileName("settings.txt");
                System.IO.File.WriteAllText(filepath, "contents");

                var fired = 0;

                Action createWatcher = () => fileSystem.WatchFileSystem(folder.Name, "*.*", (sender, args) => fired++);
                createWatcher.Should().NotThrow();
                fired.Should().Be(0);

                Directory.Delete(folder.Name, true);
                Directory.CreateDirectory(folder.Name);
                System.IO.File.WriteAllText(filepath, "newContents");

                Action check = () => fired.Should().Be(1 + 1); // deleted + changed
                check.ShouldPassIn(TimeSpan.FromSeconds(20));
            }
        }
    }
}