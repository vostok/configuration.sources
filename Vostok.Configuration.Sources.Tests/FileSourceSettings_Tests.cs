using System;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class FileSourceSettings_Tests
    {
        [Test]
        public void Equals_should_return_true_for_equal_settings()
        {
            var settings1 = new FileSourceSettings("file");
            var settings2 = new FileSourceSettings("file");

            settings1.Equals(settings2).Should().BeTrue();
        }
        
        [Test]
        public void Equals_should_compare_FilePath()
        {
            var settings1 = new FileSourceSettings("file1");
            var settings2 = new FileSourceSettings("file2");

            settings1.Equals(settings2).Should().BeFalse();
        }
        
        [Test]
        public void Equals_should_compare_FilePath_in_normalized_form()
        {
            var settings1 = new FileSourceSettings("file");
            var settings2 = new FileSourceSettings("xx/../file");

            settings1.Equals(settings2).Should().BeTrue();
        }
        
        [Test]
        public void Equals_should_compare_FilePath_with_correct_case_sensitivity()
        {
            var settings1 = new FileSourceSettings("file");
            var settings2 = new FileSourceSettings("FILE");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                settings1.Equals(settings2).Should().BeTrue();
            else
                settings1.Equals(settings2).Should().BeFalse();
        }
        
        [Test]
        public void Equals_should_compare_Encoding()
        {
            var settings1 = new FileSourceSettings("file") {Encoding = Encoding.UTF8};
            var settings2 = new FileSourceSettings("file") {Encoding = Encoding.ASCII};

            settings1.Equals(settings2).Should().BeFalse();
        }
        
        
        [Test]
        public void Equals_should_compare_FileWatcherPeriod()
        {
            var settings1 = new FileSourceSettings("file") {FileWatcherPeriod = TimeSpan.FromSeconds(1)};
            var settings2 = new FileSourceSettings("file") {FileWatcherPeriod = TimeSpan.FromSeconds(2)};

            settings1.Equals(settings2).Should().BeFalse();
        }
        
        [Test]
        public void GetHashCode_should_be_equal_for_equal_settings()
        {
            var settings1 = new FileSourceSettings("file");
            var settings2 = new FileSourceSettings("file");

            settings1.GetHashCode().Should().Be(settings2.GetHashCode());
        }
        
        [Test]
        public void GetHashCode_should_depend_on_FilePath()
        {
            var settings1 = new FileSourceSettings("file1");
            var settings2 = new FileSourceSettings("file2");

            settings1.GetHashCode().Should().NotBe(settings2.GetHashCode());
        }
        
        [Test]
        public void GetHashCode_should_depend_on_FilePath_in_normalized_form()
        {
            var settings1 = new FileSourceSettings("file");
            var settings2 = new FileSourceSettings("xx/../file");

            settings1.GetHashCode().Should().Be(settings2.GetHashCode());
        }
        
        [Test]
        public void GetHashCode_should_depend_on_FilePath_with_correct_case_sensitivity()
        {
            var settings1 = new FileSourceSettings("file");
            var settings2 = new FileSourceSettings("FILE");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                settings1.GetHashCode().Should().Be(settings2.GetHashCode());
            else
                settings1.GetHashCode().Should().NotBe(settings2.GetHashCode());
        }
        
        [Test]
        public void EGetHashCode_should_depend_on_Encoding()
        {
            var settings1 = new FileSourceSettings("file") {Encoding = Encoding.UTF8};
            var settings2 = new FileSourceSettings("file") {Encoding = Encoding.ASCII};

            settings1.GetHashCode().Should().NotBe(settings2.GetHashCode());
        }
        
        
        [Test]
        public void GetHashCode_should_depend_on_FileWatcherPeriod()
        {
            var settings1 = new FileSourceSettings("file") {FileWatcherPeriod = TimeSpan.FromSeconds(1)};
            var settings2 = new FileSourceSettings("file") {FileWatcherPeriod = TimeSpan.FromSeconds(2)};

            settings1.GetHashCode().Should().NotBe(settings2.GetHashCode());
        }
    }
}