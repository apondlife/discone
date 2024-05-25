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
        if (c.State.Curr.IsIdle) {
            return;
        }

        // if arm is moving, don't change target
        if (!arm.IsFree && !arm.IsHeld) {
            return;
        }

        var currDir = arm.RootPos - arm.GoalPos;
        var fwd = c.State.Curr.Velocity.normalized;
        if (fwd == Vector3.zero) {
            fwd = c.State.Curr.Forward;
        }

        // get the signed distance of the held arm
        var currDist = Vector3.Dot(currDir, fwd);

        // if far enough away, look for an anchor position
        // AAA: this dist is projected into the surface
        // should probably just be doing the whole move method to prevent clipping
        var maxStride = Mathf.Max(arm.Tuning.MaxLength.Max, arm.Tuning.MaxLength.Min);
        maxStride = Mathf.Max(maxStride, maxStride * arm.Tuning.MaxLength_CrossScale);

        var maxDist = Mathf.Max(
            arm.InitialLen,
            maxStride / Vector3.Cross(currDir.normalized, arm.SearchDir).magnitude
        );

        if (currDist < maxDist) {
            return;
        }

        var currProjSearch = Vector3.Project(currDir, arm.SearchDir);
        var castDir = (currDir - 2f * currProjSearch).normalized;
        var castSrc = arm.RootPos;

        var castLen = arm.InitialLen + arm.Tuning.SearchRange_OnSurface / Vector3.Dot(castDir, arm.SearchDir);

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