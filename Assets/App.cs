using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

/// the app
sealed class App: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the store")]
    [SerializeField] Store m_Store;

    [Tooltip("the character definitions")]
    [SerializeField] CharacterDefs m_Defs;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when the menu state changes")]
    [SerializeField] BoolEvent m_MenuOpenChanged;

    [Tooltip("when the online server starts")]
    [SerializeField] VoidEvent m_Online_ServerStarted;

    [Tooltip("when the online client starts")]
    [SerializeField] VoidEvent m_Online_ClientStarted;

    // -- props --
    /// the subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Awake() {
        // move to top-level
        transform.SetParent(null);

        // hide cursor in build
        #if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        m_Subscriptions.Add(m_MenuOpenChanged, OnMenuIsOpenChanged);
        #endif

        // subscribe to events
        Debug.Log($"[appppp] subscribing to online event");
        #if UNITY_SERVER
        m_Subscriptions.Add(m_Online_ServerStarted, OnOnlineStarted);
        #else
        m_Subscriptions.Add(m_Online_ClientStarted, OnOnlineStarted);
        #endif
        Debug.Log($"[appppp] subscribed to online event");
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    async void OnApplicationQuit() {
        // save the world state
        // TODO: this should probably only save the world if we are the host
        await m_Store.Save();
    }

    // -- events --
    /// .
    void OnMenuIsOpenChanged(bool isOpen) {
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// .
    void OnOnlineStarted() {
        // load the world state
        Debug.Log($"[appppp] loading store");
        Debug.Log($"[appppp] zzz");
        m_Store.Load();
    }
}

}