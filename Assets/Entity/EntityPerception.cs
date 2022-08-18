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
    /// the player's last character
    DisconeCharacter m_PrevCharacter;

    /// the last talkable character
    DisconeCharacter m_PrevTalkable;

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
        var character = player.Character;

        // set initial prev character once
        if (m_PrevCharacter == null) {
            m_PrevCharacter = character;
        }

        // we can only talk to one character at a time, not ourselves, and
        // whoever is closest
        var talkable = (DisconeCharacter)null;
        var talkableDist = float.MaxValue;

        // if we had a talkable character still in range, default to them
        if (m_PrevTalkable == character) {
            m_PrevTalkable = null;
        }

        if (m_PrevTalkable != null) {
            var dist = Vector3.Distance(
                pos,
                m_PrevTalkable.transform.position
            );

            if (dist < m_TalkingRadius) {
                talkable = m_PrevTalkable;
                talkableDist = dist;
            }
        }

        // for every character we're simulating
        var cs = characters.Simulating;

        // check perceivability by player
        foreach (var other in cs) {
            // get distance to player
            var dist = Vector3.Distance(
                pos,
                other.transform.position
            );

            // step 1: check hearing
            other.Music.SetIsAudible(dist < m_HearingRadius);

            // step 2: if prev character exited talking range, change to current character
            if (other == m_PrevCharacter && dist > m_TalkingRadius) {
                m_PrevCharacter = character;
            }

            // step 3: find nearest talkable character (only check for changes to the talkable
            // character and ignore your previous character)
            if (other != m_PrevTalkable && other != m_PrevCharacter) {
                var isNextTalkable = (
                    other != character &&
                    dist < m_TalkingRadius &&
                    dist < talkableDist
                );

                if (isNextTalkable) {
                    talkable = other;
                    talkableDist = dist;
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
