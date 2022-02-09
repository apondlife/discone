using UnityEngine;
using UnityAtoms.BaseAtoms;

/// provides a game object as an atom variable
class ProvideGameObject: MonoBehaviour {
    // -- references --
    [Tooltip("the container for this game object")]
    [SerializeField] GameObjectVariable m_Variable;

    // -- lifecycle --
    void Awake() {
        m_Variable.SetValue(gameObject);
    }
}