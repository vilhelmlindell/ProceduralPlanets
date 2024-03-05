﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

namespace ProceduralPlanets
{
    [Tool]
    public partial class Planet : MeshInstance3D
    {
        [ExportGroup("Mesh")]
        [Export(PropertyHint.Range, "0,10")]
        private int Subdivisions
        {
            get => _subdivisions;
            set
            {
                _subdivisions = value;
                Initialize();
            }
        }
        private int _subdivisions = 5;

        [Export]
        private bool RegenerateMesh
        {
            get => false;
            set
            {
                if (value)
                {
                    Initialize();
                }
            }
        }

        [ExportGroup("Procedural Generation")]
        [Export]
        private float Amplitude
        {
            get => _amplitude;
            set
            {
                _amplitude = value;
                Update();
            }
        }
        private float _amplitude = 1f;

        [Export]
        private float Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                Update();
            }
        }
        private float _frequency = 1f;

        [Export]
        private float Lacunarity
        {
            get => _lacunarity;
            set
            {
                _lacunarity = value;
                Update();
            }
        }
        private float _lacunarity = 1f;

        [Export]
        private float Gain
        {
            get => _gain;
            set
            {
                _gain = value;
                Update();
            }
        }
        private float _gain = 1f;

        [ExportGroup("Requirements")]
        [Export(PropertyHint.File)]
        private string _computeShader = "res://ComputeHeights.glsl";
        private string _spatialShader = "res://Planet.gdshader";

        private RenderingDevice _rd;
        private Rid _shader;
        private Rid _pipeline;
        private Rid _uniformSet;
        private Rid _heightBuffer;
        private Godot.Collections.Array<RDUniform> _bindings = [];

        private Vector3[] _icosphereVertices;
        private Vector3[] _vertices;
        private int[] _indices;
        private float[] _heights;

        public override void _Ready()
        {
            Initialize();
        }

        public override void _Process(double delta) { }

        private void Initialize()
        {
            (_icosphereVertices, _indices) = GenerateIcosphere(Subdivisions);
            _vertices = (Vector3[])_icosphereVertices.Clone();
            _heights = new float[_icosphereVertices.Length];
            SetupComputeShader();
            CreateUniforms();
            RunComputeShader();
            UpdateHeights();
            UpdateVertices();
            CreateMesh();
        }

        private void Update()
        {
            CreateUniforms();
            RunComputeShader();
            UpdateHeights();
            UpdateVertices();
            CreateMesh();
        }

        #region ICOSPHERE MESH

        private void CreateMesh()
        {
            var mesh = new ArrayMesh();
            var arrays = new Godot.Collections.Array();
            _ = arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = _vertices;
            arrays[(int)Mesh.ArrayType.Index] = _indices;

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            //
            //ShaderMaterial shaderMaterial = CreateShaderMaterial();
            //shaderMaterial.SetShaderParameter()
            //_mesh.SurfaceSetMaterial(0, shaderMaterial);

            Mesh = mesh;
        }

        //private ShaderMaterial CreateShaderMaterial()
        //{
        //    var material = new ShaderMaterial();
        //    Shader shader = ResourceLoader.Load<Shader>("res://Planet.gdshader");
        //    material.Shader = shader;
        //    return material;
        //}

        private static (Vector3[], int[]) GenerateIcosphere(int subdivisions)
        {
            (List<Vector3> vertices, List<int> indices) = GenerateIcosahedron();

            for (int i = 0; i < subdivisions; i++)
            {
                Subdivide(vertices, ref indices);
            }

            return (vertices.ToArray(), indices.ToArray());
        }

        private static (List<Vector3>, List<int>) GenerateIcosahedron()
        {
            const float X = 0.525731112119133606f;
            const float Z = 0.850650808352039932f;
            const float N = 0f;
            // csharpier-ignore
            List<Vector3> vertices =
            [
                new Vector3(-X,N,Z), new Vector3(X,N,Z), new Vector3(-X,N,-Z), new Vector3(X,N,-Z),
                new Vector3(N,Z,X), new Vector3(N,Z,-X), new Vector3(N,-Z,X), new Vector3(N,-Z,-X),
                new Vector3(Z,X,N), new Vector3(-Z,X, N), new Vector3(Z,-X,N), new Vector3(-Z,-X, N)
            ];
            // csharpier-ignore
            List<int> indices =
            [
                0, 4, 1, 0, 9, 4, 9, 5, 4, 4, 5, 8, 4, 8, 1,
                8, 10, 1, 8, 3, 10, 5, 3, 8, 5, 2, 3, 2, 7, 3,
                7, 10, 3, 7, 6, 10, 7, 11, 6, 11, 0, 6, 0, 1, 6,
                6, 1, 10, 9, 0, 11, 9, 11, 2, 9, 2, 5, 7, 2, 11
            ];

            return (vertices, indices);
        }

        private static void Subdivide(List<Vector3> vertices, ref List<int> indices)
        {
            var lookup = new Dictionary<(int, int), int>();
            var result = new List<int>();

            for (int i = 0; i < indices.Count; i += 3)
            {
                int[] mid = new int[3];
                for (int edge = 0; edge < 3; edge++)
                {
                    mid[edge] = VertexForEdge(
                        lookup,
                        vertices,
                        indices[i + edge],
                        indices[i + (edge + 1) % 3]
                    );
                }
                result.Add(indices[i]);
                result.Add(mid[0]);
                result.Add(mid[2]);

                result.Add(indices[i + 1]);
                result.Add(mid[1]);
                result.Add(mid[0]);

                result.Add(indices[i + 2]);
                result.Add(mid[2]);
                result.Add(mid[1]);

                result.Add(mid[0]);
                result.Add(mid[1]);
                result.Add(mid[2]);
            }

            indices = result;
        }

        private static int VertexForEdge(
            Dictionary<(int, int), int> lookup,
            List<Vector3> vertices,
            int first,
            int second
        )
        {
            (int, int) key = first > second ? (second, first) : (first, second);
            if (!lookup.TryGetValue(key, out int value))
            {
                value = vertices.Count;
                lookup.Add(key, value);
                Vector3 edge0 = vertices[first];
                Vector3 edge1 = vertices[second];
                Vector3 point = (edge0 + edge1).Normalized();
                vertices.Add(point);
            }

            return value;
        }

        #endregion ICOSPHERE MESH

        #region COMPUTE SHADER

        private void SetupComputeShader()
        {
            _rd = RenderingServer.CreateLocalRenderingDevice();

            RDShaderFile shaderFile = GD.Load<RDShaderFile>(_computeShader);
            RDShaderSpirV spirV = shaderFile.GetSpirV();
            _shader = _rd.ShaderCreateFromSpirV(spirV);

            _pipeline = _rd.ComputePipelineCreate(_shader);
            CreateUniforms();
        }

        private void CreateUniforms()
        {
            float[] _params = [_amplitude, _frequency, _lacunarity, _gain];

            _ = CreateStorageBufferUniform(_icosphereVertices, 0);
            _heightBuffer = CreateStorageBufferUniform(_heights, 1);
            _ = CreateStorageBufferUniform(_params, 2);
            _uniformSet = _rd.UniformSetCreate(_bindings, _shader, 0);
        }

        private Rid CreateStorageBufferUniform<T>(T[] array, int binding)
            where T : struct
        {
            byte[] bytes = ArrayToBytes(array);
            Rid buffer = _rd.StorageBufferCreate((uint)bytes.Length, bytes);

            RDUniform uniform =
                new()
                {
                    UniformType = RenderingDevice.UniformType.StorageBuffer,
                    Binding = binding
                };
            uniform.AddId(buffer);
            _bindings.Add(uniform);
            return buffer;
        }

        private void RunComputeShader()
        {
            long computeList = _rd.ComputeListBegin();
            _rd.ComputeListBindComputePipeline(computeList, _pipeline);
            _rd.ComputeListBindUniformSet(computeList, _uniformSet, 0);
            _rd.ComputeListDispatch(computeList, (uint)_icosphereVertices.Length, 1, 1);
            _rd.ComputeListEnd();
            _rd.Submit();
            _rd.Sync();
        }

        private void UpdateHeights()
        {
            byte[] heightBytes = _rd.BufferGetData(_heightBuffer);
            _heights = BytesToArray<float>(heightBytes);
        }

        private void UpdateVertices()
        {
            for (int i = 0; i < _heights.Length; i++)
            {
                _vertices[i] = _icosphereVertices[i] * _heights[i];
            }
        }

        private void CleanupGPU()
        {
            if (_rd is null)
            {
                return;
            }

            _rd.FreeRid(_heightBuffer);
            _rd.FreeRid(_uniformSet);
            _rd.FreeRid(_pipeline);
            _rd.FreeRid(_shader);
            _rd.Free();
            _rd = null;
        }

        #endregion COMPUTE SHADER

        private static byte[] ArrayToBytes<T>(T[] items)
            where T : struct
        {
            return MemoryMarshal.Cast<T, byte>(items).ToArray();
        }

        private static T[] BytesToArray<T>(byte[] bytes)
            where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(bytes).ToArray();
        }
    }
}
