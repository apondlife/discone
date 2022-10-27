using System;
using UnityEngine;

namespace ThirdPerson {

/// how the character moves on the ground & air
[Serializable]
sealed class MovementSystem: CharacterSystem {
    // -- lifetime --
    protected override Phase InitInitialPhase() {
        return NotMoving;
    }

    // -- NotMoving --
    Phase NotMoving => new Phase(
        name: "NotMoving",
        update: NotMoving_Update
    );

    void NotMoving_Update(float delta) {
        // start floating if no longer grounded
        if (!m_State.Prev.IsGrounded) {
            ChangeTo(Floating);
            return;
        }

        // intergrate forces
        var dv = IntegrateForces(
            m_State.Curr.GroundVelocity,
            Vector3.zero,
            m_State.IsStopped ? 0.0f : m_State.Curr.Horizontal_Drag,
            m_State.IsStopped ? m_State.Curr.Horizontal_StaticFriction : m_State.Curr.Horizontal_KineticFriction,
            delta
        );

        m_State.Curr.Velocity += dv;

        // change to sliding if moving & crouching
        if (!m_State.IsStopped && m_State.IsCrouching) {
            ChangeTo(Sliding);
            return;
        }

        // change to moving once moving
        if (!m_State.IsStopped || HasMoveInput) {
            ChangeTo(Moving);
            return;
        }
    }

