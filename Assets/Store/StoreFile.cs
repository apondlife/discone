/// a file read & written by the store
public interface StoreFile {
    // -- constants --
    /// when a file has no version
    static int NoVersion = -1;

    // -- props --
    /// the file's local version
    int Version { get; }

    // -- queries --
    /// the current version for a particular type, if any
    static int CurrentVersion<F>() where F: StoreFile {
        System.Attribute attr = System.Attribute.GetCustomAttribute(
            typeof(F),
            typeof(StoreVersion)
        );

        return (attr as StoreVersion)?.Value ?? NoVersion;
    }
}

/// store file utitlies
public static class StoreFileExt {
    /// get the version for this type
    public static int CurrentVersion<F>(
        this F file
    ) where F: StoreFile {
        return StoreFile.CurrentVersion<F>();
    }
}