using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Environment;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class EnvironmentVariablesConverter_Tests
    {
        private EnvironmentVariablesConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new EnvironmentVariablesConverter();
        }
        
        [Test]
        public void Should_return_null_when_no_variables()
        {
            var vars = CreateVariablesDictionary(new Dictionary<string, string>());
            converter.Convert(vars).Should().BeNull();
        }

        [Test]
        public void Should_parse_variables_to_single_objectNode_when_simple_keys()
        {
            var vars = CreateVariablesDictionary(
                new Dictionary<string, string>
                {
                    ["key1"] = "value1",
                    ["key2"] = "value2"
                });
            converter.Convert(vars)
                .Should()
                .Be(
                    new ObjectNode(
                        new Dictionary<string, ISettingsNode>
                        {
                            ["key1"] = new ValueNode("key1", "value1"),
                            ["key2"] = new ValueNode("key2", "value2")
                        }));
        }

        [Test]
        public void Should_parse_variables_to_tree_when_multilevel_keys()
        {
            var vars = CreateVariablesDictionary(
                new Dictionary<string, string>
                {
                    ["a.b"] = "value1",
                    ["a.c"] = "value2"
                });
            converter.Convert(vars)
                .Should()
                .Be(
                    new ObjectNode(
                        new Dictionary<string, ISettingsNode>
                        {
                            ["a"] = new ObjectNode(
                                "a",
                                new Dictionary<string, ISettingsNode>
                                {
                                    ["b"] = new ValueNode("b", "value1"),
                                    ["c"] = new ValueNode("c", "value2")
                                })
                        }));
        }

        [Test]
        public void Should_ignore_keys_case()
        {
            var vars = CreateVariablesDictionary(
                new Dictionary<string, string>
                {
                    ["PATH"] = "value"
                });
            converter.Convert(vars)["path"].Should().Be(new ValueNode("PATH", "value"));
        }

        private static IDictionary CreateVariablesDictionary(Dictionary<string, string> variables)
        {
            var hashtable = new Hashtable();
            foreach (var keyValuePair in variables)
            {
                hashtable.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return hashtable;
        }
    }
}