    // -- Moving --
    Phase Moving => new Phase(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update(float delta) {
        // start floating if no longer grounded
        if (!m_State.Prev.IsGrounded) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        // once speed is zero, stop moving
        if (!HasMoveInput && m_State.IsStopped) {
            ChangeToImmediate(NotMoving, delta);
            return;
        }

        // the current ground velocity
        var v = m_State.Curr.GroundVelocity;

        // the current forward & input direction
        var fwd = m_State.Prev.Forward;
        var inputDir = m_Input.Move;
        var inputDotFwd = Vector3.Dot(inputDir, fwd);

        // pivot if direction change was significant
        var shouldPivot = (
            inputDotFwd < m_Tunables.PivotStartThreshold &&
            v.sqrMagnitude > m_Tunables.PivotSqrSpeedThreshold
        );

        if (shouldPivot) {
            ChangeToImmediate(Pivot, delta);
            return;
        }

        // rotate towards input direction
        if (HasMoveInput) {
            fwd = Vector3.RotateTowards(
                fwd,
                inputDir,
                m_Tunables.TurnSpeed * Mathf.Deg2Rad * delta,
                Mathf.Infinity
            );

            m_State.Curr.SetProjectedForward(fwd);
        }

        // intergrate forces
        // 22.10.26: removed static friction when in moving
        var dv = IntegrateForces(
            v,
            m_Tunables.Horizontal_Acceleration * inputDir.magnitude * m_State.Curr.Forward,
            m_State.IsStopped ? 0.0f : m_State.Horizontal_Drag,
            m_State.Horizontal_KineticFriction,
            delta
        );

        m_State.Curr.Velocity += dv;

        // if crouching, change to sliding
        // 22.10.28: if this is an immediate change, you can't get out of crouch; a hack basicallly
        if (m_State.Curr.IsCrouching) {
            ChangeTo(Sliding);
            return;
        }
    }

    // -- Sliding --
    Phase Sliding => new Phase(
        name: "Sliding",
        update: Sliding_Update
    );

    void Sliding_Update(float delta) {
        // start floating if no longer grounded
        if (!m_State.Prev.IsGrounded) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        // get current forward & input direction
        var slideDir = m_State.Curr.CrouchDirection;
        var inputDir = m_Input.Move;

        // split the input axis
        var inputDotSlide = Vector3.Dot(inputDir, slideDir);
        var inputSlide = slideDir * inputDotSlide;
        var inputSlideLateral = inputDir - inputSlide;

        // make lateral thrust proportional to speed in slide direction
        var moveVel = m_State.Curr.GroundVelocity;
        var moveDir = moveVel.normalized;
        var moveMag = moveVel.magnitude;
        var scaleLateral = moveMag * (1.0f - Mathf.Abs(Vector3.Angle(moveDir, slideDir) / 90.0f));

        // calculate lateral thrust
        var thrustLateral = scaleLateral * m_Tunables.Crouch_LateralMaxSpeed * inputSlideLateral;

        // integrate forces
        var dv = IntegrateForces(
            m_State.Curr.GroundVelocity,
            m_Tunables.Horizontal_Acceleration * thrustLateral,
            m_State.IsStopped ? 0.0f : m_State.Horizontal_Drag,
            m_State.IsStopped ? m_State.Horizontal_StaticFriction : m_State.Horizontal_KineticFriction,
            delta
        );

        m_State.Curr.Velocity += dv;

        // once speed is zero, stop moving
        if (m_State.IsStopped) {
            ChangeTo(HasMoveInput ? Moving : NotMoving);
            return;
        }

        // once not crouching change to move/not move state
        if (!m_State.IsCrouching) {
            ChangeTo(!m_State.IsStopped ? Moving : NotMoving);
            return;
        }
    }

    // -- Pivot --
    Phase Pivot => new Phase(
        "Pivot",
        enter: Pivot_Enter,
        update: Pivot_Update,
        exit: Pivot_Exit
    );

    void Pivot_Enter() {
        m_State.Curr.PivotDirection = m_Input.Move;
        m_State.Curr.PivotFrame = 0;
    }

    void Pivot_Update(float delta) {
        if (!m_State.Prev.IsGrounded) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        m_State.Curr.PivotFrame += 1;

        // rotate towards pivot direction
        var dirFacing = m_State.Curr.Forward;
        dirFacing = Vector3.RotateTowards(
            dirFacing,
            m_State.Prev.PivotDirection,
            m_Tunables.PivotSpeed * Mathf.Deg2Rad * delta,
            Mathf.Infinity
        );

        m_State.Curr.SetProjectedForward(dirFacing);

        // calculate next velocity, decelerating towards zero to finish pivot
        var v0 = m_State.Prev.GroundVelocity;
        var dv = Mathf.Min(v0.magnitude, m_Tunables.PivotDeceleration * delta) * v0.normalized;

        // update velocity
        m_State.Curr.Velocity -= dv;

        // once speed is zero, transition to next state
        if (m_State.IsStopped) {
            ChangeTo(HasMoveInput ? Moving : NotMoving);
            return;
        }
    }

    void Pivot_Exit() {
        m_State.Curr.PivotFrame = -1;
    }

    // -- Floating --
    Phase Floating => new Phase(
        name: "Floating",
        update: Floating_Update
    );

    void Floating_Update(float delta) {
        // return to the ground if grounded
        if (m_State.Prev.IsGrounded) {
            var next = m_State.IsCrouching switch {
                true => Sliding,
                false => Moving,
            };

            ChangeToImmediate(next, delta);
            return;
        }

        // add aerial drift
        var v0 = m_State.Prev.PlanarVelocity;
        var vd = m_Input.Move * m_Tunables.AerialDriftAcceleration * delta;
        m_State.Curr.Velocity += vd;
    }

    // -- queries --
    /// if there is any user input
    bool HasMoveInput {
        get => m_Input.Move.sqrMagnitude > 0.0f;
    }

    // -- integrate --
    /// integrate velocity delta from all movement forces
    Vector3 IntegrateForces(
        Vector3 v0,
        Vector3 thrust,
        float drag,
        float friction,
        float delta
    ) {
        // get curr velocity dir & mag
        var v0Dir = v0.normalized;
        var v0SqrMag = v0.sqrMagnitude;

        // get separate acceleration and deceleration
        var acceleration = thrust;
        var deceleration = v0Dir * (friction + drag * v0SqrMag);

        // get velocity delta in each direction
        var dva = thrust * delta;
        var dvd = deceleration * delta;

        // if deceleration would overcome our accelerated velocity, cancel
        // current velocity instead
        var va = v0 + dva;
        if (dvd.sqrMagnitude >= va.sqrMagnitude) {
            return -v0;
        }

        return dva - dvd;
    }
}

}