using UnityAtoms;
using UnityEngine;

/// culls out-of-range entities, simulating only those near players
sealed class EntityCulling: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the world")]
    [SerializeField] WorldVariable m_World;

    // -- command --
    /// recalculate and cull out-of-range entities
    public void Run(Entities entities) {
        // get repos
        var players = entities.Players;
        var characters = entities.Characters;

        // if there are no players, don't try culling
        if (!players.Any) {
            return;
        }

        // get world chunks
        var chunks = m_World.Value.Chunks;

        // get cullers and cullees
        var ps = players.FindCullers();
        var cs = characters.All;

        // for every character
        foreach (var character in cs) {
            // get components refs (only get transform once we need it)
            var ct = null as Transform;

            // track activity
            var isSimulating = character.IsSimulating;

            // zeroth pass: don't cull a player's character
            var isPlayerControlled = false;

            // TODO: online player sends events when a character is captured/released
            // and the characters repo listens to those events to manage a set of characters
            foreach (var player in players.All) {
                if (player.Character == character) {
                    isSimulating = true;
                    isPlayerControlled = true;
                    break;
                }
            }

            if (!isPlayerControlled) {
                // first pass: cull any characters in inactive chunks
                var coord = character.Coord;

                // update coord for previously active (e.g. potentially moving) characters
                if (isSimulating) {
                    // get transforms
                    if (ct == null) {
                        ct = character.transform;
                    }

                    // update coord
                    coord.Value = coord.FromPosition(ct.position);
                }

                isSimulating = chunks.IsChunkActive(coord.Value);

                // second pass: check against proximity to players
                if (isSimulating) {
                    isSimulating = false;

                    // see if any player has vision
                    foreach (var player in ps) {
                        // skip players with no character
                        var pp = player.Character?.Perception;
                        if (pp == null) {
                            continue;
                        }

                        // get transforms
                        var pt = player.transform;
                        if (ct == null) {
                            ct = character.transform;
                        }

                        // check for vision
                        var dist = Vector3.Distance(pt.position.XNZ(), ct.position.XNZ());
                        isSimulating = dist < pp.VisionRadius;

                        // end if we find one
                        if (isSimulating) {
                            break;
                        }
                    }
                }
            }

            // finally, update simulating state
            character.SetSimulating(isSimulating);
        }
    }
}
