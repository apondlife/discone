using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

/// the sky-based navigation system
[ExecuteAlways]
class SkyChart: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the target player")]
    [SerializeField] DisconePlayerVariable m_Target;

    [Tooltip("the radius of the chart")]
    [SerializeField] FloatReference m_Radius;

    // -- props --
    /// the list of celestial bodies in the sky
    SkyChartBody[] m_Bodies;

    // -- lifeycle --
    void Awake() {
        // set props
        m_Bodies = GetComponentsInChildren<SkyChartBody>();
    }

    void FixedUpdate() {
        var target = m_Target.Value;
        if (target == null) {
            Debug.Log($"the target is null!");
            return;
        }

        // reposition chart on target
        var pos = target.transform.position;
        transform.position = pos;

        // move bodies into position
        foreach (var body in m_Bodies) {
            body.Reposition(pos, 0.9f*m_Radius);
        }
    }
}