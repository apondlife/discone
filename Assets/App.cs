using UnityEngine;

/// the app
sealed class App: MonoBehaviour {
    // -- module --
    /// the singleton instance
    static App Instance;

    // -- refs --
    [Header("refs")]
    [Tooltip("the store")]
    [SerializeField] Store m_Store;

    [Tooltip("the character definitions")]
    [SerializeField] CharacterDefs m_Defs;

    // -- lifecycle --
    void Awake() {
        // destroy this if we already have an app
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        // store the singleton
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
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