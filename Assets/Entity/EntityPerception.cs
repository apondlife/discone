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
        var chr = player.Character;
        var perception = chr?.Perception;

        // track perceivability; we can only talk to one (the first) character
        // at a time, and not ourselves
        var isTalkable = false;
        var isPerceivable = perception != null;

        // for every character we're simulating
        foreach (var other in characters.Simulating) {
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

            // step 2: check talking
            // TODO: set talking
            if (!isTalkable && other != chr && perception != null) {
                isTalkable = dist < perception.TalkingRadius;
            }

            other.Dialogue.SetIsTalkable(isTalkable);
        }
    }
}
