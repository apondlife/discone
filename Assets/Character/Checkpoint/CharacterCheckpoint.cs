using System;
using Mirror;
using Soil;
using UnityEngine;
using UnityAtoms;
using ThirdPerson;
using UnityEngine.Events;

namespace Discone {

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(Character))]
public class CharacterCheckpoint: NetworkBehaviour, CheckpointContainer {
    // -- data --
    [Header("data")]
    [Tooltip("the tuning")]
    [SerializeField] CheckpointTuning m_Tuning;

    // -- systems --
    [Header("systems")]
    [Tooltip("the save system")]
    [SerializeField] SaveCheckpointSystem m_Save;

    [Tooltip("the load system")]
    [SerializeField] LoadCheckpointSystem m_Load;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entities repos")]
    [SerializeField] EntitiesVariable m_Entities;

    // -- props --
    /// the character
    Character m_Character;

    /// the state
    CheckpointState m_State;

    /// the flower at the current checkpoint, if any
    [SyncVar]
    CharacterFlower m_Flower;

    // if this can't create checkpoints
    bool m_IsBlocked;

    /// checkpoint-specific character systems
    System<CheckpointContainer>[] m_Systems;

    /// an event when the checkpoint is created
    UnityEvent<Checkpoint> m_OnCreate = new();

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Character = GetComponent<Character>();

        // set props
        m_State = new CheckpointState();
    }

    void Start() {
        // init systems
        m_Systems = new System<CheckpointContainer>[] {
            m_Save,
            m_Load,
        };

        foreach (var system in m_Systems) {
            system.Init(this);
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

        if (!isActiveAndEnabled) {
            return;
        }

        if (!m_Character) {
            Log.Character.E($"{name} - started server w/ no container");
            return;
        }

        m_Character.Online.OnSimulationChanged += Server_OnSimulationChanged;
    }

    // -- commands --
    // -- c/save
    /// if the character is saving
    public bool IsSaving {
        get => m_State.IsSaving;
    }

    /// grab the nearby checkpoint
    public void GrabCheckpoint() {
        // grab an existing flower
        Command_GrabCheckpoint(Character.State.Position);
    }

    /// create the checkpoint
    public void CreateCheckpoint(Checkpoint checkpoint) {
        if (!m_IsBlocked) {
            // spawn a new flower
            Command_CreateCheckpoint(
                checkpoint.Position,
                checkpoint.Forward
            );
        }

        // fire event for create checkpoint
        m_OnCreate?.Invoke(m_IsBlocked ? checkpoint : null);
    }

    // -- c/s/server
    /// spawn a flower on the ground underneath the character
    [Command]
    void Command_CreateCheckpoint(Vector3 pos, Vector3 fwd) {
        Server_CreateCheckpoint(pos, fwd);
    }

    /// grab a flower on the ground underneath the character
    [Command]
    void Command_GrabCheckpoint(Vector3 pos) {
        // find the nearest flower, if any
        var flower = m_Entities.Value
            .Flowers
            .FindClosest(pos);

        // if we found one, grab it
        if (flower != null && Vector3.Distance(flower.Checkpoint.Position, pos) < m_Tuning.GrabRadius) {
            Log.Character.I($"found flower to grab {flower}");
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
        // find overlapping flower, if any
        var flower = m_Entities.Value
            .Flowers
            .FindOverlap(pos);

        // if none, spawn a new one
        if (flower == null) {
            flower = CharacterFlower.Server_Spawn(
                m_Character.Key,
                pos,
                fwd
            );
        }

        // and grab the flower
        Server_GrabCheckpoint(flower);
    }

    /// grab a nearby flower
    [Server]
    void Server_GrabCheckpoint(CharacterFlower flower) {
        // if we had a flower, let it go
        m_Flower?.Server_Release();

        // store the new flower
        m_Flower = flower;

        // i don't know why this doesn't work in the same frame...
        this.DoNextFrame(() => m_Flower?.Server_Grab());
    }

    // -- events --
    [Server]
    void Server_OnSimulationChanged(CharacterSimulation sim) {
        if (sim == CharacterSimulation.None || m_Flower != null) {
            return;
        }

        // create initial character flower
        // TODO: maybe this should be on build not on "start"?
        // TODO: "floating flowers" maybe this should happen when character is first looked at

        // maybe there's an AI
        m_Character.Events.Once(CharacterEvent.Idle, () => {
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
    /// the character's current flower
    public CharacterFlower Flower {
        get => m_Flower;
    }

    /// if the character is currently loading
    public bool IsLoading {
        get => m_State.IsLoading;
    }

    /// an event when the checkpoint is created
    public UnityEvent<Checkpoint> OnCreate {
        get => m_OnCreate;
    }

    // -- CheckpointContainer --
    /// the tuning
    public CheckpointTuning Tuning {
        get => m_Tuning;
    }

    /// the state
    public CheckpointState State {
        get => m_State;
    }

    /// the current flower's checkpoint
    public Checkpoint Checkpoint {
        get => m_Flower ? m_Flower.Checkpoint : null;
    }

    /// a reference to the character
    public Character Character {
        get => m_Character;
    }

    // -- props/hot --
    /// if this can create checkpoints
    public bool IsBlocked {
        get => m_IsBlocked;
        set => m_IsBlocked = value;
    }

    // -- factories --
    /// create a flower record for the current checkpoint
    public FlowerRec IntoRecord() {
        return m_Flower?.IntoRecord();
    }
}

}