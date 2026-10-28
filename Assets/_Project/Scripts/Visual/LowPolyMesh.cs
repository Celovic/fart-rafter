using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public static class LowPolyMesh
    {
        static Mesh rock;
        static Mesh blob;
        static Mesh trunk;
        static Mesh cone;
        static Mesh disc;

        public static Mesh Rock => rock != null ? rock : rock = Icosphere(0, "LowPolyRock");
        public static Mesh Blob => blob != null ? blob : blob = Icosphere(1, "LowPolyBlob");
        public static Mesh Trunk => trunk != null ? trunk : trunk = Prism(6, 1f, 1f, "LowPolyTrunk");
        public static Mesh Cone => cone != null ? cone : cone = Prism(7, 1f, 0f, "LowPolyCone");
        public static Mesh Disc => disc != null ? disc : disc = Prism(10, 1f, 1f, "LowPolyDisc");

        static Mesh Icosphere(int subdivisions, string name)
        {
            float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
            var points = new List<Vector3>
            {
                new Vector3(-1f, t, 0f), new Vector3(1f, t, 0f), new Vector3(-1f, -t, 0f), new Vector3(1f, -t, 0f),
                new Vector3(0f, -1f, t), new Vector3(0f, 1f, t), new Vector3(0f, -1f, -t), new Vector3(0f, 1f, -t),
                new Vector3(t, 0f, -1f), new Vector3(t, 0f, 1f), new Vector3(-t, 0f, -1f), new Vector3(-t, 0f, 1f)
            };
            var faces = new List<int>
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
                1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
                4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
            };
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i].normalized * 0.5f;
            }

            for (int s = 0; s < subdivisions; s++)
            {
                var cache = new Dictionary<long, int>();
                var next = new List<int>(faces.Count * 4);
                for (int f = 0; f < faces.Count; f += 3)
                {
                    int a = faces[f];
                    int b = faces[f + 1];
                    int c = faces[f + 2];
                    int ab = Midpoint(points, cache, a, b);
                    int bc = Midpoint(points, cache, b, c);
                    int ca = Midpoint(points, cache, c, a);
                    next.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }
                faces = next;
            }

            var vertices = new Vector3[faces.Count];
            var triangles = new int[faces.Count];
            for (int i = 0; i < faces.Count; i++)
            {
                vertices[i] = points[faces[i]];
                triangles[i] = i;
            }
            return Finish(name, vertices, triangles);
        }

        static int Midpoint(List<Vector3> points, Dictionary<long, int> cache, int a, int b)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            if (cache.TryGetValue(key, out int index))
            {
                return index;
            }
            points.Add(((points[a] + points[b]) * 0.5f).normalized * 0.5f);
            index = points.Count - 1;
            cache[key] = index;
            return index;
        }

        static Mesh Prism(int sides, float bottomRadius, float topRadius, string name)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides;
                float a1 = (i + 1) * Mathf.PI * 2f / sides;
                Vector3 d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * 0.5f;
                Vector3 d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * 0.5f;
                Vector3 b0 = d0 * bottomRadius;
                Vector3 b1 = d1 * bottomRadius;
                Vector3 t0 = d0 * topRadius + Vector3.up;
                Vector3 t1 = d1 * topRadius + Vector3.up;

                if (topRadius > 0f)
                {
                    AddTriangle(vertices, triangles, b0, t0, t1);
                    AddTriangle(vertices, triangles, b0, t1, b1);
                    AddTriangle(vertices, triangles, Vector3.up, t1, t0);
                }
                else
                {
                    AddTriangle(vertices, triangles, b0, Vector3.up, b1);
                }
                AddTriangle(vertices, triangles, Vector3.zero, b0, b1);
            }
            return Finish(name, vertices.ToArray(), triangles.ToArray());
        }

        static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            triangles.Add(vertices.Count);
            vertices.Add(a);
            triangles.Add(vertices.Count);
            vertices.Add(b);
            triangles.Add(vertices.Count);
            vertices.Add(c);
        }

        static Mesh Finish(string name, Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
