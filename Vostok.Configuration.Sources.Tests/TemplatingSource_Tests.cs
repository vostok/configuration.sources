using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Constant;
using Vostok.Configuration.Sources.Templating;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class TemplatingSource_Tests
    {
        private List<Substitution> substitutions;

        [SetUp]
        public void TestSetup()
            => substitutions = new List<Substitution>();

        [Test]
        public void Should_replace_a_single_placeholder()
        {
            Substitute("Key", "value");

            Transform("__#{Key}__").Should().Be("__value__");
        }

        [Test]
        public void Should_replace_multiple_placeholders_in_a_single_value()
        {
            Substitute("Key1", "value1");
            Substitute("Key2", "value2");

            Transform("__#{Key1}__#{Key2}__").Should().Be("__value1__value2__");
        }

        [Test]
        public void Should_be_case_insensitive()
        {
            Substitute("key", "value");

            Transform("__#{KEY}__").Should().Be("__value__");
        }

        [Test]
        public void Should_leave_unknown_placeholders_intact()
        {
            Substitute("Key", "value");

            Transform("#{Unknown1}__#{Key}__#{Unknown2}").Should().Be("#{Unknown1}__value__#{Unknown2}");
        }

        [Test]
        public void Should_prefer_last_substitution_among_ones_with_same_name()
        {
            Substitute("KEY", "value1");
            Substitute("key", "value2");

            Transform("__#{Key}__").Should().Be("__value2__");
        }

        [TestCase("100500")]
        [TestCase("with123456")]
        [TestCase("with-dash")]
        [TestCase("with_underscore")]
        [TestCase("with.dot")]
        [TestCase("with:colon")]
        [TestCase("with;semicolon")]
        [TestCase("multiple words")]
        [TestCase("кириллица")]
        public void Should_support_placeholder_name(string name)
        {
            Substitute(name, "value");

            Transform($"__#{{{name}}}__").Should().Be("__value__");
        }

        [Test]
        public void Should_not_replace_non_placeholder_words()
        {
            Substitute("Key", "value");

            Transform("A random text with a '#{Key}' in the middle of it.").Should().Be("A random text with a 'value' in the middle of it.");
        }

        private void Substitute(string name, string value)
            => substitutions.Add(new Substitution(name, value));

        private string Transform(string value)
        {
            var valueNode = new ValueNode("value", value);
            var arrayNode = new ArrayNode("array", new [] {valueNode});
            var objectNode = new ObjectNode("object", new [] {arrayNode});

            var source = new ConstantSource(objectNode).Substitute(substitutions.ToArray());

            return source.Observe().WaitFirstValue(5.Seconds()).settings?["array"]?.Children.Single()?.Value;
        }
    }
}
