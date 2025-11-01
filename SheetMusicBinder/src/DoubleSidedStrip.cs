using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace de.creinbold.SheetMusicBinder
{
    class DoubleSidedStrip : AbsStrip
    {

        private List<int> _Front;
        private List<int> _Back;

        private int _PaperCount;
        public decimal MaxFlipDuration { get; private set; }
        public bool IsFlippedInPage { get; private set; }

        public override IEnumerable<int> ContainedPages { get { return _Front.Concat(_Back); } }
        public override int PaperCount { get { return _PaperCount; } }
        public bool HasNoFrontPages { get { return _Front.IsEmpty(); } }

        public DoubleSidedStrip(IEnumerable<int> front, IEnumerable<int> back, decimal maxFlipDuration, bool inPageFlip)
        {
            _Front = front.ToList();
            _Back = back.ToList();
            Debug.Assert(!_Back.IsEmpty());
            _PaperCount = Math.Max(_Front.Count, _Back.Count);
            MaxFlipDuration = maxFlipDuration;
            IsFlippedInPage = inPageFlip;
        }

        public override IEnumerable<Paper> GetPapers(ref int currentMark, bool isLastStrip, bool flipLastInPage)
        {
            if (PaperCount == 0) return Enumerable.Empty<Paper>();

            var backReversed = new List<int>(_Back);
            backReversed.Reverse();
            var papers = new Paper[PaperCount];
            for (int i = 0; i < PaperCount; i++)
            {
                papers[i].FrontPage.Index = i < _Front.Count ? _Front[i] : EMPTY_PAGE;
                papers[i].FrontPage.LeftMarkIndex = currentMark;
                papers[i].FrontPage.RightMarkIndex = ++currentMark;
            }
            // Decrement as we will remove the marking at the last page of the strip again.
            currentMark--;
            for (int i = PaperCount - 1; i >= 0; i--)
            {
                papers[i].BackPage.Index = i < backReversed.Count ? backReversed[i] : EMPTY_PAGE;
                papers[i].BackPage.LeftMarkIndex = currentMark;
                papers[i].BackPage.RightMarkIndex = ++currentMark;

            }

            // Fix markings of the last page of the strip
            papers[PaperCount - 1].FrontPage.FlipCue = true;
            papers[PaperCount - 1].FrontPage.RightMarkIndex = PageWithMarkings.NO_MARK;
            papers[PaperCount - 1].BackPage.LeftMarkIndex = PageWithMarkings.NO_MARK;
            if (isLastStrip) papers[0].BackPage.RightMarkIndex = PageWithMarkings.NO_MARK;

            // If the last front page is repeated, set according value indicating this.
            if (_Front.Count > 0 && IsFlippedInPage) papers[_Front.Count - 1].FrontPage.FlipInPage = true;

            // If the last back page is also flipped in page, mark this
            if (flipLastInPage) papers[PaperCount - _Back.Count].BackPage.FlipInPage = true;

            return papers;
        }

        public override string ToString()
        {
            var frontWithEmptyPages = _Front.Concat(Enumerable.Repeat(EMPTY_PAGE, PaperCount - _Front.Count));
            var backWithEmptyPages = _Back.Concat(Enumerable.Repeat(EMPTY_PAGE, PaperCount - _Back.Count));
            return "[ " + String.Join(" ", frontWithEmptyPages) + " | " + String.Join(" ", backWithEmptyPages) + " ] ";
        }
    }
}
