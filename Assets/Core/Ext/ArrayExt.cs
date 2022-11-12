using UnityEngine;

static class ArrayExt {
    /// sample a random value from the array
    public static T Sample<T>(this T[] a) {
        return a[Random.Range(0, a.Length)];
    }
}