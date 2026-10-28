using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters.EditorTools.Sculpt
{
    public static class SurfaceNets
    {
        static readonly int[,] Edges =
        {
            { 0, 1 }, { 2, 3 }, { 4, 5 }, { 6, 7 },
            { 0, 2 }, { 1, 3 }, { 4, 6 }, { 5, 7 },
            { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }
        };

        public static Mesh Build(Sculpt sculpt, float cell, string name)
        {
            sculpt.Prepare();
            Bounds bounds = sculpt.Bounds;
            bounds.Expand(cell * 4f);
            Vector3 origin = bounds.min;
            int nx = Mathf.CeilToInt(bounds.size.x / cell) + 1;
            int ny = Mathf.CeilToInt(bounds.size.y / cell) + 1;
            int nz = Mathf.CeilToInt(bounds.size.z / cell) + 1;

            var field = new float[nx * ny * nz];
            for (int z = 0; z < nz; z++)
            {
                for (int y = 0; y < ny; y++)
                {
                    for (int x = 0; x < nx; x++)
                    {
                        field[x + nx * (y + ny * z)] = sculpt.Distance(origin + new Vector3(x, y, z) * cell);
                    }
                }
            }

            int cx = nx - 1;
            int cy = ny - 1;
            int cz = nz - 1;
            var cellVertex = new int[cx * cy * cz];
            var vertices = new List<Vector3>();
            var corner = new float[8];
            var cornerPos = new Vector3[8];

            for (int z = 0; z < cz; z++)
            {
                for (int y = 0; y < cy; y++)
                {
                    for (int x = 0; x < cx; x++)
                    {
                        int index = x + cx * (y + cy * z);
                        cellVertex[index] = -1;
                        int mask = 0;
                        for (int c = 0; c < 8; c++)
                        {
                            int ox = c & 1;
                            int oy = (c >> 1) & 1;
                            int oz = (c >> 2) & 1;
                            corner[c] = field[(x + ox) + nx * ((y + oy) + ny * (z + oz))];
                            cornerPos[c] = new Vector3(x + ox, y + oy, z + oz);
                            if (corner[c] < 0f)
                            {
                                mask |= 1 << c;
                            }
                        }
                        if (mask == 0 || mask == 255)
                        {
                            continue;
                        }

                        Vector3 sum = Vector3.zero;
                        int crossings = 0;
                        for (int e = 0; e < 12; e++)
                        {
                            float a = corner[Edges[e, 0]];
                            float b = corner[Edges[e, 1]];
                            if ((a < 0f) == (b < 0f))
                            {
                                continue;
                            }
                            float t = a / (a - b);
                            sum += Vector3.Lerp(cornerPos[Edges[e, 0]], cornerPos[Edges[e, 1]], t);
                            crossings++;
                        }
                        cellVertex[index] = vertices.Count;
                        vertices.Add(origin + sum / crossings * cell);
                    }
                }
            }

            var triangles = new List<int>();
            for (int z = 1; z < nz - 1; z++)
            {
                for (int y = 1; y < ny - 1; y++)
                {
                    for (int x = 1; x < nx - 1; x++)
                    {
                        float a = field[x + nx * (y + ny * z)];
                        TryQuad(a, field[(x + 1) + nx * (y + ny * z)],
                            Cell(cellVertex, cx, cy, x, y - 1, z - 1), Cell(cellVertex, cx, cy, x, y, z - 1),
                            Cell(cellVertex, cx, cy, x, y, z), Cell(cellVertex, cx, cy, x, y - 1, z),
                            Vector3.right, vertices, triangles);
                        TryQuad(a, field[x + nx * ((y + 1) + ny * z)],
                            Cell(cellVertex, cx, cy, x - 1, y, z - 1), Cell(cellVertex, cx, cy, x, y, z - 1),
                            Cell(cellVertex, cx, cy, x, y, z), Cell(cellVertex, cx, cy, x - 1, y, z),
                            Vector3.up, vertices, triangles);
                        TryQuad(a, field[x + nx * (y + ny * (z + 1))],
                            Cell(cellVertex, cx, cy, x - 1, y - 1, z), Cell(cellVertex, cx, cy, x, y - 1, z),
                            Cell(cellVertex, cx, cy, x, y, z), Cell(cellVertex, cx, cy, x - 1, y, z),
                            Vector3.forward, vertices, triangles);
                    }
                }
            }

            float eps = cell * 0.5f;
            var normals = new Vector3[vertices.Count];
            var colors = new Color[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 p = vertices[i];
                float d = Mathf.Clamp(sculpt.Distance(p), -cell * 0.3f, cell * 0.3f);
                p -= Gradient(sculpt, p, eps) * d;
                vertices[i] = p;
                normals[i] = Gradient(sculpt, p, eps);
                colors[i] = sculpt.Color(p).linear;
            }

            var mesh = new Mesh { name = name };
            mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        static int Cell(int[] cells, int cx, int cy, int x, int y, int z)
        {
            return cells[x + cx * (y + cy * z)];
        }

        static void TryQuad(float a, float b, int v0, int v1, int v2, int v3, Vector3 axis, List<Vector3> vertices, List<int> triangles)
        {
            if ((a < 0f) == (b < 0f) || v0 < 0 || v1 < 0 || v2 < 0 || v3 < 0)
            {
                return;
            }
            Vector3 outward = a < 0f ? axis : -axis;
            Vector3 p0 = vertices[v0];
            Vector3 p1 = vertices[v1];
            Vector3 p2 = vertices[v2];
            Vector3 p3 = vertices[v3];
            if (Vector3.Dot(Vector3.Cross(p2 - p0, p3 - p1), outward) < 0f)
            {
                int swap = v1;
                v1 = v3;
                v3 = swap;
                Vector3 swapPoint = p1;
                p1 = p3;
                p3 = swapPoint;
            }
            if ((p0 - p2).sqrMagnitude <= (p1 - p3).sqrMagnitude)
            {
                triangles.Add(v0);
                triangles.Add(v1);
                triangles.Add(v2);
                triangles.Add(v0);
                triangles.Add(v2);
                triangles.Add(v3);
            }
            else
            {
                triangles.Add(v0);
                triangles.Add(v1);
                triangles.Add(v3);
                triangles.Add(v1);
                triangles.Add(v2);
                triangles.Add(v3);
            }
        }

        static Vector3 Gradient(Sculpt sculpt, Vector3 p, float eps)
        {
            float dx = sculpt.Distance(p + new Vector3(eps, 0f, 0f)) - sculpt.Distance(p - new Vector3(eps, 0f, 0f));
            float dy = sculpt.Distance(p + new Vector3(0f, eps, 0f)) - sculpt.Distance(p - new Vector3(0f, eps, 0f));
            float dz = sculpt.Distance(p + new Vector3(0f, 0f, eps)) - sculpt.Distance(p - new Vector3(0f, 0f, eps));
            var g = new Vector3(dx, dy, dz);
            return g.sqrMagnitude > 1e-12f ? g.normalized : Vector3.up;
        }
    }
}
