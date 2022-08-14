using UnityAtoms.BaseAtoms;
using UnityEngine;

/// the average fps
sealed class StatsFps: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the fps from last period")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_FPS")]
    [SerializeField] FloatVariable m_Fps;

    // -- configs --
    [Header("config")]
    [Tooltip("the amount of time between calculations")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_LogPeriod")]
    [SerializeField] float m_Interval;

    // -- props --
    /// the accumulated time this period
    float m_Period;

    /// the number of frames this period
    int m_Frames;

    // Update is called once per frame
    void Update() {
        // accumulate data
        m_Frames += 1;

        // wait until period is complete
        m_Period += Time.deltaTime;
        if (m_Period < m_Interval) {
            return;
        }

        // update fps
        m_Fps.Value = (float)m_Frames / m_Period;;

        // reset period
        m_Period = 0.0f;
        m_Frames = 0;
    }
}