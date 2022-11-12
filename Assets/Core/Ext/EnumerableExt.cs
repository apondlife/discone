using System.Collections.Generic;
using System.Linq;

static class EnumerableExt {
    /// sample a random value from the array
    public static T Sample<T>(this IEnumerable<T> e) {
        return e.ToArray().Sample();
    }
}