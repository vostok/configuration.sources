using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Scoped;
using Vostok.Configuration.Sources.Tests.Commons;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    public class ScopedSource_Tests : Sources_Test
    {
        [Test]
        public void Should_return_full_tree_by_tree()
        {
            var tree = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                ["value"] = new ValueNode("1"),
            });

            var ss = new ScopedSource(tree);
            var result = ss.Get();
            result["value"].Value.Should().Be("1");
        }

        [Test]
        public void Should_scope_by_object_keys()
        {
            var tree = new ObjectNode(new SortedDictionary<string, ISettingsNode>
            {
                ["key1"] = new ObjectNode(new SortedDictionary<string, ISettingsNode>
                {
                    ["key2"] = new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        ["key3"] = new ValueNode("value")
                    })
                })
            });

            var ss = new ScopedSource(tree, "key1", "key2");
            var result = ss.Get();
            result["key3"].Value.Should().Be("value");
        }

        [Test]
        public void Should_scope_by_array_indexes()
        {
            var tree = new ArrayNode(new List<ISettingsNode>
            { 
                new ArrayNode(new List<ISettingsNode>
                {
                    new ObjectNode(new SortedDictionary<string, ISettingsNode>
                    {
                        ["key"] = new ValueNode("value")
                    })  
                })
            });

            var ss = new ScopedSource(tree, "[0]", "[0]");
            var result = ss.Get();
            result["key"].Value.Should().Be("value");
        }
    }
}