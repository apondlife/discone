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
    [Tooltip("a reference to the current player")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CurrentPlayer")]
    [SerializeField] GameObjectVariable m_CurrentPlayer;

    [Tooltip("a reference to the crruent player's character")]
    [SerializeField] GameObjectVariable m_CurrentCharacter;

    // -- events --
    [Header("events")]
    [Tooltip("switch the character")]
    [SerializeField] GameObjectEvent m_SwitchCharacter;

    // -- lifecycle --
    void Awake() {
        m_SwitchCharacter.Register(OnSwitchCharacter);
    }

    // -- NetworkBehaviour --
    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        // drive any character
        DriveInitialCharacter();
    }

    // -- commands --
    /// drive the first available character in the world
    void DriveInitialCharacter() {
        // find all available characters
        var available = GameObject
            .FindObjectsOfType<OnlineCharacter>()
            .Where(c => c.IsAvailable)
            .ToArray();

        // if there is nothing to drive
        // var character = available[Random.Range(0, available.Length)];
        var character = available[1];
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
        var srcChar = src.GetComponent<OnlineCharacter>();
        var dstChar = dst.GetComponent<OnlineCharacter>();

        // if the server doesn't have authority over this character, another player
        // already does
        if (!dstChar.IsAvailable) {
            Client_FailedToSwitchCharacter(src == null);
            return;
        }

        // assign authority to this client
        dstChar.AssignClientAuthority(connectionToClient);

        if (srcChar != null) {
            srcChar.RemoveClientAuthority();
        }

        // call back to the client
        Client_SwitchCharacter(connectionToClient, dst);
    }

    [TargetRpc]
    void Client_SwitchCharacter(NetworkConnection target, GameObject dst) {
        // if the player exists
        var p = m_CurrentPlayer.GetComponent<Player>();
        if (p == null || !p.enabled) {
            Debug.Assert(false, "[player] missing player!");
            return;
        }

        // and the character exists
        var c = dst.GetComponent<ThirdPerson.ThirdPerson>();
        if (c == null || !c.enabled) {
            Debug.Assert(false, "[player] missing character");
            return;
        }

        // drive this character
        m_CurrentCharacter.Value = dst;
        p.Drive(c);
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