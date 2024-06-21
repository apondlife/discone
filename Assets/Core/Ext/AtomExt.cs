using UnityAtoms;

namespace Discone {

static partial class AtomExt {
    public static T Next<T>(this IPair<T> p) {
        return p.Item1;
    }

    public static T Prev<T>(this IPair<T> p) {
        return p.Item2;
    }
}

}