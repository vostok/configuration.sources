using System;
using System.IO;
using System.Reactive.Subjects;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.File;
using Vostok.Configuration.Sources.Tests.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    public class FileSource_Tests
    {
        private ReplaySubject<(string, Exception)> subject;
        private ValueNode settings;

        [SetUp]
        public void SetUp()
        {
            subject = new ReplaySubject<(string, Exception)>();
            settings = new ValueNode("value");
        }
        
        [Test]
        public void Should_push_parsed_settings_when_no_errors()
        {
            var source = new FileSource(
                () => subject,
                content => settings);
            
            subject.OnNext(("settings", null));

            source.Observe().WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((settings, null));
        }
        
        [Test]
        public void Should_push_error_from_fileObserver()
        {
            var parseCalls = 0;
            var source = new FileSource(
                () => subject,
                content =>
                {
                    parseCalls++;
                    return settings;
                });
            
            var error = new IOException();
            subject.OnNext(("settings", error));

            source.Observe().WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((null, error));

            parseCalls.Should().Be(0);
        }

        [Test]
        public void Should_push_parsing_error_when_failed_to_parse()
        {
            var error = new IOException();
            var source = new FileSource(
                () => subject,
                content => throw error);
            
            subject.OnNext(("settings", null));

            source.Observe().WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((null, error));
        }

        [Test]
        public void Should_not_parse_same_content_twice([Values]bool parserThrows)
        {
            var parseCalls = 0;
            var source = new FileSource(
                () => subject,
                content =>
                {
                    parseCalls++;
                    if (parserThrows)
                        throw new FormatException();
                    return settings;
                });

            using (source.Observe().Subscribe(_ => {}))
            {
                Action assertion = () => parseCalls.Should().Be(1);
            
                subject.OnNext(("settings", null));
            
                assertion.ShouldPassIn(1.Seconds());
            
                subject.OnNext(("settings", null));
            
                assertion.ShouldNotFailIn(500.Milliseconds());
            }
        }

        [Test]
        public void Should_not_allow_null_settings()
        {
            new Action(() => new FileSource(null as FileSourceSettings, s => null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_not_allow_null_settings_parser()
        {
            new Action(() => new FileSource(new FileSourceSettings("file"), null))
                .Should().Throw<ArgumentNullException>();
        }
    }
}