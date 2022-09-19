using Mirror;
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

    /// the flower at the current checkpoint, if any
    [SyncVar]
    CharacterFlower m_Flower;

    /// a checkpoint in the process of being created
    PendingCheckpoint m_PendingCheckpoint;

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
                var c = m_Flower;
                var pct = Mathf.Clamp01(m_LoadElapsed / m_LoadCastTime);
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
        m_PendingCheckpoint = PendingCheckpoint.FromState(Character.CurrentState);
    }

    /// finish the new save
    void FinishSave() {
        // spawn flower
        Command_CreateCheckpoint(m_PendingCheckpoint.Position, m_PendingCheckpoint.Forward);

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
        m_PendingCheckpoint = null;
    }

    // -- c/s/server
    /// spawn a flower on the ground underneath the character
    [Command]
    void Command_CreateCheckpoint(Vector3 pos, Vector3 fwd) {
        Server_CreateCheckpoint(pos, fwd);
    }

    [Server]
    /// spawn a flower from an existing flower position
    public void Server_CreateCheckpoint(FlowerRec rec) {
        Server_CreateCheckpoint(rec.Pos, rec.Fwd);
    }

    [Server]
    /// spawn a flower at the checkpoint
    void Server_CreateCheckpoint(Vector3 pos, Vector3 fwd) {
        // if we had a flower, let it go
        m_Flower?.Server_Release();

        m_Flower = CharacterFlower.Server_Spawn(
            m_Container.Key,
            pos,
            Quaternion.LookRotation(fwd, Vector3.up)
        );

        // grab it
        // i don't know why this doesn't work in the same frame...
        this.DoNextFrame(() => m_Flower?.Server_Grab());

    }

    /// -- c/load
    /// restore to the current checkpoint, if any
    public void StartLoad() {
        // must have a checkpoint
        if (m_Flower == null) {
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
            m_Flower.Position
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
        Character.ForceState(m_Flower.IntoState());
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
        if (sim == DisconeCharacter.Simulation.None || m_Flower != null) {
            return;
        }

        // create initial character flower
        // TODO: maybe this should be on build not on "start"?
        // TODO: "floating flowers" maybe this should happen when character is first looked at

        // maybe there's an AI
        m_Container.Character.Events.Once(CharacterEvent.Idle, () => {
            if (m_Flower != null) {
                return;
            }

            // if this character has no checkpoints yet, spawn a new one
            // TODO: try sniffing first,
            // TODO: maybesniff move to "bot" character behaviour instead of here
            var checkpoint = PendingCheckpoint.FromState(Character.CurrentState);
            Server_CreateCheckpoint(checkpoint.Position, checkpoint.Forward);
        });
    }

    // -- queries --
    /// if the character is currently saving
    public CharacterFlower Flower {
        get => m_Flower;
    }

    public bool IsSaving {
        get => m_SaveElapsed > 0.0f;
    }

    // if the character can currently save
    bool CanSave {
        get => Character.State.IsGrounded && Character.State.IsIdle;
    }

    ThirdPerson.Character Character {
        get => m_Container.Character;
    }

    // -- types --
    /// a checkpoint in the process of being created
    public sealed class PendingCheckpoint {
        // -- props --
        /// the position
        public readonly Vector3 Position;

        /// the character facing
        public readonly Vector3 Forward;

        // -- lifetime --
        /// create a pending checkpoint
        public PendingCheckpoint(Vector3 position, Vector3 forward) {
            Position = position;
            Forward = forward;
        }

        // -- factories --
        /// create checkpoint from the current state frame
        public static PendingCheckpoint FromState(CharacterState.Frame frame) {
            return new PendingCheckpoint(
                frame.Position,
                frame.Forward
            );
        }
    }
}