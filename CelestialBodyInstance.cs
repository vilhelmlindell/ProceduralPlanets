namespace ProceduralPlanets;

using Godot;

[Tool]
public partial class CelestialBodyInstance : Node3D
{
    [Export]
    public CelestialBody Body
    {
        get => _body;
        set
        {
            _body = value;
            Position = _body.Position;
        }
    }
    private CelestialBody _body;

    public override void _Process(double delta)
    {
        Position = Body.Position;
    }
}
