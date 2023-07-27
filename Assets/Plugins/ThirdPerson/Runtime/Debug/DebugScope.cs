using UnityEngine;
using Unity.Profiling;
using System.Collections.Generic;

namespace  ThirdPerson {

/// a debug utility for adding values to the profiler
public class DebugScope: MonoBehaviour {
    // -- static --
    /// the singleton instance
    static DebugScope s_Instance;

    // -- props --
    /// the list of values
    Dictionary<string, Scope> m_Scopes = new();

    // -- lifecycle --
    void Awake() {
        s_Instance = this;
    }

    void Update() {
        foreach(var (_, scope) in m_Scopes) {
            scope.Flush();
        }
    }

    // -- commands --
    /// set the scope's current value
    public static void Push(string name, float value) {
        s_Instance.PushNext(name, value);
    }

    /// set the scope's current value
    public static void Push(string name, int value) {
        s_Instance.PushNext(name, value);
    }

    /// set the scope's current value
    public static void Push(string name, bool value) {
        s_Instance.PushNext(name, value ? 1f : 0f);
    }

    /// set the scope's current value
    void PushNext(string name, float value) {
        // find or create the scope
        if (!m_Scopes.TryGetValue(name, out var scope)) {
            scope = new Scope(new ProfilerCounterValue<float>(
                ProfilerCategory.Scripts,
                name,
                ProfilerMarkerDataUnit.Count,
                ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush
            ));

            m_Scopes.Add(name, scope);
        }

        // update the current value
        scope.Set(value);
    }

    // -- data --
    /// a scope that memoizes its current value
    record Scope {
        // -- props --
        /// the current value
        float Curr;

        /// the profiler value
        ProfilerCounterValue<float> Counter;

        // -- lifetime --
        public Scope(ProfilerCounterValue<float> Counter) {
            this.Counter = Counter;
        }

        // -- commands --
        /// .
        public void Set(float value) {
            Curr = value;
            Flush();
        }

        /// update the profiler value
        public void Flush() {
            Counter.Value = Curr;
        }
    }
}

}