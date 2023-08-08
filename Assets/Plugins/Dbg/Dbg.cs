using System.Text;
using System.Collections.Generic;
using UnityEngine;

/// debugging utilities
public static class Dbg {
    // -- commands --
    /// dump enumerable as a list-like string
    public static string Dump<T>(IEnumerable<T> list) {
        var sb = new StringBuilder();

        sb.Append("[");

        var e = list.GetEnumerator();

        // add an entry (& comma if next) for each item
        var cont = e.MoveNext();
        while (cont) {
            sb.Append(e.Current.ToString());
            cont = e.MoveNext();
            if (cont) {
                sb.Append(",");
            }
        }

        sb.Append("]");

        return sb.ToString();
    }

    // -- c/unity
    /// add a transform to a top-level named parent (hierarchy with /)
    public static void AddToParent(string name, Component obj) {
        var tail = name;
        Transform prevParent = null;
        while (tail.Length > 0) {
            var split = tail.Split('/', 2);
            var head = split[0];
            tail = split.Length > 1 ? split[1] : "";

            // create parent if necessary
            var parent = GameObject.Find(head)?.transform;
            if (parent == null) {
                parent = new GameObject(head).transform;
            }
            parent.parent = prevParent;
            prevParent = parent;
        }

        // add object to parent
        obj.transform.parent = prevParent;
    }
}