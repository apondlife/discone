using UnityEngine;

/// toggles perceivability of entities sound effects, dialogue, etc.
sealed class EntityPerception: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("how far the character can hear others")]
    [SerializeField] float m_HearingRadius;

    [Tooltip("how far the character can talk to others")]
    [SerializeField] float m_TalkingRadius;

    // -- props --
    /// the hearing distance (square dist)
    float m_MaxHearingDist;

    /// the talking distance (square dist)
    float m_MaxTalkingDist;

    /// the player's last character
    DisconeCharacter m_PrevCharacter;

    /// the last talkable character
    DisconeCharacter m_PrevTalkable;

    // -- lifecycle --
    void Awake() {
        // pre-calculate distances
        m_MaxHearingDist = m_HearingRadius * m_HearingRadius;
        m_MaxTalkingDist = m_TalkingRadius * m_TalkingRadius;
    }

    // -- command --
    /// recalculate and cull out-of-range entities
    public void Run(Entities entities) {
        // get repos
        var players = entities.Players;
        var characters = entities.Characters;

        // if we don't have a player character, there's nothing to perceive
        var player = players.Current;
        if (player == null) {
            return;
        }

        // get player character details
        var pos = player.transform.position;
        var playerCharacter = player.Character;

        // set initial prev character once
        if (m_PrevCharacter == null) {
            m_PrevCharacter = playerCharacter;
        }

        // we can only talk to one character at a time, not ourselves, and
        // whoever is closest
        var talkable = (DisconeCharacter)null;
        var talkableDist = float.MaxValue;

        // if we just switched, don't try to talk to our old character
        if (m_PrevTalkable == playerCharacter) {
            m_PrevTalkable = null;
        }

        // if we had a talkable character still in range, default to them
        if (m_PrevTalkable != null) {
            var delta = pos - m_PrevTalkable.transform.position;
            var distXz = delta.XNZ().sqrMagnitude;
            var distY = delta.y * delta.y;

            var talkDist = distXz;
            if (distY > m_MaxTalkingDist) {
                talkDist = Mathf.Infinity;
            }

            if (talkDist < m_MaxTalkingDist) {
                talkable = m_PrevTalkable;
                talkableDist = talkDist;
            }
        }

        // for every character we're simulating
        var cs = characters.Simulating;

        // check perceivability by player
        foreach (var other in cs) {
            // get distance to player
            var delta = pos - other.transform.position;
            var distXz = delta.XNZ().sqrMagnitude;
            var distY = delta.y * delta.y;

            // step 1: check hearing (spherical)
            var hearDist = distXz + distY;
            if (other.Music) other.Music.SetIsAudible(hearDist < m_MaxHearingDist);

            // step 2: check talking (cylindrical)
            var talkDist = distXz;
            if (distY > m_MaxTalkingDist) {
                talkDist = Mathf.Infinity;
            }

            // step 2.a: if prev character exited talking range, change to current character
            if (other == m_PrevCharacter && talkDist > m_MaxTalkingDist) {
                m_PrevCharacter = playerCharacter;
            }

            // step 2.b: find nearest talkable character (only check for changes to the talkable
            // character and ignore your previous character)
            if (other != m_PrevTalkable && other != m_PrevCharacter) {
                var isNextTalkable = (
                    other != playerCharacter &&
                    talkDist < m_MaxTalkingDist &&
                    talkDist < talkableDist
                );

                if (isNextTalkable) {
                    talkable = other;
                    talkableDist = talkDist;
                }

                other.Dialogue.SetIsTalkable(false);
            }
        }

        // step 3: update talkability of nearest character (and previous, if changed)
        if (talkable != null) {
            talkable.Dialogue.SetIsTalkable(true);
        }

        if (m_PrevTalkable != null && talkable != m_PrevTalkable) {
            m_PrevTalkable.Dialogue.SetIsTalkable(false);
        }

        m_PrevTalkable = talkable;
    }

}
