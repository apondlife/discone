using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// the character limb's stride tracking
[Serializable]
sealed class StrideSystem: SimpleSystem<LimbContainer> {
    // -- Soil.System --
    protected override Phase<LimbContainer> InitInitialPhase() {
        return Free;
    }

    public override void Update(float delta) {
        UpdateInputScale(delta, c);
        base.Update(delta);
    }

    // -- NotStriding --
    public static readonly Phase<LimbContainer> NotStriding = new("NotStriding",
        enter: NotStriding_Enter,
        update: NotStriding_Update,
        exit: NotStriding_Exit
    );

    static void NotStriding_Enter(System<LimbContainer> _, LimbContainer c) {
        c.State.IsNotStriding = true;
    }

    static void NotStriding_Update(float delta, System<LimbContainer> _, LimbContainer c) {
        c.State.GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
    }

    static void NotStriding_Exit(System<LimbContainer> _, LimbContainer c) {
        c.State.IsNotStriding = false;
    }

    // -- Free --
    public static readonly Phase<LimbContainer> Free = new("Free",
        enter: Free_Enter,
        update: Free_Update,
        exit: Free_Exit
    );

    static void Free_Enter(System<LimbContainer> s, LimbContainer c) {
        c.State.IsFree = true;
        c.State.GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
        // TODO: set c.State.GoalRot?
    }

    static void Free_Update(float delta, System<LimbContainer> s, LimbContainer c) {
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
            s.ChangeToImmediate(Holding, delta);
            return;
        }

