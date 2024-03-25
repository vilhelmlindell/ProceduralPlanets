using System.Linq;
using Godot;
using System.Collections.Generic;

namespace ProceduralPlanets;

[Tool]
public partial class CelestialBodyManager : Node
{
	[Export] private string GroupName = "CelestialBodies";
	[Export] private float SimulationSpeed = 1.0f;
	[Export] private bool IsRunning;
	[Export] private int PathLength = 10000;
	
	private Godot.Collections.Array<CelestialBodyInstance> CelestialBodyInstances { get; set; }

	private IEnumerable<CelestialBody> _previousCelestialBodies = [];

	public override void _PhysicsProcess(double delta)
	{
		if (!IsRunning) return;

		float timeStep = (float) delta * SimulationSpeed;

		CelestialBodyInstances = new Godot.Collections.Array<CelestialBodyInstance>(GetTree().GetNodesInGroup(GroupName)
			.Select(node => (CelestialBodyInstance)node));

		var celestialBodies = CelestialBodyInstances.Select(body => body.Body);

		_previousCelestialBodies = celestialBodies;
	}

	private static Dictionary<CelestialBody, List<Vector3>> UpdateOrbits(Godot.Collections.Array<CelestialBodyInstance> celestialBodies)
	{
		var clonedBodies = celestialBodies.Duplicate(true);
		//var dic = keys.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);	
	}

	private static void UpdateBodies(IEnumerable<CelestialBody> celestialBodies, float timeStep)
	{
		foreach (CelestialBody body in celestialBodies)
		{
			body.PropertyChanged += UpdateOrbits;
			body.UpdateVelocity(celestialBodies, timeStep);
			body.UpdatePosition(timeStep);
		}
	}
}

