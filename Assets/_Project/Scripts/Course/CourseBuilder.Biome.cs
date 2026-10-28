using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FartRafters
{
    public static partial class CourseBuilder
    {
        const float SeaTile = 64f;
        const int SeaCellsNear = 20;
        const int SeaCellsFar = 8;
        const float SeaReach = 210f;
        const float SeaStripClearance = 7.5f;

        static void BuildIce(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.ice == null || level.ice.Length == 0)
            {
                return;
            }
            var root = new GameObject("Ice").transform;
            root.SetParent(parent, false);
            var random = new System.Random(CourseGeometry.StableHash(level.id) + 91);
            foreach (IceSpec spec in level.ice)
            {
                var go = new GameObject("IceSheet", typeof(MeshFilter), typeof(MeshRenderer));
                go.SetActive(false);
                go.transform.SetParent(root, false);
                go.transform.SetPositionAndRotation(Lift(new Vector3(spec.position.x, 0f, spec.position.z)), Quaternion.Euler(0f, spec.yaw, 0f));
                go.AddComponent<IceZone>().halfSize = spec.size * 0.5f;
                go.GetComponent<MeshFilter>().sharedMesh = IceMesh(spec.size, random.Next(1000));
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = theme.iceMaterial != null ? theme.iceMaterial : theme.sculptMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                go.SetActive(true);
            }
        }

        static Mesh IceMesh(Vector2 size, int seed)
        {
            const float Cell = 1.2f;
            float hx = size.x * 0.5f;
            float hz = size.y * 0.5f;
            int nx = Mathf.Max(2, Mathf.CeilToInt(size.x / Cell) + 1);
            int nz = Mathf.Max(2, Mathf.CeilToInt(size.y / Cell) + 1);
            var vertices = new Vector3[nx * nz];
            var colors = new Color[vertices.Length];
            var ice = new Color(0.74f, 0.86f, 0.94f);
            var snow = new Color(0.95f, 0.97f, 1f);
            var crack = new Color(0.5f, 0.68f, 0.82f);
            for (int j = 0; j < nz; j++)
            {
                for (int i = 0; i < nx; i++)
                {
                    float x = Mathf.Lerp(-hx, hx, i / (float)(nx - 1));
                    float z = Mathf.Lerp(-hz, hz, j / (float)(nz - 1));
                    float jag = Mathf.PerlinNoise(x * 0.35f + seed, z > 0f ? 3.1f : 7.7f) * 1.6f;
                    float end = hz - jag;
                    if (Mathf.Abs(z) > end)
                    {
                        z = Mathf.Sign(z) * end;
                    }
                    float ramp = Mathf.Clamp01((end - Mathf.Abs(z)) / 1.4f + (j == 0 || j == nz - 1 ? -1f : 0f));
                    float y = Mathf.Lerp(-0.14f, IceZone.Top, Mathf.SmoothStep(0f, 1f, ramp));
                    y += (Mathf.PerlinNoise(x * 0.8f + seed * 0.3f, z * 0.8f) - 0.5f) * 0.025f * ramp;
                    vertices[j * nx + i] = new Vector3(x, y, z);
                    float patch = Mathf.PerlinNoise(x * 0.22f + seed * 1.7f, z * 0.22f + 4.2f);
                    float line = Mathf.Abs(Mathf.PerlinNoise(x * 0.5f + seed, z * 0.5f + 9.3f) - 0.5f);
                    Color color = Color.Lerp(ice, snow, Mathf.SmoothStep(0.55f, 0.75f, patch));
                    color = Color.Lerp(crack, color, Mathf.SmoothStep(0.015f, 0.05f, line));
                    colors[j * nx + i] = color;
                }
            }
            var triangles = new int[(nx - 1) * (nz - 1) * 6];
            int t = 0;
            for (int j = 0; j < nz - 1; j++)
            {
                for (int i = 0; i < nx - 1; i++)
                {
                    int a = j * nx + i;
                    triangles[t++] = a;
                    triangles[t++] = a + nx;
                    triangles[t++] = a + nx + 1;
                    triangles[t++] = a;
                    triangles[t++] = a + nx + 1;
                    triangles[t++] = a + 1;
                }
            }
            var mesh = new Mesh { name = "IceSheet" };
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static void BuildWhirlpools(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (level.whirlpools == null || level.whirlpools.Length == 0)
            {
                return;
            }
            var root = new GameObject("Whirlpools").transform;
            root.SetParent(parent, false);
            foreach (WhirlpoolSpec spec in level.whirlpools)
            {
                var go = new GameObject("Whirlpool");
                go.SetActive(false);
                go.transform.SetParent(root, false);
                go.transform.position = Lift(new Vector3(spec.position.x, 0f, spec.position.z));
                var pool = go.AddComponent<Whirlpool>();
                pool.radius = spec.radius;
                pool.coreRadius = Mathf.Clamp(spec.radius * 0.25f, 1.4f, 2.2f);
                pool.swirl = spec.swirl;
                pool.pull = spec.swirl * 0.55f;
                pool.direction = spec.direction;
                pool.coreDamage = 14f;
                pool.spinSpeed = 40f + spec.swirl * 15f;

                var vortex = new GameObject("Vortex", typeof(MeshFilter), typeof(MeshRenderer));
                vortex.transform.SetParent(go.transform, false);
                vortex.GetComponent<MeshFilter>().sharedMesh = VortexMesh(spec.radius);
                var renderer = vortex.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = theme.whirlpoolMaterial != null ? theme.whirlpoolMaterial : theme.water;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                pool.visual = vortex.transform;
                CreateBubbles(go.transform, new Vector3(0f, 0.05f, 0f), new Vector3(spec.radius * 1.2f, 0.1f, spec.radius * 1.2f), Vector3.zero, theme.bubbles);
                go.SetActive(true);
            }
        }

        static Mesh VortexMesh(float radius)
        {
            const int Rings = 9;
            const int Segments = 40;
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            vertices.Add(new Vector3(0f, -1.35f, 0f));
            colors.Add(new Color(1f, 1f, 1f, 1f));
            uvs.Add(new Vector2(0.5f, 0.5f));
            for (int k = 1; k <= Rings; k++)
            {
                float f = Mathf.Pow(k / (float)Rings, 1.3f);
                float r = radius * f;
                float y = -1.35f * (1f - f) * (1f - f) + 0.04f;
                float alpha = k == Rings ? 0f : Mathf.Clamp01((1f - f) * 1.6f);
                for (int s = 0; s < Segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / Segments;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                    colors.Add(new Color(1f, 1f, 1f, alpha));
                    uvs.Add(new Vector2(Mathf.Cos(angle) * f * 0.5f + 0.5f, Mathf.Sin(angle) * f * 0.5f + 0.5f));
                }
            }
            for (int s = 0; s < Segments; s++)
            {
                int a = 1 + s;
                int b = 1 + (s + 1) % Segments;
                triangles.Add(0);
                triangles.Add(b);
                triangles.Add(a);
            }
            for (int k = 1; k < Rings; k++)
            {
                int inner = 1 + (k - 1) * Segments;
                int outer = 1 + k * Segments;
                for (int s = 0; s < Segments; s++)
                {
                    int n = (s + 1) % Segments;
                    triangles.Add(inner + s);
                    triangles.Add(inner + n);
                    triangles.Add(outer + s);
                    triangles.Add(inner + n);
                    triangles.Add(outer + n);
                    triangles.Add(outer + s);
                }
            }
            var mesh = new Mesh { name = "Vortex" };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static void BuildOpenSea(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            var root = new GameObject("OpenSea").transform;
            root.SetParent(parent, false);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < level.NodeCount; i++)
            {
                min = Vector2.Min(min, level.nodes[i].position);
                max = Vector2.Max(max, level.nodes[i].position);
            }
            min -= Vector2.one * SeaReach;
            max += Vector2.one * SeaReach;
            int tilesX = Mathf.CeilToInt((max.x - min.x) / SeaTile);
            int tilesZ = Mathf.CeilToInt((max.y - min.y) / SeaTile);
            var nearby = new List<int>();
            float tileRadius = SeaTile * 0.75f;
            for (int tz = 0; tz < tilesZ; tz++)
            {
                for (int tx = 0; tx < tilesX; tx++)
                {
                    var origin = new Vector2(min.x + tx * SeaTile, min.y + tz * SeaTile);
                    Vector2 center = origin + Vector2.one * (SeaTile * 0.5f);
                    nearby.Clear();
                    float closest = float.MaxValue;
                    for (int i = 0; i < level.NodeCount; i++)
                    {
                        float d = (level.nodes[i].position - center).magnitude;
                        closest = Mathf.Min(closest, d);
                        if (d < tileRadius + theme.landWidth + 40f)
                        {
                            nearby.Add(i);
                        }
                    }
                    if (closest > SeaReach + tileRadius)
                    {
                        continue;
                    }
                    Mesh mesh = SeaTileMesh(level, theme, origin, nearby, closest < tileRadius + 50f ? SeaCellsNear : SeaCellsFar);
                    if (mesh == null)
                    {
                        continue;
                    }
                    var go = new GameObject("SeaTile", typeof(MeshFilter), typeof(MeshRenderer));
                    go.transform.SetParent(root, false);
                    go.GetComponent<MeshFilter>().sharedMesh = mesh;
                    var renderer = go.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = theme.water;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
        }

        static Mesh SeaTileMesh(LevelDefinition level, CourseTheme theme, Vector2 origin, List<int> nearby, int cells)
        {
            int n = cells + 1;
            float cell = SeaTile / cells;
            var points = new Vector3[n * n];
            var needed = new bool[n * n];
            bool any = false;
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    var point = new Vector3(origin.x + i * cell, 0f, origin.y + j * cell);
                    points[j * n + i] = point;
                    needed[j * n + i] = NeedsSea(level, theme, point, nearby);
                    any |= needed[j * n + i];
                }
            }
            if (!any)
            {
                return null;
            }
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var remap = new int[points.Length];
            for (int k = 0; k < remap.Length; k++)
            {
                remap[k] = -1;
            }
            for (int j = 0; j < n - 1; j++)
            {
                for (int i = 0; i < n - 1; i++)
                {
                    int a = j * n + i;
                    int b = a + n;
                    int c = a + n + 1;
                    int d = a + 1;
                    if (!(needed[a] || needed[b] || needed[c] || needed[d]))
                    {
                        continue;
                    }
                    foreach (int index in new[] { a, b, c, a, c, d })
                    {
                        if (remap[index] < 0)
                        {
                            remap[index] = vertices.Count;
                            vertices.Add(points[index]);
                        }
                        triangles.Add(remap[index]);
                    }
                }
            }
            var colors = new Color[vertices.Count];
            var normals = new Vector3[vertices.Count];
            for (int k = 0; k < colors.Length; k++)
            {
                colors[k] = new Color(0f, 0.5f, 0.5f, 1f);
                normals[k] = Vector3.up;
            }
            var mesh = new Mesh { name = "SeaTile" };
            mesh.SetVertices(vertices);
            mesh.normals = normals;
            mesh.colors = colors;
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            Bounds bounds = mesh.bounds;
            bounds.Expand(new Vector3(0f, 4f, 0f));
            mesh.bounds = bounds;
            return mesh;
        }

        static bool NeedsSea(LevelDefinition level, CourseTheme theme, Vector3 point, List<int> nearby)
        {
            if (nearby.Count == 0)
            {
                return true;
            }
            int best = nearby[0];
            float bestDistance = float.MaxValue;
            foreach (int i in nearby)
            {
                float d = (level.Center(i) - point).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }
            int other = best == level.NodeCount - 1 ? best - 1 : best == 0 ? 1 : (level.Center(best - 1) - point).sqrMagnitude < (level.Center(best + 1) - point).sqrMagnitude ? best - 1 : best + 1;
            int from = Mathf.Min(best, other);
            Vector3 a = level.Center(from);
            Vector3 b = level.Center(from + 1);
            Vector3 segment = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(point - a, segment) / Mathf.Max(0.0001f, segment.sqrMagnitude));
            Vector3 closest = a + segment * t;
            float half = Mathf.Lerp(level.Width(from), level.Width(from + 1), t) * 0.5f;
            Vector3 offset = point - closest;
            offset.y = 0f;
            float beyond = offset.magnitude - half;
            if (beyond < SeaStripClearance)
            {
                return false;
            }
            Vector3 right = Vector3.Cross(Vector3.up, segment).normalized;
            bool landSide = Vector3.Dot(offset, right) * theme.openSide < 0f;
            return !(landSide && beyond < theme.landWidth + 0.5f);
        }
    }
}
