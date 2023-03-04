using U = UnityEngine;

namespace ThirdPerson {

static partial class Log {
    /// a logger than outputs to the unity debug console
    sealed class Logger {
        // -- statics --
        /// a static reference to the logger
        readonly static U.ILogger s_Log = U.Debug.unityLogger;

        // -- deps --
        /// the log filter
        Filter m_Filter;

        // -- lifetime --
        /// create a new logger
        public Logger(Filter filter) {
            m_Filter = filter;
        }

        // -- commands --
        /// log a message for the given level and tag
        public void Add(Level level, int tag, string msg) {
            // if this log is visible
            if (!m_Filter.IsVisible(level, tag)) {
                return;
            }

            // determine log type
            var t = U.LogType.Log;
            switch (level) {
                case Level.Error:
                    t = U.LogType.Error; break;
                case Level.Warning:
                    t = U.LogType.Warning; break;
            }

            // format and log message
            s_Log.Log(
                t,
                $"[{m_Filter.FindTagName(tag)}] {msg}"
            );
        }
    }
}

}