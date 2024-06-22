using System;
using UnityEngine;
using Mirror;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

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
    [SerializeField] Character m_Character;

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
    [SerializeField] CharacterPairEvent m_CharacterSwitched;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current player")]
    [SerializeField] PlayerVariable m_LocalPlayer;

    [Tooltip("the local player's character")]
    [SerializeField] CharacterVariable m_LocalCharacter;

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

    /// drives the linked player's character
    CharacterEvent m_DriveCharacter;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Coord = GetComponent<WorldCoord>();

        #if UNITY_EDITOR
        Dbg.AddToParent("Players_Online", this);
        #endif

        m_PlayerCount.Value += 1;
    }

    public void Start() {
        m_Connected.Raise(this);
    }

    void Update() {
        if (m_Character) {
            transform.position = m_Character.transform.position;
        }
    }

    void OnDestroy() {
        m_PlayerCount.Value -= 1;
        m_Disconnected.Raise(this);

        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// spawn a character for this player
    public void Spawn(bool isInitial = false) {
        var onLoadFinished = (Action)(isInitial ? OnInitialLoadFinished : OnLoadFinished);

        // TODO: loading screen?
        if (m_IsHost) {
            // replay buffer should make sure this gets called again every time
            m_Subscriptions.Add(m_Store.LoadFinished, onLoadFinished);
        } else {
            onLoadFinished();
        }
    }

    /// link this online player to a local player
    public void Link(Player player) {
        m_DriveCharacter = player.DriveCharacter;
    }

    // -- l/mirror
    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        Log.Player.I($"starting local player");

        // bind to local player events
        m_Subscriptions.Add(m_SwitchCharacter, OnSwitchCharacter);

        // destroy your own star
        var target = GetComponentInChildren<SkyTarget>();
        if (target != null) {
            Destroy(target);
        }

        // spawn the initial character
        Spawn(isInitial: true);

        // dispatch events
        m_CurrentStarted.Raise(this);
    }

    // -- commands --
    /// creates a given character at the given transform
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
        Command_SpawnCharacter(character);
    }

    /// when the requests to instantiate its previous character
    [Command]
    void Command_SpawnCharacter(CharacterRec record) {
        var newCharacter = Character_Spawn.Server_Create(
            record,
            connectionToClient.connectionId.ToString()
        );

        // we need to set the character here before calling Spawn because Spawn calls the interest management and that
        // uses the player position (which is dependent on the characters).
        // TODO: if this weren't true, then we could use Server_DriveCharacter instead of Server_SwitchCharacter and
        // have fewer code paths
        var src = m_Character?.gameObject;
        m_Character = newCharacter;

        // spawn the character
        Character_Spawn.Server_Spawn(newCharacter);

        // notify all clients of the switch
        var dst = newCharacter.gameObject;
        Server_SwitchCharacter(src, dst);

        // place the character's flower, if any
        Character_Spawn.Server_FinishSpawn(newCharacter, record);
    }

    /// drive a random character marked with "IsInitial"
    [Command]
    void Command_DriveInitialCharacter() {
        // find any available character
        var character = m_Entities.Value
            .Characters
            .FindInitialCharacter()
            .Online;

        // drive the initial character
        Server_DriveCharacter(character);
    }

    /// drive a new character
    [Command]
    void Command_DriveCharacter(Character_Online dstChar) {
        Server_DriveCharacter(dstChar);
    }

    /// drive a new character
    [Server]
    void Server_DriveCharacter(Character_Online dstChar) {
        // ensure we have a destination character
        if (dstChar == null) {
            Log.Player.E($"cannot drive a null character");
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
        var srcCharacter = src?.GetComponent<Character_Online>();
        var dstCharacter = dst.GetComponent<Character_Online>();

        // if the server doesn't have authority over this character, another player
        // already does
        if (!dstCharacter.IsAvailable) {
            /// AAA: stale?
            Target_RetrySwitchCharacter(connectionToClient, isInitial: src == null);
            return;
        }

        Log.Player.I($"switching from {src?.name ?? "<none>"} to {dst.name}");

        // give this client authority over the character
        dstCharacter.Server_AssignClientAuthority(connectionToClient);
        if (srcCharacter != null) {
            srcCharacter.Server_RemoveClientAuthority();
        }

        // notify target of switch
        Target_SwitchCharacter(connectionToClient, dst);

        // notify all clients of ownership change
        m_Character = dstCharacter.Character;
        Client_ChangeOwnership(dstCharacter.gameObject);
    }

    // TODO: this should dispatch an event for the local player to listen to rather
    // than reference it directly
    /// switch to the character
    [TargetRpc]
    void Target_SwitchCharacter(NetworkConnection _, GameObject dst) {
        // if the player exists
        if (!m_DriveCharacter) {
            Log.Player.Fatal($"missing player!");
            return;
        }

        // and the character exists
        var character = dst.GetComponent<Character>();
        if (!character || !character.enabled) {
            Log.Player.Fatal($"missing character!");
            return;
        }

        m_DriveCharacter.Raise(character);
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
        var next = character.GetComponent<Character>();
        m_Character = next;

        // publish event
        var pair = new CharacterPair();
        pair.Item1 = next;
        pair.Item2 = prev;
        m_CharacterSwitched.Raise(pair);
    }

    // -- queries --
    /// the player's current character
    public Character Character {
        get => m_Character;
    }

    /// the world coordinate
    public WorldCoord Coord {
        get => m_Coord;
    }

    // -- events --
    /// when the character should switch
    void OnSwitchCharacter(GameObject obj) {
        var character = obj.GetComponent<Character_Online>();
        Command_DriveCharacter(character);
    }

    /// spawn initial player when store finishes loading
    void OnInitialLoadFinished() {
        // get the stored charater
        var character = m_Store.PlayerCharacter;

        // spawn the character, if any
        if (character != null) {
            Log.Player.I($"spawn character {character.Key.Name()} @ {character.Pos}");
            Command_SpawnCharacter(character);
        }
        // if there's no record, drive an initial character
        else {
            OnLoadFinished();
        }
    }

    /// spawn player when store finishes loading
    void OnLoadFinished() {
        Log.Player.I($"drive random character");
        SpawnCharacterAtPoint(m_InitialCharacterKey, transform);
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
        if (m_Character) {
            m_Character.Online.Server_RemoveClientAuthority();
        }
    }
}

}