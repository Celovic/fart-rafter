using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

namespace FartRafters
{
    public static partial class LandscapeBuilder
    {
        const float TroughBottom = -1.6f;
        const float CloudDepth = 38f;

        static readonly Vector2[] TroughProfile =
        {
            new Vector2(-0.3f, -0.9f),
            new Vector2(-0.06f, 0.3f),
            new Vector2(0f, 0.95f),
            new Vector2(0.18f, 1.14f),
            new Vector2(0.42f, 0.96f),
            new Vector2(0.5f, -0.6f),
            new Vector2(0.3f, TroughBottom)
        };

        static readonly Color SlideInner = new Color(0.88f, 0.34f, 0.1f);
        static readonly Color SlideRim = new Color(0.18f, 0.56f, 0.1f);
        static readonly Color SlideOuter = new Color(0.62f, 0.22f, 0.12f);

        static void BuildElevated(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                var points = new List<Vector3>();
                var outward = new List<Vector3>();
                for (int i = 0; i < level.NodeCount; i++)
                {
                    points.Add(level.Edge(i, s) + Vector3.up * level.Height(i));
                    outward.Add(Vector3.Cross(Vector3.up, level.Direction(i)).normalized * s);
                }
                BuildTrough(Subdivide(points, outward, 1.2f, out List<Vector3> normals), normals, theme, parent);
            }
            BuildTroughCaps(level, theme, parent);
            BuildUnderside(level, theme, parent);
            BuildClouds(level, theme, parent);
            BuildBackdrop(level, theme, parent);
        }

        static void BuildTrough(List<Vector3> line, List<Vector3> outward, CourseTheme theme, Transform parent)
        {
            int rows = TroughProfile.Length;
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            for (int i = 0; i < line.Count; i++)
            {
                for (int r = 0; r < rows; r++)
                {
                    Vector2 shape = TroughProfile[r];
                    vertices.Add(line[i] + outward[i] * shape.x + Vector3.up * shape.y);
                    colors.Add(r <= 2 ? SlideInner : r <= 4 ? SlideRim : SlideOuter);
                }
            }
            bool flip = false;
            if (line.Count > 1)
            {
                Vector3 normal = Vector3.Cross(vertices[rows] - vertices[0], vertices[1] - vertices[0]);
                flip = Vector3.Dot(normal, -outward[0]) < 0f;
            }
            for (int i = 0; i < line.Count - 1; i++)
            {
                for (int r = 0; r < rows - 1; r++)
                {
                    int a = i * rows + r;
                    int b = a + rows;
                    if (flip)
                    {
                        triangles.Add(a); triangles.Add(a + 1); triangles.Add(b);
                        triangles.Add(b); triangles.Add(a + 1); triangles.Add(b + 1);
                    }
                    else
                    {
                        triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                        triangles.Add(b); triangles.Add(b + 1); triangles.Add(a + 1);
                    }
                }
            }
            CreateSlideMesh("Slide", vertices, colors, triangles, theme, parent);
        }

        static void BuildTroughCaps(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            for (int c = 0; c < 2; c++)
            {
                int node = c == 0 ? 0 : level.NodeCount - 1;
                float sign = c == 0 ? -1f : 1f;
                Vector3 direction = level.Direction(node) * sign;
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
                Vector3 center = level.Surface(node) + direction * 0.8f;
                float half = level.Width(node) * 0.5f + 0.5f;
                AddBox(vertices, colors, triangles, center + Vector3.up * ((1.14f + TroughBottom) * 0.5f), right * half, Vector3.up * ((1.14f - TroughBottom) * 0.5f), direction * 0.5f, SlideOuter);
            }
            CreateSlideMesh("SlideCaps", vertices, colors, triangles, theme, parent);
        }

