using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public static class LavaDesertSculptor
    {
        public const float PumiceLength = 6.2f;

        static readonly Color Charcoal = new Color(0.12f, 0.11f, 0.11f);
        static readonly Color Ash = new Color(0.3f, 0.28f, 0.27f);
        static readonly Color BasaltDark = new Color(0.16f, 0.15f, 0.16f);
        static readonly Color BasaltGrey = new Color(0.26f, 0.25f, 0.27f);
        static readonly Color Ember = new Color(1f, 0.45f, 0.08f);
        static readonly Color Glow = new Color(1f, 0.82f, 0.2f);
        static readonly Color Magma = new Color(0.95f, 0.25f, 0.05f);

        static readonly Color Sand = new Color(0.93f, 0.76f, 0.5f);
        static readonly Color SandDark = new Color(0.78f, 0.58f, 0.36f);
        static readonly Color Rust = new Color(0.8f, 0.38f, 0.2f);
        static readonly Color RustDark = new Color(0.62f, 0.27f, 0.15f);
        static readonly Color CactusGreen = new Color(0.3f, 0.55f, 0.28f);
        static readonly Color CactusDark = new Color(0.2f, 0.42f, 0.22f);
        static readonly Color Bone = new Color(0.95f, 0.92f, 0.82f);
        static readonly Color Straw = new Color(0.72f, 0.58f, 0.34f);

        static float Range(System.Random random, float min, float max)
        {
            return min + (max - min) * (float)random.NextDouble();
        }

        static void Crack(Sculpt sculpt, Vector3 a, Vector3 b, Color color, float width = 0.04f)
        {
            Paint(sculpt, sculpt.Limb(a, b, width, width * 0.6f, color, 0.02f));
        }

        public static Sculpt CharredTree(int seed)
        {
            var tree = new Sculpt();
            var random = new System.Random(seed + 140);
            float height = 3.8f + (float)random.NextDouble() * 1.4f;
            float lean = Range(random, -0.4f, 0.4f);
            Vector3 top = V(lean, height, Range(random, -0.2f, 0.2f));
            tree.Limb(V(0f, -0.1f, 0f), top, 0.32f, 0.08f, Charcoal, 0.1f).noise = 0.04f;
            tree.Ellipsoid(V(0f, 0f, 0f), V(0.6f, 0.2f, 0.6f), Ash, 0.2f).noise = 0.05f;
            int branches = 4 + random.Next(2);
            for (int i = 0; i < branches; i++)
            {
                float t = Range(random, 0.35f, 0.85f);
                Vector3 root = Vector3.Lerp(V(0f, 0f, 0f), top, t);
                float angle = i * 2.3f + seed;
                float reach = Range(random, 0.8f, 1.6f) * (1.1f - t * 0.5f);
                Vector3 tip = root + V(Mathf.Cos(angle) * reach, Range(random, 0.4f, 1.1f), Mathf.Sin(angle) * reach);
                tree.Limb(root, tip, 0.11f, 0.03f, Charcoal, 0.05f).noise = 0.02f;
                Vector3 twig = tip + V(Mathf.Cos(angle + 0.8f) * 0.4f, 0.35f, Mathf.Sin(angle + 0.8f) * 0.4f);
                tree.Limb(tip, twig, 0.04f, 0.015f, Charcoal, 0.03f);
                if (i % 2 == 0)
                {
                    Paint(tree, tree.Sphere(twig, 0.08f, Ember, 0.04f));
                }
            }
            Paint(tree, tree.Sphere(top, 0.12f, Ember, 0.05f));
            for (int i = 0; i < 3; i++)
            {
                float y = Range(random, 0.4f, height * 0.6f);
                Vector3 point = Vector3.Lerp(V(0f, 0f, 0f), top, y / height);
                Crack(tree, point + V(0.2f, 0f, 0.18f), point + V(0.24f, 0.45f, 0.15f), Ember, 0.035f);
            }
            return tree;
        }

        public static Sculpt EmberRock(int seed)
        {
            var mound = new Sculpt();
            var random = new System.Random(seed + 160);
            int blobs = 3 + random.Next(3);
            for (int i = 0; i < blobs; i++)
            {
                float angle = i * 2.1f;
                float spread = i == 0 ? 0f : 0.45f + (float)random.NextDouble() * 0.3f;
                float radius = i == 0 ? 0.65f : 0.4f + (float)random.NextDouble() * 0.2f;
                Color color = Color.Lerp(BasaltDark, Ash, (float)random.NextDouble());
                mound.Ellipsoid(V(Mathf.Cos(angle) * spread, radius * 0.4f, Mathf.Sin(angle) * spread), V(radius, radius * 0.7f, radius), color, 0.25f).noise = 0.1f;
            }
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 1.6f + seed;
                Vector3 a = V(Mathf.Cos(angle) * 0.1f, 0.62f, Mathf.Sin(angle) * 0.1f);
                Vector3 b = V(Mathf.Cos(angle) * 0.65f, 0.15f, Mathf.Sin(angle) * 0.65f);
                Crack(mound, a, b, i % 2 == 0 ? Ember : Glow, 0.05f);
            }
            Paint(mound, mound.Sphere(V(0f, 0.68f, 0f), 0.16f, Glow, 0.06f));
            mound.Limb(V(0f, 0.6f, 0f), V(0.05f, 1.05f, 0f), 0.12f, 0.2f, Ash, 0.15f).noise = 0.12f;
            mound.Sphere(V(0.12f, 1.25f, 0.05f), 0.24f, new Color(0.45f, 0.43f, 0.42f), 0.15f).noise = 0.12f;
            return mound;
        }

        public static Sculpt Basalt(int seed)
        {
            var rock = new Sculpt();
            var random = new System.Random(seed * 17 + 5);
            int columns = 4 + random.Next(3);
            for (int i = 0; i < columns; i++)
            {
                float angle = i * 2.4f + seed;
                float spread = i == 0 ? 0f : Range(random, 0.35f, 0.75f);
                float height = Range(random, 0.8f, 1.8f);
                Vector3 center = V(Mathf.Cos(angle) * spread, height * 0.5f, Mathf.Sin(angle) * spread);
                Color color = Color.Lerp(BasaltDark, BasaltGrey, (float)random.NextDouble());
                rock.Box(center, V(Range(random, 0.6f, 0.9f), height, Range(random, 0.6f, 0.9f)), 0.06f, color, 0.05f, V(Range(random, -10f, 10f), Range(random, 0f, 360f), Range(random, -10f, 10f))).noise = 0.02f;
            }
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 2.1f + seed * 0.7f;
                Vector3 a = V(Mathf.Cos(angle) * 0.55f, 0.1f, Mathf.Sin(angle) * 0.55f);
                Vector3 b = V(Mathf.Cos(angle + 0.4f) * 0.6f, 1.1f, Mathf.Sin(angle + 0.4f) * 0.6f);
                Crack(rock, a, b, Ember, 0.045f);
            }
            Paint(rock, rock.Ellipsoid(V(0f, 0f, 0f), V(1.2f, 0.18f, 1.2f), Magma, 0.12f));
            return rock;
        }

        public static Sculpt BasaltSpikes()
        {
            var sculpt = new Sculpt();
            var random = new System.Random(33);
            sculpt.Ellipsoid(V(0f, -0.05f, 0f), V(0.5f, 0.2f, 1.0f), BasaltDark, 0.15f).noise = 0.04f;
            for (int i = 0; i < 9; i++)
            {
                float z = -0.85f + i * 0.21f;
                float lean = (i % 2 == 0 ? 1f : -1f) * Range(random, 0.15f, 0.45f);
                Vector3 root = V(lean * 0.3f, 0.02f, z);
                Vector3 tip = V(lean + Range(random, -0.1f, 0.1f), 0.8f + Range(random, 0f, 0.55f), z + Range(random, -0.12f, 0.12f));
                Color color = Color.Lerp(BasaltDark, BasaltGrey, (float)random.NextDouble());
                sculpt.Limb(root, tip, 0.14f + Range(random, 0f, 0.05f), 0.012f, color, 0.04f);
                Paint(sculpt, sculpt.Sphere(tip, 0.08f, i % 3 == 0 ? Glow : Ember, 0.03f));
            }
            for (int i = 0; i < 5; i++)
            {
                float z = -0.7f + i * 0.35f;
                sculpt.Limb(V(-0.35f, 0.05f, z), V(-0.75f, 0.45f, z + 0.1f), 0.08f, 0.01f, BasaltGrey, 0.03f);
            }
            Paint(sculpt, sculpt.Ellipsoid(V(0f, 0.08f, 0f), V(0.25f, 0.08f, 0.9f), Ember, 0.08f));
            return sculpt;
        }

        public static Sculpt Pumice()
        {
            var rock = new Sculpt();
            var random = new System.Random(51);
            float half = PumiceLength * 0.5f - 0.3f;
            Color pumice = new Color(0.42f, 0.38f, 0.36f);
            Color light = new Color(0.58f, 0.54f, 0.5f);
            rock.Ellipsoid(V(0f, 0f, 0f), V(0.85f, 0.55f, half), pumice, 0.1f).noise = 0.12f;
            rock.Ellipsoid(V(0.25f, 0.25f, -0.9f), V(0.65f, 0.5f, 1.1f), light, 0.2f).noise = 0.1f;
            rock.Ellipsoid(V(-0.2f, 0.2f, 1.2f), V(0.6f, 0.45f, 0.9f), pumice, 0.2f).noise = 0.1f;
            for (int i = 0; i < 14; i++)
            {
                float z = Range(random, -half * 0.9f, half * 0.9f);
                float angle = Range(random, -1.2f, 1.2f);
                Vector3 point = V(Mathf.Sin(angle) * 0.75f, Mathf.Cos(angle) * 0.5f, z);
                Carve(rock, rock.Sphere(point, Range(random, 0.08f, 0.16f), BasaltDark, 0.03f));
            }
            Crack(rock, V(0.4f, 0.45f, -1.8f), V(-0.3f, 0.5f, 0.2f), Ember, 0.05f);
            Crack(rock, V(-0.3f, 0.5f, 0.2f), V(0.2f, 0.42f, 1.9f), Ember, 0.045f);
            return rock;
        }

        public static Sculpt LavaBomb()
        {
            var bomb = new Sculpt();
            bomb.Sphere(V(0f, 0.75f, 0f), 0.7f, BasaltDark, 0.05f).noise = 0.12f;
            bomb.Ellipsoid(V(0.2f, 1.1f, 0.1f), V(0.45f, 0.4f, 0.45f), BasaltGrey, 0.15f).noise = 0.1f;
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f;
                Vector3 a = V(Mathf.Cos(angle) * 0.3f, 1.35f, Mathf.Sin(angle) * 0.3f);
                Vector3 b = V(Mathf.Cos(angle + 0.5f) * 0.68f, 0.35f, Mathf.Sin(angle + 0.5f) * 0.68f);
                Crack(bomb, a, b, i % 2 == 0 ? Glow : Ember, 0.07f);
            }
            Paint(bomb, bomb.Ellipsoid(V(0f, 0.15f, 0f), V(0.55f, 0.25f, 0.55f), Glow, 0.1f));
            bomb.Limb(V(0f, 1.3f, 0f), V(0.1f, 2.4f, 0.05f), 0.32f, 0.08f, Magma, 0.15f).noise = 0.08f;
            Paint(bomb, bomb.Limb(V(0f, 1.6f, 0f), V(0.1f, 2.4f, 0.05f), 0.2f, 0.05f, Glow, 0.08f));
            return bomb;
        }

        public static Sculpt MiniVolcano()
        {
            var volcano = new Sculpt();
            volcano.Limb(V(0f, -0.1f, 0f), V(0f, 1.6f, 0f), 1.45f, 0.45f, BasaltGrey, 0.15f).noise = 0.06f;
            volcano.Ellipsoid(V(0f, 0f, 0f), V(1.6f, 0.25f, 1.6f), BasaltDark, 0.2f).noise = 0.05f;
            Carve(volcano, volcano.Ellipsoid(V(0f, 1.95f, 0f), V(0.38f, 0.4f, 0.38f), Magma, 0.05f));
            volcano.Ellipsoid(V(0f, 1.72f, 0f), V(0.34f, 0.08f, 0.34f), Glow, 0.04f);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f + 0.4f;
                Vector3 a = V(Mathf.Cos(angle) * 0.42f, 1.7f, Mathf.Sin(angle) * 0.42f);
                Vector3 b = V(Mathf.Cos(angle + 0.2f) * 1.1f, 0.6f, Mathf.Sin(angle + 0.2f) * 1.1f);
                Vector3 c = V(Mathf.Cos(angle + 0.3f) * 1.5f, 0.05f, Mathf.Sin(angle + 0.3f) * 1.5f);
                volcano.Limb(a, b, 0.14f, 0.12f, Ember, 0.06f);
                volcano.Limb(b, c, 0.12f, 0.08f, Magma, 0.06f);
            }
            volcano.Sphere(V(0.1f, 2.4f, 0f), 0.32f, Ash, 0.2f).noise = 0.12f;
            volcano.Sphere(V(-0.15f, 2.85f, 0.1f), 0.26f, new Color(0.42f, 0.4f, 0.4f), 0.2f).noise = 0.12f;
            return volcano;
        }

        public static Sculpt Obsidian()
        {
            var crystal = new Sculpt();
            var random = new System.Random(23);
            Color obsidian = new Color(0.1f, 0.08f, 0.14f);
            Color sheen = new Color(0.35f, 0.25f, 0.45f);
            for (int i = 0; i < 6; i++)
            {
                float height = Range(random, 0.5f, 1.2f);
                Vector3 tilt = V(Range(random, -25f, 25f), Range(random, 0f, 360f), Range(random, -25f, 25f));
                Quaternion rotation = Quaternion.Euler(tilt);
                Vector3 center = rotation * V(0f, height * 0.5f, 0f) + V(Range(random, -0.2f, 0.2f), 0f, Range(random, -0.2f, 0.2f));
                Color color = Color.Lerp(obsidian, sheen, (float)random.NextDouble() * 0.6f);
                crystal.Box(center, V(0.18f, height, 0.18f), 0.02f, color, 0.03f, tilt);
                crystal.Limb(center + rotation * V(0f, height * 0.5f, 0f), center + rotation * V(0f, height * 0.5f + 0.18f, 0f), 0.09f, 0.01f, color, 0.02f);
            }
            crystal.Ellipsoid(V(0f, 0f, 0f), V(0.45f, 0.12f, 0.45f), BasaltDark, 0.1f);
            Paint(crystal, crystal.Torus(V(0f, 0.05f, 0f), 0.4f, 0.04f, Ember, 0.03f));
            return crystal;
        }

        public static Sculpt SmokingVent()
        {
            var vent = new Sculpt();
            vent.Ellipsoid(V(0f, 0.05f, 0f), V(0.75f, 0.2f, 0.75f), BasaltDark, 0.12f).noise = 0.06f;
            vent.Limb(V(0f, 0f, 0f), V(0.05f, 0.55f, 0f), 0.42f, 0.26f, BasaltGrey, 0.1f).noise = 0.05f;
            Carve(vent, vent.Limb(V(0.05f, 0.3f, 0f), V(0.05f, 0.75f, 0f), 0.16f, 0.2f, Magma, 0.03f));
            vent.Ellipsoid(V(0.05f, 0.42f, 0f), V(0.18f, 0.05f, 0.18f), Glow, 0.03f);
            Crack(vent, V(0.3f, 0.35f, 0.1f), V(0.6f, 0.05f, 0.25f), Ember, 0.04f);
            Crack(vent, V(-0.25f, 0.35f, -0.1f), V(-0.62f, 0.05f, -0.2f), Ember, 0.04f);
            vent.Sphere(V(0.08f, 0.95f, 0f), 0.2f, Ash, 0.15f).noise = 0.1f;
            vent.Sphere(V(0f, 1.3f, 0.06f), 0.26f, new Color(0.45f, 0.43f, 0.42f), 0.15f).noise = 0.1f;
            vent.Sphere(V(0.12f, 1.7f, -0.04f), 0.3f, new Color(0.55f, 0.53f, 0.52f), 0.15f).noise = 0.1f;
            return vent;
        }

        public static Sculpt Cactus(int seed)
        {
            var cactus = new Sculpt();
            var random = new System.Random(seed + 210);
            float height = 3.6f + (float)random.NextDouble() * 1.4f;
            cactus.Limb(V(0f, -0.1f, 0f), V(0f, height, 0f), 0.42f, 0.36f, CactusGreen, 0.08f);
            int arms = 1 + (seed + 1) % 3;
            for (int i = 0; i < arms; i++)
            {
                float side = i % 2 == 0 ? 1f : -1f;
                float y = Range(random, height * 0.35f, height * 0.6f);
                float out1 = Range(random, 0.8f, 1.1f);
                float up = Range(random, 0.9f, 1.6f);
                float angle = i * 1.1f + seed * 0.5f;
                Vector3 dir = V(Mathf.Cos(angle) * side, 0f, Mathf.Sin(angle) * 0.4f).normalized;
                Vector3 elbow = V(0f, y, 0f) + dir * out1;
                cactus.Limb(V(0f, y, 0f), elbow, 0.26f, 0.24f, CactusGreen, 0.08f);
                cactus.Limb(elbow, elbow + V(0f, up, 0f), 0.24f, 0.21f, CactusGreen, 0.08f);
                if ((seed + i) % 2 == 0)
                {
                    cactus.Sphere(elbow + V(0f, up + 0.18f, 0f), 0.12f, new Color(0.98f, 0.45f, 0.6f), 0.04f);
                }
            }
            for (int r = 0; r < 8; r++)
            {
                float angle = r * Mathf.PI / 4f;
                Paint(cactus, cactus.Limb(V(Mathf.Cos(angle) * 0.4f, 0.1f, Mathf.Sin(angle) * 0.4f), V(Mathf.Cos(angle) * 0.34f, height - 0.1f, Mathf.Sin(angle) * 0.34f), 0.05f, 0.04f, CactusDark, 0.02f));
            }
            cactus.Sphere(V(0f, height + 0.25f, 0f), 0.14f, new Color(0.98f, 0.82f, 0.25f), 0.04f);
            cactus.Ellipsoid(V(0f, -0.05f, 0f), V(0.8f, 0.15f, 0.8f), SandDark, 0.2f).noise = 0.04f;
            return cactus;
        }

        public static Sculpt Tumbleweed(int seed)
        {
            var weed = new Sculpt();
            var random = new System.Random(seed + 230);
            float radius = 0.55f + (float)random.NextDouble() * 0.2f;
            Vector3 center = V(0f, radius, 0f);
            for (int i = 0; i < 18; i++)
            {
                Vector3 a = center + Direction(random) * radius * 0.95f;
                Vector3 b = center + Direction(random) * radius * 0.95f;
                Color color = Color.Lerp(Straw, SandDark, (float)random.NextDouble());
                weed.Limb(a, b, 0.035f, 0.025f, color, 0.03f);
            }
            weed.Sphere(center, radius * 0.35f, Straw, 0.15f).noise = 0.1f;
            return weed;
        }

        static Vector3 Direction(System.Random random)
        {
            Vector3 v = V(Range(random, -1f, 1f), Range(random, -1f, 1f), Range(random, -1f, 1f));
            return v.sqrMagnitude > 0.001f ? v.normalized : Vector3.up;
        }

        public static Sculpt Sandstone(int seed)
        {
            var rock = new Sculpt();
            var random = new System.Random(seed * 19 + 7);
            int blocks = 3 + random.Next(3);
            for (int i = 0; i < blocks; i++)
            {
                Vector3 size = V(Range(random, 1f, 1.8f), Range(random, 0.9f, 1.9f), Range(random, 1f, 1.8f));
                Vector3 center = V(Range(random, -0.5f, 0.5f), Range(random, 0.15f, 0.45f), Range(random, -0.5f, 0.5f));
                Color color = Color.Lerp(Rust, Sand, (float)random.NextDouble() * 0.5f);
                rock.Box(center, size, 0.12f, color, 0.08f, V(Range(random, -8f, 8f), Range(random, 0f, 360f), Range(random, -8f, 8f))).noise = 0.03f;
            }
            for (int i = 0; i < 4; i++)
            {
                float y = 0.2f + i * 0.32f;
                Color stripe = i % 2 == 0 ? RustDark : Sand;
                Paint(rock, rock.Box(V(0f, y, 0f), V(4f, 0.1f, 4f), 0f, stripe, 0.05f));
            }
            Paint(rock, rock.Ellipsoid(V(0f, 1.3f, 0f), V(1.2f, 0.25f, 1.2f), Sand, 0.2f));
            return rock;
        }

        public static Sculpt CactusStrip()
        {
            var sculpt = new Sculpt();
            var random = new System.Random(41);
            sculpt.Ellipsoid(V(0f, -0.05f, 0f), V(0.5f, 0.2f, 1.0f), SandDark, 0.15f).noise = 0.03f;
            for (int i = 0; i < 6; i++)
            {
                float z = -0.8f + i * 0.32f;
                float x = (i % 2 == 0 ? 1f : -1f) * Range(random, 0.05f, 0.25f);
                float height = Range(random, 0.55f, 1f);
                Vector3 root = V(x, 0f, z);
                sculpt.Limb(root, root + V(0f, height, 0f), 0.13f, 0.1f, Color.Lerp(CactusGreen, CactusDark, (float)random.NextDouble()), 0.04f);
                if (i % 2 == 1)
                {
                    float side = x > 0f ? 1f : -1f;
                    Vector3 elbow = root + V(side * 0.25f, height * 0.55f, 0f);
                    sculpt.Limb(root + V(0f, height * 0.5f, 0f), elbow, 0.07f, 0.07f, CactusGreen, 0.03f);
                    sculpt.Limb(elbow, elbow + V(0f, 0.25f, 0f), 0.07f, 0.06f, CactusGreen, 0.03f);
                }
                for (int k = 0; k < 4; k++)
                {
                    float angle = k * Mathf.PI * 0.5f + i;
                    float y = Range(random, 0.15f, height * 0.9f);
                    Vector3 spineRoot = root + V(Mathf.Cos(angle) * 0.11f, y, Mathf.Sin(angle) * 0.11f);
                    sculpt.Limb(spineRoot, spineRoot + V(Mathf.Cos(angle) * 0.14f, 0.04f, Mathf.Sin(angle) * 0.14f), 0.018f, 0.004f, Bone, 0.005f);
                }
            }
            return sculpt;
        }

        public static Sculpt Boulder()
        {
            var boulder = new Sculpt();
            boulder.Sphere(V(0f, 0.75f, 0f), 0.72f, Rust, 0.05f).noise = 0.1f;
            boulder.Ellipsoid(V(0.25f, 1.05f, -0.1f), V(0.5f, 0.45f, 0.5f), Color.Lerp(Rust, Sand, 0.4f), 0.15f).noise = 0.08f;
            boulder.Ellipsoid(V(-0.3f, 0.5f, 0.2f), V(0.45f, 0.4f, 0.45f), RustDark, 0.15f).noise = 0.08f;
            for (int i = 0; i < 3; i++)
            {
                float y = 0.45f + i * 0.35f;
                Paint(boulder, boulder.Box(V(0f, y, 0f), V(2f, 0.08f, 2f), 0f, i % 2 == 0 ? RustDark : Sand, 0.04f, V(8f, 0f, 5f)));
            }
            boulder.Sphere(V(0f, 1.7f, 0f), 0.22f, SandDark, 0.2f).noise = 0.1f;
            boulder.Sphere(V(0.15f, 2.15f, 0.05f), 0.15f, Sand, 0.2f).noise = 0.1f;
            return boulder;
        }

        public static Sculpt CowSkull()
        {
            var skull = new Sculpt();
            Color shade = new Color(0.78f, 0.74f, 0.64f);
            skull.Ellipsoid(V(0f, 0.08f, 0f), V(0.11f, 0.08f, 0.13f), Bone, 0.03f);
            skull.Limb(V(0f, 0.07f, 0.08f), V(0f, 0.04f, 0.24f), 0.07f, 0.05f, Bone, 0.04f);
            skull.Mirror(s => skull.Limb(V(s * 0.09f, 0.12f, -0.03f), V(s * 0.26f, 0.15f, -0.06f), 0.03f, 0.022f, Bone, 0.02f));
            skull.Mirror(s => skull.Limb(V(s * 0.26f, 0.15f, -0.06f), V(s * 0.32f, 0.26f, -0.02f), 0.022f, 0.006f, Bone, 0.01f));
            skull.Mirror(s => Carve(skull, skull.Sphere(V(s * 0.06f, 0.12f, 0.07f), 0.03f, Ink, 0.01f)));
            skull.Mirror(s => Carve(skull, skull.Sphere(V(s * 0.022f, 0.06f, 0.25f), 0.012f, Ink, 0.005f)));
            Paint(skull, skull.Ellipsoid(V(0f, 0.01f, 0f), V(0.12f, 0.03f, 0.2f), shade, 0.03f));
            return skull;
        }

        public static Sculpt RockArch()
        {
            var arch = new Sculpt();
            const float span = 2.2f;
            const float height = 3.2f;
            arch.Mirror(s => arch.Limb(V(s * span, -0.1f, 0f), V(s * span * 0.85f, height * 0.75f, 0f), 0.85f, 0.6f, Rust, 0.25f));
            const int segments = 6;
            for (int i = 0; i < segments; i++)
            {
                float a = Mathf.PI * i / segments;
                float b = Mathf.PI * (i + 1) / segments;
                Vector3 p = V(Mathf.Cos(a) * span * 0.85f, height * 0.75f + Mathf.Sin(a) * 0.8f, 0f);
                Vector3 q = V(Mathf.Cos(b) * span * 0.85f, height * 0.75f + Mathf.Sin(b) * 0.8f, 0f);
                arch.Limb(p, q, 0.55f, 0.55f, Color.Lerp(Rust, RustDark, i % 2 * 0.5f), 0.25f).noise = 0.06f;
            }
            arch.Ellipsoid(V(0f, 0f, 0f), V(span + 1f, 0.25f, 1.2f), SandDark, 0.3f).noise = 0.05f;
            for (int i = 0; i < 5; i++)
            {
                float y = 0.4f + i * 0.7f;
                Paint(arch, arch.Box(V(0f, y, 0f), V(8f, 0.14f, 3f), 0f, i % 2 == 0 ? RustDark : Sand, 0.08f));
            }
            return arch;
        }

        public static Sculpt Pyramid()
        {
            var pyramid = new Sculpt();
            const int steps = 5;
            for (int i = 0; i < steps; i++)
            {
                float width = Mathf.Lerp(3.2f, 0.7f, i / (float)(steps - 1));
                Color color = i % 2 == 0 ? Sand : SandDark;
                pyramid.Box(V(0f, 0.25f + i * 0.5f, 0f), V(width, 0.5f, width), 0.04f, color, 0.02f).noise = 0.01f;
            }
            pyramid.Limb(V(0f, 2.6f, 0f), V(0f, 3.1f, 0f), 0.35f, 0.02f, new Color(0.98f, 0.82f, 0.3f), 0.03f);
            Carve(pyramid, pyramid.Box(V(0f, 0.35f, 1.55f), V(0.45f, 0.6f, 0.4f), 0.02f, new Color(0.25f, 0.18f, 0.12f), 0.01f));
            return pyramid;
        }
    }
}
