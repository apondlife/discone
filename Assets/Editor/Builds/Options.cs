namespace Builds {

/// the options
struct Options {
    // -- props --
    /// the variant to build, if any; empty is "release"
    public string Variant;

    /// the target to build, if any; empty is "all"
    public string Target;

    // -- queries --
    /// if the build includes the target
    public bool IncludeTarget(string target) {
        if (Target == Builds.Target.All) {
            return true;
        }

        return Target == target;
    }

    // -- lifetime --
    /// init from command line args
    /// TODO: accept a dictionary parsed from cli args in a standard format
    public static Options Decode(string[] args) {
        var o = new Options();

        // parse all the args
        var isParsing = false;
        for (var i = 0; i < args.Length; i++) {
            // scan until '--'
            if (!isParsing) {
                var arg = args[i];
                if (arg == "--") {
                    isParsing = true;
                }

                continue;
            }

            // grab key and value
            var key = args[i];
            var val = args[i + 1];

            // set option
            switch (key) {
            case "--variant":
                o.Variant = Builds.Variant.Decode(val); break;
            case "--target":
                o.Target = Builds.Target.Decode(val); break;
            }

            // move to the next pair
            i += 1;
        }

        return o;
    }
}

}