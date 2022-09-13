using Mirror;
using System.Linq;
using System;
using ThirdPerson;
using UnityEngine;

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(DisconeCharacter))]
public class CharacterCheckpoint: NetworkBehaviour {
    // -- constants --
    /// a sentinel for inactive casts
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

    [Tooltip("how offset the flower is forward, so it doesn't spawn under the character")]
    [SerializeField] float m_FlowerForwardOffset = 0.2f;

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
    DisconeCharacter m_Container;

    /// the saved state, if any
    [SyncVar]
    Checkpoint m_Checkpoint;

    /// a new Checkpoint that is attempted to be created
    Checkpoint m_TentativeCheckpoint;

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
    float m_LoadCastTime = k_CastInactive;

    /// the state when the load starts
    CharacterState.Frame m_LoadStartState;

    /// pre-allocated buffer for ground raycasts
    RaycastHit[] m_Hits = new RaycastHit[1];

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Container = GetComponent<DisconeCharacter>();
    }

    void Update() {
        // if we don't have authority, do nothing
        if (!netIdentity.IsOwner()) {
            return;
        }

        // if saving a checkpoint
        if (m_IsSaveDown) {
            // stop saving if moving
            if (!CanSave) {
                CancelSave();
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
            m_LoadElapsed -= Mathf.Max(0, Time.deltaTime * m_LoadCancelMultiplier);
        }

        // if we are currently loading
        if (m_LoadCastTime != k_CastInactive) {
            // if we returned to the start point, stop
            if (m_LoadElapsed < 0.0f) {
                CancelLoad();
            }
            // otherwise, interpoloate
            else {
                // interpolate position during load
                var t = transform;
                var c = m_Checkpoint;
                var pct = LoadPercent;
                var k = pct * pct;
                var pos = Vector3.Lerp(m_LoadStartState.Position, c.Position, k);

                var rot = Quaternion.Slerp(m_LoadStartState.LookRotation, c.Rotation, k);
                var fwd = rot * Vector3.forward;
                var state = new ThirdPerson.CharacterState.Frame(pos, fwd);
                Character.ForceState(state);

                // finish the load once elapsed
                if (m_LoadElapsed > m_LoadCastTime) {
                    FinishLoad();
                }
            }
        }
    }

    // -- l/mirror --
    [Server]
    public override void OnStartServer() {
        base.OnStartServer();

        m_Container.OnSimulationChanged += Server_OnSimulationChanged;
    }

    // -- commands --
    // -- c/save
    /// start saving a checkpoint
    public void StartSave() {
        m_IsSaveDown = true;
   }

    /// stop a save if active
    public void StopSave() {
        m_IsSaveDown = false;
    }

    /// start a new save
    void InitSave() {
        m_SaveElapsed = 0.0f;
        m_TentativeCheckpoint = Checkpoint.FromState(Character.CurrentState);
    }

    /// finish the new save
    void FinishSave() {
        // spawn flower
        Server_RequestCheckpoint(m_TentativeCheckpoint);

        // reset state
        ResetSave();
    }

    /// cancel the active save
    void CancelSave() {
        ResetSave();
    }

    /// reset save to initial state
    void ResetSave() {
        m_SaveElapsed = k_CastInactive;
        m_TentativeCheckpoint = null;
    }

    // -- c/s/server
    /// spawn a flower on the ground underneath the character
    [Command]
    void Server_RequestCheckpoint(Checkpoint newCheckpoint) {
        Server_CreateCheckpoint(newCheckpoint);
    }

    [Server]
    void Server_CreateCheckpoint(Checkpoint newCheckpoint) {
        // store position
        m_Checkpoint = newCheckpoint;

        // ???
        m_Flower?.Server_Release();

        // TODO: maybe move this to Flower.Spawn?
        // find ground position
        var hits = Physics.RaycastNonAlloc(
            newCheckpoint.Position + newCheckpoint.Forward * m_FlowerForwardOffset,
            Vector3.down,
            m_Hits,
            10.0f,
            m_GroundMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits <= 0) {
            Debug.LogError($"[chkpnt] failed to find flower for {m_Container.name} ground point");
        }

        // instantiate flower at hit point
        var pos = hits > 0 ? m_Hits[0].point : newCheckpoint.Position;
        CharacterFlower.Spawn(m_Container.Key, pos);
    }

    /// -- c/load
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
            Character.CurrentState.Position,
            m_Checkpoint.Position
        );

        // calculate cast time
        var f = m_LoadCastPointTime / m_LoadCastMaxTime;
        var d = m_LoadCastPointDistance;
        var k = f / (d * (1 - f));
        m_LoadCastTime = m_LoadCastMaxTime * (1 - 1 / (k * distance + 1));

        // pause the character
        Character.Pause();

        // and start load
        m_LoadElapsed = 0.0f;
        m_LoadStartState = Character.CurrentState;
    }

    /// cancel a load if active
    public void StopLoad() {
        m_IsLoadDown = false;
    }

    /// restore to the state
    void FinishLoad() {
        Character.ForceState(m_Checkpoint.IntoState());
        ResetLoad();
    }

    // stop loading wherever the character is
    void CancelLoad() {
        Character.ForceState(m_LoadStartState);
        ResetLoad();
    }

    // reset loading state
    void ResetLoad() {
        // reset state
        m_IsLoadDown = false;
        m_LoadElapsed = k_CastInactive;
        m_LoadCastTime = k_CastInactive;

        // resume simulation
        Character.Unpause();
    }

    // -- events --
    [Server]
    void Server_OnSimulationChanged(DisconeCharacter.Simulation sim) {
        if (sim != DisconeCharacter.Simulation.None && m_Checkpoint == null) {
            // create initial character flower
            // TODO: maybe this should be on build not on "start"?
            // TODO: "floating flowers" maybe this should happen when character is first looked at

            // maybe there's an AI
            m_Container.Character.Events.Once(CharacterEvent.Idle, () => {
                var initialCheckpoint = Checkpoint.FromState(Character.CurrentState);
                Server_CreateCheckpoint(initialCheckpoint);
            });
        }
    }

    // -- queries --
    /// if the character is currently saving
    public CharacterFlower Flower {
        get => m_Flower;
    }

    public bool IsSaving {
        get => m_SaveElapsed > 0.0f;
    }

    /// the active load's percent complete
    float LoadPercent {
        get => m_LoadCastTime > 0.0f ? Mathf.Clamp01(m_LoadElapsed / m_LoadCastTime) : 0.0f;
    }

    // if the character can currently save
    bool CanSave {
        get => Character.State.IsGrounded && Character.State.IsIdle;
    }

    ThirdPerson.Character Character {
        get => m_Container.Character;
    }

    // -- types --
    /// a checkpoint state
    [Serializable]
    public sealed class Checkpoint {
        // -- props --
        /// the position
        public Vector3 Position;

        /// the character facing
        public Vector3 Forward;

        /// the character rotation
        Quaternion m_Rotation;

        // -- lifetime --
        public Checkpoint() {
        }

        /// create a new checkpoint
        public Checkpoint(Vector3 position, Vector3 forward) {
            Position = position;
            Forward = forward;
        }

        // -- conversions --
        /// create checkpoint from the current state frame
        public static Checkpoint FromState(CharacterState.Frame frame) {
            return new Checkpoint(
                frame.Position,
                frame.Forward
            );
        }

        /// create state frame from checkpoint
        public CharacterState.Frame IntoState() {
            return new CharacterState.Frame(
                Position,
                Forward
            );
        }

        /// -- queries --
        /// the character rotation
        public Quaternion Rotation {
            get {
                if (m_Rotation == null) {
                    m_Rotation = Quaternion.LookRotation(Forward, Vector3.up);
                }
                return m_Rotation;
            }
        }
    }
}