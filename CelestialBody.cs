using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ProceduralPlanets;

[GlobalClass]
[Tool]
public partial class CelestialBody : Resource, IEquatable<CelestialBody>
{

    [Export]
    public Vector3 Position { get; set; }

    [Export]
    public Vector3 Velocity { get; set; }

    [Export]
    public float Mass { get; set; } = 1f;

    public void UpdateVelocity(IEnumerable<CelestialBody> celestialBodies, float timeStep)
    {
        foreach (CelestialBody body in celestialBodies)
        {
            if (body == this) continue;
            
            const float G = (float)6.6743015e-11;

            Vector3 difference = Position - body.Position;
            float acceleration = G * body.Mass / difference.LengthSquared();
            Velocity -= acceleration * difference.Normalized() * timeStep;
        }
    }

    public void UpdatePosition(float timeStep)
    {
        Position += Velocity * timeStep;
    }
    
    public bool Equals(CelestialBody other)
    {
        return other != null && Position == other.Position && Velocity == other.Velocity && Mass == other.Mass;
    }
}

