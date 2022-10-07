namespace Builds {

/// the build variant name
public static class Variant {
    // -- options --
    /// the release variant
    public const string Release = "release";

    /// the playtest variant
    public const string Playtest = "playtest";

    // -- factories --
    /// decode the variant from a raw arg
    public static string Decode(string variant) {
        return variant switch {
            Playtest => Playtest,
            Release  => Release,
            ""       => Release,
            _        => throw new System.Exception($"[build] unknown variant {variant}"),
        };
    }
}

}