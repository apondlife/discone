using UnityEngine;
using UnityAtoms.BaseAtoms;

/// the sky-based navigation system
[ExecuteAlways]
class SkyChart: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the target player")]
    [SerializeField] GameObjectVariable m_Target;

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
        // reposition chart on target
        var pos = m_Target.Value.transform.position;
        transform.position = pos;

        // move bodies into position
        foreach (var body in m_Bodies) {
            body.Reposition(pos, m_Radius);
        }
    }
}