using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {
    using Phase = Phase<LimbContainer>;

// TODO: extract state into container
/// the character limb's stride tracking
[Serializable]
class StrideSystem: System<LimbContainer> {
    // -- props --
    /// if the limb is not striding
    bool m_IsNotStriding;

    /// if the limb is currently free
    bool m_IsFree;

    /// if the limb is currently held
    bool m_IsHeld;

    /// the current ik position of the limb
    Vector3 m_GoalPos;

    /// the current surface placement
    Placement m_Placement;

    /// the distance to the held surface
    float m_HeldDistance;

    /// the bone the stride is anchored by
    LimbAnchor m_Anchor;

    /// an offset that translates the held position
    Vector3 m_SlideOffset;

    /// the current stride length input scale
    float m_InputScale;

    // -- Soil.System --
    protected override Phase InitInitialPhase() {
        return Free;
    }

    protected override SystemState State { get; set; } = new();

    // -- lifecycle --
    public override void Init(LimbContainer c) {
        m_Anchor = c.InitialAnchor;
        base.Init(c);
    }

    // -- commands --
    /// set if the limb is striding
    public void SetIsStriding(bool isStriding) {
        if (m_IsNotStriding != isStriding) {
            return;
        }

        if (!isStriding) {
            ChangeTo(NotStriding);
        } else {
            ChangeTo(Free);
        }
    }

    /// switch to the moving state
    public void Move(LimbAnchor anchor) {
        m_Anchor = anchor;
        ChangeTo(Moving);
    }

    /// release the limb if it's not already
    public void Release() {
        m_Anchor = null;
        if (!m_IsFree) {
            ChangeTo(Free);
        }
    }

    /// set the slide offset to translate the legs
    public void SetSlideOffset(Vector3 offset) {
        m_SlideOffset = offset;
    }

    // -- Free --
    Phase NotStriding => new(
        name: "NotStriding",
        enter: NotStriding_Enter,
        update: NotStriding_Update,
        exit: NotStriding_Exit
    );

    void NotStriding_Enter(LimbContainer c) {
        m_IsNotStriding = true;
    }

    void NotStriding_Update(float delta, LimbContainer c) {
        m_GoalPos = c.RootPos + c.InitialDir * c.InitialLen;
    }

    void NotStriding_Exit(LimbContainer c) {
        m_IsNotStriding = false;
    }

    // -- Free --
    Phase Free => new(
        name: "Free",
        enter: Free_Enter,
        update: Free_Update,
        exit: Free_Exit
    );

    void Free_Enter(LimbContainer c) {
        m_IsFree = true;
        m_GoalPos = c.RootPos + c.InitialDir * c.InitialLen;
        // TODO: set m_GoalRot?
    }

    void Free_Update(float delta, LimbContainer c) {
        // cast in the root direction, from the end of the limb
        var castDir = c.InitialDir;
        var castLen = c.InitialLen + c.Tuning.SearchRange_NoSurface;
        var castSrc = m_GoalPos - castDir * c.Tuning.CastOffset;

        var didHit = FindPlacement(
            castSrc,
            castDir,
            castLen,
            0f,
            out var placement,
            c
        );

        if (didHit) {
            m_GoalPos = placement.Pos;
            ChangeTo(Holding);
            return;
        }

        m_GoalPos = c.RootPos + c.InitialDir * c.InitialLen;
    }

    void Free_Exit(LimbContainer c) {
        m_IsFree = false;
    }

    // -- Moving --
    Phase Moving => new(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update(float delta, LimbContainer c) {
        var v = c.Character.State.Curr.SurfaceVelocity;

        var speedScale = c.Tuning.SpeedScale.Evaluate(v.magnitude);

        var inputMag = c.Character.Inputs.MoveMagnitude;
        var inputScale = m_InputScale;
        if (inputMag > m_InputScale) {
            inputScale = inputMag;
        } else {
            inputScale = Mathf.MoveTowards(inputScale, inputMag, c.Tuning.InputScale_ReleaseSpeed * delta);
        }

        var maxStrideScale = speedScale * inputScale;
        var maxStrideLenFwd = c.Tuning.MaxLength.Evaluate(maxStrideScale);
        var maxStrideLenCross = maxStrideLenFwd * c.Tuning.MaxLength_CrossScale;

        // the anchor leg vector
        var anchor = m_Anchor.RootPos - m_Anchor.GoalPos;

        // get the expected stride position based on the anchor
        var currStride = Vector3.ProjectOnPlane(anchor, Vector3.up);
        var currStrideLen = currStride.magnitude;
        var currStrideDir = currStride / currStrideLen;
        var currFwd = c.Character.State.Curr.Forward;
        var currStrideAngle = Vector3.Angle(currStrideDir, currFwd);

        // calculate max stride length on the ellipse
        var maxStrideX = maxStrideLenFwd * Mathf.Cos(currStrideAngle);
        var maxStrideY = maxStrideLenCross * Mathf.Sin(currStrideAngle);
        var maxStrideLen = Mathf.Sqrt(maxStrideX * maxStrideX + maxStrideY * maxStrideY);

        // clamp stride to the max length
        var nextStrideLenRaw = currStride.magnitude;
        var nextStrideLen = Mathf.Min(nextStrideLenRaw, maxStrideLen);

        // the movement dir to align against stride
        var moveDir = v;
        if (moveDir == Vector3.zero) {
            moveDir = currFwd;
        }

        // shape the stride along its progress curve
        var nextStrideElapsed = Mathf.Sign(Vector3.Dot(currStride, moveDir)) * nextStrideLen / maxStrideLen;
        nextStrideLen = c.Tuning.Shape.Evaluate(nextStrideElapsed) * maxStrideLen;
        var nextStride = nextStrideLen * currStrideDir;

        // the direction to the goal; mirror the stride over anchor up
        var goalDir = nextStride;
        goalDir.y = -anchor.y;
        goalDir = goalDir.normalized;

        // accumulate root offset and get root position
        var rootPos = c.RootPos;

        // the maximum stride distance projected along the leg
        var goalMax = Mathf.Max(
            c.InitialLen,
            // AAA: Vector3.down maybe limb direction?
            maxStrideLen / Vector3.Cross(goalDir, Vector3.down).magnitude
        );

        var goalPos = rootPos + goalDir * c.InitialLen;

        // find foot placement
        var castSrc = rootPos;
        var castDir = goalDir;
        var castLen = goalMax;

        FindPlacement(
            castSrc,
            castDir,
            castLen,
            0f,
            out var placement,
            c
        );

        var offset = 0f;
        if (placement.Result == CastResult.Hit) {
            offset = c.InitialLen - placement.Distance;
        }

        // add the shape's perpendicular offset
        offset = Mathf.Max(offset, c.Tuning.Shape_Offset.Evaluate(nextStrideElapsed) * inputScale);

        // displace the goal to correct for placement/offset
        goalPos += offset * -castDir;

        m_GoalPos = goalPos;
        m_Placement = placement;
        m_InputScale = inputScale;

        Debug_DrawMove(c);

        // once we complete our stride, switch to hold
        if (nextStrideElapsed >= 1f) {
            ChangeTo(Holding);
            Debug_DrawHold(c);
            return;
        }
    }

    // -- Holding --
    Phase Holding => new(
        name: "Holding",
        enter: Holding_Enter,
        update: Holding_Update,
        exit: Holding_Exit
    );

    void Holding_Enter(LimbContainer c) {
        m_IsHeld = true;

        // find placement along limb
        // the extra range is to account for entering hold from a skip, should be the same
        // value as goalMax above.
        var castSrc = c.RootPos;
        var castDir = Vector3.Normalize(m_GoalPos - castSrc);
        var castLen = c.InitialLen + c.Tuning.SearchRange_OnSurface;

        var didHit = FindPlacement(
            castSrc,
            castDir,
            castLen,
            0,
            out var placement,
            c
        );

        // since we might be casting away from the character
        var heldDistance = placement.Distance;

        // TODO: should this just be the same as below? what is different about enter?
        // if we don't find one, cast in the root direction, from the end of the limb
        if (!didHit) {
            didHit = FindPlacementFromEnd(
                castSrc + castDir * castLen,
                out placement,
                c
            );

            if (didHit) {
                heldDistance = placement.Distance;
            }
        }

        m_Placement = placement;
        m_HeldDistance = heldDistance;

        if (!didHit) {
            ChangeTo(Free);
            return;
        }

        m_GoalPos = placement.Pos;
    }

    void Holding_Update(float delta, LimbContainer c) {
        var goalPos = m_GoalPos - m_SlideOffset;

        // find placement along limb
        var castSrc = c.RootPos;
        var castDir = Vector3.Normalize(goalPos - castSrc);
        var castLen = c.InitialLen;

        var didHit = FindPlacement(
            castSrc,
            castDir,
            castLen,
            0f,
            out var placement,
            c
        );

        var heldDistance = 0f;

        // if we don't find one, cast in the root direction, from the end of the limb
        if (!didHit) {
            didHit = FindPlacementFromEnd(goalPos, out placement, c);

            if (didHit) {
                // the projected distance from the goal to the end of the limb
                var endPos = castSrc + castDir * c.InitialLen;
                heldDistance += Vector3.Dot(goalPos - endPos, c.InitialDir);
                heldDistance += placement.Distance;
            }
        }

        m_Placement = placement;
        m_HeldDistance = heldDistance;

        if (!didHit) {
            ChangeTo(Free);
            return;
        }

        goalPos = placement.Pos;
        if (Vector3.SqrMagnitude(goalPos - m_GoalPos) > c.Tuning.MinMove * c.Tuning.MinMove) {
            m_GoalPos = goalPos;
        }
    }

    void Holding_Exit(LimbContainer c) {
        m_IsHeld = false;
    }

    // -- queries --
    /// if the stride is currently active
    public bool IsActive {
        get => !m_IsNotStriding && !m_IsFree;
    }

    /// .
    public bool IsHeld {
        get => m_IsHeld;
    }

    /// .
    public bool IsFree {
        get => m_IsFree;
    }

    /// the current ik position of the limb
    public Vector3 GoalPos {
        get => m_GoalPos;
    }

    /// the current placement normal
    public Vector3 Normal {
        get {
            if (m_Placement.Result == CastResult.Hit) {
                return m_Placement.Normal;
            }

            if (m_Placement.Result == CastResult.OutOfRange && m_IsHeld && m_HeldDistance <= m_Container.Tuning.HeldDistance_OnSurface) {
                return m_Placement.Normal;
            }

            return Vector3.zero;
        }
    }

    /// the distance to the held surface
    public float HeldDistance {
        get => m_IsHeld ? m_HeldDistance : 0f;
    }

    /// the current placement result
    public CastResult PlacementResult {
        get => m_Placement.Result;
    }

    /// cast for a surface underneath the current pos
    bool FindPlacementFromEnd(
        Vector3 goalPos,
        out Placement placement,
        LimbContainer c
    ) {
        var castSrc = goalPos;
        var castDir = c.InitialDir;
        var castLen = c.InitialLen + c.Tuning.SearchRange_NoSurface;

        var currSurface = c.Character.State.Curr.MainSurface;
        if (currSurface.IsSome) {
            castDir = -currSurface.Normal;
            castLen = c.InitialLen + c.Tuning.SearchRange_OnSurface;
        }

        var didHit = FindPlacement(
            castSrc,
            castDir,
            castLen,
            c.Tuning.CastOffset,
            out placement,
            c
        );

        return didHit;
    }

    /// cast for a placement on the surface
    bool FindPlacement(
        Vector3 castSrc,
        Vector3 castDir,
        float castLen,
        float castOffset,
        out Placement placement,
        LimbContainer c
    ) {
        castSrc -= castDir * c.Tuning.CastOffset;
        castLen += c.Tuning.CastOffset;

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            c.Tuning.CastMask,
            QueryTriggerInteraction.Ignore
        );

        // if the cast missed, there's nothing
        if (!didHit) {
            placement = Placement.Miss;
            return false;
        }

        // if the cast is farther than the leg
        if (Vector3.Distance(hit.point, c.RootPos) > c.InitialLen) {
            placement = Placement.Hit(hit, castOffset, CastResult.OutOfRange);
            return true;
        }

        placement = Placement.Hit(hit, castOffset, CastResult.Hit);

        return true;
    }

    /// the result of the placement cast
    public enum CastResult {
        Hit,
        OutOfRange,
        Miss
    }

    /// a position and rotation placement for the goal
    public readonly struct Placement {
        /// .
        public readonly Vector3 Pos;

        /// .
        public readonly Vector3 Normal;

        /// the distance to the hit surface, if any
        public readonly float Distance;

        /// the kind of placement cast
        public readonly CastResult Result;

        /// .
        public Placement(
            Vector3 pos,
            Vector3 normal,
            float distance,
            CastResult result
        ) {
            Pos = pos;
            Normal = normal;
            Distance = distance;
            Result = result;
        }

        public static Placement Hit(
            RaycastHit hit,
            float offset,
            CastResult result
        ) {
            return new Placement(
                hit.point,
                hit.normal,
                Mathf.Max(hit.distance - offset, 0f),
                result
            );
        }

        public static Placement Miss {
            get => new(
                Vector3.zero,
                Vector3.zero,
                0f,
                CastResult.Miss
            );
        }
    }

    // -- debug --
    void Debug_DrawMove(LimbContainer c) {
        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride-curr"),
            m_Anchor.GoalPos,
            m_GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(), tags: DebugDraw.Tag.Model, count: 1, width: 0.03f)
        );
    }

    void Debug_DrawHold(LimbContainer c) {
        DebugDraw.Push(
            c.Goal.Debug_Name("stride-hold"),
            m_GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: DebugDraw.Tag.Model, width: 2f)
        );

        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride"),
            m_Anchor.GoalPos,
            m_GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: DebugDraw.Tag.Model, width: 0.5f)
        );
    }
}

}