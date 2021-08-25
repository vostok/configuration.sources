using System;
using System.IO;
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

                Directory.Exists(folder.Name).Should().BeTrue();
            }
        }
    }
}