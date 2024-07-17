using System;
using Random = UnityEngine.Random;

namespace Soil {

public static class ArrayExt {
    /// sample a random value from the array
    public static T Sample<T>(this T[] a) {
        return a[Random.Range(0, a.Length)];
    }

    /// copies the elements from the rhs into the lhs, mutating the lhs
    public static T[] Concat<T>(T[] lhs, Array rhs) {
        var llen = lhs.Length;
        var rlen = rhs.Length;

        Array.Resize(ref lhs, llen + rlen);
        Array.Copy(rhs, 0, lhs, llen, rlen);

        return lhs;
    }
}

}