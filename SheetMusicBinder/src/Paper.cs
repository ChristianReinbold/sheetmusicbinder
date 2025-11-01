using System.Collections;
using System.Collections.Generic;

namespace de.creinbold.SheetMusicBinder
{
    struct Paper : IEnumerable<PageWithMarkings>
    {
        public PageWithMarkings FrontPage;
        public PageWithMarkings BackPage;
        public IEnumerable<PageWithMarkings> Pages { get { yield return FrontPage; yield return BackPage; } }

        public IEnumerator<PageWithMarkings> GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

    }
}
