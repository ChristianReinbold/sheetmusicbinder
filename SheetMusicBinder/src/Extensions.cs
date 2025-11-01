using System.Collections.Generic;

namespace de.creinbold.SheetMusicBinder
{
    static class Extensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.GetEnumerator().MoveNext();
        }
    }
}
