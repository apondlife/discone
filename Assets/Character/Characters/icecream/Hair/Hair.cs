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

    // -- props --
    readonly Collider[] m_Colliders = new Collider[k_MaxColliders];

    // -- lifecycle --
    void Awake() {
        m_Bones.UnityColliders = m_Colliders;
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