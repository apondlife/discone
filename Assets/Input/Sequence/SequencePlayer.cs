using UnityEngine;
using ThirdPerson;

namespace Discone {

public class SequencePlayer : MonoBehaviour {
    [Header("refs")]
    [SerializeField] Character m_Target;

    [Header("config")]
    [SerializeField] SequenceInputSource m_SequenceSource;

    // -- lifetime --
    private void Awake() {
        if (m_Target != null) {
            Drive(m_Target);
        }
    }

    // -- commands --
    public void Drive(Character target) {
        m_Target = target;
        target.Drive(m_SequenceSource);
    }
}

}