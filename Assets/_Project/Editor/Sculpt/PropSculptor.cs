using UnityEngine;

namespace FartRafters.EditorTools.Sculpt
{
    public static class PropSculptor
    {
        static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        public static Sculpt Raft()
        {
            var raft = new Sculpt();
            Color bark = new Color(0.47f, 0.32f, 0.2f);
            Color barkDark = new Color(0.36f, 0.24f, 0.15f);
            Color wood = new Color(0.86f, 0.7f, 0.48f);
            Color ring = new Color(0.68f, 0.5f, 0.32f);
            Color rope = new Color(0.8f, 0.68f, 0.45f);
            float[] lengths = { 1.56f, 1.48f, 1.6f, 1.5f, 1.55f };
            float[] radii = { 0.29f, 0.3f, 0.31f, 0.29f, 0.3f };

            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.6f;
                float half = lengths[i];
                float offset = (i % 2 == 0 ? 0.04f : -0.03f);
                SdfShape log = raft.Cylinder(V(x, -0.1f, offset), radii[i], half, 0.04f, i % 2 == 0 ? bark : barkDark, 0.01f);
                log.noise = 0.025f;
                for (int e = -1; e <= 1; e += 2)
                {
                    float z = (e < 0 ? -half : half) + offset;
                    Paint(raft.Ellipsoid(V(x, -0.1f, z), V(radii[i] * 0.9f, radii[i] * 0.9f, 0.08f), wood, 0.02f));
                    Paint(raft.Ellipsoid(V(x, -0.1f, z), V(radii[i] * 0.55f, radii[i] * 0.55f, 0.09f), ring, 0.012f));
                    Paint(raft.Ellipsoid(V(x, -0.1f, z), V(radii[i] * 0.25f, radii[i] * 0.25f, 0.1f), wood, 0.01f));
                }
            }

            for (int b = -1; b <= 1; b += 2)
            {
                float z = b * 1.05f;
                for (int i = 0; i < 5; i++)
                {
                    float x = (i - 2) * 0.6f;
                    raft.Torus(V(x, -0.1f, z), radii[i] + 0.02f, 0.035f, rope, 0.01f, V(90f, 0f, 0f));
                    raft.Torus(V(x, -0.1f, z + 0.08f), radii[i] + 0.02f, 0.032f, rope, 0.01f, V(90f, 0f, 0f));
                }
                raft.Limb(V(-1.35f, 0.22f, z + 0.04f), V(1.35f, 0.22f, z + 0.04f), 0.04f, 0.04f, rope, 0.015f);
            }
            return raft;
        }

