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

        // simulate friction
        var vd = SimulateMove(
            m_State.Curr.GroundVelocity,
            Vector3.zero,
            m_Tunables.Horizontal_StaticFriction,
            0.0f,
            delta
        );

        m_State.Curr.Velocity += vd;

        // change to moving once moving
        if (HasInput || !IsStopped) {
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

        // pivot if direction change was significant
        if (Vector3.Dot(dirForward, dirInput) < m_Tunables.PivotStartThreshold) {
            ChangeTo(Pivot);
            return;
        }

        // rotate towards input direction
        if (HasInput) {
            dirForward = Vector3.RotateTowards(
                dirForward,
                dirInput,
                m_Tunables.TurnSpeed * Mathf.Deg2Rad * delta,
                Mathf.Infinity
            );

            m_State.Curr.SetProjectedForward(dirForward);
        }

        var vd = SimulateMove(
            m_State.Prev.GroundVelocity,
            m_Tunables.Horizontal_Acceleration * dirInput.magnitude * m_State.Curr.Forward,
            m_Tunables.Horizontal_KineticFriction,
            m_Tunables.Horizontal_Drag,
            delta
        );

        // update velocity
        m_State.Curr.Velocity += vd;

        // once speed is zero, stop moving
        if (!HasInput && IsStopped) {
            ChangeTo(NotMoving);
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
            ChangeTo(Floating);
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
            ChangeTo(HasInput ? Moving : NotMoving);
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
        if (m_State.Prev.IsGrounded) {
            ChangeTo(Moving);
            return;
        }

        var v0 = m_State.Prev.PlanarVelocity;
        var vd = m_Input.Move * m_Tunables.AerialDriftAcceleration * delta;
        m_State.Curr.Velocity += vd;
    }

    // -- queries --
    /// if there is any user input
    bool HasInput {
        get => m_Input.Move.sqrMagnitude > 0.0f;
    }

    /// if the ground speed is below the movement threshold
    bool IsStopped {
        get => m_State.Curr.GroundVelocity.magnitude < m_Tunables.Horizontal_MinSpeed;
    }

    /// calculate the velocity delta as a result of all movement forces
    Vector3 SimulateMove(
        Vector3 v0,
        Vector3 thrust,
        float friction,
        float drag,
        float delta
    ) {
        // calculate next velocity, integrating input & drag
        // vt = v0 + (acceleration - drag - friction) * t
        var v0_dir = v0.normalized;
        var v0_mag = v0.magnitude;
        var v0_mag2 = v0.sqrMagnitude;

        // deceleration opposes to movement, drag + friction
        var deceleration = v0_dir * (friction + drag * v0_mag2);

        // calculate velocity change this frame (velocity delta)
        var vd = (thrust - deceleration) * delta;

        // split the velocity delta into tangent (colinear) and normal (turning)
        var vd_tan = Vector3.Project(vd, v0_dir);
        var vd_nrm = vd - vd_tan;

        // if the aligned velocity delta produces a aligned-direction change, just stop
        if (vd_tan.sqrMagnitude > v0_mag2 && Vector3.Dot(vd_tan, v0) < 0.0f) {
            vd_tan = -v0;
        }

        return vd_tan + vd_nrm;
    }
}
}