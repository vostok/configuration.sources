using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Manual;
using Vostok.Commons.Collections;
using Vostok.Commons.Formatting;
using System.Linq;
using Vostok.Commons.Helpers.Extensions;

namespace Vostok.Configuration.Sources.Object
{
    /// <summary>
    /// <para>A source which returns settings from the object provided by user.</para>
    /// <para>Object can contain primitive types, dictionaries, sequences and other nested</para>
    /// <para>objects as public fields and properties. Keys of dictionaries must be of</para>
    /// <para>primitive types, enums, strings or Guids. Nested objects should also satisfy</para>
    /// <para>conditions listed above. If any object explicitly overrides <see cref="object.ToString"/></para>
    /// <para>method then it will be called to provide value for ISettingsNode.</para>
    /// </summary>
    public class ObjectSource : ManualFeedSource<object>
    {
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

                if (CustomFormatters.TryFormat(item, out var customFormatting))
                    return new ValueNode(name, customFormatting);

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