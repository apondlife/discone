using UnityEngine;
using System.Linq;

namespace ThirdPerson {

/// how the character is affected by gravity
sealed class CollisionSystem: CharacterSystem {
    // -- lifetime --
    public CollisionSystem(CharacterData character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return Active;
    }

    // -- NotIdle --
    CharacterPhase Active => new CharacterPhase(
        name: "Active",
        update: Active_Update
    );

    void Active_Update() {
        var v = m_State.Curr.Velocity;

        // update controller from character state
        if (v.sqrMagnitude > 0) {
            m_Controller.Move(
                m_State.Curr.Position,
                v * Time.deltaTime,
                m_State.Curr.Up
            );
        }

        // find the ground collision if it exists
        m_State.Curr.Ground = m_Controller.Collisions
            .LastOrDefault((c) => c.Surface == CollisionSurface.Ground);

        m_State.Curr.Wall = m_Controller.Collisions
            .LastOrDefault((c) => c.Surface == CollisionSurface.Wall);

        // sync controller state back to character state
        m_State.Curr.Velocity = m_Controller.Velocity;
        m_State.Curr.Position = m_Controller.Position;
    }
}

}