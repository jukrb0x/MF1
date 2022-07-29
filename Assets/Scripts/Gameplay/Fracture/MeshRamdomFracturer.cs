// script originated from
// https://gist.github.com/ditzel/73f4d1c9028cc3477bb921974f84ed56

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeshRamdomFracturer : MonoBehaviour
{
    private bool _edgeSet = false;
    private Vector3 _edgeVertex = Vector3.zero;
    private Vector2 _edgeUV = Vector2.zero;
    private Plane _edgePlane = new Plane();

    // the number of fragments is multiplied by the cut level
    public int cutLevel = 1; // don't make it too large
    public float explodeForce = 0;
    public bool isExplode;
    public float explosionRadius = 1;

    // --- test code ---
    [Header("Debug")]
    public bool triggerDestroy = false;

    private void OnValidate()
    {
        if (triggerDestroy)
        {
            FractureMesh();
        }
    }
    // --- test code end ---

    private void FractureMesh()
    {
        var originalMesh = GetComponent<MeshFilter>().mesh;
        originalMesh.RecalculateBounds();
        
        var parts = new List<PartMesh>();
        var subParts = new List<PartMesh>();

        var mainPart = new PartMesh()
        {
            UV = originalMesh.uv,
            Vertices = originalMesh.vertices,
            Normals = originalMesh.normals,
            Triangles = new int[originalMesh.subMeshCount][],
            Bounds = originalMesh.bounds
        };
        for (int i = 0; i < originalMesh.subMeshCount; i++)
            mainPart.Triangles[i] = originalMesh.GetTriangles(i);

        parts.Add(mainPart); // original mesh

        // start cutting (slicing)
        for (var c = 0; c < cutLevel; c++)
        {
            foreach (var part in parts)
            {
                var bounds = part.Bounds;
                bounds.Expand(0.5f);

                // randomly find a plane to slice within the bounds
                var plane = new Plane(UnityEngine.Random.onUnitSphere, new Vector3(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                    UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));


                // slice and load the upside, downside of the cutting plane
                subParts.Add(GenerateMesh(part, plane, true));
                subParts.Add(GenerateMesh(part, plane, false));
            }

            parts = new List<PartMesh>(subParts); // parts = all subParts
            subParts.Clear();
        }

        foreach (var part in parts) // all sliced sub parts
        {
            part.MakeGameobject(this); // make the game objects in the scene, if done in the editor mode, generated objects will be kept
            
            // add force to the fractured objects
            if (isExplode)
                part.PartMeshObj.GetComponent<Rigidbody>().AddExplosionForce(
                    explodeForce, transform.position, explosionRadius, 0, ForceMode.Impulse);
            else
                part.PartMeshObj.GetComponent<Rigidbody>()
                    .AddForceAtPosition(part.Bounds.center * explodeForce, transform.position);
        }

        // destroy the original mesh
        Destroy(gameObject);
    }

    private PartMesh GenerateMesh(PartMesh original, Plane plane, bool left)
    {
        var partMesh = new PartMesh() { };
        var ray1 = new Ray();
        var ray2 = new Ray();


        for (var i = 0; i < original.Triangles.Length; i++)
        {
            var triangles = original.Triangles[i];
            _edgeSet = false;

            for (var j = 0; j < triangles.Length; j = j + 3)
            {
                var sideA = plane.GetSide(original.Vertices[triangles[j]]) == left;
                var sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                var sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;

                var sideCount = (sideA ? 1 : 0) +
                                (sideB ? 1 : 0) +
                                (sideC ? 1 : 0);
                if (sideCount == 0)
                {
                    continue;
                }

                if (sideCount == 3)
                {
                    partMesh.AddTriangle(i,
                        original.Vertices[triangles[j]], original.Vertices[triangles[j + 1]],
                        original.Vertices[triangles[j + 2]],
                        original.Normals[triangles[j]], original.Normals[triangles[j + 1]],
                        original.Normals[triangles[j + 2]],
                        original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                    continue;
                }

                // cut points
                var singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                ray1.origin = original.Vertices[triangles[j + singleIndex]];
                var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] -
                           original.Vertices[triangles[j + singleIndex]];
                ray1.direction = dir1;
                plane.Raycast(ray1, out var enter1);
                var lerp1 = enter1 / dir1.magnitude;

                ray2.origin = original.Vertices[triangles[j + singleIndex]];
                var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] -
                           original.Vertices[triangles[j + singleIndex]];
                ray2.direction = dir2;
                plane.Raycast(ray2, out var enter2);
                var lerp2 = enter2 / dir2.magnitude;

                // first vertex = ancor
                AddEdge(i,
                    partMesh,
                    left ? plane.normal * -1f : plane.normal,
                    ray1.origin + ray1.direction.normalized * enter1,
                    ray2.origin + ray2.direction.normalized * enter2,
                    Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                        original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                    Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                        original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                if (sideCount == 1)
                {
                    partMesh.AddTriangle(i,
                        original.Vertices[triangles[j + singleIndex]],
                        //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        original.Normals[triangles[j + singleIndex]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                            original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                            original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                        original.UV[triangles[j + singleIndex]],
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                    continue;
                }

                if (sideCount == 2)
                {
                    partMesh.AddTriangle(i,
                        ray1.origin + ray1.direction.normalized * enter1,
                        original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                            original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                        original.UV[triangles[j + ((singleIndex + 2) % 3)]]);
                    partMesh.AddTriangle(i,
                        ray1.origin + ray1.direction.normalized * enter1,
                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                            original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                            original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    continue;
                }
            }
        }

        partMesh.FillArrays();

        return partMesh;
    }

    private void AddEdge(int subMesh, PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1,
        Vector2 uv2)
    {
        if (!_edgeSet)
        {
            _edgeSet = true;
            _edgeVertex = vertex1;
            _edgeUV = uv1;
        }
        else
        {
            _edgePlane.Set3Points(_edgeVertex, vertex1, vertex2);

            partMesh.AddTriangle(subMesh,
                _edgeVertex,
                _edgePlane.GetSide(_edgeVertex + normal) ? vertex1 : vertex2,
                _edgePlane.GetSide(_edgeVertex + normal) ? vertex2 : vertex1,
                normal,
                normal,
                normal,
                _edgeUV,
                uv1,
                uv2);
        }
    }

    public class PartMesh
    {
        private List<Vector3> _Verticies = new List<Vector3>();
        private List<Vector3> _Normals = new List<Vector3>();
        private List<List<int>> _Triangles = new List<List<int>>();
        private List<Vector2> _UVs = new List<Vector2>();
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public int[][] Triangles;
        public Vector2[] UV;
        public GameObject PartMeshObj; // the GameObject to be initialized
        public Bounds Bounds = new Bounds();

        public PartMesh()
        {
        }

        public void AddTriangle(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 normal1,
            Vector3 normal2, Vector3 normal3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            if (_Triangles.Count - 1 < submesh)
                _Triangles.Add(new List<int>());

            _Triangles[submesh].Add(_Verticies.Count);
            _Verticies.Add(vert1);
            _Triangles[submesh].Add(_Verticies.Count);
            _Verticies.Add(vert2);
            _Triangles[submesh].Add(_Verticies.Count);
            _Verticies.Add(vert3);
            _Normals.Add(normal1);
            _Normals.Add(normal2);
            _Normals.Add(normal3);
            _UVs.Add(uv1);
            _UVs.Add(uv2);
            _UVs.Add(uv3);

            Bounds.min = Vector3.Min(Bounds.min, vert1);
            Bounds.min = Vector3.Min(Bounds.min, vert2);
            Bounds.min = Vector3.Min(Bounds.min, vert3);
            Bounds.max = Vector3.Min(Bounds.max, vert1);
            Bounds.max = Vector3.Min(Bounds.max, vert2);
            Bounds.max = Vector3.Min(Bounds.max, vert3);
        }

        public void FillArrays()
        {
            Vertices = _Verticies.ToArray();
            Normals = _Normals.ToArray();
            UV = _UVs.ToArray();
            Triangles = new int[_Triangles.Count][];
            for (var i = 0; i < _Triangles.Count; i++)
                Triangles[i] = _Triangles[i].ToArray();
        }

        public void MakeGameobject(MeshRamdomFracturer original)
        {
            PartMeshObj = new GameObject(original.name);
            PartMeshObj.transform.position = original.transform.position;
            PartMeshObj.transform.rotation = original.transform.rotation;
            PartMeshObj.transform.localScale = original.transform.localScale;

            var mesh = new Mesh();
            mesh.name = original.GetComponent<MeshFilter>().mesh.name;

            mesh.vertices = Vertices;
            mesh.normals = Normals;
            mesh.uv = UV;
            for (var i = 0; i < Triangles.Length; i++)
                mesh.SetTriangles(Triangles[i], i, true);
            Bounds = mesh.bounds;

            var renderer = PartMeshObj.AddComponent<MeshRenderer>();
            renderer.materials = original.GetComponent<MeshRenderer>().materials;

            var filter = PartMeshObj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            var collider = PartMeshObj.AddComponent<MeshCollider>();
            collider.convex = true;

            var rigidbody = PartMeshObj.AddComponent<Rigidbody>();
            var meshDestroy = PartMeshObj.AddComponent<MeshRamdomFracturer>(); // sub-meshes can be destroyed by this component
            meshDestroy.cutLevel = original.cutLevel;
            meshDestroy.explodeForce = original.explodeForce;
        }
    }
}