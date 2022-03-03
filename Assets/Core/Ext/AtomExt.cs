using UnityAtoms.BaseAtoms;
using UnityEngine;

/// extensions for atoms
public static class AtomExt {
    /// get a component in this game object
    /// TODO: code generation
    public static T GetComponent<T>(this GameObjectVariable obj) where T: MonoBehaviour {
        return obj.Value.GetComponent<T>();
    }
}