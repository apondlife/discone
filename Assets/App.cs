using UnityEngine;

/// the app
sealed class App: MonoBehaviour {
    private static App Instance;
    // -- refs --
    [Header("refs")]
    [Tooltip("the store")]
    [SerializeField] Store m_Store;

    [Tooltip("the character definitions")]
    [SerializeField] CharacterDefs m_Defs;

    // -- lifecycle --
    void Awake() {
        if(Instance == null) {
            Instance = this;
            transform.SetParent(null);
        } else {
            Destroy(gameObject);
        }

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