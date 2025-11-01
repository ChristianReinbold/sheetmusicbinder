using System;
using System.Collections.Generic;
using System.Linq;

namespace de.creinbold.SheetMusicBinder
{
    class OneSidedStrip : AbsStrip
    {
        private List<int> _Pages;

        public static OneSidedStrip EMPTY_STRIP = new OneSidedStrip(Enumerable.Empty<int>());

        public OneSidedStrip(IEnumerable<int> pages)
        {
            _Pages = pages.ToList();
        }

        public override IEnumerable<int> ContainedPages { get { return _Pages.Where(p => p != EMPTY_PAGE); } }

        public override int PaperCount { get { return _Pages.Count; } }

        public override IEnumerable<Paper> GetPapers(ref int currentMark, bool isLastStrip, bool flipLastInPage)
        {
            if (PaperCount == 0) return Enumerable.Empty<Paper>();

            var papers = new Paper[PaperCount];
            for (int i = 0; i < PaperCount; i++)
            {
                papers[i].FrontPage.Index = _Pages[i];
                papers[i].FrontPage.LeftMarkIndex = currentMark;
                papers[i].FrontPage.RightMarkIndex = ++currentMark;
                papers[i].BackPage.Index = EMPTY_PAGE;
                papers[i].BackPage.LeftMarkIndex = PageWithMarkings.NO_MARK;
                papers[i].BackPage.RightMarkIndex = PageWithMarkings.NO_MARK;
            }
            if (isLastStrip) papers[PaperCount - 1].FrontPage.RightMarkIndex = PageWithMarkings.NO_MARK;
            if (flipLastInPage) papers[PaperCount - 1].FrontPage.FlipInPage = true;
            return papers;
        }

        public override string ToString()
        {
            return String.Join(" ", _Pages);
        }
    }
}
