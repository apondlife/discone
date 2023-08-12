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

    // the character that owns this hair
    public DisconeCharacter m_Container;

    // the colliders to disable when the character is paused
    public Collider[] m_Colliders;

    ///  -- lifecycle --
    void Awake()
    {
        // get the container
        m_Container = GetComponentInParent<DisconeCharacter>(true);

        // cache colliders
        m_Colliders = GetComponentsInChildren<Collider>();

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

    void Start() {
        // bind events
        m_Container.Character.Events.Bind(ThirdPerson.CharacterEvent.Paused, OnCharacterPaused);
        m_Container.Character.Events.Bind(ThirdPerson.CharacterEvent.Unpaused, OnCharacterUnpaused);
    }


    void OnEnable() {
        m_Rig.gameObject.SetActive(true);
        m_Rig.transform.position = transform.position;
    }

    void OnDisable() {
        m_Rig.gameObject.SetActive(false);
    }

    /// -- events --
    void OnCharacterPaused() {
        foreach(var collider in m_Colliders) {
            collider.enabled = false;
        }
        Debug.Log($"Disabling {m_Colliders.Length} Colliders for {m_Container.name}");
    }

    void OnCharacterUnpaused() {
        foreach(var collider in m_Colliders) {
            collider.enabled = true;
        }
        Debug.Log($"Enabling {m_Colliders.Length} Colliders for {m_Container.name}");
    }

    ///  -- commands --
    void AttachToCharacter(Rigidbody rb) {
        var joint = rb.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = m_AttachedTo;
    }

}
