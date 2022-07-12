using ThirdPerson;
using UnityEngine;

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(Character))]
public class CharacterCheckpoint: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the prefab for the in-world checkpoint")]
    [SerializeField] GameObject m_FlagPrefab;

    // -- props --
    // the character
    Character m_Character;

    /// the saved state, if any
    Checkpoint m_Checkpoint;

    /// the visual representation of the current checkpoint
    GameObject m_Flag;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Character = GetComponent<Character>();
    }

    // -- commands --
    /// save a checkpoint at the character's current position
    public void Save() {
        // store position
        m_Checkpoint = Checkpoint.FromState(m_Character.CurrentState);

        // create a new flag
        m_Flag = Instantiate(m_FlagPrefab);

        // move flag to the correct position
        var t = m_Flag.transform;
        t.position = m_Checkpoint.Position;
        t.forward = m_Checkpoint.Forward;
    }

    /// restore to the current checkpoint, if any
    public void Load() {
        if (m_Checkpoint == null) {
            return;
        }

        m_Character.ForceState(m_Checkpoint.IntoState());
    }

    // -- checkpoint --
    private sealed class Checkpoint {
        public Vector3 Position { get; private set; }
        public Vector3 Forward { get; private set; }

        public static Checkpoint FromState(CharacterState.Frame frame) {
            return new Checkpoint() {
                Position = frame.Position,
                Forward = frame.Forward
            };
        }

        public CharacterState.Frame IntoState() {
            return new CharacterState.Frame(Position, Forward);
        }
    }
}