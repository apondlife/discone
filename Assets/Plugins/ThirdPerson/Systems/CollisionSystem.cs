using System;

namespace ThirdPerson {

// /// system state extensions
// partial class CharacterState {
//     partial class Frame {
//         /// .
//         public SystemState CollisionState;
//     }
// }

/// how the character is affected by gravity
[Serializable]
sealed class CollisionSystem: CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return Active;
    }

    protected override SystemState State {
        get => m_State.Next.CrouchState;
    }

    // -- NotIdle --
    Phase Active => new Phase(
        name: "Active",
        update: Active_Update
    );

    void Active_Update(float delta) {
        var v = m_State.Next.Velocity;

        // move character using controller if not idle
        var frame = m_Controller.Move(
            m_State.Next.Position,
            m_State.Next.Velocity,
            m_State.Next.Up,
            delta
        );

        // find the ground collision if it exists
        m_State.Next.Ground = frame.Ground;
        m_State.Next.Wall = frame.Wall;

        // sync controller state back to character state
        m_State.Next.Velocity = frame.Velocity;
        m_State.Next.Acceleration = (m_State.Next.Velocity - m_State.Curr.Velocity) / delta;
        m_State.Next.Position = frame.Position;
    }
}

}