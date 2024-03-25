namespace ProceduralPlanets; //
using Godot;
using System;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class OrbitDisplay : MeshInstance3D
{
	[Export]
	public CelestialBody Body { get; set; }
	[Export]
	public CelestialBody RelativeBody { get; set; }
	
	public void UpdateOrbit(IEnumerable<Vector3> points)
	{
	}
}
