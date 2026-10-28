using UnityEngine;

namespace FartRafters.EditorTools.Sculpt
{
    public static class NightSwampSculptor
    {
        public const float MossLogLength = 6.2f;

        static readonly Color Ink = new Color(0.08f, 0.06f, 0.07f);
        static readonly Color Wood = new Color(0.45f, 0.3f, 0.18f);
        static readonly Color WoodDark = new Color(0.3f, 0.2f, 0.12f);
        static readonly Color Moss = new Color(0.32f, 0.52f, 0.2f);
        static readonly Color MossLight = new Color(0.48f, 0.66f, 0.28f);
        static readonly Color LanternGlow = new Color(1f, 0.86f, 0.3f);
        static readonly Color Stone = new Color(0.5f, 0.5f, 0.52f);

        static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        static SdfShape Paint(SdfShape shape)
        {
            shape.op = SdfOp.Paint;
            return shape;
        }

        static SdfShape Carve(SdfShape shape)
        {
            shape.op = SdfOp.Subtract;
            return shape;
        }

        static float Range(System.Random random, float min, float max)
        {
            return min + (max - min) * (float)random.NextDouble();
        }

        public static Sculpt LanternPost()
        {
            var post = new Sculpt();
            Color iron = new Color(0.18f, 0.18f, 0.2f);
            post.Box(V(0f, 0.1f, 0f), V(0.5f, 0.2f, 0.5f), 0.04f, Stone, 0.03f).noise = 0.02f;
            post.Box(V(0f, 1.05f, 0f), V(0.14f, 1.9f, 0.14f), 0.02f, Wood, 0.02f).noise = 0.01f;
            post.Box(V(0.28f, 1.92f, 0f), V(0.62f, 0.1f, 0.1f), 0.02f, Wood, 0.02f);
            post.Limb(V(0.05f, 1.6f, 0f), V(0.4f, 1.9f, 0f), 0.035f, 0.035f, WoodDark, 0.02f);
            post.Limb(V(0.5f, 1.88f, 0f), V(0.5f, 1.72f, 0f), 0.015f, 0.015f, iron, 0.01f);
            post.Box(V(0.5f, 1.68f, 0f), V(0.22f, 0.05f, 0.22f), 0.015f, iron, 0.01f);
            post.Box(V(0.5f, 1.52f, 0f), V(0.18f, 0.26f, 0.18f), 0.03f, LanternGlow, 0.01f);
            post.Mirror(s => post.Box(V(0.5f + s * 0.09f, 1.52f, 0f), V(0.025f, 0.28f, 0.2f), 0.005f, iron, 0.005f));
            post.Mirror(s => post.Box(V(0.5f, 1.52f, s * 0.09f), V(0.2f, 0.28f, 0.025f), 0.005f, iron, 0.005f));
            post.Box(V(0.5f, 1.37f, 0f), V(0.2f, 0.04f, 0.2f), 0.01f, iron, 0.01f);
            post.Limb(V(0.5f, 1.71f, 0f), V(0.5f, 1.78f, 0f), 0.11f, 0.02f, iron, 0.01f);
            return post;
        }

        public static Sculpt Campfire()
        {
            var fire = new Sculpt();
            var random = new System.Random(31);
            Color ash = new Color(0.22f, 0.2f, 0.2f);
            Color flame = new Color(1f, 0.5f, 0.08f);
            Color core = new Color(1f, 0.88f, 0.3f);
            fire.Ellipsoid(V(0f, 0.02f, 0f), V(0.55f, 0.06f, 0.55f), ash, 0.05f);
            for (int i = 0; i < 9; i++)
            {
                float angle = i * Mathf.PI * 2f / 9f;
                float r = Range(random, 0.11f, 0.15f);
                fire.Ellipsoid(V(Mathf.Cos(angle) * 0.62f, 0.08f, Mathf.Sin(angle) * 0.62f), V(r * 1.2f, r, r), Color.Lerp(Stone, new Color(0.38f, 0.37f, 0.4f), (float)random.NextDouble()), 0.03f, V(0f, -angle * Mathf.Rad2Deg, 0f)).noise = 0.02f;
            }
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f + 0.4f;
                Vector3 outer = V(Mathf.Cos(angle) * 0.45f, 0.06f, Mathf.Sin(angle) * 0.45f);
                fire.Limb(outer, V(Mathf.Cos(angle) * 0.05f, 0.32f, Mathf.Sin(angle) * 0.05f), 0.07f, 0.06f, Wood, 0.02f);
            }
            fire.Limb(V(0f, 0.15f, 0f), V(0f, 0.75f, 0f), 0.22f, 0.02f, flame, 0.08f);
            fire.Limb(V(0.1f, 0.15f, 0.06f), V(0.16f, 0.6f, 0.1f), 0.14f, 0.015f, flame, 0.08f);
            fire.Limb(V(-0.1f, 0.15f, -0.05f), V(-0.15f, 0.55f, -0.08f), 0.13f, 0.015f, flame, 0.08f);
            fire.Limb(V(0f, 0.18f, 0f), V(0.02f, 0.5f, 0f), 0.12f, 0.015f, core, 0.06f);
            return fire;
        }

