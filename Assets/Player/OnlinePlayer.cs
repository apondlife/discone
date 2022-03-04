using UnityEngine;
using Mirror;
using UnityAtoms.BaseAtoms;
using ThirdPerson;

/// an online player
/// TODO: swap (drive) characters by setting m_CurrentCharacter
/// TODO: what to do for multiple players? variable instancer?
[RequireComponent(typeof(Player))]
sealed class OnlinePlayer: NetworkBehaviour {
    // -- references --
    [Header("references")]
    [Tooltip("a reference to the current player")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CurrentPlayer")]
    [SerializeField] GameObjectVariable m_CurrentPlayer;

    [Tooltip("a reference to the crruent player's character")]
    [SerializeField] GameObjectVariable m_CurrentCharacter;

    // -- lifecycle --
    void Start() {
        if (m_CurrentPlayer.Value == gameObject) {
            Debug.Log($"start & isLocalPlayer");
            DriveInitialCharacter();
        }
    }

    // -- NetworkBehaviour --
    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        // transfer control from offline to online player
        Transfer();
    }

    // -- commands --
    /// drive the initial character, if it's configured
    void DriveInitialCharacter() {
        var p = m_CurrentPlayer.GetComponent<Player>();
        var c = m_CurrentCharacter.GetComponent<ThirdPerson.ThirdPerson>();

        // ensure these are configured properly
        if (p == null && c == null) {
            Debug.LogError("missing initial player or character");
            return;
        }

        // drive the initial character
        p.Drive(c);
    }

    /// connect an online copy of the player
    public static GameObject Spawn(GameObject prefab, int id, Vector3 pos) {
        var obj = GameObject.Instantiate(prefab);
        obj.name = $"Player-{id}";
        obj.transform.position = pos;
        return obj;
    }

    /// transfer control to online player and destroy offline version
    public void Transfer() {
        // give the local player a special name
        gameObject.name = "Player (local)";

        // replace the offline character with the online one (see: Online.OnCreateCharacter)
        var online = GetComponent<Player>();
        var offline = m_CurrentPlayer.GetComponent<Player>();

        var character = offline.CurrentCharacter;
        online.Drive(character);
        Destroy(offline.gameObject);

        m_CurrentPlayer.Value = gameObject;
    }
}