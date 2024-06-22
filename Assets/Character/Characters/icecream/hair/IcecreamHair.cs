using UnityAtoms;
using UnityEngine;

namespace Discone {

public class IcecreamHair: MonoBehaviour {
    [Header("config")]
    [Tooltip("if this hair is being simulated or not")]
    [SerializeField] bool m_HasPhysics;

    [Tooltip("the max distance the colliders can be from their original position before disabling")]
    [SerializeField] float m_DisableDistance;

    [Tooltip("the distance the colliders can be from their original position before re enabling")]
    [SerializeField] float m_ReenableDistance;

    [Header("refs")]
    [Tooltip("the current player's character (for toggling physics)")]
    [SerializeField] CharacterVariable m_CurrentCharacter;

    [Tooltip("the unrigged version of the hair")]
    [SerializeField] GameObject m_NoRig;

    [Tooltip("the physics/bone rig that controls the hair")]
    [SerializeField] Rigidbody m_RigPrefab;

    [Tooltip("the root bone of the left side of the rig")]
    [SerializeField] Rigidbody m_LeftRoot;

    [Tooltip("the root bone of the right side of the rig")]
    [SerializeField] Rigidbody m_RightRoot;

    [Tooltip("the physics/bone rig that controls the hair")]
    [SerializeField] Rigidbody m_AttachedTo;

    [Header("instances")]
    [Tooltip("the physics/bone rig that controls the hair")]
    [SerializeField] Rigidbody m_Rig;

    // the character that owns this hair
    Character m_Container;

    // the colliders to disable when the character is paused
    HairCollider[] m_Colliders;

    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    ///  -- lifecycle --
    void Awake()
    {
        // get the container
        m_Container = GetComponentInParent<Character>(true);

        // cache colliders
        var colliders = GetComponentsInChildren<Collider>(true);
        m_Colliders = new HairCollider[colliders.Length];
        for (int i = 0; i < colliders.Length; i++) {
            var collider = colliders[i];
            m_Colliders[i].Collider =  collider;
            m_Colliders[i].InitialPosition =  collider.transform.localPosition;
        }

        // instantiate prefabs
        //m_NoRig = Instantiate(m_NoRigPrefab, transform.position, transform.rotation, m_AttachedTo.transform);
        //m_Rig = Instantiate(m_RigPrefab, transform.position, transform.rotation, m_AttachedTo.transform);

        //m_Mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        //m_Mesh.rootBone = m_Rig.transform.GetChild(0);

        // attach rigs to character
        AttachToCharacter(m_Rig);
        AttachToCharacter(m_RightRoot);
        AttachToCharacter(m_LeftRoot);

        if (m_HasPhysics) {
            EnablePhysics();
        } else {
            DisablePhysics();
        }

        // bind events
        m_Subscriptions
            .Add(m_CurrentCharacter.ChangedWithHistory, OnCharacterChanged);
    }

    void Start() {
        // bind events
        m_Container.OnPause += OnCharacterPaused;
        m_Container.OnUnpause += OnCharacterUnpaused;
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
        m_Container.OnPause -= OnCharacterPaused;
        m_Container.OnUnpause -= OnCharacterUnpaused;
    }

    void OnEnable() {
        m_Rig.gameObject.SetActive(true);
    }

    void OnDisable() {
        m_Rig.gameObject.SetActive(false);

        // reset the rig
        // CopyTransformTree(m_RigPrefab.transform.GetChild(0), m_Rig.transform.GetChild(0));
    }

    void FixedUpdate() {
        if (m_Container.IsPaused) {
            return;
        }

        foreach (var collider in m_Colliders) {
            var sqrDist = (collider.Collider.transform.localPosition - collider.InitialPosition).sqrMagnitude;
            if (collider.Collider.enabled && sqrDist > m_DisableDistance * m_DisableDistance) {
                collider.Collider.enabled = false;
            }
            else if (!collider.Collider.enabled && sqrDist < m_ReenableDistance * m_ReenableDistance) {
                collider.Collider.enabled = true;
            }
        }
    }

    /// -- events --
    void OnCharacterChanged(CharacterPair characters) {
        var prev = characters.Item2;
        var curr = characters.Item1;

        if (prev == m_Container) {
            DisablePhysics();
        }

        if (curr == m_Container) {
            EnablePhysics();
        }
    }

    void OnCharacterPaused() {
        foreach(var collider in m_Colliders) {
            collider.Collider.enabled = false;
        }
    }

    void OnCharacterUnpaused() {
        foreach(var collider in m_Colliders) {
            collider.Collider.enabled = true;
        }
    }

    ///  -- commands --
    void AttachToCharacter(Rigidbody rb) {
        var joint = rb.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = m_AttachedTo;
    }

    void EnablePhysics() {
        m_HasPhysics = true;

        // TODO: sync position?
        // to get all possible motion from the attached rigidbody on the character,
        // we need the two rigidbody chains to be completely disconnected
        // this is the only way we get the actual inertia from the character into the hair
        m_Rig.transform.parent = null;

        m_NoRig.gameObject.SetActive(false);

        // calls on enable
        gameObject.SetActive(true);
    }

    void DisablePhysics() {
        m_HasPhysics = false;

        m_Rig.transform.parent = m_AttachedTo.transform;

        m_NoRig.gameObject.SetActive(true);

        // calls on disable
        gameObject.SetActive(false);
    }

    // https://gamedev.stackexchange.com/questions/204851/copying-transforms-from-one-object-to-another
    public static void CopyTransformTree(Transform sourceRoot, Transform destRoot) {
        // Read local pose in one operation.
        var localPos = sourceRoot.localPosition;
        var localRot = sourceRoot.localRotation;

        // Clone local pose to destination transform in one operation.
        destRoot.localRotation = localRot;
        destRoot.localPosition = localPos;

        // Clone scale (skip if your bones don't use scale).
        destRoot.localScale = sourceRoot.localScale;

        // Iterate over as many children as both roots have.
        int limit = Mathf.Min(sourceRoot.childCount, destRoot.childCount);
        for (int i = 0; i < limit; i++) // Recurse on each child sub-tree.
            CopyTransformTree(sourceRoot.GetChild(i), destRoot.GetChild(i));
    }

    struct HairCollider {
        public Collider Collider;
        public Vector3 InitialPosition;
    }
}

}