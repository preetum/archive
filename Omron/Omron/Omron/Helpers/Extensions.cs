using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron.Helpers
{
    public static class Extensions
    {
        public static List<T> CloneToList<T>(this IEnumerable<T> source)
        {
            lock (source)
            {
                return source.ToArray().ToList();
            }
        }
    }
}