        c.State.GoalPos = c.RootPos + c.SearchDir * c.InitialLen;
    }

    static void Free_Exit(System<LimbContainer> s, LimbContainer c) {
        c.State.IsFree = false;
    }

    // -- Moving --
    public static readonly Phase<LimbContainer> Moving = new("Moving",
        update: Moving_Update
    );

    static void Moving_Update(float delta, System<LimbContainer> s, LimbContainer c) {
        // the anchor leg vector
        var anchor = c.State.Anchor.GoalPos - c.State.Anchor.RootPos;
        var anchorProjSearchDir = Vector3.Project(anchor, c.SearchDir);

        // get the expected stride position based on the anchor
        var currStride = -(anchor - anchorProjSearchDir);
        var currStrideLen = currStride.magnitude;
        var currStrideDir = currStride / currStrideLen;

        // get the max length of the stride in the current direction
        var maxStrideLen = FindMaxStrideLength(currStrideDir, c);

        // clamp stride to the max length
        var nextStrideLen = Mathf.Min(currStrideLen, maxStrideLen);

        // the movement dir to align against stride
        var moveDir = c.Character.State.Curr.SurfaceVelocity;
        if (moveDir == Vector3.zero) {
            moveDir = c.Character.State.Curr.Forward;
        }

        // shape the stride along its progress curve
        var nextStrideElapsed = Mathf.Sign(Vector3.Dot(currStride, moveDir)) * nextStrideLen / maxStrideLen;
        nextStrideLen = c.Tuning.Shape.Evaluate(nextStrideElapsed) * maxStrideLen;
        var nextStride = nextStrideLen * currStrideDir;

        // the direction to the goal; mirror the stride over search dir
        var goalDir = nextStride + anchorProjSearchDir;
        goalDir = goalDir.normalized;

        // the maximum stride distance projected along the leg
        var goalMax = Mathf.Max(
            c.InitialLen,
            maxStrideLen / Vector3.Dot(goalDir, currStrideDir)
        );

        // accumulate root offset and get root position
        var rootPos = c.RootPos;
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
        offset = Mathf.Max(offset, c.Tuning.Shape_Offset.Evaluate(nextStrideElapsed) * c.State.InputScale);

        // displace the goal to correct for placement/offset
        goalPos += offset * -castDir;

        c.State.GoalPos = goalPos;
        c.State.Placement = placement;

        Debug_DrawMove(c);

        // once we complete our stride, switch to hold
        if (nextStrideElapsed >= 1f) {
            s.ChangeToImmediate(Holding, delta);
            Debug_DrawHold(c);
            return;
        }
    }

    // -- Holding --
    public static readonly Phase<LimbContainer> Holding = new("Holding",
        enter: Holding_Enter,
        update: Holding_Update,
        exit: Holding_Exit
    );

    static void Holding_Enter(System<LimbContainer> _, LimbContainer c) {
        c.State.IsHeld = true;
    }

    static void Holding_Update(float delta, System<LimbContainer> s, LimbContainer c) {
        var goalPos = c.State.GoalPos - c.State.SlideOffset;

        // get the stride position
        var currStride = Vector3.ProjectOnPlane(goalPos - c.RootPos, c.SearchDir);
        var currStrideLen = currStride.magnitude;
        var currStrideDir = currStride / currStrideLen;

        // get the max length of the stride in the current direction
        var maxStrideLen = FindMaxStrideLength(currStrideDir, c);
    
        // ensure the goal isn't farther than the current stride len if it's shrinking
        goalPos += currStrideDir * Mathf.Min(maxStrideLen - currStrideLen, 0f);

        // the direction to the goal
        var goalDir = Vector3.Normalize(goalPos - c.RootPos);

        // the maximum stride distance projected along the leg
        // var goalMax = Mathf.Max(
        //     c.InitialLen,
        //     maxStrideLen / Vector3.Dot(goalDir, currStrideDir)
        // );

        // check if there's any collision from the limb to where we want to be holding
        // we might be holding farther away from the limb itself
        var castSrc = c.RootPos;
        var castDir = goalDir;
        var castLen = c.InitialLen + c.Tuning.Hold_CastLength;

        // TODO: we'd like to be able to limit the hold length to prevent snapping when casting down, we'd like to
        // generally avoid casting down except perhaps small distances. however this breaks the held leg sliding up/down
        // slopes, and also introduces a weird frame in the run animation when both legs are held (and it casts down).
        // var castLen = goalMax;

        var didHit = FindPlacement(
            castSrc,
            castDir,
            castLen,
            0f,
            out var placement,
            c
        );

        var heldDistance = 0f;
        var heldExtension = (placement.Distance - c.InitialLen) * castDir;

        // if we don't find a surface, cast in the limb axis direction, from the end of the limb
        if (!didHit) {
            didHit = FindPlacementFromEnd(out placement, c);

            if (placement.Result == LimbPlacement.CastResult.OutOfRange) {
                var dir = (placement.Pos - c.RootPos);
                var dist = dir.magnitude - c.InitialLen;
                heldExtension = dir.normalized * dist;
            }
        }

        // project the placement distance along the search dir to know the distance from the end
        // of the limb to the collision in the search dir
        var normDotSearch = Vector3.Dot(placement.Normal, c.SearchDir);

        // AAA: finish or unroll hips changes
        // if the cast is outside the limb, and the search is opposed to the normal, there's held distance
        if (normDotSearch < 0) {
            // https://miro.com/app/board/uXjVM8nwDIU=/?moveToWidget=3458764587553685421&cot=14
            // https://miro.com/app/board/uXjVM8nwDIU=/?moveToWidget=3458764587792518618&cot=14
            var heldDotNormal = Vector3.Dot(heldExtension, placement.Normal);
            heldDistance = heldDotNormal / normDotSearch;
        }

        c.State.Placement = placement;
        c.State.HeldDistance = heldDistance;

        if (!didHit) {
            s.ChangeTo(Free);
            return;
        }

        goalPos = placement.Pos;
        // AAA: why do we do this?
        if (Vector3.SqrMagnitude(goalPos - c.State.GoalPos) > c.Tuning.MinMove * c.Tuning.MinMove) {
            c.State.GoalPos = goalPos;
        }
    }

    static void Holding_Exit(System<LimbContainer> _, LimbContainer c) {
        c.State.IsHeld = false;
        c.State.HeldDistance = Mathf.Infinity;
    }

    // -- commands --
    // TODO: this is input processing; it doesn't even need to live in the limb (other than depending on a tuning)
    /// update current input scale, interpolating towards smaller inputs
    static void UpdateInputScale(float delta, LimbContainer c) {
        var inputScale = c.State.InputScale;

        var inputMag = c.Character.Inputs.MoveMagnitude;
        if (inputMag > inputScale) {
            inputScale = inputMag;
        } else {
            inputScale = Mathf.MoveTowards(inputScale, inputMag, c.Tuning.InputScale_ReleaseSpeed * delta);
        }

        c.State.InputScale = inputScale;
    }

    // -- queries --
    static float FindMaxStrideLength(Vector3 strideDir, LimbContainer c) {
        // get the orientation of the stride
        var strideAngle = Vector2.Angle(strideDir, c.Character.State.Curr.Forward) * Mathf.Deg2Rad;

        // get the size of the stride ellipse
        var inputScale = c.State.InputScale;
        var speedScale = c.Tuning.SpeedScale.Evaluate(c.Character.State.Curr.SurfaceVelocity.magnitude);
        var ellipseRadiusFwd = c.Tuning.MaxLength.Evaluate(speedScale * inputScale);
        var ellipseRadiusCross = ellipseRadiusFwd * c.Tuning.MaxLength_CrossScale;

        // calculate stride length on the ellipse
        var maxStrideFwd = ellipseRadiusFwd * Mathf.Cos(strideAngle);
        var maxStrideCross = ellipseRadiusCross * Mathf.Sin(strideAngle);
        var maxStrideLength = Mathf.Sqrt(maxStrideFwd * maxStrideFwd + maxStrideCross * maxStrideCross);

        return maxStrideLength;
    }

    /// cast for a placement on the surface
    static bool FindPlacement(
        Vector3 castSrc,
        Vector3 castDir,
        float castLen,
        float castOffset,
        out LimbPlacement placement,
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

    /// cast for a surface underneath the current pos
    static bool FindPlacementFromEnd(
        out LimbPlacement placement,
        LimbContainer c
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

    // -- debug --
    static void Debug_DrawMove(LimbContainer c) {
        DebugDraw.PushLine(
            c.Goal.Debug_Name("stride-curr"),
            c.State.Anchor.GoalPos,
            c.State.GoalPos,
            new DebugDraw.Config(c.Goal.Debug_Color(), tags: c.Goal.Debug_Tag(), count: 1, width: 0.03f)
        );
    }

    static void Debug_DrawHold(LimbContainer c) {
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