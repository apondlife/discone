using UnityEngine;
using Mirror;
using UnityAtoms.BaseAtoms;
using ThirdPerson;
using System.Linq;

/// an online player
/// TODO: swap (drive) characters by setting m_CurrentCharacter
/// TODO: what to do for multiple players? variable instancer?
sealed class OnlinePlayer: NetworkBehaviour {
    // -- references --
    [Header("references")]
    [Tooltip("the current player")]
    [SerializeField] GameObjectVariable m_CurrentPlayer;

    [Tooltip("the current player's character")]
    [SerializeField] GameObjectVariable m_CurrentCharacter;

    // -- events --
    [Header("events")]
    [Tooltip("switch the character")]
    [SerializeField] GameObjectEvent m_SwitchCharacter;

    // -- props --
    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- NetworkBehaviour --
    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        // drive any character
        DriveInitialCharacter();

        /// listen to switch events
        m_Subscriptions.Add(m_SwitchCharacter, OnSwitchCharacter);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// drive the first available character in the world
    void DriveInitialCharacter() {
        // find all available characters
        var available = GameObject
            .FindObjectsOfType<OnlineCharacter>()
            .Where(c => c.IsAvailable && c.IsInitial)
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
    void DriveCharacter(OnlineCharacter character) {
        Server_SwitchCharacter(m_CurrentCharacter.Value, character.gameObject);
    }

    // -- c/network
    [Command]
    public void Server_SwitchCharacter(GameObject src, GameObject dst) {
        var srcCharacter = src?.GetComponent<OnlineCharacter>();
        var dstCharacter = dst.GetComponent<OnlineCharacter>();

        // if the server doesn't have authority over this character, another player
        // already does
        if (!dstCharacter.IsAvailable) {
            Client_FailedToSwitchCharacter(isInitial: src == null);
            return;
        }

        // assign authority to this client
        dstCharacter.Server_AssignClientAuthority(connectionToClient);

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
        var character = dst.GetComponent<ThirdPerson.Character>();
        if (character == null || !character.enabled) {
            Debug.Assert(false, "[player] missing character");
            return;
        }

        // drive this character
        m_CurrentCharacter.Value = dst;
        player.Drive(character);
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
        var character = obj.GetComponent<OnlineCharacter>();
        if (character == null) {
            Debug.Log($"[player] tried to switch to an character w/ no OnlineCharacter");
            return;
        }

        DriveCharacter(character);
    }
}