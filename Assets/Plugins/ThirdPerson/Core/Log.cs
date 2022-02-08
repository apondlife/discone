using System;
using System.Diagnostics;
using UnityEngine;
using U = UnityEngine;

namespace ThirdPerson {

/// a static interface to get a shared logger
static class Log {
    // -- types --
    /// a mapping of log level to `UnityEngine.LogType`
    public enum Level {
        Debug     = 420,
        Info      = U.LogType.Log,
        Warning   = U.LogType.Warning,
        Assert    = U.LogType.Assert,
        Error     = U.LogType.Error,
        Exception = U.LogType.Exception
    }

    // -- constants --
    // a logging tag
    readonly static string k_Tag = ":ThirdPerson";

    // -- statics --
    /// the current log level
    static Level s_Level = Level.Debug;

    /// a static reference to the logger
    static U.ILogger s_Log = U.Debug.unityLogger;

    // -- commands --
    [Conditional("UNITY_EDITOR")]
    public static void Init(Level level) {
        #if UNITY_EDITOR
            // set initial log level
            Filter(level);
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void Filter(Level level) {
        #if UNITY_EDITOR
            s_Level = level;
            s_Log.filterLogType = level == Level.Debug ? LogType.Log : (LogType)level;
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void D(string msg) {
        #if UNITY_EDITOR
            if (s_Level == Level.Debug) {
                s_Log.Log((U.LogType)Level.Info, k_Tag, msg);
            }
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void I(string msg) {
        #if UNITY_EDITOR
            s_Log.Log((U.LogType)Level.Info, k_Tag, msg);
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void W(string msg) {
        #if UNITY_EDITOR
            s_Log.Log((U.LogType)Level.Warning, k_Tag, msg);
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void A(string msg) {
        #if UNITY_EDITOR
            s_Log.Log((U.LogType)Level.Assert, k_Tag, msg);
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void E(string msg) {
        #if UNITY_EDITOR
            s_Log.Log((U.LogType)Level.Error, k_Tag, msg);
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void X(string msg) {
        #if UNITY_EDITOR
            s_Log.Log((U.LogType)Level.Exception, k_Tag, msg);
        #endif
    }
}

}