using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDynamo.Extensions
{
    public static class CollectionExtensions
    {
        public static string JoinByNewLine(this IEnumerable<string> source)
        {
            return string.Join(Environment.NewLine, source);
        }

        public static string JoinByNewLine<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return JoinByNewLine(source.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        }
    }
}
