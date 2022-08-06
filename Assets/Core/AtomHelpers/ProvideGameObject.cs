using UnityEngine;
using UnityAtoms.BaseAtoms;

/// provides a game object as an atom variable
sealed class ProvideGameObject: MonoBehaviour {
    // -- fields --
    [Tooltip("the container for this game object")]
    [SerializeField] GameObjectVariable m_Variable;

    [Tooltip("if the game object overrides nonnull values")]
    [SerializeField] bool m_IsTransient = true;

    // -- lifecycle --
    void Awake() {
        if (m_IsTransient || m_Variable.Value == null) {
            m_Variable.Value = gameObject;
        }
    }
}