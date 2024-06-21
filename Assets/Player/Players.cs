using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

public class Players: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the player prefab")]
    [SerializeField] Player m_PlayerPrefab;

    // -- refs --
    [Header("refs")]
    [Tooltip("the input manager")]
    [SerializeField] PlayerInputManager m_InputManager;

    [Tooltip("the initial player")]
    [SerializeField] DisconePlayerVariable m_InitialPlayer;

    // TODO: should this be gameobjectvariable / cameravariable
    [Tooltip("the transform for the player's look viewpoint")]
    [SerializeField] Transform m_Look;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip("create an online player")]
    [SerializeField] VoidEvent m_Online_CreatePlayer;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when an online player connects")]
    [SerializeField] OnlinePlayerEvent m_OnlinePlayer_Connected;

    // -- props --
    /// the subscriptions
    DisposeBag m_Subscriptions = new();

    /// the initial online player
    OnlinePlayer m_InitialOnlinePlayer;

    // -- lifecycle --
    // Start is called before the first frame update
    void Awake() {
        // bind events
        m_Subscriptions
            .Add(m_InputManager.playerJoinedEvent, OnPlayerJoined)
            .Add(m_InputManager.playerLeftEvent, OnPlayerLeft);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// create a player from the online player
    void CreatePlayer(OnlinePlayer onlinePlayer, PlayerInput input) {
        var t = onlinePlayer.transform;
        onlinePlayer.Spawn();
        var player = Instantiate(m_PlayerPrefab, t.position, t.rotation);
        FinishCreatingPlayer(player, input);
    }

    /// bind the input to the player
    void FinishCreatingPlayer(Player player, PlayerInput input) {
        player.Bind(input.actions, m_Look);

        #if UNITY_EDITOR
        var t = player.transform;
        t.parent = transform;
        input.transform.parent = t;
        #endif
    }

    // -- events --
    void OnPlayerJoined(PlayerInput input) {
        Log.Player.I($"new player joined {input.playerIndex}");

        // TODO: don't spawn the online player on client connect?
        if (m_InitialPlayer) {
            FinishCreatingPlayer(m_InitialPlayer.Value, input);
            m_InitialPlayer = null;
            return;
        }

        // listen until the next online player connects
        m_OnlinePlayer_Connected.Register(OnOnlinePlayerConnected);

        // when the online player is created, create the discone player
        void OnOnlinePlayerConnected(OnlinePlayer onlinePlayer) {
            m_OnlinePlayer_Connected.Unregister(OnOnlinePlayerConnected);
            CreatePlayer(onlinePlayer, input);
        }

        m_Online_CreatePlayer.Raise();
    }

    void OnPlayerLeft(PlayerInput input) {
        Log.Player.I($"player left {input.playerIndex}");
    }
}

}