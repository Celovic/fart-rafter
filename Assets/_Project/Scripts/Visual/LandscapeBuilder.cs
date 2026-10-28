using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

namespace FartRafters
{
    public static partial class LandscapeBuilder
    {
        public const float LandHeight = 1.74f;
        static LevelDefinition heightLevel;
        const float BaseBankOffset = 0.5f;
        static float landHeight = LandHeight;
        static float bankSpread = 1f;
        static float BankOffset => BaseBankOffset * bankSpread;
        const float ShoreClearance = 1.5f;
        const float GroundTiling = 0.45f;
        const float ChunkSize = 24f;

        static readonly float[] BankOffsets = { -0.35f, -0.05f, 0.2f, 0.42f, BaseBankOffset + 0.15f };
        static readonly float[] BankHeights = { -0.7f, 0.05f, 0.85f, 1.5f, LandHeight + 0.03f };

        public static float CurrentLandHeight => landHeight;

        static void Configure(CourseTheme theme)
        {
            float height = theme != null && theme.landHeight > 0f ? theme.landHeight : LandHeight;
            float spread = theme != null && theme.bankSpread > 0f ? theme.bankSpread : 1f;
            if (!Mathf.Approximately(height, landHeight) || !Mathf.Approximately(spread, bankSpread))
            {
                wallCache.Clear();
            }
            landHeight = height;
            bankSpread = spread;
        }

        static float BankHeight(int row)
        {
            if (row >= BankHeights.Length - 1)
            {
                return landHeight + 0.03f;
            }
            float height = BankHeights[row];
            return height > 0f ? height * landHeight / LandHeight : height;
        }

        public static void Build(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            Configure(theme);
            heightLevel = level;
            var root = new GameObject("Landscape").transform;
            root.SetParent(parent, false);
            if (theme.elevated)
            {
                BuildElevated(level, theme, root);
                var chunks = root.gameObject.AddComponent<DecorStreamer>();
                chunks.level = level;
                chunks.theme = theme;
                return;
            }
            BuildLand(level, theme, root);
            BuildBanks(level, theme, root);

            var crowd = root.gameObject.AddComponent<SpectatorStreamer>();
            crowd.level = level;
            crowd.theme = theme;

            var streamer = root.gameObject.AddComponent<DecorStreamer>();
            streamer.level = level;
            streamer.theme = theme;
        }

        static readonly Dictionary<long, float> wallCache = new Dictionary<long, float>();
        static LevelDefinition cachedLevel;

        static float CachedWallDistance(Vector3 point)
        {
            if (cachedLevel != heightLevel)
            {
                wallCache.Clear();
                cachedLevel = heightLevel;
            }
            long key = ((long)Mathf.RoundToInt(point.x) << 32) ^ (uint)Mathf.RoundToInt(point.z);
            if (!wallCache.TryGetValue(key, out float distance))
            {
                distance = CourseGeometry.WallDistance(heightLevel, new Vector3(Mathf.Round(point.x), 0f, Mathf.Round(point.z)));
                wallCache[key] = distance;
            }
            return distance;
        }

        public static float GroundHeight(Vector3 point)
        {
            if (heightLevel == null)
            {
                return landHeight;
            }
            float distance = -CachedWallDistance(point) - BankOffset;
            float rise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(7f, 24f, distance));
            if (rise <= 0f)
            {
                return landHeight;
            }
            float hills = Mathf.PerlinNoise(point.x * 0.028f + 13.1f, point.z * 0.028f + 7.7f) * 3.4f + Mathf.PerlinNoise(point.x * 0.09f + 3.3f, point.z * 0.09f + 1.9f) * 0.8f;
            return landHeight + rise * hills;
        }

