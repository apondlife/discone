using UnityEngine;

/// toggles perceivability of entities sound effects, dialogue, etc.
sealed class EntityPerception: MonoBehaviour {
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
        var perception = character?.Perception;

        // we can only talk to one character at a time, not ourselves, and
        // whoever is closest
        var talkable = (DisconeCharacter)null;
        var talkableDist = float.MaxValue;

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
            var isAudible = false;
            if (perception != null) {
                isAudible = dist < perception.HearingRadius;
            }

            other.Music.SetIsAudible(isAudible);

            // step 2: find nearest talkable character
            if (perception != null && other != character) {
                if (dist < perception.TalkingRadius && dist < talkableDist) {
                    talkable = other;
                    talkableDist = dist;
                }
            }
        }

        // step 3: update talkability
        foreach (var other in cs) {
            other.Dialogue.SetIsTalkable(other == talkable);
        }
    }
}
