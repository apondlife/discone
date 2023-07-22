namespace Discone {

// TODO: maybe an interface like `MechanicVariable`
/// the birthplace step dialogue variable
struct MechanicBirthplaceStep {
    // -- data --
    ///.
    public static readonly string Name = "$Birthplace_Step";

    /// .
    public static readonly string[] Values = new string[] {
        "Ramp",
        "Stair",
        "Wall",
        "Tunnel",
        "Exit",
    };
}

}