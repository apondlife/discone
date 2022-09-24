using Mirror;
using UnityEngine;
using UnityAtoms;
using ThirdPerson;

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(DisconeCharacter))]
public class CharacterCheckpoint: NetworkBehaviour {
    // -- constants --
    /// a sentinel for inactive casts
    private const float k_CastInactive = -1.0f;

    // -- fields --
    [Header("tuning")]
    [Tooltip("the tunables for creating a checkpoint")]
    [SerializeField] SaveCheckpointSystem.Tunables m_SaveCheckpointTunables;

    [Tooltip("the tunabled for loading from a checkpoint")]
    [SerializeField] LoadCheckpointSystem.Tunables m_LoadCheckpointTunables;

    [Tooltip("how long it takes to save")]
    [SerializeField] float m_SaveCastTime;

    [Tooltip("how long it takes to grab a nearby checkpoint, if any")]
    [SerializeField] float m_GrabCastTime;

    [Tooltip("how far from a checkpoint can you grab it")]
    [SerializeField] float m_GrabRadius;

    [Tooltip("the time it takes to get to the half distance")]
    [SerializeField] float m_LoadCastMaxTime;

    [Tooltip("the time it takes for the load to travel the point distance")]
    [SerializeField] float m_LoadCastPointTime;

    [Tooltip("the distance the load travels by the point time")]
    [SerializeField] float m_LoadCastPointDistance;

    [Tooltip("how faster cancelling a load is than the load itself")]
    [SerializeField] float m_LoadCancelMultiplier = 1;

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
    Checkpoint m_PendingCheckpoint;

    /// if the character is trying to load
    bool m_IsLoadDown;

    /// if the checkpoint is saving
    bool m_IsSaving;

    /// the current save cast time
    float m_LoadElapsed = k_CastInactive;

    /// how long it takes to load
    float m_LoadCastTime = k_CastInactive;

    /// the state when the load starts
    CharacterState.Frame m_LoadStartState;

    /// checkpoint-specific character systems
    ThirdPerson.System[] m_Systems;

    private void OnValidate() {
        m_SaveCheckpointTunables.SmellDuration = m_GrabCastTime;
        m_SaveCheckpointTunables.PlantDuration = m_SaveCastTime;

        m_LoadCheckpointTunables.LoadCastMaxTime = m_LoadCastMaxTime;
        m_LoadCheckpointTunables.LoadCastPointTime = m_LoadCastPointTime;
        m_LoadCheckpointTunables.LoadCastPointDistance = m_LoadCastPointDistance;
        m_LoadCheckpointTunables.LoadCancelMultiplier = m_LoadCancelMultiplier;
    }

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Container = GetComponent<DisconeCharacter>();
    }

    void Start() {
        // init systems
        m_Systems = new ThirdPerson.System[] {
            new SaveCheckpointSystem(
                m_SaveCheckpointTunables,
                m_Container.Character.State,
                m_Container.Checkpoint
            ),
            new LoadCheckpointSystem(
                m_LoadCheckpointTunables,
                m_Container.Character.State,
                m_Container.Checkpoint
            ),
        };

        foreach (var system in m_Systems) {
            system.Init();
        }
    }

    void Update() {
        // if we don't have authority, do nothing
        if (!netIdentity.IsOwner()) {
            return;
        }

        // update the systems
        foreach (var system in m_Systems) {
            system.Update(Time.deltaTime);
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
    [System.Obsolete]
    public void StartSave() {
        (m_Systems[0] as SaveCheckpointSystem)!.Input.IsSaving = true;
   }

    /// stop a save if active
    [System.Obsolete]
    public void StopSave() {
        (m_Systems[0] as SaveCheckpointSystem)!.Input.IsSaving = false;
    }

    // -- c/save
    /// if the character is saving
    public bool IsSaving {
        get => m_IsSaving;
        set => m_IsSaving = value;
    }

    /// grab the nearby checkpoint
    public void GrabCheckpoint() {
        // grab an existing flower
        Command_GrabCheckpoint(Character.State.Position);
    }

    /// create the checkpoint
    public void CreateCheckpoint(Checkpoint checkpoint) {
        // spawn a new flower
        Command_CreateCheckpoint(
            m_PendingCheckpoint.Position,
            m_PendingCheckpoint.Forward
        );
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
    [System.Obsolete]
    public void StartLoad() {
        if (m_Flower != null) {
            return;
        }

        (m_Systems[1] as SaveCheckpointSystem)!.Input.IsSaving = true;
    }

    /// cancel a load if active
    [System.Obsolete]
    public void StopLoad() {
        (m_Systems[1] as SaveCheckpointSystem)!.Input.IsSaving = false;
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
            var checkpoint = Checkpoint.FromState(Character.CurrentState);
            Server_CreateCheckpoint(checkpoint.Position, checkpoint.Forward);
        });
    }

    // -- queries --
    public CharacterFlower Flower {
        get => m_Flower;
    }

    public Checkpoint Checkpoint {
        get => m_Flower.Checkpoint;
    }

    /// a reference to the character
    public ThirdPerson.Character Character {
        get => m_Container.Character;
    }
}