        public static Transform BuildDecorChunk(LevelDefinition level, CourseTheme theme, Transform parent, int startNode, int endNode, int chunk)
        {
            Configure(theme);
            heightLevel = level;
            var root = new GameObject("DecorChunk_" + chunk).transform;
            root.SetParent(parent, false);
            var batch = new MeshBatch(ChunkSize);
            var random = new Random(CourseGeometry.StableHash(level.id) * 31 + chunk);
            if (theme.elevated)
            {
                ScatterElevated(level, theme, batch, random, startNode, endNode);
                batch.Build(root, "Decor", true);
                return root;
            }
            ScatterAlongBanks(level, theme, batch, random, startNode, endNode);
            ScatterWaterline(level, theme, batch, random, startNode, endNode);
            if (startNode == 0)
            {
                ScatterBehindCap(level, theme, batch, random, 0, -1f);
            }
            if (endNode >= level.NodeCount - 1)
            {
                ScatterBehindCap(level, theme, batch, random, level.NodeCount - 1, 1f);
            }
            batch.Build(root, "Decor", true);
            return root;
        }

        public static void BuildIsland(Vector3 center, float radius, CourseTheme theme, Transform parent, Random random)
        {
            landHeight = LandHeight;
            bankSpread = 1f;
            heightLevel = null;
            const int segments = 40;
            var ring = new List<Vector3>();
            var outward = new List<Vector3>();
            for (int i = 0; i <= segments; i++)
            {
                float angle = i % segments * Mathf.PI * 2f / segments;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                float wobble = 1f + (Mathf.PerlinNoise(center.x * 0.1f + Mathf.Cos(angle) * 1.5f, center.z * 0.1f + Mathf.Sin(angle) * 1.5f) - 0.5f) * 0.25f;
                ring.Add(center + direction * (radius * wobble));
                outward.Add(-direction);
            }

            var top = new List<Vector3> { Lift(center) };
            for (int i = 0; i < segments; i++)
            {
                top.Add(Lift(ring[i] + outward[i] * (BankOffset + 0.15f)));
            }
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i < segments; i++)
            {
                int j = 1 + (i + 1) % segments;
                Vector3 innerA = Vector3.Lerp(top[0], top[1 + i], 0.72f);
                Vector3 innerB = Vector3.Lerp(top[0], top[j], 0.72f);
                AddTriangleUp(vertices, triangles, top[0], innerA, innerB);
                AddTriangleUp(vertices, triangles, innerA, top[1 + i], top[j]);
                AddTriangleUp(vertices, triangles, innerA, top[j], innerB);
            }
            var islandColors = new List<Color>();
            foreach (Vector3 vertex in vertices)
            {
                Vector3 offset = vertex - center;
                offset.y = 0f;
                float dirt = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(radius - 2.5f, radius - 0.4f, offset.magnitude));
                islandColors.Add(new Color(dirt, 0f, 0f, 1f));
            }
            CreateGround("IslandTop", vertices, triangles, islandColors, theme.grass, parent, false);
            BuildBankStrip(ring, outward, theme, parent, "IslandBank", new Random(random.Next()));

            var batch = new MeshBatch(ChunkSize);
            int count = Mathf.CeilToInt(radius * radius * 0.3f);
            for (int i = 0; i < count; i++)
            {
                float angle = Next(random) * Mathf.PI * 2f;
                float distance = Mathf.Sqrt(Next(random)) * (radius - 1.2f);
                Vector3 position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                PlaceDecoration(batch, theme, random, Lift(position), radius - distance > 2.5f ? 12f : 2f);
            }
            for (int i = 0; i < segments; i += 2)
            {
                if (Next(random) < 0.55f)
                {
                    WaterRock(batch, theme, random, ring[i] - outward[i] * 0.05f);
                }
            }
            batch.Build(parent, "Island", true);
        }

        static readonly float[] LandRows = { 0f, 1f, 2.5f, 4.5f, 7f, 10f, 14f, 19f, 25f, 32f, 40f, 48f };

        static void BuildLand(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            float width = theme.landWidth;
            int rows = LandRows.Length;
            for (int s = -1; s <= 1; s += 2)
            {
                if (s == theme.openSide)
                {
                    continue;
                }
                var vertices = new List<Vector3>();
                var colors = new List<Color>();
                var triangles = new List<int>();
                for (int i = 0; i < level.NodeCount; i++)
                {
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, level.Direction(i)).normalized * s;
                    Vector3 start = level.Edge(i, s) + perpendicular * BankOffset;
                    if (i == 0 || i == level.NodeCount - 1)
                    {
                        start += level.Direction(i) * ((i == 0 ? -1f : 1f) * (BankOffset + 1.2f));
                    }
                    float clear = ClearDistance(level, start, perpendicular, width);
                    for (int r = 0; r < rows; r++)
                    {
                        float offset = Mathf.Min(LandRows[r] * width / LandRows[rows - 1], clear);
                        vertices.Add(r == 0 ? Flat(start) : Lift(start + perpendicular * offset));
                        float dirt = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 3f, offset));
                        colors.Add(new Color(dirt, 0f, 0f, 1f));
                    }
                }
                for (int i = 0; i < level.NodeCount - 1; i++)
                {
                    for (int r = 0; r < rows - 1; r++)
                    {
                        int a = i * rows + r;
                        int b = a + rows;
                        AddUpQuad(vertices, triangles, a, b, b + 1, a + 1);
                    }
                }
                CreateGround("Land", vertices, triangles, colors, theme.grass, parent, true);
            }

            var capVertices = new List<Vector3>();
            var capTriangles = new List<int>();
            AddCapLand(level, capVertices, capTriangles, 0, -1f, width);
            AddCapLand(level, capVertices, capTriangles, level.NodeCount - 1, 1f, width);
            CreateGround("CapLand", capVertices, capTriangles, null, theme.grass, parent, false);
        }

        static void AddUpQuad(List<Vector3> vertices, List<int> triangles, int a, int b, int c, int d)
        {
            if (Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]).y >= 0f)
            {
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(a); triangles.Add(c); triangles.Add(d);
            }
            else
            {
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(a); triangles.Add(d); triangles.Add(c);
            }
        }

        static void CreateGround(string name, List<Vector3> vertices, List<int> triangles, List<Color> colors, Material material, Transform parent, bool smooth)
        {
            var normals = new Vector3[vertices.Count];
            var uvs = new Vector2[vertices.Count];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z) * GroundTiling;
            }

            var mesh = new Mesh { name = name };
            mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            if (colors == null)
            {
                colors = new List<Color>(vertices.Count);
                for (int i = 0; i < vertices.Count; i++)
                {
                    colors.Add(new Color(0f, 0f, 0f, 1f));
                }
            }
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            if (smooth)
            {
                mesh.RecalculateNormals();
            }
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, true);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        static void BuildBanks(LevelDefinition level, CourseTheme theme, Transform parent)
        {
            var random = new Random(CourseGeometry.StableHash(level.id) + 7);
            for (int s = -1; s <= 1; s += 2)
            {
                if (s == theme.openSide)
                {
                    continue;
                }
                var points = new List<Vector3>();
                var outward = new List<Vector3>();
                for (int i = 0; i < level.NodeCount; i++)
                {
                    points.Add(level.Edge(i, s));
                    outward.Add(Vector3.Cross(Vector3.up, level.Direction(i)).normalized * s);
                }
                BuildBankStrip(Subdivide(points, outward, 1.2f, out List<Vector3> normals), normals, theme, parent, "Bank", random);
            }

            for (int c = 0; c < 2; c++)
            {
                int node = c == 0 ? 0 : level.NodeCount - 1;
                float sign = c == 0 ? -1f : 1f;
                Vector3 direction = level.Direction(node) * sign;
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
                float half = level.Width(node) * 0.5f + 0.8f;
                var points = new List<Vector3> { level.Center(node) - right * half, level.Center(node) + right * half };
                var outward = new List<Vector3> { direction, direction };
                BuildBankStrip(Subdivide(points, outward, 1.2f, out List<Vector3> normals), normals, theme, parent, "CapBank", random);
            }
        }

        static List<Vector3> Subdivide(List<Vector3> points, List<Vector3> outward, float step, out List<Vector3> normals)
        {
            var result = new List<Vector3>();
            normals = new List<Vector3>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                float length = Vector3.Distance(points[i], points[i + 1]);
                int steps = Mathf.Max(1, Mathf.CeilToInt(length / step));
                for (int k = 0; k < steps; k++)
                {
                    float t = (float)k / steps;
                    result.Add(Vector3.Lerp(points[i], points[i + 1], t));
                    normals.Add(Vector3.Slerp(outward[i], outward[i + 1], t).normalized);
                }
            }
            result.Add(points[points.Count - 1]);
            normals.Add(outward[outward.Count - 1]);
            return result;
        }

        static void BuildBankStrip(List<Vector3> line, List<Vector3> outward, CourseTheme theme, Transform parent, string name, Random random)
        {
            int rows = BankOffsets.Length;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            float along = 0f;
            float seed = (float)random.NextDouble() * 100f;

            for (int i = 0; i < line.Count; i++)
            {
                if (i > 0)
                {
                    along += Vector3.Distance(line[i], line[i - 1]);
                }
                for (int r = 0; r < rows; r++)
                {
                    float wobble = r == 0 || r == rows - 1 ? 0f : (Mathf.PerlinNoise(along * 0.45f + seed, r * 3.1f) - 0.5f) * 0.45f;
                    float lift = r == 0 || r == rows - 1 ? 0f : (Mathf.PerlinNoise(along * 0.3f + seed, r * 7.3f + 11f) - 0.5f) * 0.35f;
                    Vector3 point = line[i] + outward[i] * (BankOffsets[r] * bankSpread + wobble);
                    point.y = BankHeight(r) + lift * (r == 0 || r == rows - 1 ? 0f : landHeight / LandHeight);
                    vertices.Add(point);
                    uvs.Add(new Vector2(along * 0.5f, BankHeight(r) * 0.6f));
                    float mud = Mathf.PerlinNoise(along * 0.25f + seed, r * 1.7f);
                    colors.Add(BankLayer(r, rows, mud));
                }
            }

            for (int i = 0; i < line.Count - 1; i++)
            {
                for (int r = 0; r < rows - 1; r++)
                {
                    int a = i * rows + r;
                    int b = a + rows;
                    Vector3 normalGuess = Vector3.Cross(vertices[b] - vertices[a], vertices[a + 1] - vertices[a]);
                    bool flip = Vector3.Dot(normalGuess, -outward[i] + Vector3.up * 0.5f) < 0f;
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

            var mesh = new Mesh { name = name };
            mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, true);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = theme.bank != null ? theme.bank : theme.wall;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        static Color BankLayer(int row, int rows, float noise)
        {
            float t = row / (float)(rows - 1);
            float dirt = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1f, t));
            float mud = Mathf.Clamp01(1f - Mathf.Abs(t - 0.45f) * 2.6f + (noise - 0.5f) * 0.6f);
            float pebble = Mathf.Clamp01(1f - t * 2.4f + (0.5f - noise) * 0.5f);
            return new Color(dirt, mud, pebble, 1f);
        }

        static void AddCapLand(LevelDefinition level, List<Vector3> vertices, List<int> triangles, int node, float outward, float width)
        {
            Vector3 direction = level.Direction(node) * outward;
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            float extent = level.Width(node) * 0.5f + BankOffset + width;
            Vector3 back = level.Center(node) + direction * BankOffset;
            Vector3 far = back + direction * width;
            AddQuad(vertices, triangles, Lift(back - right * extent), Lift(back + right * extent), Lift(far + right * extent), Lift(far - right * extent));
        }

        static float ClearDistance(LevelDefinition level, Vector3 start, Vector3 perpendicular, float width)
        {
            for (float d = 2f; d <= width; d += 2f)
            {
                if (CourseGeometry.WallDistance(level, start + perpendicular * d) > -ShoreClearance)
                {
                    return Mathf.Max(1f, d - 2f);
                }
            }
            return width;
        }

        static void AddQuad(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            AddTriangleUp(vertices, triangles, a, b, c);
            AddTriangleUp(vertices, triangles, a, c, d);
        }

        static void AddTriangleUp(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            if (Vector3.Cross(b - a, c - a).y < 0f)
            {
                (b, c) = (c, b);
            }
            triangles.Add(vertices.Count);
            vertices.Add(a);
            triangles.Add(vertices.Count);
            vertices.Add(b);
            triangles.Add(vertices.Count);
            vertices.Add(c);
        }

        static Vector3 Lift(Vector3 point)
        {
            point.y = GroundHeight(point);
            return point;
        }

        static Vector3 Flat(Vector3 point)
        {
            point.y = landHeight;
            return point;
        }

        static void ScatterAlongBanks(LevelDefinition level, CourseTheme theme, MeshBatch batch, Random random, int startNode, int endNode)
        {
            float width = theme.landWidth;
            for (int s = -1; s <= 1; s += 2)
            {
                if (s == theme.openSide)
                {
                    continue;
                }
                for (int i = startNode; i < endNode && i < level.NodeCount - 1; i++)
                {
                    Vector3 a = level.Edge(i, s);
                    Vector3 b = level.Edge(i + 1, s);
                    float length = Vector3.Distance(a, b);
                    if (length < 0.5f)
                    {
                        continue;
                    }
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, (b - a) / length).normalized * s;
                    int count = Mathf.CeilToInt(length / theme.decorationSpacing);
                    for (int k = 0; k < count; k++)
                    {
                        float along = (k + Next(random)) / count;
                        float distance = 1.2f + Mathf.Pow(Next(random), 1.5f) * (width - 6f);
                        Vector3 position = Vector3.Lerp(a, b, along) + perpendicular * (BankOffset + distance);
                        if (CourseGeometry.WallDistance(level, position) > -ShoreClearance - 0.5f)
                        {
                            continue;
                        }
                        PlaceDecoration(batch, theme, random, Lift(position), distance);
                    }

                    int grass = Mathf.CeilToInt(length / 1.6f);
                    for (int k = 0; k < grass; k++)
                    {
                        float along = (k + Next(random)) / grass;
                        float distance = 0.7f + Next(random) * 5f;
                        Vector3 position = Vector3.Lerp(a, b, along) + perpendicular * (BankOffset + distance);
                        if (CourseGeometry.WallDistance(level, position) > -ShoreClearance)
                        {
                            continue;
                        }
                        Model(batch, theme.grassModels, random, Lift(position), Range(random, 0.28f, 0.42f));
                    }
                }
            }
        }

        static void ScatterWaterline(LevelDefinition level, CourseTheme theme, MeshBatch batch, Random random, int startNode, int endNode)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                if (s == theme.openSide)
                {
                    ScatterOpenEdge(level, theme, batch, random, startNode, endNode, s);
                    continue;
                }
                for (int i = startNode; i < endNode && i < level.NodeCount - 1; i++)
                {
                    Vector3 a = level.Edge(i, s);
                    Vector3 b = level.Edge(i + 1, s);
                    float length = Vector3.Distance(a, b);
                    if (length < 0.5f)
                    {
                        continue;
                    }
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, (b - a) / length).normalized * s;
                    int count = Mathf.CeilToInt(length / 2.2f);
                    for (int k = 0; k < count; k++)
                    {
                        Vector3 edge = Vector3.Lerp(a, b, (k + Next(random)) / count);
                        float roll = Next(random);
                        if (roll < 0.45f)
                        {
                            WaterRock(batch, theme, random, edge + perpendicular * Range(random, 0.05f, 0.3f));
                        }
                        else if (roll < 0.75f)
                        {
                            Vector3 reed = edge + perpendicular * Range(random, 0.55f, 0.9f);
                            reed.y = landHeight - 0.05f;
                            Model(batch, theme.reedModels, random, reed, Range(random, 0.45f, 0.65f));
                        }
                    }
                }
            }
        }

        static void WaterRock(MeshBatch batch, CourseTheme theme, Random random, Vector3 position)
        {
            position.y = Range(random, -0.15f, 0.2f);
            float size = Range(random, 0.45f, 0.85f);
            Vector3 scale = new Vector3(size * Range(random, 0.9f, 1.4f), size * Range(random, 0.6f, 1f), size * Range(random, 0.9f, 1.4f));
            Quaternion rotation = Quaternion.Euler(Range(random, -12f, 12f), Range(random, 0f, 360f), Range(random, -12f, 12f));
            Rock(batch, theme, random, position, scale, rotation);
        }

        static void Rock(MeshBatch batch, CourseTheme theme, Random random, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            if (theme.rockMeshes != null && theme.rockMeshes.Length > 0 && theme.rockMaterial != null)
            {
                batch.Add(theme.rockMeshes[random.Next(theme.rockMeshes.Length)], theme.rockMaterial, position, rotation, scale);
                return;
            }
            Model(batch, theme.rockModels, random, position, scale, rotation);
        }

        static void Rock(MeshBatch batch, CourseTheme theme, Random random, Vector3 position, float scale)
        {
            Rock(batch, theme, random, position, Vector3.one * scale, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f));
        }

        static void ScatterOpenEdge(LevelDefinition level, CourseTheme theme, MeshBatch batch, Random random, int startNode, int endNode, float side)
        {
            for (int i = startNode; i < endNode && i < level.NodeCount - 1; i++)
            {
                Vector3 a = level.Edge(i, side);
                Vector3 b = level.Edge(i + 1, side);
                float length = Vector3.Distance(a, b);
                if (length < 0.5f)
                {
                    continue;
                }
                Vector3 direction = (b - a) / length;
                Vector3 perpendicular = Vector3.Cross(Vector3.up, direction).normalized * side;
                int floats = Mathf.CeilToInt(length / 3.2f);
                for (int k = 0; k < floats; k++)
                {
                    Vector3 position = Vector3.Lerp(a, b, (k + 0.5f) / floats) + perpendicular * 0.4f;
                    position.y = 0.06f;
                    if (theme.buoyMesh != null && theme.sculptMaterial != null)
                    {
                        batch.Add(theme.buoyMesh, theme.sculptMaterial, position, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f), Vector3.one * 0.32f);
                    }
                }
                if (Next(random) < 0.18f)
                {
                    Vector3 reef = Vector3.Lerp(a, b, Next(random)) + perpendicular * Range(random, 3f, 14f);
                    reef.y = Range(random, -0.3f, 0.05f);
                    float size = Range(random, 0.8f, 1.6f);
                    Rock(batch, theme, random, reef, new Vector3(size * Range(random, 0.9f, 1.5f), size * Range(random, 0.5f, 0.9f), size * Range(random, 0.9f, 1.5f)), Quaternion.Euler(Range(random, -10f, 10f), Range(random, 0f, 360f), Range(random, -10f, 10f)));
                }
            }
        }

        static void ScatterBehindCap(LevelDefinition level, CourseTheme theme, MeshBatch batch, Random random, int node, float outward)
        {
            Vector3 direction = level.Direction(node) * outward;
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 back = level.Center(node) + direction * BankOffset;
            float extent = level.Width(node) * 0.5f + theme.landWidth * 0.6f;
            int count = Mathf.CeilToInt(theme.landWidth * 0.8f);
            for (int i = 0; i < count; i++)
            {
                float distance = 1.2f + Mathf.Pow(Next(random), 1.3f) * (theme.landWidth - 6f);
                Vector3 position = back + direction * distance + right * Range(random, -extent, extent);
                if (CourseGeometry.WallDistance(level, position) > -ShoreClearance - 0.5f)
                {
                    continue;
                }
                PlaceDecoration(batch, theme, random, Lift(position), distance);
            }
        }

        static void PlaceDecoration(MeshBatch batch, CourseTheme theme, Random random, Vector3 position, float shoreDistance)
        {
            float roll = Next(random);
            if (shoreDistance < 3f)
            {
                if (roll < 0.35f)
                {
                    Model(batch, theme.reedModels, random, position, Range(random, 0.45f, 0.65f));
                }
                else if (roll < 0.6f)
                {
                    Model(batch, theme.flowerModels, random, position, Range(random, 0.3f, 0.45f));
                }
                else if (roll < 0.8f)
                {
                    Model(batch, theme.pebbleModels, random, position, Range(random, 1f, 1.8f));
                }
                else
                {
                    Rock(batch, theme, random, position + Vector3.down * 0.1f, Range(random, 0.3f, 0.5f));
                }
            }
            else if (shoreDistance < 10f)
            {
                if (theme.shoreProps != null && theme.shoreProps.Length > 0 && Next(random) < theme.shorePropChance)
                {
                    batch.Add(theme.shoreProps[random.Next(theme.shoreProps.Length)], theme.sculptMaterial, position, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f), Vector3.one * Range(random, 0.9f, 1.1f));
                }
                else if (roll < 0.3f)
                {
                    Plant(batch, theme.bushMeshes, theme.bushModels, theme, random, position, Range(random, 0.8f, 1.2f));
                }
                else if (roll < 0.5f)
                {
                    Model(batch, theme.flowerModels, random, position, Range(random, 0.35f, 0.5f));
                }
                else if (roll < 0.65f)
                {
                    Model(batch, theme.plantModels, random, position, Range(random, 0.4f, 0.6f));
                }
                else if (roll < 0.78f)
                {
                    Rock(batch, theme, random, position + Vector3.down * 0.15f, Range(random, 0.4f, 0.8f));
                }
                else
                {
                    Plant(batch, theme.treeMeshes, theme.treeModels, theme, random, position, Range(random, 0.75f, 1f));
                }
            }
            else if (roll < 0.4f)
            {
                Plant(batch, theme.treeMeshes, theme.treeModels, theme, random, position, Range(random, 0.9f, 1.3f));
            }
            else if (roll < 0.75f)
            {
                Plant(batch, theme.pineMeshes, theme.pineModels, theme, random, position, Range(random, 0.85f, 1.25f));
            }
            else
            {
                Plant(batch, theme.bushMeshes, theme.bushModels, theme, random, position, Range(random, 0.9f, 1.4f));
            }
        }

        static void Plant(MeshBatch batch, Mesh[] meshes, GameObject[] fallback, CourseTheme theme, Random random, Vector3 position, float scale)
        {
            if (meshes == null || meshes.Length == 0 || theme.sculptMaterial == null)
            {
                Model(batch, fallback, random, position, scale);
                return;
            }
            batch.Add(meshes[random.Next(meshes.Length)], theme.foliageMaterial != null ? theme.foliageMaterial : theme.sculptMaterial, position, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f), Vector3.one * scale);
        }

        static void Model(MeshBatch batch, GameObject[] models, Random random, Vector3 position, float scale)
        {
            Model(batch, models, random, position, Vector3.one * scale, Quaternion.Euler(0f, Range(random, 0f, 360f), 0f));
        }

        static void Model(MeshBatch batch, GameObject[] models, Random random, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            if (models == null || models.Length == 0)
            {
                return;
            }
            batch.AddModel(models[random.Next(models.Length)], position, rotation, scale);
        }

        static float Next(Random random)
        {
            return (float)random.NextDouble();
        }

        static float Range(Random random, float min, float max)
        {
            return min + (max - min) * Next(random);
        }
    }
}
