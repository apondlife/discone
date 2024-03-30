using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public class CharacterRig: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the rotation speed in degrees towards look direction")]
    [SerializeField] float m_LookRotation_Speed;

    // -- props --
    /// the containing character
    CharacterContainer c;

    /// the character's animator
    Animator m_Animator;

    /// the list of ik limbs
    CharacterPart[] m_Limbs;

    /// the stored look rotation
    Quaternion m_LookRotation = Quaternion.identity;

    // -- lifecycle --
    void Start() {
        // set dependencies
        c = GetComponentInParent<CharacterContainer>();

        // set props
        m_Limbs = GetComponentsInChildren<CharacterPart>();

        // init animator
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator != null) {
            // init ik limbs
            foreach (var limb in m_Limbs) {
                limb.Init(m_Animator);
            }

            // proxy animator callbacks
            var proxy = m_Animator.gameObject.GetComponent<CharacterAnimatorProxy>();
            if (proxy == null) {
                proxy = m_Animator.gameObject.AddComponent<CharacterAnimatorProxy>();
            }

            proxy.Bind(OnAnimatorIK);
        } else {
            // destroy ik limbs
            Log.Model.W($"character {c.Name} has no animator, destroying limbs");
            foreach (var limb in m_Limbs) {
                Destroy(limb.gameObject);
            }
        }
    }

    void Update() {
        m_LookRotation = Quaternion.RotateTowards(
            m_LookRotation,
            c.State.Curr.LookRotation,
            m_LookRotation_Speed * Time.deltaTime
        );

        transform.localRotation =  m_LookRotation;
    }

    // -- commands --
    /// a callback for calculating IK
    void OnAnimatorIK(int layer) {
        foreach (var limb in m_Limbs) {
            limb.ApplyIk();
        }
    }
}

}