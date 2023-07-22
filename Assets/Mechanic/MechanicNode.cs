using System;

namespace Discone {

// -- impls --
readonly struct MechanicNode {
    // -- types --
    [Flags]
    public enum Tag {
        Last = 1 << 0,
        Hide = 1 << 1,
        Tree = 1 << 2,
        Fork = 1 << 3,
        Leaf = 1 << 4,
    }

    // -- constants --
    /// parts of a tree (that should not continue)
    public const Tag TreeLike = Tag.Tree | Tag.Fork | Tag.Leaf;

    /// parts of a tree that branch (that should not continue)
    public const Tag Branching = Tag.Tree | Tag.Fork;

    // -- props --
    /// the name of the next node
    public readonly string Next;

    /// the node's tags
    public readonly Tag Tags;

    /// the name of the node's scope
    public readonly string Scope;

    // -- lifetime --
    public MechanicNode(string next, Tag tags, string scope) {
        this.Next = next;
        this.Tags = tags;
        this.Scope = scope;
    }

    // -- queries --
    /// if this node is the last in a sequence
    public bool IsLast {
        get => (Tags & Tag.Last) != 0;
    }

    // if this node auto hides
    public bool isHiding {
        get => (Tags & Tag.Hide) != 0;
    }

    /// if this node is a tree
    public bool IsTree {
        get => (Tags & Tag.Tree) != 0;
    }

    /// if this node is in a leaf
    public bool IsLeaf {
        get => (Tags & Tag.Leaf) != 0;
    }

    /// if this node branches
    public bool IsBranching {
        get => (Tags & Branching) != 0;
    }

    // -- debug --
    public override string ToString() {
        return $"<MechanicNode next={Next ?? "(none)"} tags=({Tags})>";
    }
}

}