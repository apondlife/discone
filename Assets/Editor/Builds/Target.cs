namespace Builds {

/// the target name
public static class Target {
    // -- options --
    /// the mac target
    public const string All = "all";

    /// the mac target
    public const string Mac = "mac";

    /// the windows target
    public const string Windows = "win";

    /// the windows server (standalone) target
    public const string WindowsServer = "win-server";

    // -- factories --
    /// decode the target from a raw arg
    public static string Decode(string target) {
        return target switch {
            Mac            => Mac,
            Windows        => Windows,
            WindowsServer  => WindowsServer,
            All            => All,
            ""             => All,
            _              => throw new System.Exception($"[build] unknown target {target}"),
        };
    }
}

}