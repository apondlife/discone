using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcecreamHair : MonoBehaviour
{
    [Header("refs")]
    [Tooltip("the physics/bone rig that controls the hair")]
    [SerializeField] Rigidbody m_Rig;

    [Tooltip("the root bone of the left side of the rig")]
    [SerializeField] Rigidbody m_LeftRoot;

    [Tooltip("the root bone of the right side of the rig")]
    [SerializeField] Rigidbody m_RightRoot;

    [Tooltip("the physics/bone rig that controls the hair")]
    [SerializeField] Rigidbody m_AttachedTo;

    ///  -- lifecycle --
    void Awake()
    {
        // to get all possible motion from the attached rigidbody on the character,
        // we need the two rigidbody chains to be completely disconnected
        // this is the only way we get the actual inertia from the character into the hair
        m_Rig.transform.parent = null;

        // attach rigs to character
        AttachToCharacter(m_Rig);
        AttachToCharacter(m_RightRoot);
        AttachToCharacter(m_LeftRoot);

        // debug
        #if UNITY_EDITOR
        Dbg.AddToParent("Characters/Icecream_Hair", m_Rig);
        #endif
    }

    void OnEnable() {
        m_Rig.gameObject.SetActive(true);
        m_Rig.transform.position = transform.position;
    }

    void OnDisable() {
        m_Rig.gameObject.SetActive(false);
    }

    ///  -- commands --
    void AttachToCharacter(Rigidbody rb) {
        var joint = rb.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = m_AttachedTo;
    }

}
