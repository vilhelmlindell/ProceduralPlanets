namespace ProceduralPlanets; 

using Godot;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class OrbitDisplay : MeshInstance3D
{
	[Export]
	public bool Enabled
	{
		get => _enabled;
		set
		{
			_enabled = value;
			Visible = _enabled;
		}
	}
	private bool _enabled;
	
	[Export]
	public CelestialBodyInstance RelativeBodyInstance;

	[Export]
	public Color Color;
	
	public void UpdateOrbit(List<Vector3> orbit, List<Vector3> relativeOrbit)
	{
		var immediateMesh = new ImmediateMesh();
		var material = new OrmMaterial3D();

		Mesh = immediateMesh;
		
		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);


		for (var i = 0; i < orbit.Count - 1; i ++)
		{
			Vector3 relativeVertex = i >= relativeOrbit.Count ? Vector3.Zero : relativeOrbit[i];
			Vector3 vertex = orbit[i] - relativeVertex;
			
			immediateMesh.SurfaceAddVertex(vertex);
			
			Vector3 nextRelativeVertex = i + 1 >= relativeOrbit.Count ? Vector3.Zero : relativeOrbit[i + 1];
			Vector3 nextVertex = orbit[i + 1] - nextRelativeVertex;
			
			immediateMesh.SurfaceAddVertex(nextVertex);
		}
		
		immediateMesh.SurfaceEnd();

		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.AlbedoColor = Color;
	}
}
