namespace Discone {

/// how the character is simulated on the client
public enum CharacterSimulation {
    None, // no simulation
    Remote, // state is received from the server and extrapolated naively
    Local // state is being simulated locally and sent to the server
}

}