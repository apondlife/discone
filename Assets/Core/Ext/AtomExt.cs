using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// extensions for atoms
/// TODO: code generation
public static class AtomExt {
    // -- queries --
    /// get a component in this game object atom
    public static C GetComponent<C>(
        this GameObjectVariable obj
    ) where C: Component {
        return obj.Value.GetComponent<C>();
    }

    /// get a component in this player atom
    public static C GetComponent<C>(
        this DisconePlayerVariable obj
    ) where C: Component {
        return obj.Value.GetComponent<C>();
    }

    /// get a component in this character atom
    public static C GetComponent<C>(
        this DisconeCharacterVariable obj
    ) where C: Component {
        return obj.Value.GetComponent<C>();
    }
}