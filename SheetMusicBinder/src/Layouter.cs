using System;
using System.Collections.Generic;
using System.Linq;

namespace de.creinbold.SheetMusicBinder
{
    class Layouter
    {
        private List<HashSet<Layout>> _LayoutsUpTo = new List<HashSet<Layout>>();
        private int _PageSpace;

        private List<decimal> _TopFlips = new List<decimal>();
        private List<decimal> _MiddleFlips = new List<decimal>();
        private List<decimal> _BottomFlips = new List<decimal>();

        public Layouter(int pageSpace)
        {
            _PageSpace = pageSpace;
            // Add default case of an empty layout.
            var default_list = new HashSet<Layout>();
            default_list.Add(new Layout());
            _LayoutsUpTo.Add(default_list);
            _TopFlips.Add(0M);
            _MiddleFlips.Add(0M);
            _BottomFlips.Add(0M);
        }

        public void AddPage(decimal flipDurationTop, decimal flipDurationMiddle, decimal flipDurationBottom)
        {
            _TopFlips.Add(flipDurationTop);
            _MiddleFlips.Add(flipDurationMiddle);
            _BottomFlips.Add(flipDurationBottom);
            _ExtendLayouts();
        }

        public IEnumerable<Layout> GetOptimalLayouts()
        {
            var possibleLayouts = _LayoutsUpTo[_LayoutsUpTo.Count - 1];
            return possibleLayouts.Where(l => !possibleLayouts.Any(l2 => l2.IsPreferableTo(l)));
        }

        private IEnumerable<Layout> _GetDoubleSidedExtensions(Layout base_layout, int firstNewPage, int lastNewPage, bool inPageFlip)
        {
            var lastValidFrontPage = inPageFlip ? lastNewPage : lastNewPage - 1;
            for (int lastFrontPage = Math.Max(firstNewPage - 1, 0); lastFrontPage <= lastValidFrontPage; lastFrontPage++)
            {
                var maxFlipDuration = inPageFlip ? _MiddleFlips[lastFrontPage] : (_BottomFlips[lastFrontPage] + _TopFlips[lastFrontPage + 1]);
                if (maxFlipDuration <= 0) continue;

                var firstBackPage = inPageFlip ? lastFrontPage : (lastFrontPage + 1);

                var frontCount = lastFrontPage - firstNewPage + 1;
                var backCount = lastNewPage - firstBackPage + 1;
                var stripLength = Math.Max(frontCount, backCount);

                // Longer strips require longer to flip, hence it is penalized more by
                // dividing maxFlipDuration (which is assumed for single paper strip flips)
                var doubleSidedExt = new DoubleSidedStrip(Enumerable.Range(firstNewPage, frontCount),
                                                          Enumerable.Range(firstBackPage, backCount),
                                                          maxFlipDuration / stripLength, inPageFlip);
                foreach (var layout in base_layout.Extend(doubleSidedExt)) yield return layout;
            }
        }

        private void _ExtendLayouts()
        {
            var lastNewPage = _LayoutsUpTo.Count;
            var newLayouts = new List<Layout>();

            for (int addCount = 1; addCount <= Math.Min(_PageSpace, lastNewPage); addCount++)
            {
                var firstNewPage = lastNewPage - addCount + 1;
                foreach (var layout in _LayoutsUpTo[firstNewPage - 1])
                {
                    // Add one sided strip
                    var oneSidedExt = new OneSidedStrip(Enumerable.Range(firstNewPage, addCount));
                    var newLayout = layout.Extend(oneSidedExt);
                    newLayouts.Add(newLayout);
                    newLayouts.AddRange(_GetDoubleSidedExtensions(layout, firstNewPage, lastNewPage, false));
                    newLayouts.AddRange(_GetDoubleSidedExtensions(layout, firstNewPage, lastNewPage, true));
                }
            }
            var noDuplicates = new HashSet<Layout>(newLayouts);
            var validLayouts = noDuplicates.Where(l => l.PageSpace <= _PageSpace).ToList();
            var preferableLayouts = validLayouts.Where(l => !validLayouts.Any(l2 => l2.IsPreferableInAnyFollowUpCaseTo(l)));
            _LayoutsUpTo.Add(new HashSet<Layout>(preferableLayouts));
        }
    }
}
