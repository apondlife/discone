using UnityAtoms;
using UnityEngine;

/// culls out-of-range entities, simulating only those near players
sealed class EntityCulling: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("how far the character can see others")]
    [SerializeField] float m_VisionRadius;

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
            // track activity
            var isSimulating = character.IsSimulating;

            // zeroth pass: don't cull a player's character
            var isPlayerDriven = characters.IsDriven(character);
            if (isPlayerDriven) {
                isSimulating = true;
            }

            if (!isPlayerDriven) {
                // first pass: cull any characters in inactive chunks
                var coord = character.Coord;

                // update coord for previously active (e.g. potentially moving) characters
                if (isSimulating) {
                    // update coord
                    coord.Value = coord.FromPosition(character.Position);
                }

                isSimulating = chunks.IsChunkActive(coord.Value);

                // second pass: check against proximity to players
                if (isSimulating) {
                    isSimulating = false;

                    // see if any player has vision
                    foreach (var player in ps) {
                        // skip players with no character
                        if (player.Character == null) {
                            continue;
                        }

                        // check for vision
                        var dist = Vector3.Distance(player.Position.XNZ(), character.Position.XNZ());
                        isSimulating = dist < m_VisionRadius;

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
