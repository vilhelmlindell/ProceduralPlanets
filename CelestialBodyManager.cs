using System.Collections.Generic;
using Godot;
using System.Linq;
using Godot.Collections;

namespace ProceduralPlanets;

[Tool]
public partial class CelestialBodyManager : Node
{
	[Export] private string GroupName = "CelestialBodies";
	[Export] private float SimulationSpeed = 1.0f;
	[Export] private bool IsRunning;
	[Export] private int PathLength = 10000;
	
	public override void _Process(double delta)
	{
		if (!IsRunning) return;

		float timeStep = (float) delta * SimulationSpeed;

		var celestialBodies = new Array<CelestialBody>(GetTree().GetNodesInGroup(GroupName)
			.Select(node => ((CelestialBodyInstance)node).Body));
		
		UpdateBodies(celestialBodies, timeStep);

		Array<CelestialBody> clonedBodies = celestialBodies.Duplicate(true);

		for (int i = 0; i < 10000; i++)
		{
			GD.Print(i);
			UpdateBodies(clonedBodies, timeStep);
		}
	}

	private static void UpdateBodies(IEnumerable<CelestialBody> celestialBodies, float timeStep)
	{
		foreach (CelestialBody body in celestialBodies)
		{
			body.UpdateVelocity(celestialBodies, timeStep);
			body.UpdatePosition(timeStep);
		}
	}
}

