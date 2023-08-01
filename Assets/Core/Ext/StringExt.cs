/// extensions to strings
static class StringExt {
    /// get the substring until the delimiter, or the entire string if no match
    public static string SubstringUntil(this string str, char delimiter) {
        var index = str.IndexOf(delimiter);
        if (index == -1) {
            return str;
        }

        return str.Substring(0, index);
    }

    #if UNITY_EDITOR
    /// get the levenshtein distance to another string
    /// see: https://github.com/DanHarltey/Fastenshtein/blob/master/src/Fastenshtein/StaticLevenshtein.cs
    public static int DistanceTo(this string str, string other) {
        if (other.Length == 0) {
            return str.Length;
        }

        var costs = new int[other.Length];
        for (var i = 0; i < costs.Length; ) {
            costs[i] = ++i;
        }

        for (var i = 0; i < str.Length; i++) {
            var cost = i;
            var prevCost = i;
            var currChar = str[i];

            for (int j = 0; j < other.Length; j++) {
                var currCost = cost;
                cost = costs[j];

                if (currChar != other[j]) {
                    if (prevCost < currCost) {
                        currCost = prevCost;
                    }

                    if (cost < currCost) {
                        currCost = cost;
                    }

                    ++currCost;
                }

                costs[j] = currCost;
                prevCost = currCost;
            }
        }

        return costs[costs.Length - 1];
    }
    #endif
}