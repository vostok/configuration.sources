using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Constant;
using Vostok.Configuration.Sources.Nesting;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class NestingSource_Tests
    {
        [Test]
        public void Should_not_nest_null_nodes()
        {
            Nest(null, "a", "b").Should().BeNull();
        }

        [Test]
        public void Should_not_nest_without_scopes()
        {
            var node = new ValueNode("a", "b");

            Nest(node).Should().BeSameAs(node);
        }

        [Test]
        public void Should_nest_value_node_on_one_level()
        {
            var node = new ValueNode("a", "b");

            var nestedNode = Nest(node, "x");

            nestedNode["x"]?.Value.Should().Be("b");
        }

        [Test]
        public void Should_nest_value_node_on_multiple_levels()
        {
            var node = new ValueNode("a", "b");

            var nestedNode = Nest(node, "x", "y", "z");

            nestedNode["x"]?["y"]?["z"]?.Value.Should().Be("b");
        }

        [Test]
        public void Should_nest_object_node_on_one_level()
        {
            var node = new ObjectNode("o", new [] {new ValueNode("a", "b"), new ValueNode("c", "d") });

            var nestedNode = Nest(node, "x");

            nestedNode["x"]?["a"]?.Value.Should().Be("b");
            nestedNode["x"]?["c"]?.Value.Should().Be("d");
        }

        [Test]
        public void Should_nest_object_node_on_multiple_levels()
        {
            var node = new ObjectNode("o", new[] { new ValueNode("a", "b"), new ValueNode("c", "d") });

            var nestedNode = Nest(node, "x", "y", "z");

            nestedNode["x"]?["y"]?["z"]?["a"]?.Value.Should().Be("b");
            nestedNode["x"]?["y"]?["z"]?["c"]?.Value.Should().Be("d");
        }

        [Test]
        public void Should_nest_object_node_with_null_name()
        {
            var node = new ObjectNode(null, new[] { new ValueNode("a", "b"), new ValueNode("c", "d") });

            var nestedNode = Nest(node, "x", "y", "z");

            nestedNode["x"]?["y"]?["z"]?["a"]?.Value.Should().Be("b");
            nestedNode["x"]?["y"]?["z"]?["c"]?.Value.Should().Be("d");
        }

        private static ISettingsNode Nest(ISettingsNode node, params string[] scopes)
        {
            var baseSource = new ConstantSource(node);
            var nestingSource = new NestingSource(baseSource, scopes);

            return nestingSource.Observe().WaitFirstValue(TimeSpan.FromSeconds(5)).settings;
        } 
    }
}
