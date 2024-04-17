
namespace Soil {

public abstract class SimpleSystem<Container>: System<Container> {
    // -- Soil.System --
    protected override SystemState State { get; set; } = new();
}

}