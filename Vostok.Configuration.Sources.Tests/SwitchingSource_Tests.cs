using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Manual;
using Vostok.Configuration.Sources.Switching;
using ObservableExtensions = Vostok.Configuration.Abstractions.Extensions.Observable.ObservableExtensions;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class SwitchingSource_Tests
    {
        [Test]
        public void Should_switch_to_a_new_source_as_soon_as_it_becomes_available()
        {
            var source1 = CreateSource();
            var source2 = CreateSource();
            var source3 = CreateSource();
            
            var values = new List<int>();

            var switchingSource = new SwitchingSource(source1);

            using (ObservableExtensions.Subscribe(switchingSource.Observe(), pair => values.Add(int.Parse(pair.settings.Value ?? "0"))))
            {
                source1.Push(1);
                source1.Push(2);
                source1.Push(3);

                source2.Push(4);
                source2.Push(5);

                switchingSource.SwitchTo(source2);

                source1.Push(6);
                source1.Push(7);

                source2.Push(8);
                source2.Push(9);

                source3.Push(10);
                source3.Push(11);

                switchingSource.SwitchTo(source3);

                source1.Push(12);
                source1.Push(13);

                source2.Push(14);
                source2.Push(15);

                source3.Push(16);
                source3.Push(17);

                switchingSource.SwitchTo(source1);
                switchingSource.SwitchTo(source2);

                switchingSource.SwitchTo(switchingSource.CurrentSource.Transform(node => new ValueNode(node.Value + "00")));
            }

            values.Should().Equal(1, 2, 3, 5, 8, 9, 11, 16, 17, 13, 15, 1500);
        }

        private static ManualFeedSource<int> CreateSource()
            => new ManualFeedSource<int>(value => new ValueNode(value.ToString()));
    }
}
