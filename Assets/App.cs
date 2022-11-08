using UnityEngine;

/// the app
sealed class App: MonoBehaviour {
    // -- module --
    /// the singleton instance
    static App s_Instance;

    // -- refs --
    [Header("refs")]
    [Tooltip("the store")]
    [SerializeField] Store m_Store;

    [Tooltip("the character definitions")]
    [SerializeField] CharacterDefs m_Defs;

    // -- lifecycle --
    void Awake() {
        // destroy this if we already have an app
        if (s_Instance != null) {
            Destroy(gameObject);
            return;
        }

        // store the singleton
        s_Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // hide cursor in build
        #if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        #endif
    }

    void Start() {
        // load the world state
        m_Store.Load();
    }

    async void OnApplicationQuit() {
        // save the world state
        // TODO: this should probably only save the world if we are the host
        await m_Store.Save();
    }
}