        static void BuildUnderside(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            for (int i = 0; i < level.NodeCount - 1; i++)
            {
                Vector3 a0 = level.Edge(i, -1f) + Vector3.up * (level.Height(i) + TroughBottom);
                Vector3 a1 = level.Edge(i, 1f) + Vector3.up * (level.Height(i) + TroughBottom);
                Vector3 b0 = level.Edge(i + 1, -1f) + Vector3.up * (level.Height(i + 1) + TroughBottom);
                Vector3 b1 = level.Edge(i + 1, 1f) + Vector3.up * (level.Height(i + 1) + TroughBottom);
                int start = vertices.Count;
                vertices.Add(a0); vertices.Add(a1); vertices.Add(b0); vertices.Add(b1);
                for (int k = 0; k < 4; k++)
                {
                    colors.Add(SlideOuter * 0.85f);
                }
                Vector3 normal = Vector3.Cross(a1 - a0, b0 - a0);
                if (normal.y < 0f)
                {
                    triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
                    triangles.Add(start + 1); triangles.Add(start + 3); triangles.Add(start + 2);
                }
                else
                {
                    triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1);
                    triangles.Add(start + 1); triangles.Add(start + 2); triangles.Add(start + 3);
                }
            }
            CreateSlideMesh("SlideUnderside", vertices, colors, triangles, theme, parent);
        }

        static void AddBox(List<Vector3> vertices, List<Color> colors, List<int> triangles, Vector3 center, Vector3 x, Vector3 y, Vector3 z, Color color)
        {
            Vector3[] corners = new Vector3[8];
            for (int k = 0; k < 8; k++)
            {
                corners[k] = center + x * ((k & 1) == 0 ? -1f : 1f) + y * ((k & 2) == 0 ? -1f : 1f) + z * ((k & 4) == 0 ? -1f : 1f);
            }
            int[][] faces =
            {
                new[] { 0, 2, 3, 1 }, new[] { 4, 5, 7, 6 }, new[] { 0, 1, 5, 4 },
                new[] { 2, 6, 7, 3 }, new[] { 0, 4, 6, 2 }, new[] { 1, 3, 7, 5 }
            };
            foreach (int[] face in faces)
            {
                int start = vertices.Count;
                Vector3 faceCenter = Vector3.zero;
                foreach (int index in face)
                {
                    vertices.Add(corners[index]);
                    colors.Add(color);
                    faceCenter += corners[index] * 0.25f;
                }
                Vector3 normal = Vector3.Cross(vertices[start + 1] - vertices[start], vertices[start + 2] - vertices[start]);
                bool outwardFacing = Vector3.Dot(normal, faceCenter - center) > 0f;
                if (outwardFacing)
                {
                    triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
                    triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);
                }
                else
                {
                    triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1);
                    triangles.Add(start); triangles.Add(start + 3); triangles.Add(start + 2);
                }
            }
        }

        static void CreateSlideMesh(string name, List<Vector3> vertices, List<Color> colors, List<int> triangles, CourseTheme theme, Transform parent)
        {
            var mesh = new Mesh { name = name };
            mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, true);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = theme.sculptMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        static void BuildClouds(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (theme.cloudMaterial == null)
            {
                return;
            }
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            float lowest = 0f;
            for (int i = 0; i < level.NodeCount; i++)
            {
                min = Vector2.Min(min, level.nodes[i].position);
                max = Vector2.Max(max, level.nodes[i].position);
                lowest = Mathf.Min(lowest, level.Height(i));
            }
            min -= Vector2.one * 400f;
            max += Vector2.one * 400f;
            float y = lowest - CloudDepth;
            var mesh = new Mesh { name = "Clouds" };
            mesh.vertices = new[] { new Vector3(min.x, y, min.y), new Vector3(min.x, y, max.y), new Vector3(max.x, y, max.y), new Vector3(max.x, y, min.y) };
            mesh.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            var go = new GameObject("Clouds", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, true);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = theme.cloudMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        static void BuildBackdrop(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            if (theme.mesaMeshes == null || theme.mesaMeshes.Length == 0)
            {
                return;
            }
            var random = new Random(CourseGeometry.StableHash(level.id) + 404);
            var batch = new MeshBatch(120f);
            for (int i = 0; i < level.NodeCount; i += 7)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 side = Vector3.Cross(Vector3.up, level.Direction(i)).normalized * s;
                    Vector3 position = level.Center(i) + side * Range(random, 150f, 240f) + level.Direction(i) * Range(random, -30f, 30f);
                    if (CourseGeometry.WallDistance(level, position) > -110f)
                    {
                        continue;
                    }
                    position.y = level.Height(i) + Range(random, -6f, 18f);
                    float scale = Range(random, 2.2f, 3.4f);
                    batch.Add(theme.mesaMeshes[random.Next(theme.mesaMeshes.Length)], theme.sculptMaterial, position, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f), new Vector3(scale, scale * Range(random, 0.9f, 1.3f), scale));
                }
            }
            batch.Build(parent, "Backdrop", false);
        }

        static void ScatterElevated(LevelDefinition level, CourseTheme theme, MeshBatch batch, Random random, int startNode, int endNode)
        {
            for (int i = startNode; i < endNode && i < level.NodeCount - 1; i++)
            {
                Vector3 forward = level.Direction(i);
                float height = level.Height(i);
                if (theme.pillarMesh != null && i % 3 == 0)
                {
                    Vector3 pillar = level.Center(i);
                    pillar.y = height + TroughBottom + 0.1f;
                    batch.Add(theme.pillarMesh, theme.sculptMaterial, pillar, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f), new Vector3(1f, 1f, 1f) * Range(random, 0.9f, 1.15f));
                }
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 side = Vector3.Cross(Vector3.up, forward).normalized * s;
                    if (theme.gatePoleMesh != null && Next(random) < 0.22f)
                    {
                        Vector3 pole = level.Edge(i, s) + side * 0.3f + forward * Range(random, 0f, 8f);
                        pole.y = height + 0.9f;
                        batch.Add(theme.gatePoleMesh, theme.sculptMaterial, pole, Quaternion.Euler(Range(random, -6f, 6f), Range(random, 0f, 360f), Range(random, -6f, 6f)), Vector3.one * Range(random, 0.35f, 0.6f));
                    }
                    if (theme.mesaMeshes == null || theme.mesaMeshes.Length == 0 || Next(random) > 0.4f)
                    {
                        continue;
                    }
                    float distance = Range(random, 14f, 70f);
                    Vector3 position = level.Center(i) + side * (level.Width(i) * 0.5f + distance) + forward * Range(random, -4f, 4f);
                    float scale = Range(random, 0.55f, 1.25f);
                    if (CourseGeometry.WallDistance(level, position) > -(9f * scale + 4f))
                    {
                        continue;
                    }
                    position.y = height + Range(random, -9f, 5f);
                    var rotation = Quaternion.Euler(0f, Range(random, 0f, 360f), 0f);
                    batch.Add(theme.mesaMeshes[random.Next(theme.mesaMeshes.Length)], theme.sculptMaterial, position, rotation, new Vector3(scale, scale * Range(random, 0.85f, 1.2f), scale));
                    int trees = random.Next(1, 4);
                    for (int t = 0; t < trees && theme.treeMeshes != null && theme.treeMeshes.Length > 0; t++)
                    {
                        float angle = Next(random) * Mathf.PI * 2f;
                        float radius = Mathf.Sqrt(Next(random)) * 5f * scale;
                        Vector3 tree = position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                        Material foliage = theme.foliageMaterial != null ? theme.foliageMaterial : theme.sculptMaterial;
                        batch.Add(theme.treeMeshes[random.Next(theme.treeMeshes.Length)], foliage, tree, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f), Vector3.one * Range(random, 0.7f, 1.1f));
                    }
                }
            }
        }
    }
}
