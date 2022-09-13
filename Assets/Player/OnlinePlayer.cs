using UnityEngine;
using Mirror;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using ThirdPerson;

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

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("switch the character")]
    [SerializeField] GameObjectEvent m_SwitchCharacter;

    // -- published --
    [Header("published")]
    [Tooltip("when a player joins")]
    // TODO: remote player
    [SerializeField] OnlinePlayerEvent m_Connected;

    [Tooltip("when a player leaves")]
    // TODO: remote player
    [SerializeField] OnlinePlayerEvent m_Disconnected;

    [Tooltip("when a player switches character")]
    // TODO: remote player
    [SerializeField] DisconeCharacterPairEvent m_SwitchedCharacter;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current player")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CurrentPlayer")]
    [SerializeField] DisconePlayerVariable m_LocalPlayer;

    [Tooltip("the local player's character")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CurrentCharacter")]
    [SerializeField] DisconeCharacterVariable m_LocalCharacter;

    [Tooltip("is this the hosts player")]
    [SerializeField] BoolReference m_IsHost;

    [Tooltip("the entities repos")]
    [SerializeField] EntitiesVariable m_Entities;

    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

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
        // TODO: move this into RemotePlayer, other than mixing Local and Remote
        m_PlayerCount.Value += 1;
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

        // destroy your own star
        var target = GetComponentInChildren<SkyTarget>();
        if (target != null) {
            Destroy(target);
        }

        // listen to switch events
        m_Subscriptions
            .Add(m_SwitchCharacter, OnSwitchCharacter);

        // TODO: loading screen?
        if (m_IsHost) {
            // replay buffer should make sure this gets called again every time
            m_Subscriptions.Add(m_Store.LoadFinished, OnLoadFinished);
        } else {
            var character = m_Store.PlayerCharacter;
            if (character == null) {
                Debug.LogError($"[player] local player has no character record");
                return;
            }

            Server_CreateAndDriveCharacter(character);
        }
    }

    void OnDestroy() {
        m_PlayerCount.Value -= 1;
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
        Command_SwitchCharacter(src, dst);
    }

    // -- c/network
    /// request to switch the character
    [Command]
    void Command_SwitchCharacter(GameObject src, GameObject dst) {
        Server_SwitchCharacter(src, dst);
    }

    [Server]
    void Server_SwitchCharacter(GameObject src, GameObject dst) {
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
        // change character
        var prev = m_Character;
        var next = character.GetComponent<DisconeCharacter>();
        m_Character = next;

        // publish event
        var pair = new DisconeCharacterPair();
        pair.Item1 = next;
        pair.Item2 = prev;
        m_SwitchedCharacter.Raise(pair);
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
        DriveCharacter(character);
    }

    // this only happens to the host player (single player)
    void OnLoadFinished() {
        var character = m_Store.PlayerCharacter;
        if (character != null) {
            Debug.Log($"[local player] character found in store: ${character.Key.Name()} at ${character.Pos}");
            // Instantiate Character...
            Server_CreateAndDriveCharacter(character);
        }
        // if there's no character record, drive an initial character
        else {
            DriveInitialCharacter();
        }
    }

    /// c/server
    /// when the requests to instantiate its previous character
    [Command]
    public void Server_CreateAndDriveCharacter(CharacterRec character) {
        var prefab = CharacterDefs.Instance.Find(character.Key).Character;

        // TODO: character spawns exactly in the ground, and because of chunk delay it ends up falling through the ground
        var offset = 1.0f;
        var dstCharacter = Instantiate(
            prefab,
            character.Pos + Vector3.up * offset,
            character.Rot
        );

        var dst = dstCharacter.gameObject;
        NetworkServer.Spawn(dst);

        Server_SwitchCharacter(null, dst);
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