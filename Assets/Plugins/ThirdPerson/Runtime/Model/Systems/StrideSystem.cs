using System;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

using Container = LimbContainer;
using Phase = Phase<LimbContainer>;

// TODO: collapse tuning & other config values into container
/// the character limb's stride tracking
[Serializable]
class StrideSystem: System<Container> {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the root bone")]
    [SerializeField] Transform m_Root;

    [Tooltip("the cast offset")]
    [SerializeField] float m_CastOffset;

    [FormerlySerializedAs("m_LayerMask")]
    [Tooltip("the cast layer mask")]
    [SerializeField] LayerMask m_CastMask;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the threshold under which movements are ignored")]
    [SerializeField] float m_MinMove;

    [Tooltip("the max distance before searching for a new dest")]
    [SerializeField] FloatRange m_MaxLength;

    [Tooltip("the shape of the stride as a fn of progress through the complete stride")]
    [SerializeField] AnimationCurve m_Shape;

    [Tooltip("the release speed on the stride scale as a fn of input")]
    [SerializeField] float m_InputScale_ReleaseSpeed;

    [Tooltip("the stride scale as a fn of speed")]
    [SerializeField] MapCurve m_SpeedScale;

    [Tooltip("the stride scale as a fn of the angle between facing & velocity")]
    [SerializeField] MapOutCurve m_FacingScale;

    [Tooltip("the extra search distance when on a surface")]
    [SerializeField] float m_SearchRange_Surface;

    [Tooltip("the extra search distance when not on a surface")]
    [SerializeField] float m_SearchRange_NoSurface;

    // -- props --
    /// if the limb is currently free
    bool m_IsFree;

    /// if the limb is currently held
    bool m_IsHeld;

    /// the current ik position of the limb
    Vector3 m_GoalPos;

    /// the current ik rotation of the limb
    Quaternion m_GoalRot;

    /// the bone the stride is anchored by
    CharacterBone m_Anchor;

    /// an offset that translates the held position
    Vector3 m_Offset;

    /// the current stride length input scale
    float m_InputScale;

    // -- Soil.System --
    protected override Phase InitInitialPhase() {
        return Holding;
    }

    protected override SystemState State { get; set; } = new();

    // -- lifecycle --
    public override void Init(Container c) {
        m_Anchor = c.Anchor;
        base.Init(c);
    }

    // -- commands --
    /// switch to the moving state
    public void Move(CharacterBone anchor) {
        m_Anchor = anchor;
        ChangeTo(Moving);
    }

    /// release the limb if it's not already
    public void Release() {
        m_Anchor = null;
        if (!IsFree) {
            ChangeTo(Free);
        }
    }

    /// set the current offset to translate the legs
    public void SetOffset(Vector3 offset) {
        m_Offset = offset;
    }

    // -- Free --
    Phase Free => new(
        name: "Free",
        enter: Free_Enter,
        update: Free_Update,
        exit: Free_Exit
    );

    void Free_Enter(Container c) {
        m_IsFree = true;
        m_GoalPos = m_Root.position + RootDir * c.Length;
    }

    void Free_Update(float delta, Container c) {
        var didHit = FindSurface(m_GoalPos, out var hit, c);
        if (didHit) {
            m_GoalPos = hit.point;
            ChangeTo(Holding);
            return;
        }

        m_GoalPos = m_Root.position + RootDir * c.Length;
    }

    void Free_Exit(Container c) {
        m_IsFree = false;
    }

    // -- Moving --
    Phase Moving => new(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update(float delta, Container c) {
        var v = c.Character.State.Curr.SurfaceVelocity;

        var speedScale = m_SpeedScale.Evaluate(v.magnitude);

        var inputMag = c.Character.Inputs.MoveMagnitude;
        var inputScale = m_InputScale;
        if (inputMag > m_InputScale) {
            inputScale = inputMag;
        } else {
            inputScale = Mathf.MoveTowards(inputScale, inputMag, m_InputScale_ReleaseSpeed * delta);
        }

        var facingScale = m_FacingScale.Evaluate(Vector3.Angle(
            c.Character.State.Curr.PlanarVelocity,
            c.Character.State.Curr.Forward
        ));

        var maxStrideScale = speedScale * inputScale * facingScale;
        var maxStrideLen = m_MaxLength.Evaluate(maxStrideScale);

        // the anchor leg vector
        var anchor = m_Anchor.RootPos - m_Anchor.GoalPos;

        // curve the current stride
        var currStride = Vector3.ProjectOnPlane(anchor, Vector3.up);
        var currStrideLen = Mathf.Min(currStride.magnitude, maxStrideLen);
        var currStrideDir = currStride / currStrideLen;
        var currStrideElapsed = Mathf.Sign(Vector3.Dot(currStride, v)) * currStrideLen / maxStrideLen;
        currStrideLen = m_Shape.Evaluate(currStrideElapsed) * maxStrideLen;
        currStride = currStrideLen * currStrideDir;

        // the direction to the goal; mirror the stride over anchor up
        var goalDir = currStride;
        goalDir.y = -anchor.y;
        goalDir = goalDir.normalized;

        // accumulate root offset an get root position
        var rootPos = m_Root.position;

        // the maximum stride distance projected along the leg
        var goalMax = Mathf.Max(
            c.Length,
            maxStrideLen / Vector3.Cross(goalDir, Vector3.down).magnitude
        );

        var goalPos = rootPos + goalDir * goalMax;
        var goalRot = Quaternion.identity;

        // find foot placement
        var castSrc = rootPos;
        var castDir = goalDir;
        var castLen = goalMax;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            m_CastMask
        );

        if (didHit) {
            goalPos = hit.point;
            goalRot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(currStride, hit.normal),
                hit.normal
            );
        }

