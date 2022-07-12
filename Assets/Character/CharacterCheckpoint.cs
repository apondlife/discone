using ThirdPerson;
using UnityEngine;

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(Character))]
public class CharacterCheckpoint: MonoBehaviour {
    // -- constants --
    private const float k_CastInactive = -1.0f;

    // -- fields --
    [Header("fields")]
    [Tooltip("how long it takes to save")]
    [SerializeField] float m_SaveCastTime;

    [Tooltip("a multiplier on load cast speed")]
    [SerializeField] float m_LoadCastMultiplier;

    // -- refs --
    [Header("refs")]
    [Tooltip("the prefab for the in-world checkpoint")]
    [SerializeField] GameObject m_FlagPrefab;

    // -- props --
    // the character
    Character m_Character;

    /// the saved state, if any
    Checkpoint m_Checkpoint;

    /// a new checkpoint being placed
    Checkpoint m_NewCheckpoint;

    /// the visual representation of the current checkpoint
    GameObject m_Flag;

    /// the current save cast time
    float m_SaveElapsed = k_CastInactive;

    /// the current save cast time
    float m_LoadElapsed = k_CastInactive;

    /// how long it takes to load
    float m_LoadCastTime = 0.0f;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Character = GetComponent<Character>();
    }

    void Update() {
        // if saving a checkpoint
        if (m_SaveElapsed >= 0) {
            // if we can no longer save, cancel it
            if (!CanSave()) {
                CancelSave();
            }
            // otherwise, aggregate until complete
            else {
                m_SaveElapsed += Time.deltaTime;

                if (m_SaveElapsed > m_SaveCastTime) {
                    Save();
                }
            }
        }

        // if loading a checkpoint
        if (m_LoadElapsed >= 0) {
            // if we can no longer load, cancel it
            if (!CanLoad()) {
                CancelLoad();
            }
            // otherwise, aggregate until complete
            else {
                m_LoadElapsed += Time.deltaTime;

                if (m_LoadElapsed > m_LoadCastTime) {
                    Load();
                }
            }
        }
    }

    // -- commands --
    /// start saving a checkpoint
    public void StartSave() {
        // only save if not moving
        if (!CanSave()) {
            return;
        }

        // start saving new checkpoint
        m_SaveElapsed = 0.0f;
        m_NewCheckpoint = Checkpoint.FromState(m_Character.CurrentState);
   }

    /// cancel a save if active
    public void CancelSave() {
        // if there is an active save
        if (m_SaveElapsed < 0.0f) {
            return;
        }

        // cancel it
        m_SaveElapsed = k_CastInactive;
        m_NewCheckpoint = null;
    }

    void Save() {
        // store position
        m_Checkpoint = m_NewCheckpoint;

        // create a new flag
        m_Flag = Instantiate(m_FlagPrefab);

        // move flag to the correct position
        var t = m_Flag.transform;
        t.position = m_Checkpoint.Position;
        t.forward = m_Checkpoint.Forward;

        // clear active save
        m_SaveElapsed = k_CastInactive;
        m_NewCheckpoint = null;
    }

    /// restore to the current checkpoint, if any
    public void StartLoad() {
        // only load if not moving
        if (!CanLoad()) {
            return;
        }

        // start loading current checkpoint
        var distance = Vector3.Distance(
            m_Character.CurrentState.Position,
            m_Checkpoint.Position
        );

        m_LoadCastTime = Mathf.Log10(distance) * m_LoadCastMultiplier;
        m_LoadElapsed = 0.0f;
    }

    /// cancel a save if active
    public void CancelLoad() {
        if (m_LoadElapsed >= 0.0f) {
            m_LoadElapsed = k_CastInactive;
            m_LoadCastTime = k_CastInactive;
        }
    }

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

    // -- queries --
    /// the active save's percent complete
    public float SavePercent {
        get => Mathf.Clamp01(m_SaveElapsed / m_SaveCastTime);
    }

    /// the active load's percent complete
    public float LoadPercent {
        get => m_LoadCastTime > 0.0f ? Mathf.Clamp01(m_LoadElapsed / m_LoadCastTime) : 0.0f;
    }

    // if the character can currently save
    bool CanSave() {
        var s = m_Character.State;

        return (
            s.IsGrounded &&
            s.IsIdle
        );
    }

    // if the character can currently load
    bool CanLoad() {
        return CanSave();
    }
}