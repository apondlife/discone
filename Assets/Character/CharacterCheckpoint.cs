using ThirdPerson;
using UnityEngine;

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(Character))]
public class CharacterCheckpoint: MonoBehaviour {
    // -- constants --
    private const float k_CastInactive = -1.0f;

    // -- fields --
    [Header("tuning")]
    [Tooltip("how long it takes to save")]
    [SerializeField] float m_SaveCastTime;

    [Tooltip("the time it takes to get to the half distance")]
    [SerializeField] float m_LoadCastMaxTime;

    [Tooltip("the time it takes to the load to travel the point distance")]
    [SerializeField] float m_LoadCastPointTime;

    [Tooltip("the distance the load travels by the point time")]
    [SerializeField] float m_LoadCastPointDistance;

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

    /// if the character is trying to save
    bool m_IsSaveDown;

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
        if (m_IsSaveDown) {
            // stop saving if moving
            if (!CanSave) {
                m_SaveElapsed = k_CastInactive;
                m_NewCheckpoint = null;
            }
            // start a save if there isn't one
            else if (m_SaveElapsed == k_CastInactive) {
                m_SaveElapsed = 0.0f;
                m_NewCheckpoint = Checkpoint.FromState(m_Character.CurrentState);
            }
            // otherwise, keep saving
            else {
                m_SaveElapsed += Time.deltaTime;

                if (m_SaveElapsed > m_SaveCastTime) {
                    Save();
                    m_IsSaveDown = false;
                }
            }
        }

        // if loading a checkpoint
        if (m_LoadElapsed >= 0) {
            // if we can no longer load, cancel it
            if (!CanLoad) {
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
        m_IsSaveDown = true;
   }

    /// cancel a save if active
    public void CancelSave() {
        m_IsSaveDown = false;
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
        if (!CanLoad) {
            return;
        }

        // get distance to current checkpoint
        var distance = Vector3.Distance(
            m_Character.CurrentState.Position,
            m_Checkpoint.Position
        );

        // calculate cast time
        var f_d = m_LoadCastPointTime / m_LoadCastMaxTime;
        var d = m_LoadCastPointDistance;
        var k = f_d / (d * (1 - f_d));
        m_LoadCastTime = m_LoadCastMaxTime * (1 - 1 / (k * distance + 1));
        Debug.Log($"time is {m_LoadCastTime} for distance {distance}");

        // and start load
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
    bool CanSave {
        get {
            var s = m_Character.State;
            return s.IsGrounded && s.IsIdle;
        }
    }

    // if the character can currently load
    bool CanLoad {
        get => CanSave;
    }
}