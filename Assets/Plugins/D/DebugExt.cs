using System.Text;
using System.Collections.Generic;

namespace D {

/// debugging utilities
public static class Dbg {
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
}

}