        public static Sculpt Crocodile()
        {
            var croc = new Sculpt();
            Color skin = new Color(0.34f, 0.55f, 0.26f);
            Color dark = new Color(0.2f, 0.38f, 0.16f);
            Color belly = new Color(0.86f, 0.82f, 0.52f);
            Color tooth = new Color(0.98f, 0.96f, 0.88f);
            Color eye = new Color(0.98f, 0.86f, 0.3f);
            Color ink = new Color(0.08f, 0.06f, 0.05f);

            croc.Ellipsoid(V(0f, 0f, -0.08f), V(0.42f, 0.28f, 0.36f), skin, 0.08f);
            croc.Ellipsoid(V(0f, 0.0f, 0.22f), V(0.3f, 0.2f, 0.2f), skin, 0.08f);
            croc.Ellipsoid(V(0f, -0.04f, 0.42f), V(0.19f, 0.1f, 0.14f), skin, 0.06f);
            croc.Limb(V(0f, 0f, -0.3f), V(0f, 0.03f, -0.58f), 0.22f, 0.05f, skin, 0.08f);
            Paint(croc.Ellipsoid(V(0f, -0.3f, -0.02f), V(0.38f, 0.12f, 0.5f), belly, 0.08f));
            for (int i = 0; i < 7; i++)
            {
                float z = Mathf.Lerp(-0.48f, 0.12f, i / 6f);
                float y = Mathf.Lerp(0.13f, 0.26f, Mathf.Sin(i / 6f * Mathf.PI));
                croc.Mirror(s => croc.Ellipsoid(V(s * 0.07f, y, z), V(0.045f, 0.05f, 0.05f), dark, 0.02f));
            }
            croc.Mirror(s => croc.Sphere(V(s * 0.12f, 0.15f, 0.28f), 0.075f, skin, 0.04f));
            croc.Mirror(s => Paint(croc.Sphere(V(s * 0.13f, 0.19f, 0.32f), 0.05f, eye, 0.012f)));
            croc.Mirror(s => Paint(croc.Ellipsoid(V(s * 0.13f, 0.195f, 0.355f), V(0.012f, 0.035f, 0.02f), ink, 0.006f)));
            croc.Mirror(s => croc.Sphere(V(s * 0.05f, 0.04f, 0.54f), 0.03f, skin, 0.02f));
            croc.Mirror(s => Carve(croc.Sphere(V(s * 0.05f, 0.06f, 0.56f), 0.016f, ink, 0.006f)));
            croc.Mirror(s => Paint(croc.Limb(V(s * 0.19f, -0.06f, 0.24f), V(s * 0.12f, -0.07f, 0.55f), 0.012f, 0.012f, ink, 0.008f)));
            for (int i = 0; i < 5; i++)
            {
                float z = Mathf.Lerp(0.28f, 0.52f, i / 4f);
                float x = Mathf.Lerp(0.18f, 0.12f, i / 4f);
                croc.Mirror(s => croc.Limb(V(s * x, -0.06f, z), V(s * x, -0.12f, z + 0.01f), 0.018f, 0.004f, tooth, 0.005f));
            }
            for (int sz = -1; sz <= 1; sz += 2)
            {
                float z = sz * 0.2f - 0.06f;
                croc.Mirror(s => croc.Limb(V(s * 0.3f, -0.12f, z), V(s * 0.46f, -0.24f, z + 0.05f), 0.08f, 0.06f, skin, 0.05f));
            }
            return croc;
        }

        public static Sculpt Buoy()
        {
            var buoy = new Sculpt();
            Color red = new Color(0.92f, 0.24f, 0.18f);
            Color white = new Color(0.97f, 0.96f, 0.92f);
            buoy.Ellipsoid(V(0f, 0f, 0f), V(0.46f, 0.36f, 0.46f), red, 0.02f);
            Paint(buoy.Torus(V(0f, 0.02f, 0f), 0.45f, 0.07f, white, 0.015f));
            buoy.Limb(V(0f, 0.28f, 0f), V(0f, 0.44f, 0f), 0.13f, 0.1f, white, 0.04f);
            buoy.Torus(V(0f, 0.45f, 0f), 0.09f, 0.025f, red, 0.01f);
            return buoy;
        }

        public static Sculpt Bramble()
        {
            var bramble = new Sculpt();
            Color vine = new Color(0.26f, 0.22f, 0.12f);
            Color leaf = new Color(0.18f, 0.3f, 0.12f);
            Color thorn = new Color(0.9f, 0.82f, 0.62f);
            Color berry = new Color(0.25f, 0.1f, 0.25f);
            var random = new System.Random(5);

            for (int i = 0; i < 6; i++)
            {
                float z = -0.8f + i * 0.32f;
                float lean = (i % 2 == 0 ? 1f : -1f) * 0.25f;
                Vector3 a = V(lean * 0.4f, 0f, z);
                Vector3 b = V(lean, 0.75f + (float)random.NextDouble() * 0.25f, z + 0.25f);
                Vector3 c = V(lean * 1.3f, 0.35f, z + 0.55f);
                bramble.Limb(a, b, 0.09f, 0.06f, vine, 0.04f);
                bramble.Limb(b, c, 0.06f, 0.04f, vine, 0.03f);
                for (int t = 0; t < 7; t++)
                {
                    float f = (t + 0.5f) / 7f;
                    Vector3 p = t < 2 ? Vector3.Lerp(a, b, f * 2f) : Vector3.Lerp(b, c, f * 2f - 1f);
                    Vector3 dir = new Vector3((float)random.NextDouble() - 0.5f, (float)random.NextDouble() * 0.6f, (float)random.NextDouble() - 0.5f).normalized;
                    bramble.Limb(p, p + dir * 0.3f, 0.05f, 0.004f, thorn, 0.012f);
                }
                bramble.Ellipsoid(b + V(0.08f, 0.05f, 0f), V(0.12f, 0.05f, 0.08f), leaf, 0.03f, V(0f, 30f * i, 20f));
                bramble.Ellipsoid(Vector3.Lerp(a, b, 0.5f) + V(-0.08f, 0f, 0.04f), V(0.1f, 0.04f, 0.07f), leaf, 0.03f, V(0f, -40f * i, -15f));
                if (i % 2 == 1)
                {
                    bramble.Sphere(c + V(0f, 0.05f, 0f), 0.045f, berry, 0.01f);
                }
            }
            bramble.Ellipsoid(V(0f, 0.02f, 0f), V(0.45f, 0.12f, 0.95f), vine, 0.1f);
            return bramble;
        }

