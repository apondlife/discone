using Mirror;
using UnityEngine;
using UnityAtoms;
using ThirdPerson;

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(DisconeCharacter))]
public class CharacterCheckpoint: NetworkBehaviour {
    /// the character's state in the save process
    /// not => smelling => (grab) planting => (plant) done
    sealed class SaveSystem: ThirdPerson.System {
        public class Tunables {
            public float SmellDuration;
            public float PlantDuration;
        }

        public class SaveInput {
            public bool IsSaving;
        }

        // -- deps --
        /// the tunables
        Tunables m_Tunables;

        /// the checkpoint
        CharacterCheckpoint m_Checkpoint;

        // -- props --
        /// the input state
        SaveInput m_Input = new SaveInput();

        /// the character's state
        CharacterState m_State;

        /// the save elapsed time
        float m_SaveElapsed;

        // -- lifetime --
        public SaveSystem(
            Tunables tunables,
            CharacterState state,
            CharacterCheckpoint checkpoint
        ): base() {
            m_Tunables = tunables;
            m_State = state;
            m_Checkpoint = checkpoint;
        }

        // -- queries --
        /// the input state
        public SaveInput Input {
            get => m_Input;
        }

        // -- phases --
        protected override Phase InitInitialPhase() {
            return NotSaving;
        }

        // -- NotSaving --
        Phase NotSaving => new Phase(
            name: "NotSaving",
            enter: NotSaving_Enter,
            update: NotSaving_Update,
            exit: NotSaving_Exit
        );

        void NotSaving_Enter() {
            m_Checkpoint.ResetSave();
            m_SaveElapsed = 0.0f;
        }

        void NotSaving_Update(float delta) {
            if (m_Input.IsSaving) {
                ChangeTo(Smelling);
            }
        }

        void NotSaving_Exit() {
            m_Checkpoint.InitSave();
        }

        // -- Smelling --
        Phase Smelling => new Phase(
            name: "Smelling",
            update: Smelling_Update
        );

        void Smelling_Update(float delta) {
            Active_Update(delta);

            // start planting once you finish smelling around for a flower
            if (m_SaveElapsed > m_Tunables.SmellDuration) {
                ChangeTo(Planting);
            }
        }

        // -- Planting --
        Phase Planting => new Phase(
            name: "Planting",
            enter: Planting_Enter,
            update: Planting_Update
        );

        void Planting_Enter() {
            m_Checkpoint.FinishGrab();
        }

        void Planting_Update(float delta) {
            Active_Update(delta);

            // switch to simply existing after planting
            if (m_SaveElapsed > m_Tunables.PlantDuration) {
                ChangeTo(Being);
            }
        }

        // -- Being --
        Phase Being => new Phase(
            name: "Being",
            enter: Being_Enter,
            update: Being_Update
        );

        void Being_Enter() {
            m_Checkpoint.FinishSave();
        }

        void Being_Update(float delta) {
            Active_Update(delta);
        }

        // -- shared --
        // the base update when attempting to save
        void Active_Update(float delta) {
            m_SaveElapsed += delta;

            if (!m_Input.IsSaving || !m_State.IsGrounded || !m_State.IsIdle) {
                ChangeTo(NotSaving);
            }
        }

        // -- queries --
        /// TODO: this should be written to some external state structure
        public bool IsSaving {
            get => m_SaveElapsed > 0.0f;
        }
    }

    // -- constants --
    /// a sentinel for inactive casts
    private const float k_CastInactive = -1.0f;

    // -- fields --
    [Header("tuning")]
    [Tooltip("how long it takes to save")]
    [SerializeField] float m_SaveCastTime;

    [Tooltip("how long it takes to grab a nearby checkpoint, if any")]
    [SerializeField] float m_GrabCastTime;

    [Tooltip("how far from a checkpoint can you grab it")]
    [SerializeField] float m_GrabRadius;

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
    [Tooltip("the entities repos")]
    [SerializeField] EntitiesVariable m_Entities;

