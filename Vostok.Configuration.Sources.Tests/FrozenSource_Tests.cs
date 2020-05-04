using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Manual;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class FrozenSource_Tests
    {
        private ManualFeedSource source;
        private IDisposable subscription;
        private List<(ISettingsNode settings, Exception error)> notifications;

        [SetUp]
        public void TestSetup()
        {
            source = new ManualFeedSource();
            notifications = new List<(ISettingsNode, Exception)>();
            subscription = source.Freeze().Observe().Subscribe(notifications.Add);
        }

        [TearDown]
        public void TearDown()
            => subscription.Dispose();

        [Test]
        public void Should_only_return_first_settings()
        {
            source.Push(new ValueNode("1"));
            source.Push(new ValueNode("2"));
            source.Push(new ValueNode("3"));

            notifications.Should().ContainSingle().Which.settings.Value.Should().Be("1");
        }

        [Test]
        public void Should_pass_errors_coming_before_first_settings()
        {
            source.Push(null, new Exception("error1"));
            source.Push(null, new Exception("error2"));
            source.Push(new ValueNode("1"));
            source.Push(new ValueNode("2"));
            source.Push(new ValueNode("3"));

            notifications.Should().HaveCount(3);
            notifications[0].error.Message.Should().Be("error1");
            notifications[1].error.Message.Should().Be("error2");
            notifications[2].settings.Value.Should().Be("1");
        }

        [Test]
        public void Should_not_pass_any_errors_coming_after_first_settings()
        {
            source.Push(new ValueNode("1"));
            source.Push(new ValueNode("1"), new Exception("error"));
            source.Push(new ValueNode("2"));

            notifications.Should().ContainSingle().Which.settings.Value.Should().Be("1");
        }
    }
}
