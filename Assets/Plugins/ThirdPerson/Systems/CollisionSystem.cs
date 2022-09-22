using System;
using UnityEngine;

namespace ThirdPerson {

/// how the character is affected by gravity
[Serializable]
sealed class CollisionSystem: CharacterSystem {
    // -- lifetime --
    protected override Phase InitInitialPhase() {
        return Active;
    }

    // -- NotIdle --
    Phase Active => new Phase(
        name: "Active",
        update: Active_Update
    );

    void Active_Update(float delta) {
        var v = m_State.Curr.Velocity;

        // move character using controller if not idle
        if (v.sqrMagnitude > 0.0f) {
            m_Controller.Move(
                m_State.Curr.Position,
                v * delta,
                m_State.Curr.Up
            );
        }

        // find the ground collision if it exists
        m_State.Curr.Ground = m_Controller.Ground;
        m_State.Curr.Wall = m_Controller.Wall;

        // sync controller state back to character state
        m_State.Curr.Velocity = m_Controller.Velocity;
        m_State.Curr.Acceleration = (m_State.Curr.Velocity - m_State.Prev.Velocity) / delta;
        m_State.Curr.Position = m_Controller.Position;
    }
}

}