        public static Sculpt Beans()
        {
            var pot = new Sculpt();
            Color clay = new Color(0.78f, 0.4f, 0.22f);
            Color clayDark = new Color(0.55f, 0.26f, 0.14f);
            Color sauce = new Color(0.82f, 0.28f, 0.12f);
            Color bean = new Color(0.95f, 0.88f, 0.75f);
            Color ring = new Color(0.98f, 0.62f, 0.25f);
            var random = new System.Random(11);

            pot.Ellipsoid(V(0f, 0.26f, 0f), V(0.4f, 0.28f, 0.4f), clay, 0.03f);
            pot.Torus(V(0f, 0.46f, 0f), 0.34f, 0.06f, clayDark, 0.03f);
            Carve(pot.Ellipsoid(V(0f, 0.55f, 0f), V(0.32f, 0.14f, 0.32f), clayDark, 0.03f));
            pot.Ellipsoid(V(0f, 0.43f, 0f), V(0.31f, 0.05f, 0.31f), sauce, 0.02f);
            for (int i = 0; i < 16; i++)
            {
                float angle = i * 2.4f;
                float radius = 0.24f * Mathf.Sqrt((i + 0.5f) / 16f);
                Vector3 p = V(Mathf.Cos(angle) * radius, 0.47f, Mathf.Sin(angle) * radius);
                pot.Ellipsoid(p, V(0.055f, 0.03f, 0.035f), bean, 0.01f, V(0f, (float)random.NextDouble() * 180f, 0f));
            }
            pot.Mirror(s => pot.Torus(V(s * 0.42f, 0.36f, 0f), 0.06f, 0.02f, clayDark, 0.01f, V(0f, 0f, 90f)));
            pot.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return pot;
        }

        public static Sculpt Pastirma()
        {
            var plate = new Sculpt();
            Color board = new Color(0.62f, 0.42f, 0.24f);
            Color meat = new Color(0.6f, 0.14f, 0.12f);
            Color fat = new Color(0.95f, 0.85f, 0.78f);
            Color crust = new Color(0.72f, 0.32f, 0.1f);
            Color ring = new Color(0.95f, 0.32f, 0.3f);

            plate.Box(V(0f, 0.05f, 0f), V(0.9f, 0.08f, 0.56f), 0.03f, board, 0.01f);
            for (int i = 0; i < 3; i++)
            {
                float y = 0.13f + i * 0.055f;
                var euler = V(0f, -18f + i * 22f, 4f - i * 3f);
                plate.Box(V(-0.05f + i * 0.06f, y, -0.02f + i * 0.03f), V(0.62f, 0.04f, 0.34f), 0.015f, meat, 0.008f, euler);
                Paint(plate.Box(V(-0.05f + i * 0.06f, y, -0.02f + i * 0.03f), V(0.66f, 0.06f, 0.38f), 0.015f, crust, 0.012f, euler));
                Paint(plate.Box(V(-0.05f + i * 0.06f, y, -0.02f + i * 0.03f), V(0.56f, 0.07f, 0.28f), 0.015f, meat, 0.01f, euler));
                Paint(plate.Box(V(-0.05f + i * 0.06f + 0.12f, y, -0.02f + i * 0.03f), V(0.04f, 0.07f, 0.26f), 0.01f, fat, 0.01f, euler));
            }
            plate.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return plate;
        }

