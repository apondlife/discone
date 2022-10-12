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
        enter: NotMoving_Enter,
        update: NotMoving_Update
    );

    void NotMoving_Enter() {
        // cancel horizontal movement
        m_State.Curr.Velocity -= m_State.Curr.GroundVelocity;
    }

    void NotMoving_Update(float delta) {
        // start floating if no longer grounded
        if (!m_State.Prev.IsGrounded) {
            ChangeTo(Floating);
            return;
        }

        var vd = IntegrateVelocity(
            m_State.Curr.GroundVelocity,
            delta,
            new Move(
                thrust: Vector3.zero,
                drag: 0.0f,
                friction: m_State.Horizontal_StaticFriction
            )
        );

        m_State.Curr.Velocity += vd;

        // change to sliding if moving & crouching
        if (!IsStopped && m_State.IsCrouching) {
            ChangeTo(Sliding);
            return;
        }

        // change to moving once moving
        if (!IsStopped || HasMoveInput) {
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
            ChangeTo(Floating);
            return;
        }

        // get current forward & input direction
        var dirForward = m_State.Curr.Forward;
        var dirInput = m_Input.Move;
        var dotForward = Vector3.Dot(dirForward, dirInput);

        // pivot if direction change was significant
        if (dotForward < m_Tunables.PivotStartThreshold) {
            ChangeToImmediate(Pivot, delta);
            return;
        }

        // rotate towards input direction
        if (HasMoveInput) {
            dirForward = Vector3.RotateTowards(
                dirForward,
                dirInput,
                m_Tunables.TurnSpeed * Mathf.Deg2Rad * delta,
                Mathf.Infinity
            );

            m_State.Curr.SetProjectedForward(dirForward);
        }

        // intergrate move
        var vd = IntegrateVelocity(
            m_State.Prev.GroundVelocity,
            delta,
            new Move(
                thrust: m_Tunables.Horizontal_Acceleration * dirInput.magnitude * m_State.Curr.Forward,
                drag: m_Tunables.Horizontal_Drag,
                friction: m_State.Horizontal_KineticFriction
            )
        );

        // update velocity
        m_State.Curr.Velocity += vd;

        // once speed is zero, stop moving
        if (!HasMoveInput && IsStopped) {
            ChangeTo(NotMoving);
            return;
        }

        // if crouching, changet to sliding
        if (m_State.Curr.IsCrouching) {
            ChangeTo(Sliding);
            return;
        }
    }

    // -- Sliding --
    /// the slide's initial direction
    Vector3 m_DirSlide;

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
        var groundVel = m_State.Prev.GroundVelocity;
        var groundDir = groundVel.normalized;
        var groundMag = groundVel.magnitude;
        var scaleLateral = groundMag * (1.0f - Mathf.Abs(Vector3.Angle(groundDir, slideDir) / 90.0f));

        // calculate lateral thrust
        var thrustLateral = scaleLateral * m_Tunables.Crouch_LateralMaxSpeed * inputSlideLateral;

        // run move
        var vd = IntegrateVelocity(
            m_State.Prev.GroundVelocity,
            delta,
            new Move(
                thrust: m_Tunables.Horizontal_Acceleration * thrustLateral,
                drag: m_Tunables.Horizontal_Drag,
                friction: m_State.Horizontal_KineticFriction
            )
        );

        // update velocity
        m_State.Curr.Velocity += vd;

        // once speed is zero, stop moving
        if (!HasMoveInput && IsStopped) {
            ChangeTo(NotMoving);
            return;
        }

        // once not crouching change to move/not move state
        if (!m_State.IsCrouching) {
            ChangeTo(!IsStopped ? Moving : NotMoving);
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
        var vd = Mathf.Min(v0.magnitude, m_Tunables.PivotDeceleration * delta) * v0.normalized;

        // update velocity
        m_State.Curr.Velocity -= vd;

        // once speed is zero, transition to next state
        if (IsStopped) {
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

    /// if the ground speed is below the movement threshold
    bool IsStopped {
        get => m_State.Curr.GroundVelocity.magnitude < m_Tunables.Horizontal_MinSpeed;
    }

    // -- simulate --
    /// the move params
    readonly struct Move {
        /// the thrust acceleration to apply
        public readonly Vector3 Thrust;

        /// the quadratic drag
        public readonly float Drag;

        /// the constant friction
        public readonly float Friction;

        /// create a new move (theoretically on the stack)
        /// TODO: use profiler to figure out if this is causing allocations
        public Move(Vector3 thrust, float drag, float friction) {
            Thrust = thrust;
            Drag = drag;
            Friction = friction;
        }
    }

    /// integrate velocity delta from all movement forces
    Vector3 IntegrateVelocity(
        Vector3 v0,
        float delta,
        in Move move
    ) {
        // calculate next velocity, integrating input & drag
        // vt = v0 + (acceleration - drag - friction) * t
        var v0_dir = v0.normalized;
        var v0_mag2 = v0.sqrMagnitude;

        var v1 = Mathx.Integrate_Heun(Acceleration, v0, delta, move);
        var vd = v1 - v0;

        // split the velocity delta into tangent (colinear) and normal (turning)
        var vd_tan = Vector3.Project(vd, v0_dir);
        var vd_nrm = vd - vd_tan;

        // if the velocity delta produces an aligned-direction change, just stop
        if (vd_tan.sqrMagnitude > v0_mag2 && Vector3.Dot(vd_tan, v0) < 0.0f) {
            vd_tan = -v0;
        }

        return vd_tan + vd_nrm;
    }

    /// calculate the acceleration for a move
    Vector3 Acceleration(Vector3 vel, Move m) {
        var v_dir = vel.normalized;
        var v_mag2 = vel.sqrMagnitude;

        // deceleration opposes movement, friction + drag * v0 ^ 2
        var deceleration = v_dir * (m.Friction + m.Drag * v_mag2);
        var acceleration = (m.Thrust - deceleration);

        return acceleration;
    }
}

}