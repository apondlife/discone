using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

[ExecuteAlways]
sealed class DemoRunningView: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("if we are currently running the demo")]
    [SerializeField] BoolVariable m_IsRunning;

    [Tooltip("the title text")]
    [SerializeField] GameObject m_Title;

    // -- lifecycle --
    void Update() {
        m_Title.SetActive(m_IsRunning.Value);
    }
}

}