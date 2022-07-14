using UnityEngine;
using Mirror;
using ThirdPerson;

/// wrap the character from the bottom -> top of the world, if necessary
public class CharacterWrap : NetworkBehaviour {
    /// the min y-position the character wraps from
    [SerializeField] float m_WrapMinY = -4000.0f;

    /// the max y-position the character wraps to
    [SerializeField] float m_WrapMaxY = 6000.0f;

    Character m_Character;

    private void Awake() {
        m_Character = GetComponent<Character>();
    }

    private void FixedUpdate() {
        // if we don't have authority, do nothing
        if (!hasAuthority || !isClient) {
            return;
        }

        if (m_Character.IsPaused) {
          return;
        }

        var state = m_Character.CurrentState;

        // if we haven't reached the min y, do nothing
        if (state.Position.y > m_WrapMinY) {
            return;
        }

        // wrap to the max y (we shouldn't need to force state b/c the frame
        // is a reference type, but in case that changes...)
        state.Position.y = m_WrapMaxY;
        m_Character.ForceState(state);
    }
}