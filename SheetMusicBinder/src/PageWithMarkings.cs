namespace de.creinbold.SheetMusicBinder
{
    struct PageWithMarkings
    {
        public static readonly int NO_MARK = 0;

        public int Index;
        public int LeftMarkIndex;
        public int RightMarkIndex;
        public bool FlipInPage;
        public bool FlipCue;
    }
}
