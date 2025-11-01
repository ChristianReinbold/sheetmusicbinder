using System.Collections.Generic;
using System.Linq;

namespace de.creinbold.SheetMusicBinder
{
    abstract class AbsStrip
    {
        public static readonly int EMPTY_PAGE = 0;

        public abstract int PaperCount { get; }
        public abstract IEnumerable<int> ContainedPages { get; }

        public bool HasPages { get { return !ContainedPages.IsEmpty(); } }

        public int FirstPage { get { return ContainedPages.Min(); } }
        public int LastPage { get { return ContainedPages.Max(); } }

        public abstract IEnumerable<Paper> GetPapers(ref int currentMark, bool isLastStrip, bool flipLastInPage);
    }
}
