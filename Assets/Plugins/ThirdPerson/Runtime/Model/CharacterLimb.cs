using System;
using Soil;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Compiler;

namespace ThirdPerson {

/// the current position of a bone
public interface CharacterBone {
    /// the root position
    public Vector3 RootPos { get; }

    /// the goal position
    public Vector3 GoalPos { get; }
}

/// center of mass? move character down?
/// an ik limb for the character model
public sealed class CharacterLimb: MonoBehaviour, CharacterPart, CharacterBone {
    enum State {
        Hold,
        Move,
        Idle,
    }

    // -- deps --
    /// the containing character
    CharacterContainer c;

    /// the animator for this limb
    Animator m_Animator;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the type of goal of this limb")]
    [SerializeField] AvatarIKGoal m_Goal;

    [Tooltip("the scale for the speed")]
    [SerializeField] MapInCurve m_SpeedScale;

    [Tooltip("the cast layer mask")]
    [SerializeField] LayerMask m_LayerMask;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the turn speed of the ik rotation")]
    [SerializeField] float m_TurnSpeed;

    [Tooltip("the duration of the ik blend when searching for target")]
    [SerializeField] float m_BlendInDuration;

    [Tooltip("the duration of the ik blend when dropping target")]
    [SerializeField] float m_BlendOutDuration;

    [Header("tuning - stride")]
    [Tooltip("the max distance before searching for a new dest")]
    [SerializeField] MapOutCurve m_StrideLength;

    [Tooltip("the move speed of the ik position, when striding")]
    [SerializeField] MapOutCurve m_StrideSpeed;

    // -- props --
    /// the current limb state
    [SerializeField] State m_State;

    /// the transform of the goal bone, if any
    Transform m_GoalBone;

    /// the default limb length;
    float m_Length;

    /// the blending weight for this limb
    float m_Weight;

    /// the current ik position of the limb
    Vector3 m_GoalPos;

    /// the current ik rotation of the limb
    Quaternion m_GoalRot;

    /// the destination ik position of the limb in world space
    Vector3 m_DestPos;

    /// the destination ik rotation of the limb
    Quaternion m_DestRot;

    /// the bone the stride is anchored by
    CharacterBone m_Anchor;

    /// the current speed scale
    float m_CurrSpeedScale;

    /// the current stride length
    float m_CurrStrideLength;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // TODO: unclear if we really ever want this
        // start as our own anchor
        m_Anchor = this;
    }

    void Update() {
        if (!IsValid) {
            return;
        }

        var speed = c.State.Curr.SurfaceVelocity.magnitude;
        m_CurrSpeedScale = m_SpeedScale.Evaluate(speed);
        m_CurrStrideLength = m_StrideLength.Evaluate(m_CurrSpeedScale);

        switch (m_State) {
        case State.Idle:
            // AAA: what are we doing with this state
            // find target and move to
            // FindTarget();
            MoveToGround();
            break;
        case State.Move:
            MoveToTarget();
            break;
        case State.Hold:
            MoveToGround();
            break;
        }

        // blend the weight
        var isBlendingIn = m_State != State.Idle;
        m_Weight = Mathf.MoveTowards(
            m_Weight,
            isBlendingIn ? 1.0f : 0.0f,
            Time.deltaTime / (isBlendingIn ? m_BlendInDuration : m_BlendOutDuration)
        );

        // AAA: blend ik
        m_Weight = isBlendingIn ? 1.0f : 0.0f;

        if (m_Goal <= AvatarIKGoal.RightFoot) {
            Draw("limb", width: 2f);
        }
    }

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        // set props
        m_Animator = animator;

        // cache the bone; we can't really do anything if we don't find a bone
        m_GoalBone = m_Animator.GetBoneTransform(
            m_Goal switch {
                AvatarIKGoal.RightHand => HumanBodyBones.RightHand,
                AvatarIKGoal.LeftHand => HumanBodyBones.LeftHand,
                AvatarIKGoal.RightFoot => HumanBodyBones.RightFoot,
                AvatarIKGoal.LeftFoot => HumanBodyBones.LeftFoot,
                _ => throw new Exception($"invalid goal {m_Goal}")
            }
        );

        // get limb length
        m_Length = Vector3.Distance(transform.position, m_GoalBone.position);
        Log.Model.I($"{c.Name} limb {name} length: {m_Length}");

        // set initial position
        MoveToGround();