        m_GoalPos = goalPos;
        m_GoalRot = goalRot;
        m_InputScale = inputScale;

        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride-curr"),
            m_Anchor.GoalPos,
            m_GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(), tags: DebugDraw.Tag.Model, count: 1, width: 0.03f)
        );

        // once we complete our stride, switch to hold
        if (currStrideElapsed >= 1f) {
            ChangeTo(Holding);

            DebugDraw.Push(
                c.Goal.Debug_Name("stride-hold"),
                m_GoalPos,
                new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: DebugDraw.Tag.Model, width: 5f)
            );

            DebugDraw.PushLine(
                c.Goal.Debug_Name("stride"),
                m_Anchor.GoalPos,
                m_GoalPos,
                new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: DebugDraw.Tag.Model, width: 0.5f)
            );
        }
    }

    // -- Holding --
    Phase Holding => new(
        name: "Holding",
        enter: Holding_Enter,
        update: Holding_Update,
        exit: Holding_Exit
    );

    void Holding_Enter(Container c) {
        m_IsHeld = true;

        // find placement along limb
        var castSrc = m_Root.position;
        var castDir = Vector3.Normalize(m_GoalPos - castSrc);
        var castLen = c.Length + m_SearchRange_Surface;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            m_CastMask
        );

        // if we don't find one, cast in the root direction, from the end of the limb
        if (!didHit) {
            didHit = FindSurface(castSrc + castDir * castLen, out hit, c);
        }

        if (!didHit) {
            ChangeTo(Free);
            return;
        }

        m_GoalPos = hit.point;
    }

    void Holding_Update(float delta, Container c) {
        var goalPos = m_GoalPos - m_Offset;

        // find placement along limb
        var castSrc = m_Root.position;
        var castDir = Vector3.Normalize(goalPos - castSrc);
        var castLen = c.Length + m_SearchRange_Surface;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            m_CastMask
        );

        // if we don't find one, cast in the root direction, from the end of the limb
        if (!didHit) {
            didHit = FindSurface(goalPos, out hit, c);
        }

        if (!didHit) {
            ChangeTo(Free);
            return;
        }

        goalPos = hit.point;
        if (Vector3.SqrMagnitude(goalPos - m_GoalPos) > m_MinMove * m_MinMove) {
            m_GoalPos = goalPos;
        }
    }

    void Holding_Exit(Container c) {
        m_IsHeld = false;
    }

    // -- queries --
    /// if the limb is currently free
    public bool IsFree {
        get => m_IsFree;
    }

    /// if the limb is currently held
    public bool IsHeld {
        get => m_IsHeld;
    }

    /// the current ik position of the limb
    public Vector3 GoalPos {
        get => m_GoalPos;
    }

    /// the current ik rotation of the limb
    public Quaternion GoalRot {
        get => m_GoalRot;
    }

    /// the direction towards the surface
    Vector3 RootDir {
        get => -m_Root.up;
    }

    /// cast for a surface underneath the current pos
    bool FindSurface(
        Vector3 goalPos,
        out RaycastHit hit,
        Container c
    ) {
        var castDir = RootDir;
        var castLen = c.Length + m_SearchRange_NoSurface;

        var currSurface = c.Character.State.Curr.MainSurface;
        if (currSurface.IsSome) {
            castDir = -currSurface.Normal;
            castLen = c.Length + m_SearchRange_Surface;
        }

        var castSrc = goalPos - castDir * m_CastOffset;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out hit,
            castLen,
            m_CastMask,
            QueryTriggerInteraction.Ignore
        );

        return didHit;
    }
}

}