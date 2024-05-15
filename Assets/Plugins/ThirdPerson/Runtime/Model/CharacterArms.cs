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

    /// the left arm anchor
    ArmAnchor m_LeftAnchor;

    /// the right arm anchor
    ArmAnchor m_RightAnchor;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // set props
        m_LeftAnchor = new ArmAnchor(m_Left);
        m_RightAnchor = new ArmAnchor(m_Right);
    }

    void Start() {
        m_InitialPos = transform.localPosition;
        m_InitialModelPos = m_Model.transform.localPosition;
    }

    void Update() {
        MoveArm(m_Left, m_LeftAnchor);
        MoveArm(m_Right, m_RightAnchor);
    }

    // -- commands --
    void MoveArm(Limb arm, ArmAnchor anchor) {
        // TODO: if arm just became free
        if (!arm.IsFree) {
            return;
        }

        var currDir = arm.RootPos - arm.GoalPos;
        var currProjSearch = Vector3.Project(currDir, arm.SearchDir);
        var currStride = currDir - currProjSearch;
        var currStrideLen = currStride.magnitude;
        if (currStrideLen < arm.Tuning.MaxLength.Max) {
            return;
        }

        var castDir = currDir - 2f * currProjSearch;
        var castSrc = arm.RootPos;

        // AAA: better length
        var castLen = arm.InitialLen * 2f;

        DebugDraw.Push(arm.Goal.Debug_Name("phantom-cast"), castSrc, castDir * castLen, new DebugDraw.Config(arm.Goal.Debug_Color(), count: 1));

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            arm.Tuning.CastMask,
            QueryTriggerInteraction.Ignore
        );

        if (!didHit) {
            return;
        }

        anchor.Move(hit.point);
        arm.Move(anchor);
    }

    /// a phantom anchor for the arm
    struct ArmAnchor: LimbAnchor {
        /// a reference to the arm to reuse its moving root pos
        Limb m_Arm;

        /// the position of the arm anchor
        Vector3 m_GoalPos;

        // -- lifetime --
        public ArmAnchor(Limb arm) {
            m_Arm = arm;
            m_GoalPos = Vector3.zero;
        }

        // -- commands --
        /// move the goal position
        public void Move(Vector3 pos) {
            m_GoalPos = pos;
        }

        // -- LimbAnchor --
        public Vector3 RootPos {
            get => m_Arm.RootPos;
        }

        public Vector3 GoalPos {
            get => m_GoalPos;
        }
    }
}

}