        // error on misconfiguration
        if (!IsValid) {
            Log.Character.E($"{c.Name} - <limb: {m_Goal}> no matching bone");
        }
    }

    /// starts a new stride for the limb
    public void Move(CharacterBone anchor) {
        m_State = State.Move;
        m_Anchor = anchor;
    }

    public void Hold() {
        m_State = State.Hold;
    }

    /// applies the limb ik
    public void ApplyIk() {
        if (!IsValid) {
            return;
        }

        m_Animator.SetIKPositionWeight(
            m_Goal,
            m_Weight
        );

        if (m_Weight != 0.0f) {
            m_Animator.SetIKPosition(
                m_Goal,
                m_GoalPos
            );

            m_Animator.SetIKRotation(
                m_Goal,
                m_GoalRot
            );
        }
    }

    /// move towards the dest ik target
    void MoveToTarget() {
        // get offset to anchor and mirror it over of anchor dir
        var offset = m_Anchor.RootPos - m_Anchor.GoalPos;
        var direction = Vector3.ProjectOnPlane(offset, Vector3.up);
        var planarDirection = direction;

        direction.y = -offset.y;
        direction = direction.normalized;

        var angle = Vector3.Angle(direction, Vector3.down);
        // the maximum stride distance in "leg space"
        var maxDist = m_CurrStrideLength / Mathf.Sin(angle);
        var rootPos = transform.position;
        var goalPos = rootPos + direction * maxDist;
        var goalRot = Quaternion.identity;

        // find foot placement
        var didHit = Physics.Raycast(
            rootPos,
            direction,
            out var hit,
            maxDist,
            m_LayerMask
        );

        if (didHit) {
            goalPos = hit.point;
            goalRot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(planarDirection, hit.normal),
                hit.normal
            );
        }

        m_GoalPos = goalPos;
        m_GoalRot = goalRot;

        DebugDraw.PushLine(
            $"stride-curr-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
            m_Anchor.GoalPos,
            m_GoalPos,
            new DebugDraw.Config(DebugColor(), tags: DebugDraw.Tag.Movement, width: 1f, count: 1)
        );

        // once we complete our stride, switch to hold
        if (Vector3.SqrMagnitude(planarDirection) > m_CurrStrideLength * m_CurrStrideLength) {
            MoveToGround();

            DebugDraw.Push(
                $"stride-hold-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
                m_GoalPos,
                new DebugDraw.Config(DebugColor(0.5f), tags: DebugDraw.Tag.Movement, width: 5f)
            );

            DebugDraw.PushLine(
                $"stride-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
                m_Anchor.GoalPos,
                m_GoalPos,
                new DebugDraw.Config(DebugColor(0.5f), tags: DebugDraw.Tag.Movement, width: 0.5f)
            );
        }
    }

    /// move position to nearest surface
    void MoveToGround() {
        var castDir = -transform.up; // TODO: arms ? maybe t.forward
        var castLen = m_Length;

        var didHit = Physics.Raycast(
            m_GoalPos,
            castDir,
            out var hit,
            castLen,
            m_LayerMask,
            QueryTriggerInteraction.Ignore
        );

        if (!didHit) {
            m_GoalPos = transform.position + castDir * m_Length;
            m_State = State.Idle;
            return;
        }

        m_State = State.Hold;
        m_GoalPos = hit.point;
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    public bool IsValid {
        get => m_GoalBone;
    }

    /// the current root bone position
    public Vector3 RootPos {
        get => transform.position;
    }

    /// the current goal bone position
    public Vector3 GoalPos {
        get => m_GoalPos;
    }

    /// the square length of the bone
    public float SqrLength {
        get => Vector3.SqrMagnitude(RootPos - GoalPos);
    }

    /// .
    public bool IsIdle {
        get => m_State == State.Idle;
    }

    /// .
    public bool IsHeld {
        get => m_State == State.Hold;
    }

    // -- debug --
    /// gets the debug color for a limb with given alpha (red is right)
    Color DebugColor(float alpha = 1f) {
        var color = m_Goal switch {
            AvatarIKGoal.LeftFoot => Color.blue,
            AvatarIKGoal.RightFoot => Color.red,
            AvatarIKGoal.LeftHand => Color.cyan,
            AvatarIKGoal.RightHand => Color.magenta,
            _ => throw new ArgumentOutOfRangeException()
        };

        color.a = alpha;

        return color;
    }

    public void Draw(string name, float alpha = 1f, float width = 1f, int count = 1) {
        var color = DebugColor(alpha);
        DebugDraw.PushLine(
            $"{name}-bone-{m_Goal}",
            m_GoalPos,
            transform.position,
            new DebugDraw.Config(color, tags: DebugDraw.Tag.Movement, minAlpha: color.a, width: width, count: count)
        );

        DebugDraw.Push(
            $"{name}-foot-{m_Goal}",
            m_GoalPos,
            new DebugDraw.Config(color, tags: DebugDraw.Tag.Movement, minAlpha: color.a, width: width * 2f, count: count)
        );
    }
}
}