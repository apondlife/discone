using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

using Phase = Phase<LimbContainer>;
using Container = LimbContainer;

// TODO: extract state into container
/// the character limb's stride tracking
[Serializable]
class StrideSystem: System<Container> {
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
    public override void Init(Container c) {
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

    /// switch to the holding state
    public void Hold(float delta) {
        if (!m_IsHeld) {
            ChangeToImmediate(Holding, delta);
        }
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

    // -- NotStriding --
    Phase NotStriding => new(
        name: "NotStriding",
        enter: NotStriding_Enter,
        update: NotStriding_Update,
        exit: NotStriding_Exit
    );

    void NotStriding_Enter(Container c) {
        m_IsNotStriding = true;
    }

    void NotStriding_Update(float delta, Container c) {
        m_GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
    }

    void NotStriding_Exit(Container c) {
        m_IsNotStriding = false;
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
        m_GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
        // TODO: set m_GoalRot?
    }

    void Free_Update(float delta, Container c) {
        // check for collision within the limb
        var castSrc = c.RootPos;
        var castDir = c.SearchDir;
        var castLen = c.InitialLen;
        var didHit = FindPlacement(
            castSrc,
            castDir,
            castLen,
            0f,
            out var placement,
            c
        );
        
        // cast in the root direction, from the end of the limb
        if (!didHit) {
            didHit = FindPlacementFromEnd(
                out placement,
                c
            );
        }

        if (didHit) {
            m_GoalPos = placement.Pos;
            ChangeToImmediate(Holding, delta);
            return;
        }

        m_GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
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
        var currStride = Vector3.ProjectOnPlane(anchor, c.SearchDir);
        var currStrideLen = currStride.magnitude;
        var currStrideDir = currStride / currStrideLen;
        var currFwd = c.Character.State.Curr.Forward;
        var currStrideAngle = Vector3.Angle(currStrideDir, currFwd) * Mathf.Deg2Rad;

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
        var goalDir = nextStride - Vector3.Project(anchor, c.SearchDir);
        goalDir = goalDir.normalized;

        // accumulate root offset and get root position
        var rootPos = c.RootPos;

        // the maximum stride distance projected along the leg
        var goalMax = Mathf.Max(
            c.InitialLen,
            maxStrideLen / Vector3.Cross(goalDir, c.SearchDir).magnitude
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
            ChangeToImmediate(Holding, delta);
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

    void Holding_Enter(Container c) {
        m_IsHeld = true;
    }

    void Holding_Update(float delta, Container c) {
        var goalPos = m_GoalPos - m_SlideOffset;

        // check if there's any collision from the limb to where we want to be holding
        // we might be holding farther away from the limb itself
        var castSrc = c.RootPos;
        var castDir = Vector3.Normalize(goalPos - castSrc);

        // TODO(done?): this search range is bad, should be similar to goalMax in moving, or just maxmaxstride projected
        // search range is the maximum we are allowing ourselves to extend our limb at this point
        // which is as far as we are willing to have our limb away from the search dir
        var castLen = c.InitialLen + c.Tuning.SearchRange_OnSurface / Vector3.Dot(castDir, c.SearchDir);

        var didHit = FindPlacement(
            castSrc,
            castDir,
            castLen,
            0f,
            out var placement,
            c
        );

        var heldDistance = 0f;
        var heldExtension = Mathf.Max(0, placement.Distance - c.InitialLen) * castDir;

        // if we don't find a surface, cast in the limb axis direction, from the end of the limb
        if (!didHit) {
            didHit = FindPlacementFromEnd(out placement, c);

            if (placement.Result == CastResult.OutOfRange) {
                var dir = (placement.Pos - c.RootPos);
                var dist = dir.magnitude - c.InitialLen;
                heldExtension = dir * dist;
            }
        }

        // project the placement distance along the search dir to know the distance from the end
        // of the limb to the collision in the search dir
        var normDotSearch = Vector3.Dot(placement.Normal, c.SearchDir);

        // if the cast is outside the limb, and the search is opposed to the normal, there's held distance
        if (placement.Result == CastResult.OutOfRange && normDotSearch < 0) {
            // https://miro.com/app/board/uXjVM8nwDIU=/?moveToWidget=3458764587553685421&cot=14
            // https://miro.com/app/board/uXjVM8nwDIU=/?moveToWidget=3458764587792518618&cot=14
            var heldDotNormal = Vector3.Dot(heldExtension, placement.Normal);
            heldDistance = heldDotNormal / normDotSearch;
        }

        m_Placement = placement;
        m_HeldDistance = heldDistance;

        if (!didHit) {
            ChangeTo(Free);
            return;
        }

        goalPos = placement.Pos;
        // AAA: why do we do this?
        if (Vector3.SqrMagnitude(goalPos - m_GoalPos) > c.Tuning.MinMove * c.Tuning.MinMove) {
            m_GoalPos = goalPos;
        }
    }

    void Holding_Exit(Container c) {
        m_IsHeld = false;
        m_HeldDistance = 0f;
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
        get => m_HeldDistance;
    }

    /// the current placement result
    public CastResult PlacementResult {
        get => m_Placement.Result;
    }

    /// cast for a surface underneath the current pos
    bool FindPlacementFromEnd(
        out Placement placement,
        Container c
    ) {
        var castSrc = c.RootPos + Vector3.Normalize(m_GoalPos - c.RootPos) * c.InitialLen;
        var castDir = c.SearchDir;
        var castLen = 0f;

        if (m_IsHeld) {
            castLen = c.Tuning.SearchRange_OnSurface;
        } else {
            // TODO: scale search range based on velocity magnitude?
            // TODO: this might just be unified (arms might want the search scale to change)
            var searchScale = Math.Max(Vector3.Dot(c.Character.State.Curr.Velocity.normalized, c.SearchDir), 0f);
            castLen = Mathf.Max(c.Tuning.SearchRange_NoSurface * searchScale, c.Tuning.HeldDistance_OnSurface);
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
        Container c
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

    // -- types --
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
    void Debug_DrawMove(Container c) {
        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride-curr"),
            m_Anchor.GoalPos,
            m_GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(), tags: c.Goal.Debug_Tag(), count: 1, width: 0.03f)
        );
    }

    void Debug_DrawHold(Container c) {
        DebugDraw.Push(
            c.Goal.Debug_Name("stride-hold"),
            m_GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: c.Goal.Debug_Tag(), width: 2f)
        );

        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride"),
            m_Anchor.GoalPos,
            m_GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: c.Goal.Debug_Tag(), width: 0.5f)
        );
    }
}

}