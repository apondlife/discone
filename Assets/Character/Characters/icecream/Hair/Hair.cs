using BoingKit;
using Soil;
using ThirdPerson;
using UnityEngine;
using UnityEngine.Serialization;

namespace Discone {

/// the character's hair
class Hair: MonoBehaviour {
    // -- consts --
    /// the maximum number of nearby colliders to search for
    const int k_MaxColliders = 10;

    // -- config --
    [Header("config")]
    [Tooltip("collider search range")]
    [SerializeField] float m_SearchRadius;

    [FormerlySerializedAs("m_LayerMask")]
    [Tooltip("the collision mask")]
    [SerializeField] LayerMask m_SearchMask;

    [Header("config - full jump")]
    [Tooltip("the timer for a fully charged jump effect")]
    [SerializeField] EaseTimer m_FullJump;

    [Tooltip("the amplitude of the fully charged jump effect")]
    [SerializeField] float m_FullJump_Amplitude;

    [FormerlySerializedAs("m_FullJump_RightOffset")]
    [Tooltip("the right offset of the fully charged jump effect")]
    [SerializeField] float m_FullJump_Offset;

    [Tooltip("the target scale of the fully charged jump effect")]
    [SerializeField] float m_FullJump_TargetScale;

    [Tooltip("the scale ease of the fully charged jump effect")]
    [SerializeField] DynamicEase<float> m_FullJump_Scale;

    // -- refs --
    [Header("refs")]
    [Tooltip("the boingkit bones")]
    [SerializeField] BoingBones m_Bones;

    [Tooltip("the left hair root")]
    [SerializeField] Transform m_Left;

    [Tooltip("the right hair root")]
    [SerializeField] Transform m_Right;

    [Tooltip("the transform for scaling")]
    [SerializeField] Transform m_Scale;

    [Header("refs - effector")]
    [Tooltip("the hair effector parent")]
    [SerializeField] Transform m_Effectors;

    [Tooltip("the left hair effector")]
    [SerializeField] Transform m_EffectorLeft;

    [Tooltip("the right hair effector")]
    [SerializeField] Transform m_EffectorRight;

    // -- props --
    /// .
    CharacterContainer c;

    /// a buffer for colliders around the hair
    readonly Collider[] m_Colliders = new Collider[k_MaxColliders];

    /// the initial offset of the left effector
    Vector3 m_Effector_Left_InitialOffset;

    /// the initial offset of the right effector
    Vector3 m_Effector_Right_InitialOffset;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // configure bones
        m_Bones.UnityColliders = m_Colliders;

        if (!m_Bones.BoneChains[0].Root) {
            m_Bones.BoneChains[0].Root = m_Left;
        }

        if (!m_Bones.BoneChains[1].Root) {
            m_Bones.BoneChains[1].Root = m_Right;
        }

        // capture initial state
        m_FullJump_Scale.Init(1f);
        m_Effector_Left_InitialOffset = m_EffectorLeft.localPosition;
        m_Effector_Right_InitialOffset = m_EffectorRight.localPosition;
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // get nearby colliders
        Physics.OverlapSphereNonAlloc(
            transform.position,
            m_SearchRadius,
            m_Colliders,
            m_SearchMask,
            QueryTriggerInteraction.Ignore
        );


        // if the jump is not fully charged, stop the timer
        if (c.State.NextJumpPower < 1f) {
            m_FullJump.Stop();
            m_Effectors.gameObject.SetActive(false);
        }
        // if the jump is fully charged, start effect
        else if (m_FullJump.IsInactive) {
            m_FullJump.Play();
            m_Effectors.gameObject.SetActive(true);
        }

        var targetScale = 1f;
        if (m_FullJump.TryTick()) {
            // change target scale
            targetScale = m_FullJump_TargetScale;

            // oscillate the effector vertically
            var scale = Mathf.PingPong(m_FullJump.Pct * 2f, 1f) * 2f - 1f;
            scale *= m_FullJump_Amplitude;

            var trs = m_Effectors.transform;
            var up = trs.right;

            var dy = trs.localRotation * (scale * up);
            var dx = m_FullJump_Offset * Vector3.right;

            m_EffectorLeft.localPosition = m_Effector_Left_InitialOffset - dx + dy;
            m_EffectorRight.localPosition = m_Effector_Right_InitialOffset + dx - dy;
        }

        m_FullJump_Scale.Update(delta, targetScale);
        m_Scale.localScale = m_FullJump_Scale.Value * Vector3.one;
    }
}

}