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
}