    // -- props --
    // the character
    DisconeCharacter m_Container;

    /// the flower at the current checkpoint, if any
    [SyncVar]
    CharacterFlower m_Flower;

    /// a checkpoint in the process of being created
    PendingCheckpoint m_PendingCheckpoint;

    /// if the character is trying to load
    bool m_IsLoadDown;

    /// the current save cast time
    float m_LoadElapsed = k_CastInactive;

    /// how long it takes to load
    float m_LoadCastTime = k_CastInactive;

    /// the state when the load starts
    CharacterState.Frame m_LoadStartState;

    /// the system that controls the checkpoint
    SaveSystem m_SaveSystem;

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Container = GetComponent<DisconeCharacter>();
    }

    void Start() {
        // construct save system
        var tunables = new SaveSystem.Tunables() {
            SmellDuration = m_GrabCastTime,
            PlantDuration = m_SaveCastTime,
        };

        m_SaveSystem = new SaveSystem(
            tunables,
            Character.State,
            this
        );

        m_SaveSystem.Init();
    }

    void Update() {
        // if we don't have authority, do nothing
        if (!netIdentity.IsOwner()) {
            return;
        }

        // update the save system
        m_SaveSystem.Update(Time.deltaTime);

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
        m_SaveSystem.Input.IsSaving = true;
   }

    /// stop a save if active
    public void StopSave() {
        m_SaveSystem.Input.IsSaving = false;
    }

    /// start a new save
    void InitSave() {
        m_PendingCheckpoint = PendingCheckpoint.FromState(Character.CurrentState);
    }

    /// finish the grab
    void FinishGrab() {
        // grab an existing flower
        Command_GrabCheckpoint(Character.State.Position);
    }

    /// finish the new save
    void FinishSave() {
        // spawn a new flower
        Command_CreateCheckpoint(
            m_PendingCheckpoint.Position,
            m_PendingCheckpoint.Forward
        );

        // reset state
        ResetSave();
    }

    /// reset save to initial state, if necessary
    void ResetSave() {
        m_PendingCheckpoint = null;
    }

    // -- c/s/server
    /// spawn a flower on the ground underneath the character
    [Command]
    void Command_CreateCheckpoint(Vector3 pos, Vector3 fwd) {
        Server_CreateCheckpoint(pos, fwd);
    }

    /// spawn a flower on the ground underneath the character
    [Command]
    void Command_GrabCheckpoint(Vector3 pos) {
        // find the nearest flower, if any
        var flower = m_Entities.Value
            .Flowers
            .FindClosest(pos);

        // if we found one, grab it
        if (flower != null && Vector3.Distance(flower.transform.position, pos) < m_GrabRadius) {
            Debug.Log($"[checkpoint] found a flower to grab! {flower}");
            Server_GrabCheckpoint(flower);
        }
    }

    /// spawn a flower from an existing flower position
    [Server]
    public void Server_CreateCheckpoint(FlowerRec rec) {
        Server_CreateCheckpoint(rec.Pos, rec.Fwd);
    }

    /// spawn a flower at the checkpoint
    [Server]
    void Server_CreateCheckpoint(Vector3 pos, Vector3 fwd) {
        // spawn the new flower
        var flower = CharacterFlower.Server_Spawn(
            m_Container.Key,
            pos,
            Quaternion.LookRotation(fwd, Vector3.up)
        );

        // and grab it
        Server_GrabCheckpoint(flower);
    }

    /// spawn a flower at the checkpoint
    [Server]
    void Server_GrabCheckpoint(CharacterFlower flower) {
        // if we had a flower, let it go
        m_Flower?.Server_Release();

        // grab the new flower
        m_Flower = flower;

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
        get => m_SaveSystem.IsSaving;
    }

    ThirdPerson.Character Character {
        get => m_Container.Character;
    }

    // -- types --
    /// a checkpoint in the process of being created
    public record PendingCheckpoint {
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