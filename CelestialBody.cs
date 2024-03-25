using System.Collections.Generic;

namespace ProceduralPlanets;

using Godot;

[GlobalClass]
[Tool]
public partial class CelestialBody : Resource
{
    [Export]
    public Vector3 Position { get; set; } 
    [Export]
    public Vector3 Velocity { get; set;}
    [Export]
    public float Mass { get; set; }
    
    public delegate void
    
    public void UpdateVelocity(IEnumerable<CelestialBody> celestialBodies, float timeStep)
    {
        foreach (CelestialBody body in celestialBodies)
        {
            if (body == this) return;
            
			const float G = (float) 6.6743015e-11;

			Vector3 difference = Position - body.Position;
			float acceleration = G * body.Mass / difference.LengthSquared();
			Velocity -= acceleration * difference.Normalized() * timeStep;
        }
    }

    public void UpdatePosition(float timeStep)
    {
        Position += Velocity * timeStep;
    }
}
