using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

using Phase = Phase<LimbContainer>;
using Container = LimbContainer;

/// the character limb's stride tracking
[Serializable]
sealed class StrideSystem: SimpleSystem<Container> {
    // -- Soil.System --
    protected override Phase InitInitialPhase() {
        return Free;
    }

    // -- commands --
    /// set if the limb is striding
    public void SetIsStriding(bool isStriding) {
        // TODO: should `ChangeTo` be public and should this kind of method live on the obj
        // that owns the system?
        if (c.State.IsNotStriding != isStriding) {
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
        c.State.Anchor = anchor;
        ChangeTo(Moving);
    }

    /// switch to the holding state
    public void Hold(float delta) {
        if (!c.State.IsHeld) {
            ChangeToImmediate(Holding, delta);
        }
    }

    /// release the limb if it's not already
    public void Release() {
        c.State.Anchor = null;

        if (!c.State.IsFree) {
            ChangeTo(Free);
        }
    }

    /// set the slide offset to translate the legs
    public void SetSlideOffset(Vector3 offset) {
        c.State.SlideOffset = offset;
    }

    // -- NotStriding --
    Phase NotStriding => new(
        name: "NotStriding",
        enter: NotStriding_Enter,
        update: NotStriding_Update,
        exit: NotStriding_Exit
    );

    void NotStriding_Enter(Container c) {
        c.State.IsNotStriding = true;
    }

    void NotStriding_Update(float delta, Container c) {
        c.State.GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
    }

    void NotStriding_Exit(Container c) {
        c.State.IsNotStriding = false;
    }

    // -- Free --
    Phase Free => new(
        name: "Free",
        enter: Free_Enter,
        update: Free_Update,
        exit: Free_Exit
    );

    void Free_Enter(Container c) {
        c.State.IsFree = true;
        c.State.GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
        // TODO: set c.State.GoalRot?
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
            c.State.GoalPos = placement.Pos;
            ChangeToImmediate(Holding, delta);
            return;
        }

        c.State.GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
    }

    void Free_Exit(Container c) {
        c.State.IsFree = false;
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
        var inputScale = c.State.InputScale;
        if (inputMag > c.State.InputScale) {
            inputScale = inputMag;
        } else {
            inputScale = Mathf.MoveTowards(inputScale, inputMag, c.Tuning.InputScale_ReleaseSpeed * delta);
        }

        var maxStrideScale = speedScale * inputScale;
        var maxStrideLenFwd = c.Tuning.MaxLength.Evaluate(maxStrideScale);
        var maxStrideLenCross = maxStrideLenFwd * c.Tuning.MaxLength_CrossScale;

        // the anchor leg vector
        var anchor = c.State.Anchor.RootPos - c.State.Anchor.GoalPos;

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
        if (placement.Result == LimbPlacement.CastResult.Hit) {
            offset = c.InitialLen - placement.Distance;
        }

        // add the shape's perpendicular offset
        offset = Mathf.Max(offset, c.Tuning.Shape_Offset.Evaluate(nextStrideElapsed) * inputScale);

        // displace the goal to correct for placement/offset
        goalPos += offset * -castDir;

        c.State.GoalPos = goalPos;
        c.State.Placement = placement;
        c.State.InputScale = inputScale;

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
        c.State.IsHeld = true;
    }

    void Holding_Update(float delta, Container c) {
        var goalPos = c.State.GoalPos - c.State.SlideOffset;

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

            if (placement.Result == LimbPlacement.CastResult.OutOfRange) {
                var dir = (placement.Pos - c.RootPos);
                var dist = dir.magnitude - c.InitialLen;
                heldExtension = dir * dist;
            }
        }

        // project the placement distance along the search dir to know the distance from the end
        // of the limb to the collision in the search dir
        var normDotSearch = Vector3.Dot(placement.Normal, c.SearchDir);

        // if the cast is outside the limb, and the search is opposed to the normal, there's held distance
        if (placement.Result == LimbPlacement.CastResult.OutOfRange && normDotSearch < 0) {
            // https://miro.com/app/board/uXjVM8nwDIU=/?moveToWidget=3458764587553685421&cot=14
            // https://miro.com/app/board/uXjVM8nwDIU=/?moveToWidget=3458764587792518618&cot=14
            var heldDotNormal = Vector3.Dot(heldExtension, placement.Normal);
            heldDistance = heldDotNormal / normDotSearch;
        }

        c.State.Placement = placement;
        c.State.HeldDistance = heldDistance;

        if (!didHit) {
            ChangeTo(Free);
            return;
        }

        goalPos = placement.Pos;
        // AAA: why do we do this?
        if (Vector3.SqrMagnitude(goalPos - c.State.GoalPos) > c.Tuning.MinMove * c.Tuning.MinMove) {
            c.State.GoalPos = goalPos;
        }
    }

    void Holding_Exit(Container c) {
        c.State.IsHeld = false;
        c.State.HeldDistance = 0f;
    }

    // -- queries --
    /// cast for a surface underneath the current pos
    bool FindPlacementFromEnd(
        out LimbPlacement placement,
        Container c
    ) {
        var castSrc = c.RootPos + Vector3.Normalize(c.State.GoalPos - c.RootPos) * c.InitialLen;
        var castDir = c.SearchDir;
        var castLen = 0f;

        if (c.State.IsHeld) {
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
        out LimbPlacement placement,
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
            placement = LimbPlacement.Miss;
            return false;
        }

        // if the cast is farther than the leg
        if (Vector3.Distance(hit.point, c.RootPos) > c.InitialLen) {
            placement = LimbPlacement.Hit(hit, castOffset, LimbPlacement.CastResult.OutOfRange);
            return true;
        }

        placement = LimbPlacement.Hit(hit, castOffset, LimbPlacement.CastResult.Hit);

        return true;
    }

    // -- debug --
    void Debug_DrawMove(Container c) {
        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride-curr"),
            c.State.Anchor.GoalPos,
            c.State.GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(), tags: c.Goal.Debug_Tag(), count: 1, width: 0.03f)
        );
    }

    void Debug_DrawHold(Container c) {
        DebugDraw.Push(
            c.Goal.Debug_Name("stride-hold"),
            c.State.GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: c.Goal.Debug_Tag(), width: 2f)
        );

        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride"),
            c.State.Anchor.GoalPos,
            c.State.GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(0.5f), tags: c.Goal.Debug_Tag(), width: 0.5f)
        );
    }
}

}