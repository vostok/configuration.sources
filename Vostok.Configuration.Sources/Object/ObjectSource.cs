using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Collections;
using Vostok.Commons.Formatting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Manual;

namespace Vostok.Configuration.Sources.Object
{
    /// <summary>
    /// <para>A source which returns settings from the object provided by user.</para>
    /// <para>Object can contain primitive types, dictionaries, sequences and other nested objects as public fields and properties.
    /// Keys of dictionaries must be of primitive types, enums, strings or guids. Nested objects should also satisfy conditions listed above.
    /// If any object explicitly overrides <see cref="object.ToString"/> method then it's result will be used as value for ISettingsNode.
    /// When passing null into a constructor, the null should be explicitly type cast. For example:
    /// <list type="bullet">
    ///     <item>var source = new ObjectSource((CustomObject) null);</item>
    ///     <item>var source = new ObjectSource((<see cref="ObjectSourceSettings"/>) null);</item>
    /// </list>
    /// See also <see cref="ObjectSourceSettings"/>.</para>
    /// </summary>
    [PublicAPI]
    public class ObjectSource : ManualFeedSource<object>
    {
        public ObjectSource([CanBeNull] ObjectSourceSettings settings = null)
            : base(obj => Parse(obj, settings ?? new ObjectSourceSettings()))
        {
        }

        public ObjectSource([CanBeNull] object source, [CanBeNull] ObjectSourceSettings settings = null)
            : this(settings)
        {
            Push(source);
        }

        private static ISettingsNode Parse([CanBeNull] object obj, [NotNull] ObjectSourceSettings settings)
        {
            if (obj == null)
                return null;
            return ParseObject(null, obj, new HashSet<object>(ByReferenceEqualityComparer<object>.Instance), settings);
        }

        private static ISettingsNode ParseObject([CanBeNull] string name, [CanBeNull] object item, [NotNull] HashSet<object> path, [NotNull] ObjectSourceSettings settings)
        {
            if (item == null)
                return new ValueNode(name, null);

            if (!path.Add(item))
                throw new ArgumentException("Object has cyclic dependency.");

            try
            {
                var itemType = item.GetType();

                if (CustomFormatters.TryFormat(item, out var customFormatting))
                    return new ValueNode(name, customFormatting);
                
                if (ShouldUseCustomToString(itemType))
                    return new ValueNode(name, item.ToString());

                if (DictionaryInspector.IsSimpleDictionary(itemType))
                    return ParseDictionary(name, DictionaryInspector.EnumerateSimpleDictionary(item), path, settings);

                if (item is IEnumerable sequence)
                    return ParseEnumerable(name, sequence, path, settings);

                var fieldsAndProperties = new List<ISettingsNode>();

                foreach (var field in itemType.GetInstanceFields())
                {
                    var fieldValue = field.GetValue(item);
                    if (fieldValue != null || !settings.IgnoreFieldsWithNullValue)
                        fieldsAndProperties.Add(ParseObject(field.Name, fieldValue, path, settings));
                }

                foreach (var property in itemType.GetInstanceProperties())
                {
                    var propertyValue = property.GetValue(item);
                    if (propertyValue != null || !settings.IgnoreFieldsWithNullValue)
                        fieldsAndProperties.Add(ParseObject(property.Name, propertyValue, path, settings));
                }

                return new ObjectNode(name, fieldsAndProperties);
            }
            finally
            {
                path.Remove(item);
            }
        }

        private static ArrayNode ParseEnumerable(string name, IEnumerable sequence, HashSet<object> path, ObjectSourceSettings settings)
        {
            var items = new List<ISettingsNode>();
            foreach (var element in sequence)
                items.Add(ParseObject(null, element, path, settings));
            return new ArrayNode(name, items);
        }

        private static ObjectNode ParseDictionary(string name, IEnumerable<(string, object)> pairs, HashSet<object> path, ObjectSourceSettings settings)
        {
            var tokens = pairs.Select(pair => ParseObject(pair.Item1, pair.Item2, path, settings)).ToArray();
            return new ObjectNode(name, tokens);
        }
        
        private static bool ShouldUseCustomToString(Type itemType) =>
            ParseMethodFinder.HasAnyKindOfParseMethod(itemType) && ToStringDetector.HasCustomToString(itemType);
    }
}