        public static Sculpt GlowMushrooms()
        {
            var cluster = new Sculpt();
            var random = new System.Random(57);
            Color stem = new Color(0.92f, 0.9f, 0.82f);
            Color[] caps = { new Color(0.15f, 0.95f, 0.9f), new Color(0.3f, 1f, 0.75f), new Color(0.2f, 0.85f, 1f) };
            Color dot = new Color(0.85f, 1f, 0.98f);
            cluster.Ellipsoid(V(0f, 0f, 0f), V(0.45f, 0.06f, 0.4f), new Color(0.25f, 0.3f, 0.18f), 0.06f).noise = 0.03f;
            int count = 5;
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2.4f;
                float spread = i == 0 ? 0f : Range(random, 0.18f, 0.34f);
                float height = i == 0 ? 0.42f : Range(random, 0.16f, 0.32f);
                float capRadius = height * Range(random, 0.55f, 0.7f);
                Vector3 foot = V(Mathf.Cos(angle) * spread, 0f, Mathf.Sin(angle) * spread);
                Vector3 top = foot + V(Mathf.Cos(angle) * 0.04f, height, Mathf.Sin(angle) * 0.04f);
                Color cap = caps[i % caps.Length];
                cluster.Limb(foot, top, capRadius * 0.28f, capRadius * 0.2f, stem, 0.02f);
                cluster.Ellipsoid(top + V(0f, capRadius * 0.15f, 0f), V(capRadius, capRadius * 0.55f, capRadius), cap, 0.02f);
                Carve(cluster.Ellipsoid(top - V(0f, capRadius * 0.15f, 0f), V(capRadius * 0.85f, capRadius * 0.3f, capRadius * 0.85f), stem, 0.01f));
                for (int k = 0; k < 3; k++)
                {
                    float a = k * 2.1f + i;
                    Paint(cluster.Sphere(top + V(Mathf.Cos(a) * capRadius * 0.55f, capRadius * 0.55f, Mathf.Sin(a) * capRadius * 0.55f), capRadius * 0.14f, dot, 0.01f));
                }
            }
            return cluster;
        }

        public static Sculpt OwlStump()
        {
            var stump = new Sculpt();
            Color bark = new Color(0.4f, 0.27f, 0.16f);
            Color ring = new Color(0.78f, 0.62f, 0.4f);
            Color feather = new Color(0.55f, 0.4f, 0.26f);
            Color belly = new Color(0.86f, 0.76f, 0.58f);
            Color eye = new Color(1f, 0.85f, 0.15f);
            Color beak = new Color(0.95f, 0.65f, 0.15f);
            stump.Cylinder(V(0f, 0.4f, 0f), 0.42f, 0.4f, 0.04f, bark, 0.03f, V(90f, 0f, 0f)).noise = 0.03f;
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f + 0.3f;
                stump.Limb(V(Mathf.Cos(angle) * 0.3f, 0.15f, Mathf.Sin(angle) * 0.3f), V(Mathf.Cos(angle) * 0.62f, 0f, Mathf.Sin(angle) * 0.62f), 0.13f, 0.06f, bark, 0.05f);
            }
            Paint(stump.Ellipsoid(V(0f, 0.8f, 0f), V(0.38f, 0.03f, 0.38f), ring, 0.01f));
            Paint(stump.Torus(V(0f, 0.81f, 0f), 0.2f, 0.012f, bark, 0.005f));
            Paint(stump.Ellipsoid(V(0.2f, 0.45f, 0.38f), V(0.16f, 0.2f, 0.06f), Moss, 0.04f));
            stump.Ellipsoid(V(0f, 1.12f, 0f), V(0.24f, 0.3f, 0.22f), feather, 0.05f);
            Paint(stump.Ellipsoid(V(0f, 1.06f, 0.14f), V(0.17f, 0.22f, 0.1f), belly, 0.04f));
            stump.Sphere(V(0f, 1.46f, 0f), 0.2f, feather, 0.06f);
            stump.Mirror(s => stump.Limb(V(s * 0.12f, 1.58f, 0f), V(s * 0.18f, 1.72f, -0.02f), 0.05f, 0.015f, feather, 0.03f));
            stump.Mirror(s => stump.Ellipsoid(V(s * 0.22f, 1.12f, -0.02f), V(0.06f, 0.24f, 0.16f), feather, 0.04f, V(0f, 0f, s * 8f)));
            stump.Mirror(s => Paint(stump.Sphere(V(s * 0.085f, 1.49f, 0.16f), 0.075f, eye, 0.01f)));
            stump.Mirror(s => Paint(stump.Sphere(V(s * 0.085f, 1.49f, 0.2f), 0.035f, Ink, 0.005f)));
            stump.Limb(V(0f, 1.43f, 0.18f), V(0f, 1.36f, 0.24f), 0.035f, 0.01f, beak, 0.01f);
            stump.Mirror(s => stump.Limb(V(s * 0.07f, 0.86f, 0.06f), V(s * 0.09f, 0.82f, 0.16f), 0.025f, 0.015f, beak, 0.01f));
            return stump;
        }

        public static Sculpt Tent()
        {
            var tent = new Sculpt();
            Color canvas = new Color(0.86f, 0.45f, 0.18f);
            Color flap = new Color(0.72f, 0.34f, 0.12f);
            Color dark = new Color(0.12f, 0.1f, 0.1f);
            Color pole = new Color(0.55f, 0.55f, 0.58f);
            Color rope = new Color(0.85f, 0.8f, 0.65f);
            tent.Mirror(s => tent.Box(V(s * 0.55f, 0.6f, 0f), V(0.06f, 1.55f, 2f), 0.02f, canvas, 0.02f, V(0f, 0f, s * 45f)));
            Carve(tent.Box(V(0f, -0.5f, 0f), V(4f, 1f, 4f), 0f, canvas, 0.01f));
            tent.Limb(V(0f, 1.16f, -1.05f), V(0f, 1.16f, 1.05f), 0.04f, 0.04f, pole, 0.02f);
            tent.Mirror(s => tent.Limb(V(0f, 0f, s * 1.02f), V(0f, 1.2f, s * 1.02f), 0.03f, 0.03f, pole, 0.01f));
            tent.Box(V(0f, 0.45f, 0.98f), V(0.75f, 0.8f, 0.04f), 0.01f, dark, 0.01f);
            tent.Box(V(0.32f, 0.5f, 1.02f), V(0.3f, 0.9f, 0.04f), 0.01f, flap, 0.01f, V(0f, -25f, 18f));
            tent.Box(V(0f, 0.6f, -0.99f), V(1.3f, 1.1f, 0.04f), 0.01f, flap, 0.01f);
            Carve(tent.Box(V(0f, 0.6f, -0.99f), V(1.6f, 1.6f, 0.2f), 0f, flap, 0.01f, V(0f, 0f, 45f)));
            tent.Mirror(s => tent.Limb(V(s * 0.9f, 0.32f, 0.8f), V(s * 1.45f, 0f, 1.25f), 0.012f, 0.012f, rope, 0.005f));
            tent.Mirror(s => tent.Limb(V(s * 0.9f, 0.32f, -0.8f), V(s * 1.45f, 0f, -1.25f), 0.012f, 0.012f, rope, 0.005f));
            tent.Ellipsoid(V(0f, 0f, 0f), V(1.3f, 0.03f, 1.3f), new Color(0.3f, 0.26f, 0.18f), 0.02f);
            return tent;
        }

        public static Sculpt Willow(int seed)
        {
            var tree = new Sculpt();
            var random = new System.Random(seed * 17 + 5);
            Color bark = new Color(0.33f, 0.27f, 0.2f);
            Color leaf = seed % 2 == 0 ? new Color(0.3f, 0.45f, 0.2f) : new Color(0.34f, 0.48f, 0.24f);
            Color hanging = new Color(0.52f, 0.58f, 0.36f);
            float height = 4.4f + (float)random.NextDouble() * 1.4f;
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f + Range(random, -0.3f, 0.3f);
                tree.Limb(V(Mathf.Cos(angle) * 0.9f, -0.2f, Mathf.Sin(angle) * 0.9f), V(Mathf.Cos(angle) * 0.2f, 1f, Mathf.Sin(angle) * 0.2f), 0.14f, 0.2f, bark, 0.15f).noise = 0.03f;
            }
            Vector3 lean = V(Range(random, -0.4f, 0.4f), 0f, Range(random, -0.4f, 0.4f));
            Vector3 crown = V(0f, height * 0.7f, 0f) + lean;
            tree.Limb(V(0f, 0.8f, 0f), crown, 0.32f, 0.2f, bark, 0.1f).noise = 0.04f;
            int boughs = 5;
            for (int i = 0; i < boughs; i++)
            {
                float angle = i * Mathf.PI * 2f / boughs + Range(random, -0.3f, 0.3f);
                Vector3 dir = V(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 tip = crown + dir * Range(random, 1.3f, 1.8f) + V(0f, Range(random, 0.2f, 0.7f), 0f);
                tree.Limb(crown, tip, 0.13f, 0.06f, bark, 0.08f);
                tree.Ellipsoid(tip + V(0f, 0.25f, 0f), V(1f, 0.6f, 1f), leaf, 0.4f).noise = 0.15f;
                for (int k = 0; k < 3; k++)
                {
                    float a = angle + (k - 1) * 0.45f;
                    Vector3 start = tip + V(Mathf.Cos(a) * 0.7f, -0.1f, Mathf.Sin(a) * 0.7f);
                    float drop = Range(random, 1.4f, 2.4f);
                    tree.Limb(start, start + V(Mathf.Cos(a) * 0.15f, -drop, Mathf.Sin(a) * 0.15f), 0.13f, 0.04f, hanging, 0.12f).noise = 0.04f;
                }
            }
            tree.Ellipsoid(crown + V(0f, 0.6f, 0f), V(1.3f, 0.75f, 1.3f), leaf, 0.4f).noise = 0.15f;
            Paint(tree.Ellipsoid(V(0f, 0.5f, 0f), V(0.6f, 0.35f, 0.6f), Moss, 0.15f));
            return tree;
        }

        public static Sculpt Cattails(int seed)
        {
            var clump = new Sculpt();
            var random = new System.Random(seed * 11 + 9);
            Color reed = new Color(0.42f, 0.56f, 0.24f);
            Color reedDark = new Color(0.32f, 0.46f, 0.2f);
            Color head = new Color(0.4f, 0.24f, 0.13f);
            clump.Ellipsoid(V(0f, 0f, 0f), V(0.55f, 0.1f, 0.5f), new Color(0.28f, 0.24f, 0.16f), 0.08f).noise = 0.04f;
            int stalks = 7 + random.Next(4);
            for (int i = 0; i < stalks; i++)
            {
                float angle = i * 2.39f;
                float spread = Range(random, 0f, 0.4f);
                Vector3 foot = V(Mathf.Cos(angle) * spread, 0f, Mathf.Sin(angle) * spread);
                float height = Range(random, 0.9f, 1.5f);
                Vector3 top = foot + V(Mathf.Cos(angle) * height * 0.15f, height, Mathf.Sin(angle) * height * 0.15f);
                Color color = i % 2 == 0 ? reed : reedDark;
                clump.Limb(foot, top, 0.025f, 0.012f, color, 0.02f);
                if (i % 2 == 0)
                {
                    Vector3 headCenter = Vector3.Lerp(foot, top, 0.78f);
                    clump.Limb(headCenter - V(0f, 0.12f, 0f), headCenter + V(0f, 0.12f, 0f), 0.05f, 0.05f, head, 0.02f);
                }
                else
                {
                    Vector3 bend = top + V(Mathf.Cos(angle + 1f) * 0.3f, -0.25f, Mathf.Sin(angle + 1f) * 0.3f);
                    clump.Limb(Vector3.Lerp(foot, top, 0.55f), bend, 0.02f, 0.006f, color, 0.02f);
                }
            }
            return clump;
        }

        public static Sculpt MossRock(int seed)
        {
            var rock = new Sculpt();
            var random = new System.Random(seed * 19 + 4);
            Color stone = new Color(0.42f, 0.42f, 0.38f);
            Color stoneDark = new Color(0.32f, 0.33f, 0.3f);
            int blobs = 3 + random.Next(2);
            for (int i = 0; i < blobs; i++)
            {
                Vector3 radii = V(Range(random, 0.7f, 1f), Range(random, 0.5f, 0.85f), Range(random, 0.7f, 1f));
                Vector3 center = V(Range(random, -0.45f, 0.45f), Range(random, 0.2f, 0.45f), Range(random, -0.45f, 0.45f));
                rock.Ellipsoid(center, radii, Color.Lerp(stone, stoneDark, (float)random.NextDouble()), 0.15f, V(Range(random, -20f, 20f), Range(random, 0f, 360f), Range(random, -20f, 20f))).noise = 0.12f;
            }
            Paint(rock.Ellipsoid(V(0f, 1.05f, 0f), V(1.2f, 0.45f, 1.2f), Moss, 0.25f)).noise = 0.2f;
            Paint(rock.Ellipsoid(V(0.3f, 0.9f, 0.2f), V(0.5f, 0.25f, 0.5f), MossLight, 0.15f)).noise = 0.15f;
            return rock;
        }

        public static Sculpt StiltShack()
        {
            var shack = new Sculpt();
            Color plank = new Color(0.5f, 0.38f, 0.25f);
            Color roof = new Color(0.36f, 0.3f, 0.22f);
            Color dark = new Color(0.12f, 0.1f, 0.09f);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    shack.Limb(V(x * 0.85f, -0.4f, z * 0.7f), V(x * 0.85f, 1f, z * 0.7f), 0.08f, 0.08f, WoodDark, 0.02f).noise = 0.02f;
                }
            }
            shack.Box(V(0f, 1f, 0f), V(2.2f, 0.1f, 1.9f), 0.02f, plank, 0.02f);
            shack.Box(V(0f, 1.65f, 0f), V(1.7f, 1.2f, 1.4f), 0.03f, plank, 0.02f).noise = 0.015f;
            for (int i = 0; i < 5; i++)
            {
                Paint(shack.Box(V(0f, 1.15f + i * 0.24f, 0.71f), V(1.72f, 0.02f, 0.04f), 0f, WoodDark, 0.01f));
                Paint(shack.Box(V(0f, 1.15f + i * 0.24f, -0.71f), V(1.72f, 0.02f, 0.04f), 0f, WoodDark, 0.01f));
            }
            shack.Box(V(-0.35f, 1.55f, 0.72f), V(0.45f, 0.8f, 0.05f), 0.01f, dark, 0.01f);
            shack.Box(V(0.45f, 1.85f, 0.72f), V(0.35f, 0.3f, 0.05f), 0.01f, LanternGlow, 0.01f);
            shack.Mirror(s => shack.Box(V(s * 0.55f, 2.45f, 0f), V(1.35f, 0.08f, 1.8f), 0.02f, roof, 0.02f, V(0f, 0f, s * -28f)));
            shack.Limb(V(-0.35f, 1.05f, 1.05f), V(-0.35f, -0.3f, 1.8f), 0.05f, 0.05f, WoodDark, 0.02f);
            shack.Limb(V(-0.05f, 1.05f, 1.05f), V(-0.05f, -0.3f, 1.8f), 0.05f, 0.05f, WoodDark, 0.02f);
            for (int i = 0; i < 4; i++)
            {
                float t = (i + 0.5f) / 4f;
                shack.Box(Vector3.Lerp(V(-0.2f, 1.02f, 1.05f), V(-0.2f, -0.28f, 1.8f), t), V(0.4f, 0.04f, 0.14f), 0.01f, plank, 0.01f);
            }
            Paint(shack.Ellipsoid(V(0.6f, 2.6f, 0.3f), V(0.5f, 0.15f, 0.6f), Moss, 0.1f));
            return shack;
        }

        public static Sculpt MossyLog()
        {
            var log = new Sculpt();
            Color bark = new Color(0.38f, 0.27f, 0.17f);
            Color wood = new Color(0.78f, 0.62f, 0.42f);
            Color cap = new Color(0.86f, 0.42f, 0.2f);
            Color stem = new Color(0.92f, 0.88f, 0.78f);
            log.Cylinder(V(0f, 0.3f, 0f), 0.32f, 0.95f, 0.05f, bark, 0.02f, V(0f, 90f, 0f)).noise = 0.03f;
            log.Mirror(s => Paint(log.Ellipsoid(V(s * 0.96f, 0.3f, 0f), V(0.04f, 0.27f, 0.27f), wood, 0.02f)));
            Paint(log.Ellipsoid(V(-0.1f, 0.6f, 0f), V(0.8f, 0.12f, 0.3f), Moss, 0.08f)).noise = 0.08f;
            Paint(log.Ellipsoid(V(0.4f, 0.55f, 0.15f), V(0.3f, 0.1f, 0.2f), MossLight, 0.05f));
            for (int i = 0; i < 4; i++)
            {
                float x = -0.6f + i * 0.38f;
                float z = i % 2 == 0 ? 0.28f : -0.26f;
                float h = 0.14f + (i % 3) * 0.04f;
                log.Limb(V(x, 0.45f, z * 0.8f), V(x, 0.45f + h, z), 0.025f, 0.02f, stem, 0.01f);
                log.Ellipsoid(V(x, 0.47f + h, z), V(0.09f, 0.05f, 0.09f), cap, 0.01f);
            }
            return log;
        }

        public static Sculpt MossLog()
        {
            var log = new Sculpt();
            Color bark = new Color(0.36f, 0.27f, 0.17f);
            Color dark = new Color(0.26f, 0.19f, 0.12f);
            Color wood = new Color(0.8f, 0.64f, 0.42f);
            float half = MossLogLength * 0.5f - 0.3f;
            log.Cylinder(V(0f, 0f, 0f), 0.52f, half, 0.08f, bark, 0.03f).noise = 0.03f;
            log.Mirror(s => Paint(log.Ellipsoid(V(0f, 0f, s * (half + 0.02f)), V(0.46f, 0.46f, 0.08f), wood, 0.02f)));
            log.Mirror(s => Paint(log.Ellipsoid(V(0f, 0f, s * (half + 0.03f)), V(0.3f, 0.3f, 0.09f), new Color(0.66f, 0.5f, 0.32f), 0.012f)));
            log.Limb(V(0.3f, 0.35f, -1.1f), V(0.85f, 1f, -1.5f), 0.14f, 0.05f, dark, 0.04f);
            log.Limb(V(-0.35f, 0.3f, 1.5f), V(-0.75f, 0.8f, 2f), 0.12f, 0.04f, dark, 0.04f);
            Paint(log.Ellipsoid(V(0f, 0.45f, 0f), V(0.55f, 0.18f, half * 0.85f), Moss, 0.08f)).noise = 0.1f;
            Paint(log.Ellipsoid(V(0.25f, 0.4f, -1.4f), V(0.35f, 0.15f, 0.6f), MossLight, 0.06f));
            Paint(log.Ellipsoid(V(-0.3f, 0.38f, 1.1f), V(0.3f, 0.14f, 0.5f), MossLight, 0.06f));
            log.Limb(V(0.4f, -0.1f, 0.6f), V(0.52f, -0.7f, 0.7f), 0.08f, 0.03f, new Color(0.5f, 0.56f, 0.32f), 0.05f);
            log.Limb(V(-0.42f, -0.1f, -0.9f), V(-0.55f, -0.65f, -1f), 0.07f, 0.03f, new Color(0.5f, 0.56f, 0.32f), 0.05f);
            return log;
        }
    }
}
