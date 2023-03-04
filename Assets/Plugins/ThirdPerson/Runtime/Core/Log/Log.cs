using System.Diagnostics;

namespace ThirdPerson {

/// a static interface to a shared logger
static partial class Log {
    // -- deps --
    /// the log filter
    static Filter m_Filter = new Filter();

    /// the logger
    static Logger m_Log = new Logger(m_Filter);

    // -- constants --
    /// the default logging tag
    static readonly int k_DefaultTag = Tag("");

    // -- commands --
    /// set the log level
    [Conditional("UNITY_EDITOR")]
    public static void Show(Level level) {
        #if UNITY_EDITOR
        m_Filter.Show(level);
        #endif
    }

    /// show logs with this tag
    [Conditional("UNITY_EDITOR")]
    public static void ShowTag(int tag) {
        #if UNITY_EDITOR
        m_Filter.ShowTag(tag);
        #endif
    }

    /// hide logs with this tag
    [Conditional("UNITY_EDITOR")]
    public static void HideTag(int tag) {
        #if UNITY_EDITOR
        m_Filter.HideTag(tag);
        #endif
    }

    /// registers a unique tag w/ the given name and shows it. note, this method is impure.
    public static int Tag(string name, bool show = true) {
        return m_Filter.CreateTag(name, show);
    }

    /// -- c/logging
    /// log a debug message
    [Conditional("UNITY_EDITOR")]
    public static void D(string msg) {
        #if UNITY_EDITOR
        D(k_DefaultTag, msg);
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    /// log a tagged debug message
    public static void D(int tag, string msg) {
        #if UNITY_EDITOR
        m_Log.Add(Level.Debug, tag, msg);
        #endif
    }

    /// log an info message
    [Conditional("UNITY_EDITOR")]
    public static void I(string msg) {
        #if UNITY_EDITOR
        I(k_DefaultTag, msg);
        #endif
    }

    /// log a tagged info message
    [Conditional("UNITY_EDITOR")]
    public static void I(int tag, string msg) {
        #if UNITY_EDITOR
        m_Log.Add(Level.Info, tag, msg);
        #endif
    }

    /// log a warning
    [Conditional("UNITY_EDITOR")]
    public static void W(string msg) {
        #if UNITY_EDITOR
        W(k_DefaultTag, msg);
        #endif
    }

    /// log a tagged warning
    [Conditional("UNITY_EDITOR")]
    public static void W(int tag, string msg) {
        #if UNITY_EDITOR
        m_Log.Add(Level.Warning, tag, msg);
        #endif
    }

    /// log an error
    [Conditional("UNITY_EDITOR")]
    public static void E(string msg) {
        #if UNITY_EDITOR
        E(k_DefaultTag, msg);
        #endif
    }

    /// log a tagged error
    [Conditional("UNITY_EDITOR")]
    public static void E(int tag, string msg) {
        #if UNITY_EDITOR
        m_Log.Add(Level.Error, tag, msg);
        #endif
    }
}

}