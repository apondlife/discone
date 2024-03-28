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

    [Tooltip("the length of the cast")]
    [SerializeField] float m_Length;

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

    /// the transform of the root bone, if any
    Transform m_RootBone;

    /// the transform of the goal bone, if any
    Transform m_GoalBone;

    /// the direction of the cast
    Vector3 m_CastDir;

    /// the blending weight for this limb
    float m_Weight;

    /// the current ik position of the limb in local space
    Vector3 m_GoalPos;

    /// the current ik rotation of the limb
    Quaternion m_CurrRot;

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

    /// the max cast length
    #if UNITY_EDITOR
    float m_CastLen => Mathf.Sqrt(m_Length * m_Length + m_StrideLength.Dst.Max * m_StrideLength.Dst.Max);
    #else
    float m_CastLen;
    #endif

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();

        // TODO: unclear if we really ever want this
        // start as our own anchor
        m_Anchor = this;

        // cache stride length
        #if !UNITY_EDITOR
        m_CastLen = Mathf.Sqrt(m_Length * m_Length + m_StrideLength.Dst.Dst * m_StrideLength.Dst.Dst);
        #endif
    }

    void Update() {
        if (!IsValid) {
            return;
        }

        // 2 half strides + hip distance is as far as a leg can go
        var maxDistance = m_StrideLength.Dst.Max + Vector3.Magnitude(m_Anchor.RootPos - transform.position);
        if (Vector3.Magnitude(m_GoalPos - m_Anchor.GoalPos) > maxDistance) {
            MoveToGround();

            DebugDraw.Push(
                $"stride-leg-break-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
                m_GoalPos,
                m_Anchor.GoalPos - m_GoalPos,
            new DebugDraw.Config(m_Goal == AvatarIKGoal.LeftFoot ? Color.cyan : Color.yellow, tags: DebugDraw.Tag.Movement, width: 5f, count: 100)
        );

        }

        var speed = c.State.Curr.SurfaceVelocity.magnitude;
        m_CurrSpeedScale = m_SpeedScale.Evaluate(speed);
        m_CurrStrideLength = m_StrideLength.Evaluate(m_CurrSpeedScale);

        switch (m_State) {
        case State.Idle:
            // AAA: what are we doing with this state
            FindTarget();
            break;
        case State.Move:
            MoveToTarget();
            break;
        case State.Hold:
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
            DebugDraw.Push(
                $"stride-leg-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
                m_GoalPos,
                transform.position - m_GoalPos,
                new DebugDraw.Config(m_Goal == AvatarIKGoal.LeftFoot ? Color.cyan : Color.yellow, tags: DebugDraw.Tag.Movement, width: 2f, count: 1)
            );
            DebugDraw.Push(
                $"stride-foot-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
                m_GoalPos,
                new DebugDraw.Config(m_Goal == AvatarIKGoal.LeftFoot ? Color.cyan : Color.yellow, tags: DebugDraw.Tag.Movement, width: 4f, count: 1)
            );
        }
    }

    // -- commands --
    /// initialize this limb w/ an animator
    public void Init(Animator animator) {
        // set props
        m_Animator = animator;

        // cache the bone; we can't really do anything if we don't find a bone
        m_RootBone = m_Animator.GetBoneTransform(
            m_Goal switch {
                AvatarIKGoal.RightHand => HumanBodyBones.RightUpperArm,
                AvatarIKGoal.LeftHand => HumanBodyBones.LeftUpperArm,
                AvatarIKGoal.RightFoot => HumanBodyBones.RightUpperLeg,
                AvatarIKGoal.LeftFoot => HumanBodyBones.LeftUpperLeg,
                _ => throw new Exception($"invalid goal {m_Goal}")
            }
        );

        m_GoalBone = m_Animator.GetBoneTransform(
            m_Goal switch {
                AvatarIKGoal.RightHand => HumanBodyBones.RightHand,
                AvatarIKGoal.LeftHand => HumanBodyBones.LeftHand,
                AvatarIKGoal.RightFoot => HumanBodyBones.RightFoot,
                AvatarIKGoal.LeftFoot => HumanBodyBones.LeftFoot,
                _ => throw new Exception($"invalid goal {m_Goal}")
            }
        );

        // use the initial direction of the anchor as cast dir
        m_CastDir = m_RootBone.up;

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
        }
    }

    /// try to find a new ik target
    void FindTarget() {
        var castPos = m_RootBone.position;
        var castDst = m_Anchor.GoalPos + m_CurrStrideLength * c.State.Curr.SurfaceVelocity.normalized;
        var castDir = castDst - castPos;
        var castLen = m_CastLen;

        // (there's a world in which we want to calculate an cast length based on anchor & start pos)
        var didHit = Physics.Raycast(
            castPos,
            castDir,
            out var hit,
            castLen,
            m_LayerMask,
            QueryTriggerInteraction.Ignore
        );

        // if we miss, switch to idle
        if (!didHit) {
            return;
        }

        var pos = hit.point;

        // var hitDist = Vector3.SqrMagnitude(pos - m_StrideStart);
        // if (hitDist > m_CurrStrideLength * m_CurrStrideLength) {
        //     return;
        // }

        // if this is farther than the current target, ignore it
        // var destDist = Vector3.SqrMagnitude(m_DestPos - m_StrideStart);
        // if (hitDist > destDist) {
        //     return;
        // }

        if (m_State != State.Move) {
            // set current position from the bone's current position in our local space
            m_GoalPos = transform.InverseTransformPoint(m_GoalBone.position);
        }

        // start moving towards the target
        m_State = State.Move;

        // move towards the closest point on surface
        m_DestPos = pos;
    }

    /// move towards the dest ik target
    void MoveToTarget() {
        // get offset to anchor and mirror it over of anchor dir
        var offset = m_Anchor.RootPos - m_Anchor.GoalPos;
        var direction = Vector3.ProjectOnPlane(offset, Vector3.up);
        direction = Vector3.ClampMagnitude(direction, m_CurrStrideLength / 2f);
        var planarDirection = direction;
        direction.y = -offset.y;

        m_GoalPos = transform.position + direction;

        DebugDraw.Push(
            $"stride-curr-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
            m_Anchor.GoalPos,
            m_GoalPos - m_Anchor.GoalPos,
            new DebugDraw.Config(m_Goal == AvatarIKGoal.LeftFoot ? Color.cyan : Color.yellow, tags: DebugDraw.Tag.Movement, width: 1f, count: 1)
        );

        // once we complete our stride, switch to hold
        if (Vector3.SqrMagnitude(planarDirection) > m_CurrStrideLength * m_CurrStrideLength / 4) {
            m_State = State.Hold;

            DebugDraw.Push(
                $"stride-hold-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
                m_GoalPos,
                new DebugDraw.Config(m_Goal == AvatarIKGoal.LeftFoot ? Color.blue : Color.red, tags: DebugDraw.Tag.Movement, width: 3f)
            );

            DebugDraw.Push(
                $"stride-{(m_Goal == AvatarIKGoal.LeftFoot ? "l" : "r")}",
                m_Anchor.GoalPos,
                m_GoalPos - m_Anchor.GoalPos,
                new DebugDraw.Config(m_Goal == AvatarIKGoal.LeftFoot ? Color.blue : Color.red, tags: DebugDraw.Tag.Movement, width: 0.5f)
            );
        }
    }

    /// move position to nearest surface
    void MoveToGround() {
        var didHit = Physics.Raycast(
            m_RootBone.position,
            -transform.up,
            out var hit,
            10f,
            m_LayerMask,
            QueryTriggerInteraction.Ignore
        );

        if (!didHit) {
            Log.Character.E($"{c.Name} - <limb: {m_Goal}> failed to find starting pos");
            return;
        }

        m_GoalPos = hit.point;
    }

    // -- queries --
    /// if this limb has the dependencies it needs to apply ik
    public bool IsValid {
        get => m_RootBone && m_GoalBone;
    }

    /// the current root bone position
    public Vector3 RootPos {
        get => transform.position;
    }

    /// the current goal bone position
    public Vector3 GoalPos {
        get => m_GoalPos;
    }

    /// .
    public bool IsIdle {
        get => m_State == State.Idle;
    }

    /// .
    public bool IsHeld {
        get => m_State == State.Hold;
    }

    // -- gizmos --
    void OnDrawGizmosSelected() {
        if (!IsValid) {
            return;
        }

        var anchorPos = m_RootBone.position;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            anchorPos,
            radius: 0.05f
        );

        var castPos = anchorPos;
        var castDir = m_CastDir + m_StrideLength.Evaluate(m_CurrSpeedScale) * 0.5f * c.State.Curr.SurfaceVelocity.normalized;
        var castLen = m_CastLen;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            castPos,
            castPos + castDir.normalized * castLen
        );
    }
}

}