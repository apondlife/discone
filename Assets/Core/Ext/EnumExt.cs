using System;
using System.Collections.Generic;
using System.Linq;

static class EnumExt {
    /// get the enum values as an enumerable
    public static IEnumerable<T> Enumerable<T>() {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// get the enum values as an array
    public static T[] Array<T>() {
        return EnumExt.Enumerable<T>().ToArray();
    }
}