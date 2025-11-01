using System;
using System.Collections.Generic;
using System.Linq;

namespace de.creinbold.SheetMusicBinder
{
    class Layout
    {
        private static decimal LexInverseLengthTieCompare(List<decimal> d1, List<decimal> d2)
        {
            var value = d1.Zip(d2, (e1, e2) => e1 - e2).FirstOrDefault(n => n != 0);
            if (value != 0) return value;
            return d2.Count - d1.Count;

        }

        public Layout PreviousLayout { get; private set; } = null;
        public AbsStrip CurrentStrip { get; private set; } = OneSidedStrip.EMPTY_STRIP;

        public int CurrentEnd { get; private set; }
        public int PageSpace { get; private set; }
        public int PaperCount { get; private set; }
        public List<decimal> MaxFlipDurations { get; private set; } = new List<decimal>();

        public int LastPageIndex { get { return CurrentStrip.HasPages ? CurrentStrip.LastPage : AbsStrip.EMPTY_PAGE; } }

        private Layout ExtendWithEmptyStrip(int length)
        {
            if (length == 0) return this;
            var l = new Layout();
            if (PaperCount > 0)
            {
                l.PreviousLayout = this;
                l.CurrentStrip = new OneSidedStrip(Enumerable.Repeat(AbsStrip.EMPTY_PAGE, length));
            }
            l.CurrentEnd = CurrentEnd + length;
            l.PageSpace = Math.Max(PageSpace, l.CurrentEnd);
            l.MaxFlipDurations = MaxFlipDurations;
            return l;
        }

        public Layout Extend(OneSidedStrip strip)
        {
            var l = new Layout();

            l.PreviousLayout = this;
            l.CurrentStrip = strip;
            l.CurrentEnd = CurrentEnd + strip.PaperCount;
            l.PageSpace = Math.Max(PageSpace, l.CurrentEnd);
            l.PaperCount = PaperCount + strip.PaperCount;
            l.MaxFlipDurations = MaxFlipDurations;
            return l;
        }

        public IEnumerable<Layout> Extend(DoubleSidedStrip strip)
        {
            var extendedMaxFlipDurations = new List<decimal>(MaxFlipDurations);
            // Sorted Insert, see List<T>.BinarySearch as reference.
            var idx = extendedMaxFlipDurations.BinarySearch(strip.MaxFlipDuration);
            if (idx < 0) idx = ~idx;
            else idx++;
            extendedMaxFlipDurations.Insert(idx, strip.MaxFlipDuration);

            // Try to introduce empty pages until the left end of the layout is not overshot anymore.
            for (int addedSpaces = 0; addedSpaces <= Math.Max(strip.PaperCount - CurrentEnd, 0); addedSpaces++)
            {
                var i_layout = this.ExtendWithEmptyStrip(addedSpaces);

                var l = new Layout();
                l.PreviousLayout = i_layout;
                l.CurrentStrip = strip;
                l.CurrentEnd = i_layout.CurrentEnd;
                l.PageSpace = Math.Max(i_layout.PageSpace, i_layout.CurrentEnd + strip.PaperCount);
                l.PaperCount = i_layout.PaperCount + strip.PaperCount;
                l.MaxFlipDurations = extendedMaxFlipDurations;

                // It may be the case that the double sided page is overshooting the left end of the current layout after flipping it over.
                // In this case, shift positions such that the minimal position of the layout is zero again.
                var lowerBound = l.CurrentEnd - strip.PaperCount;
                if (lowerBound < 0)
                {
                    l.CurrentEnd -= lowerBound;
                    l.PageSpace -= lowerBound;
                }
                yield return l;
            }
        }

        public IEnumerable<Paper> GetPapers()
        {
            int mark = PageWithMarkings.NO_MARK;
            return _RecGetPapers(ref mark, false, true);
        }

        private IEnumerable<Paper> _RecGetPapers(ref int currentMark, bool flipInLastPage, bool last)
        {
            var previousPapers = Enumerable.Empty<Paper>();
            if (PreviousLayout != null)
            {
                var doubleSidedStrip = CurrentStrip as DoubleSidedStrip;
                var flipInPreviousPage = (doubleSidedStrip != null && doubleSidedStrip.HasNoFrontPages && doubleSidedStrip.IsFlippedInPage);
                previousPapers = previousPapers.Concat(PreviousLayout._RecGetPapers(ref currentMark, flipInPreviousPage, false));
            }
            var currentPapers = CurrentStrip.GetPapers(ref currentMark, last, flipInLastPage);
            return previousPapers.Concat(currentPapers);
        }

        public bool IsPreferableInAnyFollowUpCaseTo(Layout other)
        {
            if (PageSpace > other.PageSpace) return false;
            if (CurrentEnd > other.CurrentEnd) return false;
            if (LastPageIndex < other.LastPageIndex) return false;
            return IsPreferableTo(other);
        }

        public bool IsPreferableTo(Layout other)
        {
            if (LastPageIndex != other.LastPageIndex) throw new ArgumentException("Can only compare layouts ending at the same page.");

            if (PaperCount < other.PaperCount) return true;
            return LexInverseLengthTieCompare(MaxFlipDurations, other.MaxFlipDurations) > 0;
        }

        // We use the unique string representation to identify identical layouts.

        public override bool Equals(object obj)
        {
            var item = obj as Layout;

            if (item == null)
            {
                return false;
            }

            return this.ToString().Equals(item.ToString());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            var s = "";
            if (PreviousLayout != null) s += PreviousLayout.ToString();
            if (s.Length > 0) s += " ";
            s += CurrentStrip.ToString();
            return s;
        }
    }
}
