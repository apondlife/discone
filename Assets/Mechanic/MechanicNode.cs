using System;

namespace Discone {

// -- impls --
readonly struct MechanicNode {
    // -- types --
    [Flags]
    public enum Tag {
        Last = 1 << 0,
        Tree = 1 << 1,
        Fork = 1 << 2,
        Leaf = 1 << 3,
    }

    // -- constants --
    /// parts of a tree (that should not continue)
    public const Tag TreeLike = Tag.Tree | Tag.Fork | Tag.Leaf;

    // -- props --
    /// the name of the next node
    public readonly string Next;

    /// the node's tags
    public readonly Tag Tags;

    // -- lifetime --
    public MechanicNode(string next, Tag tags) {
        this.Next = next;
        this.Tags = tags;
    }

    // -- queries --
    /// if this node is the last in a sequence
    public bool IsLast {
        get => (Tags & Tag.Last) != 0;
    }

    /// if this node is a tree
    public bool IsTree {
        get => (Tags & Tag.Tree) != 0;
    }

    /// if this node is in a leaf
    public bool IsLeaf {
        get => (Tags & Tag.Leaf) != 0;
    }

    /// if this node is tree-like
    public bool IsTreeLike {
        get => (Tags & TreeLike) != 0;
    }

    // -- debug --
    public override string ToString() {
        return $"<MechanicNode next={Next ?? "(none)"} tags=({Tags})>";
    }
}

}