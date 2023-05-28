using System;
using UnityEngine;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState MovementState;
    }
}

/// how the character moves on the ground & air
[Serializable]
sealed class MovementSystem: CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return NotMoving;
    }

    protected override SystemState State {
        get => m_State.Next.MovementState;
        set => m_State.Next.MovementState = value;
    }

    // -- NotMoving --
    Phase NotMoving => new Phase(
        name: "NotMoving",
        update: NotMoving_Update
    );

    void NotMoving_Update(float delta) {
        // start floating if no longer grounded
        if (!m_State.Next.IsOnGround) {
            ChangeTo(Floating);
            return;
        }

        // intergrate forces
        var dv = IntegrateForces(
            m_State.Next.GroundVelocity,
            Vector3.zero,
            m_State.WasStopped ? 0.0f : m_State.Next.Horizontal_Drag,
            m_State.WasStopped ? m_State.Next.Horizontal_StaticFriction : m_State.Next.Horizontal_KineticFriction,
            delta
        );

        m_State.Next.Velocity += dv;

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
        if (!m_State.Next.IsOnGround) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        // once speed is zero, stop moving
        if (!HasMoveInput && m_State.IsStopped) {
            ChangeToImmediate(NotMoving, delta);
            return;
        }

        // the current ground velocity
        var v = m_State.Next.GroundVelocity;

        // the current forward & input direction
        var fwd = m_State.Curr.Forward;
        var inputDir = m_Input.Move;
        var inputDotFwd = Vector3.Dot(inputDir, fwd);

        // pivot if direction change was significant
        var shouldPivot = (
            inputDotFwd < m_Tuning.PivotStartThreshold &&
            v.sqrMagnitude > m_Tuning.PivotSqrSpeedThreshold
        );

        if (shouldPivot) {
            ChangeToImmediate(Pivot, delta);
            return;
        }

        // turn towards input direction
        TurnTowards(
            m_Input.Move,
            m_Tuning.TurnSpeed,
            delta
        );

        // intergrate forces
        // 22.10.26: removed static friction when in moving
        var dv = IntegrateForces(
            v,
            m_Tuning.Horizontal_Acceleration * Vector3.Project(inputDir, m_State.Next.Forward),
            m_State.WasStopped ? 0.0f : m_State.Horizontal_Drag,
            m_State.Horizontal_KineticFriction,
            delta
        );

        m_State.Next.Velocity += dv;

        // if crouching, change to sliding
        // 22.10.28: if this is an immediate change, you can't get out of crouch; a hack basicallly
        if (m_State.Next.IsCrouching) {
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
        if (!m_State.Next.IsOnGround) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        // get current forward & input direction
        var slideDir = m_State.Next.CrouchDirection;
        var inputDir = m_Input.Move;

        // split the input axis
        var inputDotSlide = Vector3.Dot(inputDir, slideDir);
        var inputSlide = slideDir * inputDotSlide;
        var inputSlideLateral = inputDir - inputSlide;

        // make lateral thrust proportional to speed in slide direction
        var moveVel = m_State.Next.GroundVelocity;
        var moveDir = moveVel.normalized;
        var moveMag = moveVel.magnitude;
        var scaleLateral = moveMag * (1.0f - Mathf.Abs(Vector3.Angle(moveDir, slideDir) / 90.0f));

        // calculate lateral thrust
        var thrustLateral = scaleLateral * m_Tuning.Crouch_LateralMaxSpeed * inputSlideLateral;

        // integrate forces
        var dv = IntegrateForces(
            m_State.Next.GroundVelocity,
            m_Tuning.Horizontal_Acceleration * thrustLateral,
            m_State.WasStopped ? 0.0f : m_State.Horizontal_Drag,
            m_State.WasStopped ? m_State.Horizontal_StaticFriction : m_State.Horizontal_KineticFriction,
            delta
        );

        m_State.Next.Velocity += dv;

        // turn towards input direction
        TurnTowards(
            m_Input.Move,
            m_Tuning.Crouch_TurnSpeed,
            delta
        );

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
        m_State.Next.PivotDirection = m_Input.Move;
        m_State.Next.PivotFrame = 0;
    }

    void Pivot_Update(float delta) {
        if (!m_State.Next.IsOnGround) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        m_State.Next.PivotFrame += 1;

        // rotate towards pivot direction
        TurnTowards(
            m_State.Curr.PivotDirection,
            m_Tuning.PivotSpeed,
            delta
        );

        // calculate next velocity, decelerating towards zero to finish pivot
        var v0 = m_State.Curr.GroundVelocity;
        var dv = Mathf.Min(v0.magnitude, m_Tuning.PivotDeceleration * delta) * v0.normalized;

        // update velocity
        m_State.Next.Velocity -= dv;

        // once speed is zero, transition to next state
        if (m_State.IsStopped) {
            ChangeTo(HasMoveInput ? Moving : NotMoving);
            return;
        }
    }

    void Pivot_Exit() {
        m_State.Next.PivotFrame = -1;
    }

    // -- Floating --
    Phase Floating => new Phase(
        name: "Floating",
        update: Floating_Update
    );

    void Floating_Update(float delta) {
        // return to the ground if grounded
        if (m_State.Next.IsOnGround) {
            var next = m_State.IsCrouching switch {
                true => Sliding,
                false => Moving,
            };

            ChangeToImmediate(next, delta);
            return;
        }

        // rotate towards input direction
        // TODO: this should be a discone feature, not a third person one
        // the ability to modify tuning at run time
        if (m_Input.IsCrouchPressed) {
            TurnTowards(
                m_Input.Move,
                m_Tuning.Air_TurnSpeed,
                delta
            );
        }

        // add aerial drift
        var v0 = m_State.Curr.PlanarVelocity;
        var vd = m_Input.Move * m_Tuning.AerialDriftAcceleration * delta;
        m_State.Next.Velocity += vd;
    }

    // -- commands --
    /// turn the character towards the direction by turn speed; impure command
    void TurnTowards(Vector3 direction, float turnSpeed, float delta) {
        // if no direction, do nothing
        if (direction.sqrMagnitude <= 0.0f) {
            return;
        }

        // rotate towards input
        var fwd = Vector3.RotateTowards(
            m_State.Curr.Forward,
            m_Input.Move,
            turnSpeed * Mathf.Deg2Rad * delta,
            Mathf.Infinity
        );

        // project current direction
        m_State.Next.SetProjectedForward(fwd);
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