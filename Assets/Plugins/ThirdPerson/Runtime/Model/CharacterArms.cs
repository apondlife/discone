using UnityEngine;

namespace ThirdPerson {

/// a pair of legs working in unison
class CharacterArms: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the left arm")]
    [SerializeField] Limb m_Left;

    [Tooltip("the right arm")]
    [SerializeField] Limb m_Right;

    // -- refs --
    [Header("refs")]
    [Tooltip("the attached model")]
    [SerializeField] Transform m_Model;

    // -- props --
    /// the character's dependency container
    CharacterContainer c;

    /// the initial position of the arm
    Vector3 m_InitialPos;

    /// the initial position of the model
    Vector3 m_InitialModelPos;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void Start() {
        m_InitialPos = transform.localPosition;
        m_InitialModelPos = m_Model.transform.localPosition;
    }

    void Update() {
        MoveArm(m_Left);
        MoveArm(m_Right);
    }

    // -- commands --
    void MoveArm(Limb arm) {

    }
}

}