using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.Object
{
    /// <summary>
    /// Settings for <see cref="ObjectSource"/>.
    /// </summary>
    [PublicAPI]
    public class ObjectSourceSettings
    {
        /// <summary>
        /// If set, <see cref="ObjectSource"/> does include fields and properties with null value in settings tree.
        /// </summary>
        public bool IncludeFieldsWithNullValue { get; set; }
    }
}
