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

        // wait until we have a current player
        if (players.Current == null) {
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
            var co = character.gameObject;

            // track activity
            var isActive = co.activeSelf;
            var isActivePrev = isActive;

            // zeroth pass: don't cull a player's character
            var isPlayerControlled = false;

            foreach (var player in players.All) {
                if (player.Character == character) {
                    isActive = true;
                    isPlayerControlled = true;
                    break;
                }
            }

            if (!isPlayerControlled) {
                // first pass: cull any characters in inactive chunks
                var coord = character.Coord.Value;

                // update coord for previously active (e.g. potentially moving) characters
                if (isActivePrev) {
                    // get transforms
                    if (ct == null) {
                        ct = character.transform;
                    }

                    // update coord
                    coord = WorldCoord.FromPosition(ct.position, chunks.ChunkSize);
                    character.Coord.Value = coord;
                }

                isActive = chunks.IsChunkActive(coord);

                // second pass: check against proximity to players
                if (isActive) {
                    isActive = false;

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
                        isActive = dist < pp.VisionRadius;

                        // end if we find one
                        if (isActive) {
                            break;
                        }
                    }
                }
            }

            // finally, update active if changed (very slow to make redundant calls to SetActive)
            if (isActive != isActivePrev) {
                co.SetActive(isActive);
            }
        }
    }
}
