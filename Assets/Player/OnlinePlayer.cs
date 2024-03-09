using UnityEngine;
using Mirror;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using ThirdPerson;
using Discone;

namespace Discone {

// TODO: swap (drive) characters by setting m_LocalCharacter
// TODO: what to do for multiple players? variable instancer?
// TODO: rename to something like player sync?
/// an online player
[RequireComponent(typeof(WorldCoord))]
public sealed class OnlinePlayer: NetworkBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the character to spawn when first joining")]
    [SerializeField] CharacterKey m_InitialCharacterKey;

    // -- state --
    [Header("state")]
    [Tooltip("this player's current character")]
    [SerializeField] DisconeCharacter m_Character;

    [Tooltip("the number of connected players")]
    [SerializeField] IntVariable m_PlayerCount;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("switch the character")]
    [SerializeField] GameObjectEvent m_SwitchCharacter;

    // -- published --
    [Header("published")]
    [Tooltip("when a player joins")]
    [SerializeField] OnlinePlayerEvent m_Connected;

    [Tooltip("when a player leaves")]
    [SerializeField] OnlinePlayerEvent m_Disconnected;

    [Tooltip("when the current player starts")]
    [SerializeField] OnlinePlayerEvent m_CurrentStarted;

    [Tooltip("when a player switches character")]
    [SerializeField] DisconeCharacterPairEvent m_CharacterSwitched;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current player")]
    [SerializeField] DisconePlayerVariable m_LocalPlayer;

    [Tooltip("the local player's character")]
    [SerializeField] DisconeCharacterVariable m_LocalCharacter;

    [Tooltip("is this the hosts player")]
    [SerializeField] BoolReference m_IsHost;

    [Tooltip("the entities repos")]
    [SerializeField] EntitiesVariable m_Entities;

    [Tooltip("the spawn points list")]
    [SerializeField] GameObjectValueList m_SpawnPoints;

    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    // -- props --
    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new();

    /// the world coordinate
    WorldCoord m_Coord;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Coord = GetComponent<WorldCoord>();

        #if UNITY_EDITOR
        Dbg.AddToParent("Players", this);
        #endif

        m_PlayerCount.Value += 1;
        m_Connected.Raise(this);
    }

    void Update() {
        if (m_Character != null) {
            transform.position = m_Character.transform.position;
        }
    }

    void OnDestroy() {
        m_PlayerCount.Value -= 1;
        m_Disconnected.Raise(this);

        m_Subscriptions.Dispose();
    }

    // -- l/mirror
    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        Debug.Log(Tag.Player.F($"starting local player"));

        // bind to local player events
        m_Subscriptions.Add(m_SwitchCharacter, OnSwitchCharacter);

        // destroy your own star
        var target = GetComponentInChildren<SkyTarget>();
        if (target != null) {
            Destroy(target);
        }

        // TODO: loading screen?
        if (m_IsHost) {
            // replay buffer should make sure this gets called again every time
            m_Subscriptions.Add(m_Store.LoadFinished, OnLoadFinished);
        } else {
            OnLoadFinished();
        }

        // dispatch events
        m_CurrentStarted.Raise(this);
    }

    // -- commands --
    /// creates a given charater at the given transform
    public void SpawnCharacterAtPoint(CharacterKey key, Transform t) {
        // spawn a new character
        var pos = t.position;
        var fwd = t.forward;

        // create a debug character rec
        var character = new CharacterRec(
            key,
            pos,
            Quaternion.LookRotation(
                Vector3.ProjectOnPlane(fwd, Vector3.up),
                Vector3.up
            ),
            null
        );

        // spawn a new character
        Command_DriveSpawnedCharacter(character);
    }

    /// when the requests to instantiate its previous character
    [Command]
    public void Command_DriveSpawnedCharacter(CharacterRec character) {
        var prefab = CharacterDefs.Instance.Find(character.Key).Character;

        // TODO: character spawns exactly in the ground, and because of chunk
        // delay it ends up falling through the ground
        var offset = 1.0f;
        var dstCharacter = Instantiate(
            prefab,
            character.Pos + Vector3.up * offset,
            character.Rot
        );

        // we need to set the character here before calling Spawn because Spawn
        // calls the interest management and that uses the player position
        // (which is dependent on the characters).
        // NOTE/TODO: if this weren't true, then we could use
        // Server_DriveCharacter instead of Server_SwitchCharacter and have
        // fewer code paths
        var src = m_Character?.gameObject;
        m_Character = dstCharacter;

        #if UNITY_EDITOR
        dstCharacter.name = $"{character.Key.Name()} <spawned@{connectionToClient.connectionId}>";
        #endif

        // spawn the character
        var dst = dstCharacter.gameObject;
        NetworkServer.Spawn(dst);

        // notify all clients of the switch
        Server_SwitchCharacter(src, dst);

        // place the character's flower, if any
        if (character.Flower != null) {
            dstCharacter.Checkpoint.Server_CreateCheckpoint(character.Flower);
        }
    }

    /// drive a random character marked with "IsInitial"
    [Command]
    void Command_DriveInitialCharacter() {
        // find any available character
        var character = m_Entities.Value
            .Characters
            .FindInitialCharacter();

        // drive the initial character
        Server_DriveCharacter(character);
    }

    /// drive a new character
    [Command]
    void Command_DriveCharacter(DisconeCharacter dstChar) {
        Server_DriveCharacter(dstChar);
    }

    /// drive a new character
    [Server]
    void Server_DriveCharacter(DisconeCharacter dstChar) {
        // ensure we have a destination character
        if (dstChar == null) {
            Debug.LogError(Tag.Player.F($"cannot drive a null character"));
            return;
        }

        // don't carry over destroyed characters from scene change
        var srcChar = m_Character;
        if (srcChar == null) {
            srcChar = null;
        }

        // switch to the new character
        var src = srcChar?.gameObject;
        var dst = dstChar.gameObject;
        Server_SwitchCharacter(src, dst);
    }

    // -- c/network
    /// request to switch the character
    [Server]
    void Server_SwitchCharacter(GameObject src, GameObject dst) {
        var srcCharacter = src?.GetComponent<DisconeCharacter>();
        var dstCharacter = dst.GetComponent<DisconeCharacter>();

        // if the server doesn't have authority over this character, another player
        // already does
        if (!dstCharacter.IsAvailable) {
            Target_RetrySwitchCharacter(connectionToClient, isInitial: src == null);
            return;
        }

        Debug.Log(Tag.Player.F($"switching from {src?.name ?? "<none>"} to {dst.name}"));

        // give this client authority over the character
        dstCharacter.Server_AssignClientAuthority(connectionToClient);
        if (srcCharacter != null) {
            srcCharacter.Server_RemoveClientAuthority();
        }

        // notify target of switch
        Target_SwitchCharacter(connectionToClient, dst);

        // notify all clients of ownership change
        m_Character = dstCharacter;
        Client_ChangeOwnership(dstCharacter.gameObject);
    }

    /// switch to the character
    [TargetRpc]
    void Target_SwitchCharacter(NetworkConnection _, GameObject dst) {
        // if the player exists
        var player = m_LocalPlayer.GetComponent<Player>();
        if (player == null || !player.enabled) {
            Debug.Assert(false, Tag.Player.F($"missing player!"));
            return;
        }

        // and the character exists
        var character = dst.GetComponent<DisconeCharacter>();
        if (character == null || !character.enabled) {
            Debug.Assert(false, Tag.Player.F($"missing character"));
            return;
        }

        // drive the new character character
        m_LocalCharacter.Value = character;
        player.Drive(character.Character);
    }

    /// try to switch to a new character
    [TargetRpc]
    void Target_RetrySwitchCharacter(NetworkConnection _, bool isInitial) {
        // if you can't switch to your initial character, just keep trying
        if (isInitial) {
            Command_DriveInitialCharacter();
        }
    }

    /// notify all clients that a player switched to a character
    [ClientRpc]
    void Client_ChangeOwnership(GameObject character) {
        // change character
        var prev = m_Character;
        var next = character.GetComponent<DisconeCharacter>();
        m_Character = next;

        // publish event
        var pair = new DisconeCharacterPair();
        pair.Item1 = next;
        pair.Item2 = prev;
        m_CharacterSwitched.Raise(pair);
    }

    // -- queries --
    /// the player's current character
    public DisconeCharacter Character {
        get => m_Character;
    }

    /// the player's current position
    public Vector3 Position {
        get => m_Character.Position;
    }

    /// the world coordinate
    public WorldCoord Coord {
        get => m_Coord;
    }

    // -- events --
    /// when the character should switch
    void OnSwitchCharacter(GameObject obj) {
        var character = obj.GetComponent<DisconeCharacter>();
        Command_DriveCharacter(character);
    }

    void OnLoadFinished() {
        // get the stored charater
        var character = m_Store.PlayerCharacter;

        // spawn the character, if any
        if (character != null) {
            Debug.Log(Tag.Player.F($"spawn character {character.Key.Name()} @ {character.Pos}"));
            Command_DriveSpawnedCharacter(character);
        }
        // if there's no record, drive an initial character
        else {
            Debug.Log(Tag.Player.F($"drive random character"));
            SpawnCharacterAtPoint(m_InitialCharacterKey, transform);
        }
    }

    // -- e/server
    /// when the player disconnects on the server
    [Server]
    public void Server_OnDisconnect() {
        // don't do anything if server is shutting down
        if (!NetworkServer.active) {
            return;
        }

        // release this player's character when they disconnect
        if (m_Character != null) {
            m_Character.Server_RemoveClientAuthority();
        }
    }
}

}