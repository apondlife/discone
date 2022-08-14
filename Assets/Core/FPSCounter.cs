using UnityAtoms.BaseAtoms;
using UnityEngine;

/// calculates average fps
/// - todo: AverageFps/CurrentFps
public class FPSCounter: MonoBehaviour {
    // -- configs --
    [Header("config")]
    [Tooltip("the period over which to average fps")]
    [SerializeField] float m_LogPeriod;

    [Tooltip("the delta threshold before logging the fps")]
    [SerializeField] float m_LogThreshold;

    // -- refs --
    [Header("refs")]
    [Tooltip("the last updated fps")]
    [SerializeField] FloatVariable m_FPS;

    // -- props --
    /// the accumulated time this period
    float m_PeriodTime;

    /// the number of frames this period
    int m_PeriodFrames;
    /// the last logged fps
    float m_LastLoggedFps = 0.0f;

    // Update is called once per frame
    void Update() {
        // accumulate data this period
        m_PeriodTime += Time.deltaTime;
        m_PeriodFrames += 1;

        // if period completes, output data
        if (m_PeriodTime > m_LogPeriod) {
            // update the fps
            var fps = (float)m_PeriodFrames / m_PeriodTime;
            m_FPS.SetValue(fps);

            // log if significant
            if (Mathf.Abs(fps - m_LastLoggedFps) >= m_LogThreshold) {
                m_LastLoggedFps = fps;
                Debug.Log($"[report] {fps} fps over {m_PeriodFrames}");
            }

            // reset period
            m_PeriodTime = 0;
            m_PeriodFrames = 0;
        }
    }
}
