using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WaterMesh : MonoBehaviour
    {
        public Vector2 size = new Vector2(60f, 230f);
        [Min(0.5f)] public float cellSize = 2f;

        [NonSerialized] public Func<Vector3, float> shoreFactor;

        Mesh mesh;

        void OnEnable()
        {
            Build();
        }

        void OnDisable()
        {
            if (mesh == null)
            {
                return;
            }
            if (Application.isPlaying)
            {
                Destroy(mesh);
            }
            else
            {
                DestroyImmediate(mesh);
            }
            mesh = null;
        }

        void Build()
        {
            int nx = Mathf.Max(2, Mathf.CeilToInt(size.x / cellSize) + 1);
            int nz = Mathf.Max(2, Mathf.CeilToInt(size.y / cellSize) + 1);
            int count = nx * nz;

            var vertices = new Vector3[count];
            var normals = new Vector3[count];
            var colors = new Color[count];
            var triangles = new int[(nx - 1) * (nz - 1) * 6];
            Vector3 origin = transform.position;

            for (int z = 0; z < nz; z++)
            {
                for (int x = 0; x < nx; x++)
                {
                    int i = z * nx + x;
                    float u = x / (float)(nx - 1);
                    float v = z / (float)(nz - 1);
                    vertices[i] = new Vector3((u - 0.5f) * size.x, 0f, (v - 0.5f) * size.y);
                    normals[i] = Vector3.up;
                    float shore = shoreFactor != null ? Mathf.Clamp01(shoreFactor(origin + vertices[i])) : 0f;
                    colors[i] = new Color(shore, 0.5f, 0.56f, Mathf.Lerp(1f, 0.12f, shore));
                }
            }

            int t = 0;
            for (int z = 0; z < nz - 1; z++)
            {
                for (int x = 0; x < nx - 1; x++)
                {
                    int i = z * nx + x;
                    triangles[t++] = i;
                    triangles[t++] = i + nx;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + nx;
                    triangles[t++] = i + nx + 1;
                }
            }

            if (mesh == null)
            {
                mesh = new Mesh { name = "WaterMesh", hideFlags = HideFlags.HideAndDontSave };
            }
            mesh.Clear();
            mesh.indexFormat = count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(size.x, 4f, size.y));
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
}
