using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
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
    }
}