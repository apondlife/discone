using System;
using Mirror;
using UnityEngine;
using UnityAtoms;
using ThirdPerson;
using UnityEngine.Events;

namespace Discone {

/// the character's ability to save and reload to a particular state in
/// the world, like planting a flag.
[RequireComponent(typeof(Character))]
public class CharacterCheckpoint: NetworkBehaviour {
    // -- fields --
    [Header("tuning")]
    [Tooltip("how far from a checkpoint can you grab it")]
    [SerializeField] float m_GrabRadius;

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
    Character m_Container;

    /// the flower at the current checkpoint, if any
    [SyncVar]
    CharacterFlower m_Flower;

    // if this can't create checkpoints
    bool m_IsBlocked;

    /// checkpoint-specific character systems
    CheckpointSystem[] m_Systems;

    /// an event when the checkpoint is created
    UnityEvent<Checkpoint> m_OnCreate = new();

    // -- lifecycle --
    void Awake() {
        // set deps
        m_Container = GetComponent<Character>();
    }

    void Start() {
        // init systems
        m_Systems = new CheckpointSystem[] {
            m_Save,
            m_Load,
        };

        foreach (var system in m_Systems) {
            system.Init(
                m_Container.State,
                m_Container.Checkpoint
            );
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

        if (!m_Container) {
            Log.Charss.E($"{name} - started server w/ no container");
            return;
        }

        m_Container.Online.OnSimulationChanged += Server_OnSimulationChanged;
    }

    // -- commands --
    // -- c/save
    /// if the character is saving
    public bool IsSaving {
        get => m_Save.IsSaving;
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
        if (flower != null && Vector3.Distance(flower.Checkpoint.Position, pos) < m_GrabRadius) {
            Debug.Log($"[chkpnt] found flower to grab {flower}");
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
                m_Container.Key,
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

    /// -- c/load
    /// restore to the current checkpoint, if any
    [Obsolete]
    public void StartLoad() {
        m_Load.Input.IsLoading = true;
    }

    /// cancel a load if active
    [Obsolete]
    public void StopLoad() {
        m_Load.Input.IsLoading = false;
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
        m_Container.Events.Once(CharacterEvent.Idle, () => {
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

    /// the current flower's checkpoint
    public Checkpoint Checkpoint {
        get => m_Flower?.Checkpoint;
    }

    /// if the character is currently loading
    public bool IsLoading {
        get => m_Load.IsLoading;
    }

    /// a reference to the character
    public Character Character {
        get => m_Container;
    }

    /// an event when the checkpoint is created
    public UnityEvent<Checkpoint> OnCreate {
        get => m_OnCreate;
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