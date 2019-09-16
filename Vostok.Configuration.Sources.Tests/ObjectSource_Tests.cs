using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Object;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class ObjectSource_Tests
    {
        [Test]
        public void Should_return_null_value_node_for_null_object()
        {
            Observe(null).Should().Be(new ValueNode(null));
        }

        [Test]
        public void Should_return_empty_object_node_for_empty_object()
        {
            Observe(new object()).Should().Be(new ObjectNode(Array.Empty<ISettingsNode>()));
        }

        [Test]
        public void Should_return_empty_array_node_for_empty_sequence()
        {
            Observe(Array.Empty<int>()).Should().Be(new ArrayNode(Array.Empty<ISettingsNode>()));
            Observe(new List<string>()).Should().Be(new ArrayNode(new List<ISettingsNode>()));
        }

        [Test]
        public void Should_return_value_node_for_value_object()
        {
            Observe("string").Should().Be(new ValueNode("string"));
            Observe(Guid.Empty).Should().Be(new ValueNode(Guid.Empty.ToString()));
            Observe(100).Should().Be(new ValueNode("100"));
            Observe(Encoding.Unicode).Should().Be(new ValueNode(Encoding.Unicode.WebName));
        }

        [Test]
        public void Should_handle_dictionaries()
        {
            var dictionary1 = new Dictionary<string, TimeSpan>
            {
                {"1", TimeSpan.Zero},
                {"2", TimeSpan.MinValue},
                {"3", TimeSpan.MaxValue}
            };
            Observe(dictionary1)
                .Should()
                .Be(
                    new ObjectNode(
                        new[]
                        {
                            new ValueNode("1", TimeSpan.Zero.ToString()),
                            new ValueNode("2", TimeSpan.MinValue.ToString()),
                            new ValueNode("3", TimeSpan.MaxValue.ToString())
                        }));

            var dictionary2 = new Dictionary<int, string>
            {
                {1, "2"},
                {3, "4"},
                {5, "6"}
            };
            Observe(dictionary2)
                .Should()
                .Be(
                    new ObjectNode(
                        new[]
                        {
                            new ValueNode("1", "2"),
                            new ValueNode("3", "4"),
                            new ValueNode("5", "6")
                        }));
        }

        [Test]
        public void Should_handle_sequences()
        {
            Observe(Enumerable.Range(1, 5).ToArray())
                .Should()
                .Be(new ArrayNode(Enumerable.Range(1, 5).Select(i => new ValueNode(i.ToString())).ToArray()));
            Observe(Enumerable.Range(1, 10).ToList())
                .Should()
                .Be(new ArrayNode(Enumerable.Range(1, 10).Select(i => new ValueNode(i.ToString())).ToArray()));
            Observe(Enumerable.Repeat(Guid.Empty, 1).ToArray())
                .Should()
                .Be(new ArrayNode(Enumerable.Repeat(Guid.Empty, 1).Select(i => new ValueNode(i.ToString())).ToArray()));
        }

        [Test]
        public void Should_handle_objects_with_null_properties()
        {
            var johnDoe = new Person
            {
                Name = "John Doe",
                Age = 65
            };

            Observe(johnDoe)
                .Should()
                .Be(
                    new ObjectNode(
                        new ISettingsNode[]
                        {
                            new ValueNode("Age", "65"),
                            new ValueNode("Name", "John Doe"),
                            new ValueNode("Children", null),
                            new ValueNode("Info", null),
                        }));
        }

        [Test]
        public void Should_handle_complex_objects()
        {
            var johnDoe = new Person("John Doe", 65);
            var judyDoe = new Person("Judy Doe", 22);
            var jamesDoe = new Person {Name = "James Doe", Age = 40};

            johnDoe.Children.Add(judyDoe);
            johnDoe.Children.Add(jamesDoe);

            johnDoe.Info.Add(PersonInfo.BirthDayDate, "Date");
            judyDoe.Info.Add(PersonInfo.SocialSecurityNumber, "Number");

            Observe(johnDoe)
                .Should()
                .Be(
                    new ObjectNode(
                        new ISettingsNode[]
                        {
                            new ValueNode("Age", "65"),
                            new ValueNode("Name", "John Doe"),
                            new ArrayNode(
                                "Children",
                                new ISettingsNode[]
                                {
                                    new ObjectNode(
                                        new ISettingsNode[]
                                        {
                                            new ValueNode("Age", "22"),
                                            new ValueNode("Name", "Judy Doe"),
                                            new ArrayNode("Children", Array.Empty<ISettingsNode>()),
                                            new ObjectNode(
                                                "Info",
                                                new ISettingsNode[]
                                                {
                                                    new ValueNode("SocialSecurityNumber", "Number")
                                                })
                                        }),
                                    new ObjectNode(
                                        new ISettingsNode[]
                                        {
                                            new ValueNode("Age", "40"),
                                            new ValueNode("Name", "James Doe"),
                                            new ValueNode("Children", null),
                                            new ValueNode("Info", null)
                                        })
                                }),
                            new ObjectNode(
                                "Info",
                                new ISettingsNode[]
                                {
                                    new ValueNode("BirthDayDate", "Date")
                                })
                        }));
        }

        [Test]
        public void Should_handle_sequence_of_dictionaries()
        {
            var arrayOfDictionaries = new[]
            {
                new Dictionary<string, int>
                {
                    {"100", 100},
                    {"200", 200}
                },
                new Dictionary<string, int>
                {
                    {"1000", 1000}
                }
            };

            Observe(arrayOfDictionaries)
                .Should()
                .Be(
                    new ArrayNode(
                        null,
                        new ISettingsNode[]
                        {
                            new ObjectNode(
                                null,
                                new List<ISettingsNode>
                                {
                                    new ValueNode("100", "100"),
                                    new ValueNode("200", "200")
                                }),
                            new ObjectNode(
                                null,
                                new List<ISettingsNode>
                                {
                                    new ValueNode("1000", "1000")
                                })
                        }));
        }

        [Test]
        public void Should_handle_dictionary_of_objects()
        {
            var dictionaryOfPersons = new Dictionary<string, Person>
            {
                ["Father"] = new Person("John Doe", 65),
                ["Daughter"] = new Person("Judy Doe", 22),
                ["Son"] = new Person("James Doe", 40)
            };

            Observe(dictionaryOfPersons)
                .Should()
                .Be(
                    new ObjectNode(
                        new ISettingsNode[]
                        {
                            new ObjectNode(
                                "Father",
                                new ISettingsNode[]
                                {
                                    new ValueNode("Age", "65"),
                                    new ValueNode("Name", "John Doe"),
                                    new ArrayNode("Children", Array.Empty<ISettingsNode>()),
                                    new ObjectNode("Info", Array.Empty<ISettingsNode>())
                                }),
                            new ObjectNode(
                                "Daughter",
                                new ISettingsNode[]
                                {
                                    new ValueNode("Age", "22"),
                                    new ValueNode("Name", "Judy Doe"),
                                    new ArrayNode("Children", Array.Empty<ISettingsNode>()),
                                    new ObjectNode("Info", Array.Empty<ISettingsNode>())
                                }),
                            new ObjectNode(
                                "Son",
                                new ISettingsNode[]
                                {
                                    new ValueNode("Age", "40"),
                                    new ValueNode("Name", "James Doe"),
                                    new ArrayNode("Children", Array.Empty<ISettingsNode>()),
                                    new ObjectNode("Info", Array.Empty<ISettingsNode>())
                                })
                        }));
        }

        [Test]
        public void Should_error_on_cyclic_dependency()
        {
            var johnDoe = new Person("John Doe", 65);
            var judyDoe = new Person("Judy Doe", 22);
            var jamesDoe = new Person("James Doe", 40);

            johnDoe.Children.Add(judyDoe);
            judyDoe.Children.Add(jamesDoe);
            jamesDoe.Children.Add(johnDoe);

            var error = new ObjectSource(johnDoe).Observe().WaitFirstValue(1.Seconds()).error;
            error.Should().NotBeNull().And.BeOfType<ArgumentException>();
        }

        [Test]
        public void Should_handle_objects_with_repeated_elements()
        {
            var listOfInts = new List<int> {1, 2, 3};
            var dictionaryOfLists = new Dictionary<string, IList<int>>
            {
                ["First"] = listOfInts,
                ["Second"] = listOfInts,
                ["Third"] = listOfInts
            };

            Observe(dictionaryOfLists)
                .Should()
                .Be(
                    new ObjectNode(
                        null,
                        new ISettingsNode[]
                        {
                            new ArrayNode(
                                "First",
                                new List<ISettingsNode>
                                {
                                    new ValueNode("1"), new ValueNode("2"), new ValueNode("3")
                                }),
                            new ArrayNode(
                                "Second",
                                new List<ISettingsNode>
                                {
                                    new ValueNode("1"), new ValueNode("2"), new ValueNode("3")
                                }),
                            new ArrayNode(
                                "Third",
                                new List<ISettingsNode>
                                {
                                    new ValueNode("1"), new ValueNode("2"), new ValueNode("3")
                                })
                        }));
        }

        private static ISettingsNode Observe(object obj)
        {
            return new ObjectSource(obj).Observe().WaitFirstValue(1.Seconds()).settings;
        }

        private enum PersonInfo
        {
            BirthDayDate,
            SocialSecurityNumber
        }

        private class Person
        {
            public int Age;

            public Person()
            {
            }

            public Person(string name, int age)
            {
                Name = name;
                Age = age;
                Children = new List<Person>();
                Info = new Dictionary<PersonInfo, string>();
            }

            public string Name { get; set; }
            public ICollection<Person> Children { get; set; }
            public IDictionary<PersonInfo, string> Info;
        }
    }
}