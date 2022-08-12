using UnityAtoms;
using UnityEngine;

/// processes collisions (perception) for all players and characters
[RequireComponent(typeof(Entities))]
sealed class EntityCollisions: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the world")]
    [SerializeField] WorldVariable m_World;

    // -- props --
    /// the entities
    Entities m_Entities;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Entities = GetComponent<Entities>();
    }

    void FixedUpdate() {
        // get repos
        var players = m_Entities.Players;
        var characters = m_Entities.Characters;

        // wait until there is player with a character
        if (players.Current?.Character == null) {
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

            foreach (var player in players.All) {
                if (player.Character == character) {
                    isSimulating = true;
                    isPlayerControlled = true;
                    break;
                }
            }

            if (!isPlayerControlled) {
                // first pass: cull any characters in inactive chunks
                var coord = character.Coord.Value;

                // update coord for previously active (e.g. potentially moving) characters
                if (isSimulating) {
                    // get transforms
                    if (ct == null) {
                        ct = character.transform;
                    }

                    // update coord
                    coord = WorldCoord.FromPosition(ct.position, chunks.ChunkSize);
                    character.Coord.Value = coord;
                }

                isSimulating = chunks.IsChunkActive(coord);

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
                        var dist = Vector3.Distance(pt.position, ct.position);
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
