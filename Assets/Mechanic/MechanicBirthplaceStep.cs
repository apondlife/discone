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
        "Stair",
        "Wall",
        "Exit",
    };
}

}