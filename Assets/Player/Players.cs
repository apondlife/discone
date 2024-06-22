using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("m_InitialPlayer")]
    [Tooltip("the current player")]
    [SerializeField] PlayerVariable m_CurrentPlayer;

    [Tooltip("the entities repository")]
    [SerializeField] EntitiesVariable m_Entities;

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

    /// if the initial player has their input bound
    bool m_IsInitialPlayerBound;

    // -- lifecycle --
    void Awake() {
        // listen until the initial online player connects
        m_OnlinePlayer_Connected.Register(OnOnlinePlayerConnected);

        // when the online player is created, create the discone player
        void OnOnlinePlayerConnected(OnlinePlayer onlinePlayer) {
            m_OnlinePlayer_Connected.Unregister(OnOnlinePlayerConnected);
            onlinePlayer.Link(m_CurrentPlayer.Value);
        }

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
    void CreatePlayer(OnlinePlayer onlinePlayer, Input input) {
        var t = onlinePlayer.transform;
        onlinePlayer.transform.position = m_CurrentPlayer.Value.transform.position;
        onlinePlayer.Spawn();

        var player = Instantiate(m_PlayerPrefab, t.position, t.rotation);
        onlinePlayer.Link(player);

        FinishCreatingPlayer(player, input);
    }

    /// bind the input to the player
    void FinishCreatingPlayer(Player player, Input input) {
        player.Bind(input);

        #if UNITY_EDITOR
        var t = player.transform;
        t.parent = transform;
        input.transform.parent = t;
        #endif
    }

    // -- events --
    void OnPlayerJoined(PlayerInput playerInput) {
        Log.Player.I($"player joined - {playerInput.playerIndex} [scheme: {playerInput.currentControlScheme}]");

        var input = playerInput.GetComponent<Input>();

        // TODO: don't spawn the online player on client connect?
        if (!m_IsInitialPlayerBound) {
            FinishCreatingPlayer(m_CurrentPlayer.Value, input);
            m_IsInitialPlayerBound = true;
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
        Log.Player.I($"player left - {input.playerIndex}");
    }
}

}