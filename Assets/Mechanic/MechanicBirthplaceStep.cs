namespace Discone {

// TODO: maybe an interface like `MechanicVariable`
/// the birthplace step dialogue variable
struct MechanicBirthplaceStep {
    // -- data --
    ///.
    public static readonly string Name = "$BirthplaceStep";

    /// .
    public static readonly string[] Values = new string[] {
        "Ramp",
        "Wall",
        "Exit",
    };

    // -- queries --
    /// .
    public static string InitialValue {
        get => Values[0];
    }
}

}