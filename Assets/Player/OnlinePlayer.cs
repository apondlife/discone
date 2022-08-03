using UnityEngine;
using Mirror;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using ThirdPerson;
using System.Linq;

/// an online player
/// TODO: swap (drive) characters by setting m_CurrentCharacter
/// TODO: what to do for multiple players? variable instancer?
sealed class OnlinePlayer: NetworkBehaviour {
    // -- atoms --
    [Header("atoms")]
    [Tooltip("the current player")]
    [SerializeField] DisconePlayerVariable m_CurrentPlayer;

    [Tooltip("the current player's character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    // -- atom refs --
    [Tooltip("the number of players connected")]
    [SerializeField] IntVariable m_PlayerCount;

    // -- events --
    [Header("events")]
    [Tooltip("switch the character")]
    [SerializeField] GameObjectEvent m_SwitchCharacter;

    // -- props --
    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    /// the player's synchronized character
    [SyncVar]
    GameObject m_Character;

    // -- lifecycle --
    void Awake() {
        #if UNITY_EDITOR
        Dbg.AddToParent("Players", this);
        #endif
    }

    void Start() {
        // TODO: onOnlinePlayerJoin / onOnlinePlayerLeave events
        m_PlayerCount.Value++;
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

        // listen to switch events
        m_Subscriptions.Add(m_SwitchCharacter, OnSwitchCharacter);
    }

    public override void OnStopServer() {
        base.OnStopServer();

        // don't do anything if server is shutting down
        if (!NetworkServer.active) {
            return;
        }

        // release this player's character when they disconnect
        var character = m_CurrentCharacter?.Value;
        if (character != null) {
            character.Server_RemoveClientAuthority();
        }
    }

    void OnDestroy() {
        m_PlayerCount.Value--;
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// drive the first available character in the world
    void DriveInitialCharacter() {
        // find all available characters
        var all = GameObject
            .FindObjectsOfType<DisconeCharacter>();

        // use debug characters if available, otherwise the first initial character
        var sets = new[] {
            #if UNITY_EDITOR
            all.Where(c => c.IsDebug),
            #endif
            all.Where(c => c.IsAvailable && c.IsInitial)
        };

        var available = sets
            .Where((cs) => cs.Any())
            .First()
            .ToArray();

        // if there is nothing to drive
        var character = available[Random.Range(0, available.Length)];
        if (character == null) {
            Debug.LogError("[player] the was no character to drive");
            Application.Quit();
            return;
        }

        // drive the initial character
        DriveCharacter(character);
    }

    /// drive a new character
    void DriveCharacter(DisconeCharacter character) {
        // don't carry over destroyed characters from scene change
        var srcChar = m_CurrentCharacter.Value;
        if (srcChar == null) {
            srcChar = null;
        }

        // switch to the new character
        var src = srcChar?.gameObject;
        var dst = character.gameObject;
        Server_SwitchCharacter(src, dst);
    }

    // -- c/network
    [Command]
    public void Server_SwitchCharacter(GameObject src, GameObject dst) {
        var srcCharacter = src?.GetComponent<DisconeCharacter>();
        var dstCharacter = dst.GetComponent<DisconeCharacter>();

        // if the server doesn't have authority over this character, another player
        // already does
        if (!dstCharacter.IsAvailable) {
            Client_FailedToSwitchCharacter(isInitial: src == null);
            return;
        }

        // give this client authority over the character
        dstCharacter.Server_AssignClientAuthority(connectionToClient);
        m_Character = dstCharacter.gameObject;

        if (srcCharacter != null) {
            srcCharacter.Server_RemoveClientAuthority();
        }

        // call back to the client
        Client_SwitchCharacter(connectionToClient, dst);
    }

    [TargetRpc]
    void Client_SwitchCharacter(NetworkConnection target, GameObject dst) {
        // if the player exists
        var player = m_CurrentPlayer.GetComponent<Player>();
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
        m_CurrentCharacter.Value = character;
        player.Drive(character.Character);
    }

    [TargetRpc]
    void Client_FailedToSwitchCharacter(bool isInitial) {
        // if you can't switch to your initial character, just keep trying
        if (isInitial) {
            DriveInitialCharacter();
        }
    }

    // -- events --
    /// when the character should switch
    void OnSwitchCharacter(GameObject obj) {
        var character = obj.GetComponent<DisconeCharacter>();
        if (character == null) {
            Debug.Log($"[player] tried to switch to an character w/ no DisconeCharacter");
            return;
        }

        DriveCharacter(character);
    }
}