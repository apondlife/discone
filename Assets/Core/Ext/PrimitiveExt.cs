/// extensions on int primitives
static class IntExtensions {
    /// formats the int with a + or - sign
    public static string ToSignedString(this int i) {
        return i < 0 ? i.ToString() : "+" + i;
    }
}