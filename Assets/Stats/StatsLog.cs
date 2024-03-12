using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

/// periodically logs stats
sealed class StatsLog: MonoBehaviour {
    // -- types --
    /// a log record
    private struct Record {
        /// the time of the log
        public float Time;

        /// the fps
        public float Fps;

        /// the player count
        public float PlayerCount;
    }

    // -- configs --
    [Header("config")]
    [Tooltip("the amount of time between change logs")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_SampleInterval")]
    [SerializeField] float m_Interval;

    [Tooltip("the maximum amount of time between logs")]
    [SerializeField] float m_MaxInterval;

    [Tooltip("the change if fps requiured before logging")]
    [SerializeField] float m_FpsDeltaThreshold;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current fps")]
    [SerializeField] FloatReference m_Fps;

    [Tooltip("the current player count")]
    [SerializeField] IntReference m_PlayerCount;

    // -- props --
    /// the accumulated time this period
    float m_Period = 0.0f;

    /// the accumulated time since last log
    float m_MaxPeriod = 0.0f;

    /// the record of the last log
    Record m_Last;

    // -- lifecycle --
    void Update() {
        // accumulate time since last log
        m_MaxPeriod += Time.deltaTime;

        // wait until period is complete
        m_Period += Time.deltaTime;
        if (m_Period < m_Interval) {
            return;
        }

        // if the maximum time elaspsed, log
        var isChanged = m_MaxPeriod >= m_MaxInterval;

        // otherwise, see if data changed significantly
        if (!isChanged && m_PlayerCount != m_Last.PlayerCount) {
            isChanged = true;
        }

        if (!isChanged && Mathf.Abs(m_Fps - m_Last.Fps) >= m_FpsDeltaThreshold) {
            isChanged = true;
        }

        // if it did, log the change
        if (isChanged) {
            // update record
            m_Last.Fps = m_Fps;
            m_Last.PlayerCount = m_PlayerCount;

            // print the log
            Log.Online.I($"<{(int)(Time.time % 100.0f)}> fps: {(int)m_Last.Fps} players: {m_Last.PlayerCount}");

            // reset time since last lot
            m_MaxPeriod = 0.0f;
        }

        // and reset period
        m_Period = 0.0f;
    }
}

}