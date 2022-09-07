using UnityEngine;

/// the app
sealed class App: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the store")]
    [SerializeField] Store m_Store;

    // -- lifecycle --
    void Start() {
        // load the world state
        m_Store.Load();
    }

    void OnApplicationQuit() {
        // save the world state
        m_Store.Save();
    }
}