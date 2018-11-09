using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    public class BaseFileRawSource_Tests
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
            var source = new BaseFileRawSource(
                () => subject,
                content => settings);
            
            subject.OnNext(("settings", null));

            source.ObserveRaw().WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((settings, null));
        }
        
        [Test]
        public void Should_push_error_from_fileObserver()
        {
            var parseCalls = 0;
            var source = new BaseFileRawSource(
                () => subject,
                content =>
                {
                    parseCalls++;
                    return settings;
                });
            
            var error = new IOException();
            subject.OnNext(("settings", error));

            source.ObserveRaw().WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((null, error));

            parseCalls.Should().Be(0);
        }

        [Test]
        public void Should_push_parsing_error_when_failed_to_parse()
        {
            var error = new IOException();
            var source = new BaseFileRawSource(
                () => subject,
                content => throw error);
            
            subject.OnNext(("settings", null));

            source.ObserveRaw().WaitFirstValue(100.Milliseconds())
                .Should()
                .Be((null, error));
        }

        [Test]
        public void Should_not_parse_same_content_twice([Values]bool parserThrows)
        {
            var parseCalls = 0;
            var source = new BaseFileRawSource(
                () => subject,
                content =>
                {
                    parseCalls++;
                    if (parserThrows)
                        throw new FormatException();
                    return settings;
                });

            var task = Task.Run(
                () =>
                {
                    var pushes = source.ObserveRaw()
                        .Buffer(200.Microseconds())
                        .ToEnumerable()
                        .ToArray();
                });

            task.Wait(50.Microseconds());
            
            subject.OnNext(("settings", null));
            subject.OnNext(("settings", null));

            Action assertion = () => parseCalls.Should().Be(1);
            assertion.ShouldPassIn(1.Seconds());
        }

        [Test]
        public void Should_cache_fileWatcher()
        {
            var watcherProviderCalls = 0;
            var source = new BaseFileRawSource(
                () =>
                {
                    watcherProviderCalls++;
                    return subject;
                },
                content => settings);
            
            source.ObserveRaw();
            source.ObserveRaw();

            watcherProviderCalls.Should().Be(1);
        }
    }
}