// script originated from
// https://gist.github.com/ditzel/73f4d1c9028cc3477bb921974f84ed56

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Fracture.Quick
{
    public class MeshFracturer : MonoBehaviour
    {
        private bool _edgeSet = false;
        private Vector3 _edgeVertex = Vector3.zero;
        private Vector2 _edgeUV = Vector2.zero;
        private Plane _edgePlane = new Plane();

        // the number of fragments is multiplied by the cut level
        public int cutLevel = 1; // don't make it too large
        public float explodeForce = 0;
        [Header("Explosion")] public bool isExplode;
        public float explosionRadius = 1;

        // --- test code ---
#if DEBUG
        [Header("Debug")] public bool triggerDestroy = false;

        private void OnValidate()
        {
            if (triggerDestroy)
            {
                FractureMesh();
            }
        }
#endif
        // --- test code end ---

        private void FractureMesh()
        {
            var originalMesh = GetComponent<MeshFilter>().mesh;
            originalMesh.RecalculateBounds();

            var parts = new List<FragmentMeshData>();
            var subParts = new List<FragmentMeshData>();

            var mainPart = new FragmentMeshData()
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

                    var plane = new Plane(UnityEngine.Random.onUnitSphere, // random unit vector as the normal
                        // random point within the mesh bounds
                        new Vector3(
                            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));

                    // slice the original mesh by the plane, get the upside and downside
                    subParts.Add(GenerateFragMesh(part, plane, true)); // subParts[0]
                    subParts.Add(GenerateFragMesh(part, plane, false)); // subParts[1]
                }

                parts = new List<FragmentMeshData>(subParts); // parts = all subParts
                subParts.Clear();
            }

            foreach (var part in parts) // all sliced sub parts
            {
                part.MakeFragMeshObj(
                    this); // make the game objects in the scene, if done in the editor mode, generated objects will be kept

                // add force to the fractured objects
                if (isExplode)
                    part.FragmentMeshObj.GetComponent<Rigidbody>().AddExplosionForce(
                        explodeForce, transform.position, explosionRadius, 0, ForceMode.Impulse);
                else
                    part.FragmentMeshObj.GetComponent<Rigidbody>()
                        .AddForceAtPosition(part.Bounds.center * explodeForce, transform.position);
            }

            // destroy the original mesh
            Destroy(gameObject);
        }

        // slice the original mesh by the plane
        private FragmentMeshData GenerateFragMesh(FragmentMeshData original, Plane plane, bool left)
        {
            var fragmesh = new FragmentMeshData() { };
            var ray1 = new Ray();
            var ray2 = new Ray();


            for (var i = 0; i < original.Triangles.Length; i++)
            {
                var triangles = original.Triangles[i];
                _edgeSet = false;

                // triangulation
                for (var j = 0; j < triangles.Length; j += 3)
                {
                    var sideA = plane.GetSide(original.Vertices[triangles[j]]) == left;
                    var sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                    var sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;

                    var sideCount = (sideA ? 1 : 0) +
                                    (sideB ? 1 : 0) +
                                    (sideC ? 1 : 0);

                    if (sideCount == 0) // all right
                    {
                        continue;
                    }

                    if (sideCount == 3)
                    {
                        fragmesh.AddTriangle(i,
                            original.Vertices[triangles[j]], original.Vertices[triangles[j + 1]],
                            original.Vertices[triangles[j + 2]],
                            original.Normals[triangles[j]], original.Normals[triangles[j + 1]],
                            original.Normals[triangles[j + 2]],
                            original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                        continue;
                    }

                    // cut points
                    var singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                    // Ray 1
                    ray1.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] - ray1.origin;
                    ray1.direction = dir1;
                    plane.Raycast(ray1, out var enter1);
                    var lerp1 = enter1 / dir1.magnitude;

                    // Ray 2
                    ray2.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] - ray2.origin;
                    ray2.direction = dir2;
                    plane.Raycast(ray2, out var enter2);
                    var lerp2 = enter2 / dir2.magnitude;
                    
                    
                    // Ray 1 and Ray 2 share the same origin

                    // first vertex = anchor
                    AddEdge(i,
                        fragmesh,
                        left ? plane.normal * -1f : plane.normal,
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                    if (sideCount == 1)
                    {
                        fragmesh.AddTriangle(i,
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
                        fragmesh.AddTriangle(i,
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
                        fragmesh.AddTriangle(i,
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

            fragmesh.FillArrays();

            return fragmesh;
        }

        private void AddEdge(int subMesh, FragmentMeshData fragmentMeshData, Vector3 normal, Vector3 vertex1,
            Vector3 vertex2,
            Vector2 uv1,
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

                fragmentMeshData.AddTriangle(subMesh,
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

        private void OnCollisionEnter(Collision collision)
        {
            // todo
            throw new NotImplementedException();
        }
    }
}