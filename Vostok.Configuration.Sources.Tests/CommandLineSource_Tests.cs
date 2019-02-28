using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.CommandLine;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class CommandLineSource_Tests
    {
        [Test]
        public void Should_return_null_settings_for_null_args()
        {
            Observe(null).Should().BeNull();
        }

        [Test]
        public void Should_return_null_settings_for_empty_args()
        {
            Observe().Should().BeNull();
        }

        [Test]
        public void Should_handle_syntax_with_double_minus_and_equals_sign()
        {
            Observe("--key1=value1", "--key2=value2").Should().Be(new ObjectNode(null, new ISettingsNode[]
            {
                new ValueNode("key1", "value1"), 
                new ValueNode("key2", "value2") 
            }));
        }

        [Test]
        public void Should_handle_syntax_with_double_minus_and_space_before_value()
        {
            Observe("--key1", "value1", "--key2", "value2").Should().Be(new ObjectNode(null, new ISettingsNode[]
            {
                new ValueNode("key1", "value1"),
                new ValueNode("key2", "value2")
            }));
        }

        [Test]
        public void Should_handle_syntax_with_forward_slash_and_equals_sign()
        {
            Observe("/key1=value1", "/key2=value2").Should().Be(new ObjectNode(null, new ISettingsNode[]
            {
                new ValueNode("key1", "value1"),
                new ValueNode("key2", "value2")
            }));
        }

        [Test]
        public void Should_handle_syntax_with_forward_slash_and_space_before_value()
        {
            Observe("/key1", "value1", "/key2", "value2").Should().Be(new ObjectNode(null, new ISettingsNode[]
            {
                new ValueNode("key1", "value1"),
                new ValueNode("key2", "value2")
            }));
        }

        [Test]
        public void Should_handle_syntax_without_prefixes_but_with_equals_sign()
        {
            Observe("key1=value1", "key2=value2").Should().Be(new ObjectNode(null, new ISettingsNode[]
            {
                new ValueNode("key1", "value1"),
                new ValueNode("key2", "value2")
            }));
        }

        [Test]
        public void Should_handle_mixed_syntax()
        {
            Observe("key1=value1", "--key2", "value2", "/key3=value3", "--key4=value4", "/key5", "value5")
                .Should().Be(new ObjectNode(null, new ISettingsNode[]
            {
                new ValueNode("key1", "value1"),
                new ValueNode("key2", "value2"),
                new ValueNode("key3", "value3"),
                new ValueNode("key4", "value4"),
                new ValueNode("key5", "value5"),
            }));
        }

        [Test]
        public void Should_parse_hierarchical_keys_with_dots()
        {
            Observe("foo.key1=value1", "--foo.key2", "value2", "/bar.key3=value3", "--bar.key4=value4", "/bar.key5", "value5")
                .Should().Be(new ObjectNode(null, new ISettingsNode[]
                {
                    new ObjectNode("foo", new ISettingsNode[]
                    {
                        new ValueNode("key1", "value1"),
                        new ValueNode("key2", "value2"),
                    }), 
                    new ObjectNode("bar", new ISettingsNode[]
                    {
                        new ValueNode("key3", "value3"),
                        new ValueNode("key4", "value4"),
                        new ValueNode("key5", "value5")
                    })
                }));
        }

        [Test]
        public void Should_skip_incorrect_arguments()
        {
            Observe("bullshit", "key1=value1", "", "key2=value2", "whatever", "--key3", "value3", "nope").Should().Be(new ObjectNode(null, new ISettingsNode[]
            {
                new ValueNode("key1", "value1"),
                new ValueNode("key2", "value2"),
                new ValueNode("key3", "value3")
            }));
        }

        [Test]
        public void Should_merge_value_nodes_with_same_keys_into_arrays()
        {
            Observe("key1=value1", "KEY1=value2", "key2=value3", "Key2=value4", "key3=value5")
                .Should().Be(new ObjectNode(null, new ISettingsNode[]
                {
                    new ArrayNode("key1", new ISettingsNode[]
                    {
                        new ValueNode("0", "value1"),
                        new ValueNode("1", "value2")
                    }), 
                    new ArrayNode("key2", new ISettingsNode[]
                    {
                        new ValueNode("0", "value3"),
                        new ValueNode("1", "value4")
                    }),
                    new ValueNode("key3", "value5") 
                }));
        }

        private static ISettingsNode Observe(params string[] args)
            => new CommandLineSource(args).Observe().WaitFirstValue(1.Seconds()).settings;
    }
}