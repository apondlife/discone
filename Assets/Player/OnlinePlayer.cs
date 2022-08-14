using UnityEngine;
using Mirror;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using ThirdPerson;
using System.Linq;

/// an online player
/// TODO: swap (drive) characters by setting m_LocalCharacter
/// TODO: what to do for multiple players? variable instancer?
/// TODO: rename to something like player sync?
[RequireComponent(typeof(WorldCoord))]
public sealed class OnlinePlayer: NetworkBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("this player's current character")]
    [SerializeField] DisconeCharacter m_Character;

    [Tooltip("the number of connected players")]
    [SerializeField] IntVariable m_PlayerCount;

    // -- events --
    [Header("events")]
    [Tooltip("switch the character")]
    [SerializeField] GameObjectEvent m_SwitchCharacter;

    [Tooltip("when the played joins")]
    [SerializeField] OnlinePlayerEvent m_Connected;

    [Tooltip("when the player leaves")]
    [SerializeField] OnlinePlayerEvent m_Disconnected;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current player")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CurrentPlayer")]
    [SerializeField] DisconePlayerVariable m_LocalPlayer;

    [Tooltip("the local player's character")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CurrentCharacter")]
    [SerializeField] DisconeCharacterVariable m_LocalCharacter;

    [Tooltip("the entities repos")]
    [SerializeField] EntitiesVariable m_Entities;

    // -- props --
    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    /// the world coordinate
    WorldCoord m_Coord;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Coord = GetComponent<WorldCoord>();

        #if UNITY_EDITOR
        Dbg.AddToParent("Players", this);
        #endif
    }

    void Start() {
        m_PlayerCount.Value++;
        m_Connected.Raise(this);
    }

    void Update() {
        if (m_Character != null) {
            transform.position = m_Character.transform.position;
        }
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        Debug.Log("[online] starting local player");

        // drive any character
        DriveInitialCharacter();

        // destroy your own star
        var target = GetComponentInChildren<SkyTarget>();
        if (target != null) {
            Destroy(target);
        }

        // listen to switch events
        m_Subscriptions.Add(m_SwitchCharacter, OnSwitchCharacter);
    }

    void OnDestroy() {
        m_PlayerCount.Value--;
        m_Disconnected.Raise(this);

        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// drive the first available character in the world
    void DriveInitialCharacter() {
        // find any available character
        var character = m_Entities.Value.Characters.FindInitialCharacter();

        // drive the initial character
        DriveCharacter(character);
    }

    /// drive a new character
    void DriveCharacter(DisconeCharacter dstChar) {
        // ensure we have a destination character
        if (dstChar == null) {
            Debug.LogError($"[player] cannot drive a null character");
            return;
        }

        // don't carry over destroyed characters from scene change
        var srcChar = m_LocalCharacter.Value;
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
    [Command]
    public void Server_SwitchCharacter(GameObject src, GameObject dst) {
        var srcCharacter = src?.GetComponent<DisconeCharacter>();
        var dstCharacter = dst.GetComponent<DisconeCharacter>();

        // if the server doesn't have authority over this character, another player
        // already does
        if (!dstCharacter.IsAvailable) {
            Target_RetrySwitchCharacter(isInitial: src == null);
            return;
        }

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
    void Target_SwitchCharacter(NetworkConnection target, GameObject dst) {
        // if the player exists
        var player = m_LocalPlayer.GetComponent<Player>();
        if (player == null || !player.enabled) {
            Debug.Assert(false, "[player] missing player!");
            return;
        }

        // and the character exists
        var character = dst.GetComponent<DisconeCharacter>();
        if (character == null || !character.enabled) {
            Debug.Assert(false, "[player] missing character");
            return;
        }

        // drive this character
        m_LocalCharacter.Value = character;
        player.Drive(character.Character);
    }

    /// try to switch to a new character
    [TargetRpc]
    void Target_RetrySwitchCharacter(bool isInitial) {
        // if you can't switch to your initial character, just keep trying
        if (isInitial) {
            DriveInitialCharacter();
        }
    }

    /// notify all clients that a player switched to a character
    [ClientRpc]
    void Client_ChangeOwnership(GameObject character) {
        m_Character = character.GetComponent<DisconeCharacter>();
    }

    // -- queries --
    /// the player's current character
    public DisconeCharacter Character {
        get => m_Character;
    }

    /// the world coordinate
    public WorldCoord Coord {
        get => m_Coord;
    }

    // -- events --
    /// when the character should switch
    void OnSwitchCharacter(GameObject obj) {
        var character = obj.GetComponent<DisconeCharacter>();
        DriveCharacter(character);
    }

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