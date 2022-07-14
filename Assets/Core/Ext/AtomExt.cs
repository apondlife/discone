using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// extensions for atoms
/// TODO: code generation
public static class AtomExt {
    // -- queries --
    /// get a component in this game object atom
    public static T GetComponent<T>(
        this GameObjectVariable obj
    ) where T: MonoBehaviour {
        return obj.Value.GetComponent<T>();
    }

    /// get a component in this player atom
    public static T GetComponent<T>(
        this DisconePlayerVariable obj
    ) where T: MonoBehaviour {
        return obj.Value.GetComponent<T>();
    }

    /// get a component in this character atom
    public static T GetComponent<T>(
        this DisconeCharacterVariable obj
    ) where T: MonoBehaviour {
        return obj.Value.GetComponent<T>();
    }
}