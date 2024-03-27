using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public class CharacterRig: MonoBehaviour {
    // -- props --
    /// the containing character
    CharacterContainer c;

    /// the character's animator
    Animator m_Animator;

    /// the list of ik limbs
    CharacterPart[] m_Limbs;

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

    // -- commands --
    /// a callback for calculating IK
    void OnAnimatorIK(int layer) {
        foreach (var limb in m_Limbs) {
            limb.ApplyIk();
        }
    }
}

}