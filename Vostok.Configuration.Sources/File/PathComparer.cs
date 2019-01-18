using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Vostok.Configuration.Sources.File
{
    internal class PathComparer : IEqualityComparer<string>
    {
        public static readonly PathComparer Instance = new PathComparer();

        private static readonly IEqualityComparer<string> NormalizedPathComparer = ChoosePathComparer();

        public bool Equals(string x, string y)
        {
            if (x == null || y == null)
                return x == null && y == null;

            return NormalizedPathComparer.Equals(Path.GetFullPath(x), Path.GetFullPath(y));
        }

        public int GetHashCode(string obj) => NormalizedPathComparer.GetHashCode(Path.GetFullPath(obj));

        private static IEqualityComparer<string> ChoosePathComparer() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }
}