        public static Sculpt Onion()
        {
            var onion = new Sculpt();
            Color skin = new Color(0.62f, 0.24f, 0.5f);
            Color light = new Color(0.88f, 0.62f, 0.82f);
            Color sprout = new Color(0.45f, 0.72f, 0.3f);
            Color root = new Color(0.86f, 0.8f, 0.62f);
            Color ring = new Color(0.78f, 0.52f, 0.98f);

            onion.Ellipsoid(V(0f, 0.32f, 0f), V(0.32f, 0.3f, 0.32f), skin, 0.02f);
            onion.Limb(V(0f, 0.55f, 0f), V(0f, 0.68f, 0f), 0.09f, 0.04f, skin, 0.05f);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f;
                Paint(onion.Ellipsoid(V(Mathf.Cos(angle) * 0.3f, 0.32f, Mathf.Sin(angle) * 0.3f), V(0.03f, 0.3f, 0.03f), light, 0.02f, V(0f, -angle * Mathf.Rad2Deg, 0f)));
            }
            onion.Limb(V(0f, 0.66f, 0f), V(0.05f, 0.92f, 0.02f), 0.035f, 0.015f, sprout, 0.02f);
            onion.Limb(V(0f, 0.66f, 0f), V(-0.06f, 0.86f, -0.03f), 0.03f, 0.012f, sprout, 0.02f);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 1.25f;
                onion.Limb(V(0f, 0.05f, 0f), V(Mathf.Cos(angle) * 0.09f, -0.02f, Mathf.Sin(angle) * 0.09f), 0.012f, 0.006f, root, 0.01f);
            }
            onion.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return onion;
        }

        public static Sculpt MillWheel()
        {
            var wheel = new Sculpt();
            Color wood = new Color(0.55f, 0.36f, 0.2f);
            Color dark = new Color(0.38f, 0.24f, 0.13f);
            Color iron = new Color(0.3f, 0.3f, 0.32f);
            const float radius = 4f;

            wheel.Torus(Vector3.zero, radius * 0.92f, 0.16f, wood, 0.02f, V(90f, 0f, 0f)).noise = 0.015f;
            wheel.Torus(Vector3.zero, radius * 0.55f, 0.12f, dark, 0.02f, V(90f, 0f, 0f));
            wheel.Cylinder(Vector3.zero, 0.55f, 0.6f, 0.06f, iron, 0.03f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                wheel.Limb(direction * 0.4f, direction * radius * 0.92f, 0.11f, 0.09f, dark, 0.03f);
                Vector3 center = direction * radius * 0.82f;
                wheel.Box(center, new Vector3(radius * 0.4f, 0.18f, 1.5f), 0.04f, wood, 0.02f, V(0f, 0f, angle * Mathf.Rad2Deg)).noise = 0.01f;
            }
            return wheel;
        }

        public static Sculpt Chickpea()
        {
            var bowl = new Sculpt();
            Color ceramic = new Color(0.3f, 0.55f, 0.85f);
            Color pea = new Color(0.93f, 0.78f, 0.45f);
            Color ring = new Color(0.98f, 0.86f, 0.42f);
            var random = new System.Random(17);

            bowl.Ellipsoid(V(0f, 0.2f, 0f), V(0.42f, 0.22f, 0.42f), ceramic, 0.03f);
            Carve(bowl.Ellipsoid(V(0f, 0.36f, 0f), V(0.36f, 0.18f, 0.36f), ceramic, 0.03f));
            Paint(bowl.Torus(V(0f, 0.3f, 0f), 0.42f, 0.03f, Color.white, 0.02f));
            for (int i = 0; i < 22; i++)
            {
                float angle = i * 2.4f;
                float r = 0.3f * Mathf.Sqrt((i + 0.5f) / 22f);
                float lift = (float)random.NextDouble() * 0.06f;
                Vector3 p = V(Mathf.Cos(angle) * r, 0.33f + lift + (0.3f - r) * 0.25f, Mathf.Sin(angle) * r);
                bowl.Sphere(p, 0.05f, pea, 0.008f);
                Paint(bowl.Sphere(p + V(0.03f, 0.03f, 0.01f), 0.012f, new Color(0.8f, 0.6f, 0.3f), 0.004f));
            }
            bowl.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return bowl;
        }

        public static Sculpt Pepper()
        {
            var pepper = new Sculpt();
            Color red = new Color(0.9f, 0.12f, 0.08f);
            Color stem = new Color(0.3f, 0.6f, 0.2f);
            Color ring = new Color(1f, 0.3f, 0.15f);

            Vector3 a = V(-0.32f, 0.22f, 0f);
            Vector3 b = V(0.05f, 0.3f, 0f);
            Vector3 c = V(0.32f, 0.18f, 0.04f);
            Vector3 d = V(0.45f, 0.05f, 0.08f);
            pepper.Limb(a, b, 0.14f, 0.12f, red, 0.06f);
            pepper.Limb(b, c, 0.12f, 0.07f, red, 0.06f);
            pepper.Limb(c, d, 0.07f, 0.015f, red, 0.04f);
            Paint(pepper.Sphere(V(-0.1f, 0.42f, 0.05f), 0.06f, new Color(1f, 0.6f, 0.55f), 0.04f));
            pepper.Sphere(V(-0.4f, 0.22f, 0f), 0.09f, stem, 0.03f);
            pepper.Limb(V(-0.45f, 0.24f, 0f), V(-0.58f, 0.36f, 0.02f), 0.04f, 0.025f, stem, 0.02f);
            pepper.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return pepper;
        }

        public static Sculpt Egg()
        {
            var plate = new Sculpt();
            Color white = new Color(0.98f, 0.97f, 0.93f);
            Color yolk = new Color(1f, 0.72f, 0.12f);
            Color dish = new Color(0.62f, 0.82f, 0.9f);
            Color ring = new Color(0.96f, 0.96f, 0.86f);

            plate.Cylinder(V(0f, 0.04f, 0f), 0.45f, 0.03f, 0.02f, dish, 0.01f, V(90f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 center = V(s * 0.17f, 0.12f, 0f);
                plate.Ellipsoid(center, V(0.15f, 0.09f, 0.21f), white, 0.03f, V(0f, s * 15f, 0f));
                Carve(plate.Ellipsoid(center + V(0f, 0.1f, 0f), V(0.2f, 0.06f, 0.26f), white, 0.01f, V(0f, s * 15f, 0f)));
                plate.Sphere(center + V(0f, 0.04f, 0f), 0.075f, yolk, 0.015f);
            }
            plate.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return plate;
        }

        public static Sculpt RepairKit()
        {
            var kit = new Sculpt();
            Color red = new Color(0.86f, 0.16f, 0.14f);
            Color white = new Color(0.97f, 0.97f, 0.95f);
            Color steel = new Color(0.72f, 0.74f, 0.78f);
            Color ring = new Color(0.4f, 0.95f, 0.45f);

            kit.Box(V(0f, 0.22f, 0f), V(0.7f, 0.36f, 0.42f), 0.06f, red, 0.02f);
            kit.Torus(V(0f, 0.44f, 0f), 0.13f, 0.03f, steel, 0.01f, V(90f, 0f, 0f));
            for (int side = -1; side <= 1; side += 2)
            {
                Paint(kit.Box(V(0f, 0.22f, side * 0.21f), V(0.09f, 0.26f, 0.03f), 0.01f, white, 0.005f));
                Paint(kit.Box(V(0f, 0.22f, side * 0.21f), V(0.26f, 0.09f, 0.03f), 0.01f, white, 0.005f));
            }
            kit.Limb(V(-0.34f, 0.43f, 0.08f), V(0.16f, 0.43f, -0.06f), 0.035f, 0.035f, steel, 0.02f);
            kit.Torus(V(0.23f, 0.43f, -0.08f), 0.07f, 0.03f, steel, 0.01f);
            kit.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return kit;
        }

        public static Sculpt Bubble()
        {
            var shield = new Sculpt();
            Color blue = new Color(0.22f, 0.5f, 0.92f);
            Color rim = new Color(0.95f, 0.8f, 0.3f);
            Color white = new Color(0.95f, 0.97f, 1f);
            Color ring = new Color(0.45f, 0.85f, 1f);

            shield.Cylinder(V(0f, 0.45f, 0f), 0.36f, 0.05f, 0.03f, blue, 0.01f);
            shield.Torus(V(0f, 0.45f, 0f), 0.36f, 0.05f, rim, 0.01f, V(90f, 0f, 0f));
            shield.Sphere(V(0f, 0.45f, 0.06f), 0.09f, rim, 0.02f);
            for (int side = -1; side <= 1; side += 2)
            {
                Paint(shield.Box(V(0f, 0.45f, side * 0.05f), V(0.07f, 0.6f, 0.03f), 0.01f, white, 0.005f));
                Paint(shield.Box(V(0f, 0.45f, side * 0.05f), V(0.6f, 0.07f, 0.03f), 0.01f, white, 0.005f));
            }
            shield.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return shield;
        }

        public static Sculpt Magnet()
        {
            var magnet = new Sculpt();
            Color red = new Color(0.9f, 0.15f, 0.2f);
            Color steel = new Color(0.82f, 0.84f, 0.88f);
            Color ring = new Color(1f, 0.45f, 0.7f);
            const float radius = 0.22f;
            Vector3 center = V(0f, 0.38f, 0f);

            const int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float a = Mathf.PI + Mathf.PI * i / segments;
                float b = Mathf.PI + Mathf.PI * (i + 1) / segments;
                magnet.Limb(center + V(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius, center + V(Mathf.Cos(b), Mathf.Sin(b), 0f) * radius, 0.075f, 0.075f, red, 0.03f);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                magnet.Limb(center + V(side * radius, 0f, 0f), center + V(side * radius, 0.24f, 0f), 0.075f, 0.075f, red, 0.03f);
                magnet.Limb(center + V(side * radius, 0.25f, 0f), center + V(side * radius, 0.36f, 0f), 0.078f, 0.078f, steel, 0.01f);
            }
            magnet.Torus(V(0f, -0.02f, 0f), 0.62f, 0.05f, ring, 0.01f);
            return magnet;
        }

        public static Sculpt RoundTree(int seed)
        {
            var tree = new Sculpt();
            var random = new System.Random(seed);
            Color bark = new Color(0.45f, 0.3f, 0.19f);
            Color[] greens =
            {
                new Color(0.32f, 0.6f, 0.24f), new Color(0.4f, 0.68f, 0.26f), new Color(0.26f, 0.52f, 0.22f), new Color(0.5f, 0.72f, 0.3f)
            };
            Color leaf = greens[seed % greens.Length];
            Color leafLight = Color.Lerp(leaf, new Color(0.85f, 0.95f, 0.45f), 0.35f);

            float height = 2.6f + (float)random.NextDouble() * 1.2f;
            float lean = ((float)random.NextDouble() - 0.5f) * 0.6f;
            Vector3 top = V(lean, height, ((float)random.NextDouble() - 0.5f) * 0.4f);
            tree.Limb(V(0f, -0.1f, 0f), top, 0.32f, 0.17f, bark, 0.12f).noise = 0.03f;
            tree.Mirror(s => tree.Limb(V(0f, 0.15f, 0f), V(s * 0.45f, 0f, s * 0.2f), 0.14f, 0.05f, bark, 0.12f));
            tree.Limb(Vector3.Lerp(V(0f, 0f, 0f), top, 0.6f), top + V(0.9f, 0.4f, 0.2f), 0.12f, 0.06f, bark, 0.06f);

            int blobs = 5 + random.Next(3);
            for (int i = 0; i < blobs; i++)
            {
                float angle = i * 2.4f + (float)random.NextDouble();
                float spread = i == 0 ? 0f : 0.85f + (float)random.NextDouble() * 0.5f;
                Vector3 center = top + V(Mathf.Cos(angle) * spread, 0.5f + (float)random.NextDouble() * 0.9f - (i == 0 ? 0f : 0.3f), Mathf.Sin(angle) * spread);
                float radius = i == 0 ? 1.45f : 0.9f + (float)random.NextDouble() * 0.45f;
                SdfShape blob = tree.Sphere(center, radius, leaf, 0.45f);
                blob.noise = 0.22f;
            }
            Paint(tree.Ellipsoid(top + V(0.3f, 1.6f, 0.2f), V(1.8f, 0.9f, 1.8f), leafLight, 0.6f));
            return tree;
        }

        public static Sculpt Pine(int seed)
        {
            var tree = new Sculpt();
            var random = new System.Random(seed);
            Color bark = new Color(0.4f, 0.27f, 0.17f);
            Color needle = seed % 2 == 0 ? new Color(0.16f, 0.42f, 0.28f) : new Color(0.2f, 0.47f, 0.25f);
            Color tip = Color.Lerp(needle, new Color(0.6f, 0.8f, 0.45f), 0.35f);

            float height = 4.2f + (float)random.NextDouble() * 1.4f;
            tree.Limb(V(0f, -0.1f, 0f), V(0f, height * 0.55f, 0f), 0.24f, 0.14f, bark, 0.08f).noise = 0.02f;
            int tiers = 4;
            for (int i = 0; i < tiers; i++)
            {
                float t = i / (float)(tiers - 1);
                float y = Mathf.Lerp(1.1f, height - 0.9f, t);
                float radius = Mathf.Lerp(1.7f, 0.55f, t);
                SdfShape tier = tree.Limb(V(0f, y, 0f), V(0f, y + Mathf.Lerp(1.5f, 1f, t), 0f), radius, 0.08f, needle, 0.25f);
                tier.noise = 0.12f;
                Paint(tree.Ellipsoid(V(0f, y + 0.6f, 0f), V(radius * 0.6f, 0.35f, radius * 0.6f), tip, 0.3f));
            }
            return tree;
        }

        public static Sculpt Bush(int seed)
        {
            var bush = new Sculpt();
            var random = new System.Random(seed);
            Color leaf = seed % 2 == 0 ? new Color(0.34f, 0.6f, 0.26f) : new Color(0.28f, 0.52f, 0.24f);
            Color flower = seed % 3 == 0 ? new Color(0.95f, 0.45f, 0.6f) : seed % 3 == 1 ? new Color(0.98f, 0.9f, 0.4f) : new Color(0.75f, 0.6f, 0.95f);

            int blobs = 4 + random.Next(3);
            for (int i = 0; i < blobs; i++)
            {
                float angle = i * 2.1f;
                float spread = i == 0 ? 0f : 0.45f + (float)random.NextDouble() * 0.25f;
                float radius = i == 0 ? 0.7f : 0.45f + (float)random.NextDouble() * 0.2f;
                bush.Sphere(V(Mathf.Cos(angle) * spread, radius * 0.75f, Mathf.Sin(angle) * spread), radius, leaf, 0.3f).noise = 0.12f;
            }
            if (seed % 2 == 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    float angle = i * 0.9f;
                    float height = 0.6f + (i % 3) * 0.2f;
                    Paint(bush.Sphere(V(Mathf.Cos(angle) * 0.6f, height, Mathf.Sin(angle) * 0.6f), 0.1f, flower, 0.04f));
                }
            }
            return bush;
        }

        public static Sculpt Frog()
        {
            var frog = new Sculpt();
            Color skin = new Color(0.38f, 0.72f, 0.28f);
            Color belly = new Color(0.86f, 0.9f, 0.55f);
            Color white = new Color(0.98f, 0.97f, 0.93f);
            Color ink = new Color(0.06f, 0.05f, 0.05f);
            Color mouth = new Color(0.35f, 0.12f, 0.12f);

            frog.Ellipsoid(V(0f, 0.22f, 0f), V(0.32f, 0.22f, 0.36f), skin, 0.06f);
            Paint(frog.Ellipsoid(V(0f, 0.14f, 0.18f), V(0.24f, 0.13f, 0.2f), belly, 0.06f));
            frog.Mirror(s => frog.Sphere(V(s * 0.15f, 0.42f, 0.16f), 0.1f, skin, 0.06f));
            frog.Mirror(s => Paint(frog.Sphere(V(s * 0.16f, 0.46f, 0.22f), 0.07f, white, 0.01f)));
            frog.Mirror(s => Paint(frog.Sphere(V(s * 0.16f, 0.47f, 0.27f), 0.035f, ink, 0.006f)));
            frog.Mirror(s => Paint(frog.Sphere(V(s * 0.14f, 0.49f, 0.28f), 0.012f, white, 0.003f)));
            Carve(frog.Ellipsoid(V(0f, 0.22f, 0.36f), V(0.2f, 0.018f, 0.06f), mouth, 0.015f));
            frog.Mirror(s => frog.Ellipsoid(V(s * 0.3f, 0.1f, -0.08f), V(0.14f, 0.1f, 0.22f), skin, 0.06f));
            frog.Mirror(s => frog.Ellipsoid(V(s * 0.36f, 0.02f, 0.1f), V(0.1f, 0.03f, 0.14f), skin, 0.03f));
            frog.Mirror(s => frog.Limb(V(s * 0.16f, 0.12f, 0.22f), V(s * 0.22f, 0.02f, 0.34f), 0.05f, 0.04f, skin, 0.04f));
            return frog;
        }

        public static Sculpt LilyPad()
        {
            var pad = new Sculpt();
            Color leaf = new Color(0.3f, 0.62f, 0.26f);
            Color vein = new Color(0.4f, 0.72f, 0.3f);
            Color petal = new Color(0.98f, 0.7f, 0.82f);
            Color core = new Color(1f, 0.85f, 0.3f);

            pad.Cylinder(V(0f, 0f, 0f), 0.75f, 0.025f, 0.02f, leaf, 0.01f, V(90f, 0f, 0f));
            Carve(pad.Box(V(0.45f, 0f, 0.12f), V(0.9f, 0.2f, 0.1f), 0f, leaf, 0.01f, V(0f, -15f, 0f)));
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f + 0.5f;
                Paint(pad.Limb(V(0f, 0.03f, 0f), V(Mathf.Cos(angle) * 0.65f, 0.03f, Mathf.Sin(angle) * 0.65f), 0.015f, 0.008f, vein, 0.008f));
            }
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f;
                pad.Ellipsoid(V(-0.3f + Mathf.Cos(angle) * 0.09f, 0.1f, -0.2f + Mathf.Sin(angle) * 0.09f), V(0.06f, 0.1f, 0.04f), petal, 0.02f, V(Mathf.Sin(angle) * 30f, -angle * Mathf.Rad2Deg, Mathf.Cos(angle) * 30f));
            }
            pad.Sphere(V(-0.3f, 0.14f, -0.2f), 0.04f, core, 0.02f);
            return pad;
        }

        public static Sculpt BirdBody()
        {
            var bird = new Sculpt();
            Color white = new Color(0.96f, 0.96f, 0.95f);
            Color grey = new Color(0.62f, 0.66f, 0.7f);
            Color beak = new Color(1f, 0.72f, 0.2f);
            Color ink = new Color(0.06f, 0.05f, 0.05f);

            bird.Ellipsoid(V(0f, 0f, 0f), V(0.14f, 0.13f, 0.32f), white, 0.05f);
            bird.Sphere(V(0f, 0.08f, 0.26f), 0.11f, white, 0.06f);
            bird.Limb(V(0f, 0.07f, 0.35f), V(0f, 0.05f, 0.47f), 0.035f, 0.01f, beak, 0.015f);
            bird.Mirror(s => Paint(bird.Sphere(V(s * 0.07f, 0.13f, 0.31f), 0.022f, ink, 0.006f)));
            bird.Ellipsoid(V(0f, 0.02f, -0.36f), V(0.1f, 0.025f, 0.12f), grey, 0.04f);
            return bird;
        }

        public static Sculpt BirdWing()
        {
            var wing = new Sculpt();
            Color white = new Color(0.94f, 0.94f, 0.93f);
            Color grey = new Color(0.55f, 0.6f, 0.66f);
            wing.Ellipsoid(V(0.28f, 0f, 0f), V(0.3f, 0.025f, 0.13f), white, 0.03f, V(0f, -8f, 0f));
            Paint(wing.Ellipsoid(V(0.48f, 0f, -0.04f), V(0.15f, 0.04f, 0.12f), grey, 0.03f));
            return wing;
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
    }
}
