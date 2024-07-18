using BoingKit;
using UnityEngine;
using UnityEngine.Serialization;

namespace Discone {

/// the character's hair
class Hair: MonoBehaviour {
    // -- consts --
    const int k_MaxColliders = 10;

    // -- config --
    [Header("config")]
    [Tooltip("collider search range")]
    [SerializeField] float m_SearchRadius;

    [FormerlySerializedAs("m_LayerMask")]
    [Tooltip("the collision mask")]
    [SerializeField] LayerMask m_SearchMask;

    // -- refs --
    [Header("refs")]
    [Tooltip("the boingkit bones")]
    [SerializeField] BoingBones m_Bones;

    [Tooltip("the left hair root")]
    [SerializeField] Transform m_Left;

    [Tooltip("the right hair root")]
    [SerializeField] Transform m_Right;

    // -- props --
    readonly Collider[] m_Colliders = new Collider[k_MaxColliders];

    // -- lifecycle --
    void Awake() {
        m_Bones.UnityColliders = m_Colliders;

        if (!m_Bones.BoneChains[0].Root) {
            m_Bones.BoneChains[0].Root = m_Left;
        }

        if (!m_Bones.BoneChains[1].Root) {
            m_Bones.BoneChains[1].Root = m_Right;
        }
    }

    void FixedUpdate() {
        Physics.OverlapSphereNonAlloc(
            transform.position,
            m_SearchRadius,
            m_Colliders,
            m_SearchMask,
            QueryTriggerInteraction.Ignore
        );
    }
}

}