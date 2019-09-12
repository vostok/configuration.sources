using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Manual;
using Vostok.Commons.Collections;
using Vostok.Configuration.Extensions;
using Vostok.Commons.Formatting;
using System.Linq;
using System.Text;

namespace Vostok.Configuration.Sources.Object
{
    /// <summary>
    /// <para>A source which returns settings from the POCO object provided by user.</para>
    /// <para>POCO object should contain simple properties and fields like dictionaries, </para>
    /// <para>lists, arrays and other POCO objects. Keys of dictionaries should be of </para>
    /// <para>primitive types, enums, strings or Guids.</para>
    /// </summary>
    public class ObjectSource : ManualFeedSource<object>
    {
        private static readonly Dictionary<Type, Func<object, string>> CustomFormatters
            = new Dictionary<Type, Func<object, string>>
            {
                [typeof(Encoding)] = value => ((Encoding)value).WebName
            };

        public ObjectSource()
            : base(obj => ParseObject(null, obj, new HashSet<object>(ByReferenceEqualityComparer<object>.Instance)))
        {
        }

        public ObjectSource([CanBeNull] object source)
            : this()
        {
            Push(source);
        }

        private static ISettingsNode ParseObject([CanBeNull] string name, [CanBeNull] object item, [NotNull] HashSet<object> path)
        {
            if (item == null)
                return null;

            if (!path.Add(item))
                throw new ArgumentException("Object has cyclic dependency.");

            try
            {
                var itemType = item.GetType();

                if (ToStringDetector.HasCustomToString(itemType))
                    return new ValueNode(name, item.ToString());

                foreach (var pair in CustomFormatters)
                {
                    if (pair.Key.IsAssignableFrom(itemType))
                        return new ValueNode(name, pair.Value(item));
                }

                if (DictionaryInspector.IsSimpleDictionary(itemType))
                    return ParseDictionary(name, DictionaryInspector.EnumerateSimpleDictionary(item), path);

                if (item is IEnumerable sequence)
                    return ParseEnumerable(name, sequence, path);

                var fieldsAndProperties = new List<ISettingsNode>();

                foreach (var field in itemType.GetInstanceFields())
                    fieldsAndProperties.Add(ParseObject(field.Name, field.GetValue(item), path));

                foreach (var property in itemType.GetInstanceProperties())
                    fieldsAndProperties.Add(ParseObject(property.Name, property.GetValue(item), path));

                return new ObjectNode(name, fieldsAndProperties);
            }
            finally
            {
                path.Remove(item);
            }
        }

        private static ArrayNode ParseEnumerable(string name, IEnumerable sequence, HashSet<object> path)
        {
            var items = new List<ISettingsNode>();
            foreach (var element in sequence)
                items.Add(ParseObject(null, element, path));
            return new ArrayNode(name, items);
        }

        private static ObjectNode ParseDictionary(string name, IEnumerable<(string, object)> pairs, HashSet<object> path)
        {
            var tokens = pairs.Select(pair => ParseObject(pair.Item1, pair.Item2, path)).ToArray();
            return new ObjectNode(name, tokens);
        }
    }
}