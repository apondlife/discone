using ThirdPerson;
using UnityEngine;
using Mirror;

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(Character))]
public class CharacterCheckpoint: NetworkBehaviour {
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

    [Tooltip("how faster cancelling a load is than the load itself")]
    [SerializeField] float m_LoadCancelMultiplier = 1;

    // -- config --
    [Header("config")]
    [Tooltip("the layer mask for the ground")]
    [SerializeField] LayerMask m_GroundMask;

    // -- refs --
    [Header("refs")]
    [Tooltip("the prefab for the in-world checkpoint")]
    [SerializeField] CharacterFlower m_FlowerPrefab;

    // -- props --
    // the character
    Character m_Character;

    /// the saved state, if any
    Checkpoint m_Checkpoint;

    /// a new checkpoint being placed
    Checkpoint m_NewCheckpoint;

    /// the visual representation of the current checkpoint
    CharacterFlower m_Flower;

    /// if the character is trying to save
    bool m_IsSaveDown;

    /// the current save cast time
    float m_SaveElapsed = k_CastInactive;

    /// if the character is trying to load
    bool m_IsLoadDown;

    /// the current save cast time
    float m_LoadElapsed = k_CastInactive;

    /// how long it takes to load
    float m_LoadCastTime = 0.0f;

    /// the position when the load starts
    Vector3 m_LoadStartPosition;

    /// the forward direction when the load starts
    Vector3 m_LoadStartForward;

    /// pre-allocated buffer for ground raycasts
    RaycastHit[] m_Hits = new RaycastHit[1];

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Character = GetComponent<Character>();
    }

    void Update() {
        // if we don't have authority, do nothing
        if (!hasAuthority || !isClient) {
            return;
        }

        // if saving a checkpoint
        if (m_IsSaveDown) {
            // stop saving if moving
            if (!CanSave) {
                StopSave();
            }

            // start a save if there isn't one
            else if (m_SaveElapsed == k_CastInactive) {
                InitSave();
            }
            // otherwise, keep saving
            else {
                m_SaveElapsed += Time.deltaTime;

                if (m_SaveElapsed > m_SaveCastTime) {
                    FinishSave();
                    m_IsSaveDown = false;
                }
            }
        } else {
            m_SaveElapsed = k_CastInactive;
        }

        // if loading, aggregate time
        if (m_IsLoadDown) {
            m_LoadElapsed += Time.deltaTime;
        } else if (m_LoadElapsed >= 0.0f) {
            m_LoadElapsed -= Time.deltaTime * m_LoadCancelMultiplier;
        }

        // if we returned to the start point, stop
        if (m_LoadElapsed < 0.0f) {
            StopLoad();
        }
        // otherwise, interpoloate
        else {
            // interpolate position during load
            var t = transform;
            var c = m_Checkpoint;
            var pct = LoadPercent;
            var k = pct * pct;
            t.position = Vector3.Lerp(m_LoadStartPosition, c.Position, k);
            t.forward = Vector3.Lerp(m_LoadStartForward, c.Forward, k);

            // finish the load once elapsed
            if (m_LoadElapsed > m_LoadCastTime) {
                FinishLoad();
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

    /// start a new save
    void InitSave() {
        m_SaveElapsed = 0.0f;
        m_NewCheckpoint = Checkpoint.FromState(m_Character.CurrentState);
    }

    /// stop the new save
    void StopSave() {
        m_SaveElapsed = k_CastInactive;
        m_NewCheckpoint = null;
    }

    /// finish the new save
    void FinishSave() {
        // store position
        m_Checkpoint = m_NewCheckpoint;

        // release old flower
        // m_Flower?.Release();

        // create a new flower
        // m_Flower = SpawnFlower();

        Server_SpawnFlower();

        // clear active save
        m_SaveElapsed = k_CastInactive;
        m_NewCheckpoint = null;
    }

    // -- c/network
    [Command]
    public void Server_SpawnFlower() {
        m_Flower?.Server_Release();

        // find ground position
        var hits = Physics.RaycastNonAlloc(
            m_Character.transform.position,
            Vector3.down,
            m_Hits,
            3.0f,
            m_GroundMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits <= 0) {
            Debug.LogError("[checkpoint] failed to find flower ground point");
            return;
        }

        // instantiate flower at hit point
        var hit = m_Hits[0];
        m_Flower = Instantiate(
            m_FlowerPrefab,
            hit.point,
            Quaternion.identity
        );

        // spawn the game object for everyone
        NetworkServer.Spawn(m_Flower.gameObject);
    }

    /// restore to the current checkpoint, if any
    public void StartLoad() {
        // must have a checkpoint
        if (m_Checkpoint == null) {
            return;
        }

        // track input
        m_IsLoadDown = true;

        // if we're already loading, don't initializ
        if (m_LoadCastMaxTime < 0.0f) {
            return;
        }

        // get distance to current checkpoint
        var distance = Vector3.Distance(
            m_Character.CurrentState.Position,
            m_Checkpoint.Position
        );

        // calculate cast time
        var f = m_LoadCastPointTime / m_LoadCastMaxTime;
        var d = m_LoadCastPointDistance;
        var k = f / (d * (1 - f));
        m_LoadCastTime = m_LoadCastMaxTime * (1 - 1 / (k * distance + 1));

        // pause the character
        m_Character.Pause();

        // and start load
        m_LoadElapsed = 0.0f;
        m_LoadStartPosition = transform.position;
        m_LoadStartForward = transform.forward;
    }

    /// cancel a load if active
    public void CancelLoad() {
        m_IsLoadDown = false;
    }

    // stop loading wherever the character is
    void StopLoad() {
        // reset state
        m_IsLoadDown = false;
        m_LoadElapsed = k_CastInactive;
        m_LoadCastTime = k_CastInactive;

        // resume simulation
        m_Character.Unpause();
    }

    /// restore to the state
    void FinishLoad() {
        m_Character.ForceState(m_Checkpoint.IntoState());
        StopLoad();
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
    public float SaveElapsed {
        get => m_SaveElapsed;
    }

    /// the active load's percent complete
    public float LoadPercent {
        get => m_LoadCastTime > 0.0f ? Mathf.Clamp01(m_LoadElapsed / m_LoadCastTime) : 0.0f;
    }

    // if the character can currently save
    bool CanSave {
        get => m_Character.State.IsGrounded && m_Character.State.IsIdle;
    }
}