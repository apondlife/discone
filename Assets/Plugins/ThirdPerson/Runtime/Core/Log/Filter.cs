namespace ThirdPerson {

static partial class Log {
    /// a log filter
    sealed class Filter {
        // -- props --
        /// the visible log level
        Level m_Level;

        /// a bitmask of the visible tags
        int m_Tags = 0;

        /// the next tag value
        int m_NextTag = 0;

        /// the names of any registered tags
        readonly string[] m_TagNames = new string[64];

        // -- commands --
        /// show logs at or below this level
        public void Show(Level level) {
            m_Level = level;
        }

        /// show logs with this tag
        public void ShowTag(int tag) {
            m_Tags |= 1 << tag;
        }

        /// hide logs with this tag
        public void HideTag(int tag) {
            m_Tags &= ~(1 << tag);
        }

        /// create a unique tag w/ the given name and shows it. note, this method is impure.
        public int CreateTag(string name, bool show = true) {
            // register the tag
            var tag = m_NextTag++;
            m_TagNames[tag] = name;

            // show the tag
            if (show) {
                ShowTag(tag);
            }

            return tag;
        }

        // -- queries --
        /// if the log is visible
        public bool IsVisible(Level level, int tag) {
            return level >= m_Level && (m_Tags & (1 << tag)) != 0;
        }

        /// finds the name for this tag
        public string FindTagName(int tag) {
            return m_TagNames[tag];
        }
    }
}

}