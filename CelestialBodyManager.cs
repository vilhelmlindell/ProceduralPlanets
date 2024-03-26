using System.Linq;
using Godot;
using System.Collections.Generic;

namespace ProceduralPlanets;

[Tool]
public partial class CelestialBodyManager : Node
{
	[Export] private bool _isRunning;
	[Export] private string _groupName = "CelestialBodies";
	[Export] private float _simulationSpeed = 1.0f;
	[Export] private int _orbitIterations = 10;

	private Dictionary<CelestialBody, List<Vector3>> _orbitsByBody = [];

	public override void _PhysicsProcess(double delta)
	{
		float timeStep = (float) delta * _simulationSpeed;

		var celestialBodyInstances = new List<CelestialBodyInstance>(GetTree().GetNodesInGroup(_groupName)
			.Select(node => (CelestialBodyInstance)node));

		List<CelestialBody> celestialBodies = celestialBodyInstances.Select(body => body.Body).ToList();
		
		
		if (_isRunning && !Engine.IsEditorHint())
		{
			UpdateBodies(celestialBodies, timeStep);
		}

		_orbitsByBody = ComputeOrbits(celestialBodies, timeStep);
		UpdateOrbits(celestialBodyInstances);
	}

	private void UpdateOrbits(List<CelestialBodyInstance> celestialBodyInstances)
	{
		foreach (CelestialBodyInstance bodyInstance in celestialBodyInstances)
		{
			foreach (Node node in bodyInstance.GetChildren())
			{
				if (node is not OrbitDisplay orbitDisplay) continue;

				List<Vector3> orbit = _orbitsByBody[bodyInstance.Body];
				List<Vector3> relativeOrbit = orbitDisplay.RelativeBodyInstance != null ? _orbitsByBody[orbitDisplay.RelativeBodyInstance.Body] : [];
				orbitDisplay.UpdateOrbit(orbit, relativeOrbit);
			}
		}
	}

	private Dictionary<CelestialBody, List<Vector3>> ComputeOrbits(IReadOnlyList<CelestialBody> celestialBodies, float timeStep)
	{
		List<CelestialBody> clonedBodies = celestialBodies.Select(body => new CelestialBody
		{
			Mass = body.Mass,
			Velocity = body.Velocity,
			Position = body.Position,
		}).ToList();
		List<List<Vector3>> orbits = Enumerable.Repeat(new List<Vector3>(), clonedBodies.Count).ToList();
		var orbitsByBody = new Dictionary<CelestialBody, List<Vector3>>();

		for (var i = 0; i < _orbitIterations; i++)
		{
			UpdateBodies(clonedBodies, timeStep);
			for (var j = 0; j < clonedBodies.Count; j++)
			{
				CelestialBody body = clonedBodies[j];
				orbits[j].Add(body.Position);
			}
		}

		for (var i = 0; i < celestialBodies.Count; i++)
		{
			CelestialBody body = celestialBodies[i];
			List<Vector3> orbit = orbits[i];
			orbitsByBody.Add(body, orbit);
		}

		for (var i = 0; i < orbits[0].Count; i++)
		{
			GD.Print(orbits[0][i]);
		}
		return orbitsByBody;
	}

	private static void UpdateBodies(List<CelestialBody> celestialBodies, float timeStep)
	{
		foreach (CelestialBody body in celestialBodies)
		{
			body.UpdateVelocity(celestialBodies, timeStep);
		}
		foreach (CelestialBody body in celestialBodies)
		{
			body.UpdatePosition(timeStep);
		}
	}
}

