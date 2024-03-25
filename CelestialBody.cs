using System;
using System.Collections.Generic;
using Godot;

namespace ProceduralPlanets;

[GlobalClass]
[Tool]
public partial class CelestialBody : Resource
{

    [Export]
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
        }
    }
    private Vector3 _position;

    [Export]
    public Vector3 Velocity
    {
        get => _velocity;
        set
        {
            _velocity = value;
        }
    }
    private Vector3 _velocity;

    [Export]
    public float Mass
    {
        get => _mass;
        set
        {
            _mass = value;
        }
    }
    private float _mass;
    
    public void UpdateVelocity(IEnumerable<CelestialBody> celestialBodies, float timeStep)
    {
        foreach (CelestialBody body in celestialBodies)
        {
            if (body == this) return;

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
}

