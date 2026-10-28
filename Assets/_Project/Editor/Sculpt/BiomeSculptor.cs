using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public static class BiomeSculptor
    {
        public const float OrcaLength = 6.1f;
        public const float SharkLength = 5.7f;

        static readonly Color Snow = new Color(0.95f, 0.97f, 1f);
        static readonly Color IceBlue = new Color(0.72f, 0.88f, 0.98f);
        static readonly Color IceDeep = new Color(0.55f, 0.76f, 0.92f);

        static float Range(System.Random random, float min, float max)
        {
            return min + (max - min) * (float)random.NextDouble();
        }

        static void Fin(Sculpt sculpt, Vector3 root, Vector3 tip, float thickness, float rootLength, Color color, int steps = 6)
        {
            steps *= 3;
            float height = (tip - root).magnitude;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 point = Vector3.Lerp(root, tip, t);
                sculpt.Ellipsoid(point, V(thickness * (1f - t * 0.6f), Mathf.Max(0.06f, height / steps * 1.6f), Mathf.Lerp(rootLength, 0.04f, Mathf.Pow(t, 0.8f))), color, 0.12f);
            }
        }

        public const float RollerLength = 6.2f;

        static readonly Color Pink = new Color(0.95f, 0.35f, 0.62f);
        static readonly Color Cream = new Color(0.98f, 0.96f, 0.92f);
        static readonly Color Terracotta = new Color(0.84f, 0.44f, 0.3f);

        public static Sculpt Bumper(int seed)
        {
            var bumper = new Sculpt();
            var random = new System.Random(seed + 210);
            Color red = seed % 2 == 0 ? new Color(0.88f, 0.2f, 0.25f) : new Color(0.95f, 0.45f, 0.15f);
            Color green = new Color(0.35f, 0.78f, 0.45f);
            bumper.Ellipsoid(V(0f, 0.25f, 0f), V(1.45f, 1.05f, 1.45f), red, 0.05f);
            bumper.Torus(V(0f, -0.05f, 0f), 1.4f, 0.28f, green, 0.08f);
            Paint(bumper, bumper.Torus(V(0f, 0.55f, 0f), 1.22f, 0.07f, Cream, 0.03f));
            for (int i = 0; i < 11; i++)
            {
                float angle = i * 2.4f + (float)random.NextDouble() * 0.3f;
                float up = 0.25f + 0.55f * ((i * 7) % 11) / 11f;
                var direction = new Vector3(Mathf.Cos(angle) * (1f - up), up, Mathf.Sin(angle) * (1f - up)).normalized;
                Vector3 root = V(0f, 0.25f, 0f) + Vector3.Scale(direction, V(1.4f, 1f, 1.4f));
                bumper.Limb(root - direction * 0.1f, root + direction * 0.5f, 0.2f, 0.02f, Cream, 0.04f);
            }
            bumper.Sphere(V(0f, 1.3f, 0f), 0.18f, Cream, 0.05f);
            return bumper;
        }

        public static Sculpt SpikeRoller()
        {
            var roller = new Sculpt();
            float half = RollerLength * 0.5f - 0.2f;
            roller.Cylinder(V(0f, 0f, 0f), 0.48f, half, 0.12f, Pink, 0.03f);
            for (float z = -half + 0.3f; z < half; z += 0.6f)
            {
                Paint(roller, roller.Torus(V(0f, 0f, z), 0.48f, 0.09f, Cream, 0.02f, V(90f, 0f, 0f)));
            }
            int ring = 0;
            for (float z = -half + 0.45f; z < half - 0.1f; z += 0.6f)
            {
                for (int k = 0; k < 6; k++)
                {
                    float angle = (k + (ring % 2) * 0.5f) * Mathf.PI / 3f;
                    var direction = V(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                    roller.Limb(V(0f, 0f, z) + direction * 0.4f, V(0f, 0f, z) + direction * 0.95f, 0.13f, 0.012f, Cream, 0.03f);
                }
                ring++;
            }
            roller.Mirror(s => roller.Cylinder(V(0f, 0f, s * (half + 0.05f)), 0.56f, 0.12f, 0.06f, new Color(0.55f, 0.2f, 0.45f), 0.03f));
            return roller;
        }

        public static Sculpt SpinnerArm()
        {
            var arm = new Sculpt();
            const float length = 4f;
            arm.Cylinder(V(0f, 0f, 0f), 0.42f, 0.35f, 0.08f, new Color(0.55f, 0.2f, 0.45f), 0.04f, V(90f, 0f, 0f));
            arm.Cylinder(V(0f, 0f, 0f), 0.22f, length, 0.08f, Pink, 0.03f, V(0f, 90f, 0f));
            for (float x = -length + 0.35f; x < length; x += 0.7f)
            {
                Paint(arm, arm.Torus(V(x, 0f, 0f), 0.22f, 0.07f, Cream, 0.02f, V(0f, 0f, 90f)));
            }
            for (float x = -length + 0.5f; x < length - 0.2f; x += 0.7f)
            {
                if (Mathf.Abs(x) < 0.6f)
                {
                    continue;
                }
                for (int k = 0; k < 4; k++)
                {
                    float angle = k * Mathf.PI * 0.5f + 0.4f;
                    var direction = V(0f, Mathf.Sin(angle), Mathf.Cos(angle));
                    arm.Limb(V(x, 0f, 0f) + direction * 0.18f, V(x, 0f, 0f) + direction * 0.6f, 0.09f, 0.01f, Cream, 0.02f);
                }
            }
            arm.Mirror(s => arm.Sphere(V(s * length, 0f, 0f), 0.32f, new Color(0.98f, 0.8f, 0.25f), 0.05f));
            return arm;
        }

        public static Sculpt SpikeStrip()
        {
            var strip = new Sculpt();
            strip.Cylinder(V(0f, 0f, 0f), 0.2f, 0.95f, 0.06f, Pink, 0.02f);
            for (float z = -0.8f; z <= 0.81f; z += 0.4f)
            {
                Paint(strip, strip.Torus(V(0f, 0f, z), 0.2f, 0.06f, Cream, 0.02f, V(90f, 0f, 0f)));
            }
            for (float z = -0.75f; z <= 0.76f; z += 0.3f)
            {
                for (int k = 0; k < 5; k++)
                {
                    float angle = k * Mathf.PI / 4f;
                    var direction = V(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                    strip.Limb(V(0f, 0f, z) + direction * 0.16f, V(0f, 0f, z) + direction * 0.55f, 0.08f, 0.008f, Cream, 0.02f);
                }
            }
            return strip;
        }

        public static Sculpt BambooPole()
        {
            var pole = new Sculpt();
            Color bamboo = new Color(0.72f, 0.8f, 0.35f);
            Color node = new Color(0.52f, 0.6f, 0.25f);
            Color leaf = new Color(0.35f, 0.65f, 0.25f);
            const float height = 5.2f;
            pole.Limb(V(0f, 0f, 0f), V(0f, height, 0f), 0.15f, 0.12f, bamboo, 0.02f);
            for (float y = 0.6f; y < height; y += 0.75f)
            {
                pole.Torus(V(0f, y, 0f), 0.14f, 0.035f, node, 0.02f);
            }
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 1.6f;
                var direction = V(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                pole.Ellipsoid(V(0f, height - 0.2f - i * 0.25f, 0f) + direction * 0.35f, V(0.38f, 0.04f, 0.1f), leaf, 0.05f, V(0f, -angle * Mathf.Rad2Deg, -25f));
            }
            return pole;
        }

        public static Sculpt SlidePillar()
        {
            var pillar = new Sculpt();
            var random = new System.Random(77);
            Color[] bands = { Terracotta, new Color(0.95f, 0.62f, 0.4f), new Color(0.72f, 0.36f, 0.26f) };
            for (int i = 0; i < 9; i++)
            {
                float top = -i * 6f;
                pillar.Cylinder(V(Range(random, -0.15f, 0.15f), top - 3f, Range(random, -0.15f, 0.15f)), 1.35f + Range(random, -0.1f, 0.15f), 3.1f, 0.3f, bands[i % bands.Length], 0.4f, V(90f, Range(random, 0f, 40f), 0f)).noise = 0.12f;
            }
            pillar.Cylinder(V(0f, 0.05f, 0f), 1.7f, 0.25f, 0.1f, Terracotta, 0.2f, V(90f, 0f, 0f));
            return pillar;
        }

        public static Sculpt Mesa(int seed)
        {
            var mesa = new Sculpt();
            var random = new System.Random(seed + 310);
            Color[] bands = { Terracotta, new Color(0.96f, 0.63f, 0.42f), new Color(0.74f, 0.37f, 0.27f), new Color(0.9f, 0.52f, 0.34f) };
            Color top = new Color(0.45f, 0.76f, 0.32f);
            float radius = Range(random, 6.5f, 9.5f);
            int layers = 8;
            for (int i = 0; i < layers; i++)
            {
                float y = -i * 9f - 4.5f;
                float r = radius * Range(random, 0.92f, 1.08f) * (1f + i * 0.02f);
                mesa.Cylinder(V(Range(random, -0.8f, 0.8f), y, Range(random, -0.8f, 0.8f)), r, 4.8f, 1.2f, bands[(i + seed) % bands.Length], 1.4f, V(90f, Range(random, 0f, 60f), 0f)).noise = 0.6f;
            }
            mesa.Cylinder(V(0f, -0.6f, 0f), radius * 1.02f, 0.7f, 0.5f, top, 0.8f, V(90f, 0f, 0f)).noise = 0.3f;
            Paint(mesa, mesa.Cylinder(V(0f, -1.6f, 0f), radius * 1.15f, 0.5f, 0.2f, new Color(0.35f, 0.62f, 0.28f), 0.6f, V(90f, 0f, 0f)));
            return mesa;
        }


        public const float LogLength = 6.2f;
        public const float EelLength = 5f;

        public static Sculpt Log()
        {
            var log = new Sculpt();
            Color bark = new Color(0.42f, 0.28f, 0.17f);
            Color dark = new Color(0.3f, 0.2f, 0.12f);
            Color wood = new Color(0.85f, 0.68f, 0.45f);
            Color moss = new Color(0.35f, 0.55f, 0.25f);
            float half = LogLength * 0.5f - 0.3f;
            log.Cylinder(V(0f, 0f, 0f), 0.52f, half, 0.08f, bark, 0.03f).noise = 0.03f;
            log.Mirror(s => Paint(log, log.Ellipsoid(V(0f, 0f, s * (half + 0.02f)), V(0.46f, 0.46f, 0.08f), wood, 0.02f)));
            log.Mirror(s => Paint(log, log.Ellipsoid(V(0f, 0f, s * (half + 0.03f)), V(0.3f, 0.3f, 0.09f), new Color(0.7f, 0.52f, 0.33f), 0.012f)));
            log.Limb(V(0.3f, 0.35f, -1.2f), V(0.9f, 1.1f, -1.6f), 0.14f, 0.05f, dark, 0.04f);
            log.Limb(V(-0.35f, 0.3f, 1.4f), V(-0.8f, 0.85f, 1.9f), 0.12f, 0.04f, dark, 0.04f);
            Paint(log, log.Ellipsoid(V(0f, 0.45f, 0.4f), V(0.5f, 0.12f, 1.4f), moss, 0.06f));
            return log;
        }

        public static Sculpt IceFloe()
        {
            var floe = new Sculpt();
            Color ice = new Color(0.74f, 0.88f, 0.97f);
            Color deep = new Color(0.55f, 0.76f, 0.92f);
            Color snow = new Color(0.95f, 0.97f, 1f);
            float half = LogLength * 0.5f - 0.3f;
            floe.Box(V(0f, 0f, 0f), V(1.4f, 0.9f, half * 2f), 0.12f, ice, 0.06f).noise = 0.03f;
            floe.Box(V(0.3f, 0.3f, -0.8f), V(0.9f, 1.0f, 1.3f), 0.1f, deep, 0.06f, V(0f, 20f, 8f)).noise = 0.03f;
            floe.Box(V(-0.3f, 0.25f, 1.3f), V(0.8f, 0.8f, 1.0f), 0.1f, deep, 0.06f, V(0f, -15f, -6f)).noise = 0.03f;
            Paint(floe, floe.Ellipsoid(V(0f, 0.55f, 0f), V(0.75f, 0.25f, half), snow, 0.15f));
            return floe;
        }

        public static Sculpt Crate()
        {
            var crate = new Sculpt();
            Color wood = new Color(0.78f, 0.6f, 0.36f);
            Color dark = new Color(0.55f, 0.4f, 0.22f);
            Color iron = new Color(0.35f, 0.32f, 0.3f);
            float half = LogLength * 0.5f - 0.3f;
            for (int i = -1; i <= 1; i++)
            {
                Vector3 center = V(0f, 0f, i * 1.85f);
                crate.Box(center, V(1.5f, 1.3f, 1.6f), 0.06f, wood, 0.02f);
                Paint(crate, crate.Box(center, V(1.56f, 0.12f, 1.66f), 0.01f, dark, 0.01f));
                Paint(crate, crate.Box(center + V(0f, 0.5f, 0f), V(1.56f, 0.12f, 1.66f), 0.01f, dark, 0.01f));
                Paint(crate, crate.Box(center + V(0f, -0.5f, 0f), V(1.56f, 0.12f, 1.66f), 0.01f, dark, 0.01f));
                Paint(crate, crate.Box(center, V(0.12f, 1.36f, 1.66f), 0.01f, iron, 0.01f));
            }
            crate.Limb(V(0f, 0.2f, -half), V(0f, 0.2f, half), 0.05f, 0.05f, iron, 0.01f);
            return crate;
        }

        public static Sculpt InflatableTube()
        {
            var tube = new Sculpt();
            Color yellow = new Color(0.98f, 0.82f, 0.2f);
            Color blue = new Color(0.25f, 0.55f, 0.95f);
            float half = LogLength * 0.5f - 0.3f;
            tube.Cylinder(V(0f, 0f, 0f), 0.62f, half, 0.4f, yellow, 0.03f);
            for (float z = -half + 0.5f; z < half; z += 1f)
            {
                Paint(tube, tube.Box(V(0f, 0f, z), V(1.6f, 1.6f, 0.45f), 0f, blue, 0.02f));
            }
            tube.Mirror(s => tube.Sphere(V(0f, 0f, s * (half + 0.1f)), 0.5f, yellow, 0.1f));
            return tube;
        }

        public static Sculpt Barrel()
        {
            var barrel = new Sculpt();
            Color wood = new Color(0.55f, 0.35f, 0.18f);
            Color band = new Color(0.3f, 0.3f, 0.32f);
            Color red = new Color(0.85f, 0.15f, 0.12f);
            Color label = new Color(0.95f, 0.9f, 0.75f);
            barrel.Cylinder(V(0f, 0.3f, 0f), 0.62f, 0.72f, 0.08f, wood, 0.03f, V(90f, 0f, 0f)).noise = 0.015f;
            barrel.Torus(V(0f, 0.3f, 0f), 0.66f, 0.07f, wood, 0.06f);
            Paint(barrel, barrel.Torus(V(0f, 0.82f, 0f), 0.62f, 0.06f, band, 0.01f));
            Paint(barrel, barrel.Torus(V(0f, -0.2f, 0f), 0.62f, 0.06f, band, 0.01f));
            Paint(barrel, barrel.Box(V(0f, 0.3f, 0.62f), V(0.7f, 0.5f, 0.1f), 0.02f, label, 0.01f));
            Paint(barrel, barrel.Box(V(0f, 0.3f, 0.66f), V(0.5f, 0.12f, 0.06f), 0.01f, red, 0.005f));
            Paint(barrel, barrel.Box(V(0f, 0.3f, -0.62f), V(0.7f, 0.5f, 0.1f), 0.02f, label, 0.01f));
            Paint(barrel, barrel.Box(V(0f, 0.3f, -0.66f), V(0.5f, 0.12f, 0.06f), 0.01f, red, 0.005f));
            barrel.Limb(V(0f, 1.0f, 0f), V(0.12f, 1.35f, 0.08f), 0.035f, 0.025f, new Color(0.9f, 0.85f, 0.7f), 0.01f);
            barrel.Sphere(V(0.14f, 1.4f, 0.1f), 0.07f, new Color(1f, 0.6f, 0.15f), 0.02f);
            return barrel;
        }

        public static Sculpt Eel()
        {
            var eel = new Sculpt();
            Color skin = new Color(0.25f, 0.45f, 0.3f);
            Color belly = new Color(0.75f, 0.85f, 0.55f);
            Color spark = new Color(1f, 0.95f, 0.3f);
            float half = EelLength * 0.5f;
            Vector3 previous = V(0f, 0f, -half + 0.2f);
            for (int i = 1; i <= 8; i++)
            {
                float t = i / 8f;
                Vector3 point = V(Mathf.Sin(t * Mathf.PI * 2f) * 0.35f, 0f, Mathf.Lerp(-half + 0.2f, half - 0.3f, t));
                float r = Mathf.Lerp(0.18f, 0.42f, Mathf.Sin(t * Mathf.PI));
                eel.Limb(previous, point, Mathf.Lerp(0.18f, 0.42f, Mathf.Sin((i - 1) / 8f * Mathf.PI)), r, skin, 0.12f);
                previous = point;
            }
            eel.Sphere(V(0f, 0.05f, half - 0.3f), 0.42f, skin, 0.1f);
            Paint(eel, eel.Ellipsoid(V(0f, -0.25f, 0f), V(0.35f, 0.15f, half), belly, 0.15f));
            for (int i = 0; i < 5; i++)
            {
                Paint(eel, eel.Box(V(0f, 0.2f, -half + 0.8f + i * 0.9f), V(1.2f, 0.5f, 0.12f), 0f, spark, 0.02f));
            }
            eel.Mirror(s => Paint(eel, eel.Sphere(V(s * 0.25f, 0.28f, half - 0.05f), 0.09f, Color.white, 0.01f)));
            eel.Mirror(s => Paint(eel, eel.Sphere(V(s * 0.27f, 0.3f, half + 0.03f), 0.05f, Ink, 0.005f)));
            eel.Ellipsoid(V(0f, 0.38f, 0f), V(0.04f, 0.3f, half * 0.8f), spark, 0.05f);
            return eel;
        }

        public static Sculpt IcicleSpike()
        {
            var spike = new Sculpt();
            Color ice = new Color(0.74f, 0.88f, 0.98f);
            Color deep = new Color(0.55f, 0.76f, 0.92f);
            spike.Limb(V(0f, 2.6f, 0f), V(0f, 0f, 0f), 0.55f, 0.03f, ice, 0.05f);
            spike.Limb(V(0.35f, 2.4f, 0.1f), V(0.45f, 1.1f, 0.15f), 0.25f, 0.02f, deep, 0.05f);
            spike.Limb(V(-0.3f, 2.5f, -0.15f), V(-0.4f, 1.4f, -0.2f), 0.22f, 0.02f, deep, 0.05f);
            spike.Ellipsoid(V(0f, 2.7f, 0f), V(0.75f, 0.3f, 0.75f), new Color(0.95f, 0.97f, 1f), 0.1f).noise = 0.03f;
            return spike;
        }

        public static Sculpt Orca()
        {
            var orca = new Sculpt();
            Color black = new Color(0.07f, 0.08f, 0.1f);
            Color white = new Color(0.95f, 0.96f, 0.97f);
            Color grey = new Color(0.55f, 0.58f, 0.63f);
            const float y = -0.35f;

            orca.Ellipsoid(V(0f, y, 0f), V(0.62f, 0.6f, 2.5f), black, 0.3f);
            orca.Ellipsoid(V(0f, y, 1.95f), V(0.56f, 0.52f, 0.95f), black, 0.35f);
            orca.Limb(V(0f, y + 0.05f, -1.8f), V(0f, y + 0.15f, -2.95f), 0.42f, 0.14f, black, 0.3f);
            orca.Mirror(s => orca.Ellipsoid(V(s * 0.55f, y + 0.15f, -3.0f), V(0.62f, 0.07f, 0.28f), black, 0.1f, V(0f, s * -25f, 0f)));
            Fin(orca, V(0f, y + 0.5f, 0.2f), V(0f, y + 1.75f, -0.35f), 0.1f, 0.55f, black);
            orca.Mirror(s => orca.Ellipsoid(V(s * 0.72f, y - 0.35f, 1.1f), V(0.48f, 0.06f, 0.26f), black, 0.12f, V(0f, s * 25f, s * -30f)));
            Paint(orca, orca.Ellipsoid(V(0f, y - 0.5f, 0.6f), V(0.52f, 0.26f, 2.0f), white, 0.2f));
            Paint(orca, orca.Ellipsoid(V(0f, y - 0.33f, 2.3f), V(0.42f, 0.2f, 0.6f), white, 0.1f));
            orca.Mirror(s => Paint(orca, orca.Ellipsoid(V(s * 0.5f, y + 0.18f, 1.55f), V(0.14f, 0.13f, 0.34f), white, 0.05f, V(0f, s * 12f, 0f))));
            Paint(orca, orca.Ellipsoid(V(0f, y + 0.52f, -0.75f), V(0.45f, 0.14f, 0.5f), grey, 0.12f));
            orca.Mirror(s => Paint(orca, orca.Sphere(V(s * 0.52f, y + 0.06f, 2.02f), 0.065f, Ink, 0.012f)));
            orca.Mirror(s => Paint(orca, orca.Limb(V(s * 0.44f, y - 0.16f, 1.85f), V(s * 0.14f, y - 0.22f, 2.85f), 0.03f, 0.03f, Ink, 0.012f)));
            for (int i = 0; i < 5; i++)
            {
                float z = 2.05f + i * 0.16f;
                float x = Mathf.Lerp(0.4f, 0.16f, i / 4f);
                orca.Mirror(s => orca.Limb(V(s * x, y - 0.17f, z), V(s * x, y - 0.29f, z + 0.01f), 0.038f, 0.008f, Tooth, 0.01f));
            }
            return orca;
        }

        public static Sculpt Shark()
        {
            var shark = new Sculpt();
            Color grey = new Color(0.25f, 0.3f, 0.37f);
            Color dark = new Color(0.15f, 0.18f, 0.24f);
            Color white = new Color(0.94f, 0.95f, 0.95f);
            const float y = -0.38f;

            shark.Ellipsoid(V(0f, y, 0f), V(0.52f, 0.52f, 2.3f), grey, 0.3f);
            shark.Limb(V(0f, y, 1.3f), V(0f, y - 0.05f, 2.75f), 0.5f, 0.14f, grey, 0.35f);
            shark.Limb(V(0f, y + 0.02f, -1.6f), V(0f, y + 0.08f, -2.6f), 0.33f, 0.1f, grey, 0.25f);
            Fin(shark, V(0f, y + 0.1f, -2.6f), V(0f, y + 1.05f, -3.05f), 0.07f, 0.3f, grey, 5);
            Fin(shark, V(0f, y - 0.05f, -2.6f), V(0f, y - 0.65f, -2.85f), 0.06f, 0.22f, grey, 4);
            Fin(shark, V(0f, y + 0.45f, 0.35f), V(0f, y + 1.45f, -0.15f), 0.09f, 0.55f, grey);
            shark.Mirror(s => shark.Ellipsoid(V(s * 0.68f, y - 0.32f, 0.9f), V(0.55f, 0.06f, 0.26f), grey, 0.12f, V(0f, s * 30f, s * -25f)));
            Paint(shark, shark.Ellipsoid(V(0f, y - 0.42f, 0.5f), V(0.44f, 0.24f, 2.2f), white, 0.2f));
            Paint(shark, shark.Ellipsoid(V(0f, y + 0.5f, 0f), V(0.4f, 0.12f, 1.8f), dark, 0.2f));
            shark.Mirror(s =>
            {
                for (int g = 0; g < 4; g++)
                {
                    Paint(shark, shark.Limb(V(s * 0.47f, y + 0.12f, 1.3f - g * 0.13f), V(s * 0.47f, y - 0.22f, 1.24f - g * 0.13f), 0.018f, 0.018f, dark, 0.01f));
                }
                return shark.Sphere(V(s * 0.38f, y + 0.12f, 2.05f), 0.07f, Ink, 0.012f);
            });
            Carve(shark, shark.Ellipsoid(V(0f, y - 0.3f, 2.15f), V(0.34f, 0.1f, 0.45f), Mouth, 0.05f));
            for (int i = -4; i <= 4; i++)
            {
                float x = i * 0.065f;
                float z = 2.45f - Mathf.Abs(i) * 0.05f;
                shark.Limb(V(x, y - 0.22f, z), V(x, y - 0.33f, z + 0.01f), 0.03f, 0.006f, Tooth, 0.008f);
            }
            return shark;
        }

        public static Sculpt Dolphin()
        {
            var dolphin = new Sculpt();
            Color grey = new Color(0.5f, 0.6f, 0.7f);
            Color belly = new Color(0.88f, 0.9f, 0.92f);
            dolphin.Ellipsoid(V(0f, 0f, 0f), V(0.3f, 0.32f, 0.9f), grey, 0.15f);
            dolphin.Limb(V(0f, 0.02f, 0.6f), V(0f, -0.04f, 1.25f), 0.24f, 0.07f, grey, 0.15f);
            dolphin.Limb(V(0f, 0f, -0.6f), V(0f, 0.05f, -1.1f), 0.2f, 0.06f, grey, 0.12f);
            dolphin.Mirror(s => dolphin.Ellipsoid(V(s * 0.2f, 0.05f, -1.15f), V(0.24f, 0.03f, 0.1f), grey, 0.05f, V(0f, s * -25f, 0f)));
            Fin(dolphin, V(0f, 0.25f, 0.05f), V(0f, 0.6f, -0.2f), 0.04f, 0.22f, grey, 4);
            dolphin.Mirror(s => dolphin.Ellipsoid(V(s * 0.3f, -0.15f, 0.35f), V(0.2f, 0.03f, 0.09f), grey, 0.05f, V(0f, s * 25f, s * -30f)));
            Paint(dolphin, dolphin.Ellipsoid(V(0f, -0.2f, 0.3f), V(0.24f, 0.14f, 0.9f), belly, 0.1f));
            dolphin.Mirror(s => Paint(dolphin, dolphin.Sphere(V(s * 0.2f, 0.06f, 0.82f), 0.035f, Ink, 0.008f)));
            dolphin.Mirror(s => Paint(dolphin, dolphin.Limb(V(s * 0.14f, -0.06f, 0.95f), V(s * 0.04f, -0.08f, 1.24f), 0.012f, 0.012f, Ink, 0.006f)));
            return dolphin;
        }

        public static Sculpt IceChunk(int seed)
        {
            var ice = new Sculpt();
            var random = new System.Random(seed * 13 + 3);
            int blocks = 3 + random.Next(3);
            for (int i = 0; i < blocks; i++)
            {
                Vector3 size = V(Range(random, 1f, 1.8f), Range(random, 0.9f, 1.9f), Range(random, 1f, 1.8f));
                Vector3 center = V(Range(random, -0.5f, 0.5f), Range(random, 0.15f, 0.45f), Range(random, -0.5f, 0.5f));
                Color color = Color.Lerp(IceBlue, IceDeep, (float)random.NextDouble());
                ice.Box(center, size, 0.07f, color, 0.06f, V(Range(random, -25f, 25f), Range(random, 0f, 360f), Range(random, -25f, 25f))).noise = 0.02f;
            }
            Paint(ice, ice.Ellipsoid(V(0f, 1.25f, 0f), V(1.3f, 0.3f, 1.3f), Snow, 0.2f));
            return ice;
        }

        public static Sculpt Icicles()
        {
            var sculpt = new Sculpt();
            var random = new System.Random(21);
            sculpt.Ellipsoid(V(0f, -0.05f, 0f), V(0.5f, 0.2f, 1.0f), Snow, 0.15f).noise = 0.03f;
            for (int i = 0; i < 9; i++)
            {
                float z = -0.85f + i * 0.21f;
                float lean = (i % 2 == 0 ? 1f : -1f) * Range(random, 0.15f, 0.45f);
                Vector3 root = V(lean * 0.3f, 0.02f, z);
                Vector3 tip = V(lean + Range(random, -0.1f, 0.1f), 0.8f + Range(random, 0f, 0.55f), z + Range(random, -0.12f, 0.12f));
                Color color = Color.Lerp(IceBlue, IceDeep, (float)random.NextDouble());
                sculpt.Limb(root, tip, 0.14f + Range(random, 0f, 0.05f), 0.012f, color, 0.04f);
            }
            for (int i = 0; i < 5; i++)
            {
                float z = -0.7f + i * 0.35f;
                sculpt.Limb(V(-0.35f, 0.05f, z), V(-0.75f, 0.45f, z + 0.1f), 0.08f, 0.01f, IceBlue, 0.03f);
            }
            return sculpt;
        }

        public static Sculpt Reef()
        {
            var reef = new Sculpt();
            var random = new System.Random(33);
            Color stone = new Color(0.5f, 0.46f, 0.4f);
            Color urchin = new Color(0.2f, 0.1f, 0.25f);
            Color spike = new Color(0.32f, 0.16f, 0.38f);
            Color[] corals = { new Color(1f, 0.45f, 0.55f), new Color(1f, 0.62f, 0.25f), new Color(0.95f, 0.85f, 0.4f) };
            reef.Ellipsoid(V(0f, -0.88f, 0f), V(0.55f, 0.25f, 1.05f), stone, 0.15f).noise = 0.05f;
            for (int i = 0; i < 4; i++)
            {
                Vector3 center = V(Range(random, -0.25f, 0.25f), -0.62f, -0.75f + i * 0.5f);
                reef.Sphere(center, 0.15f, urchin, 0.04f);
                for (int k = 0; k < 12; k++)
                {
                    float a = k * 2.4f;
                    float up = Range(random, -0.1f, 0.9f);
                    Vector3 direction = new Vector3(Mathf.Cos(a) * (1f - up * 0.5f), up, Mathf.Sin(a) * (1f - up * 0.5f)).normalized;
                    reef.Limb(center, center + direction * 0.38f, 0.035f, 0.005f, spike, 0.01f);
                }
            }
            for (int c = 0; c < 3; c++)
            {
                Color color = corals[c];
                Vector3 root = V(Range(random, -0.3f, 0.3f), -0.75f, -0.6f + c * 0.6f);
                Vector3 top = root + V(Range(random, -0.15f, 0.15f), Range(random, 0.55f, 0.85f), Range(random, -0.1f, 0.1f));
                reef.Limb(root, top, 0.07f, 0.04f, color, 0.04f);
                for (int b = 0; b < 3; b++)
                {
                    Vector3 from = Vector3.Lerp(root, top, 0.35f + b * 0.2f);
                    Vector3 to = from + V(Range(random, -0.3f, 0.3f), Range(random, 0.2f, 0.4f), Range(random, -0.25f, 0.25f));
                    reef.Limb(from, to, 0.045f, 0.025f, color, 0.03f);
                    reef.Sphere(to, 0.04f, Color.Lerp(color, Color.white, 0.3f), 0.02f);
                }
            }
            return reef;
        }

        public static Sculpt FrozenWheel()
        {
            var wheel = new Sculpt();
            Color wood = new Color(0.5f, 0.52f, 0.58f);
            Color dark = new Color(0.36f, 0.4f, 0.48f);
            Color iron = new Color(0.3f, 0.32f, 0.36f);
            const float radius = 4f;
            var random = new System.Random(5);

            wheel.Torus(Vector3.zero, radius * 0.92f, 0.16f, wood, 0.02f, V(90f, 0f, 0f)).noise = 0.015f;
            wheel.Torus(Vector3.zero, radius * 0.55f, 0.12f, dark, 0.02f, V(90f, 0f, 0f));
            wheel.Cylinder(Vector3.zero, 0.55f, 0.6f, 0.06f, iron, 0.03f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var tangent = new Vector3(-direction.y, direction.x, 0f);
                wheel.Limb(direction * 0.4f, direction * radius * 0.92f, 0.11f, 0.09f, dark, 0.03f);
                Vector3 center = direction * radius * 0.82f;
                wheel.Box(center, new Vector3(radius * 0.4f, 0.18f, 1.5f), 0.04f, wood, 0.02f, V(0f, 0f, angle * Mathf.Rad2Deg)).noise = 0.01f;
                Paint(wheel, wheel.Box(center + tangent * 0.1f, new Vector3(radius * 0.42f, 0.1f, 1.55f), 0.04f, Snow, 0.04f, V(0f, 0f, angle * Mathf.Rad2Deg)));
                for (int k = 0; k < 3; k++)
                {
                    Vector3 root = direction * radius * (0.7f + k * 0.1f) - tangent * 0.12f + V(0f, 0f, Range(random, -0.6f, 0.6f));
                    wheel.Limb(root, root - tangent * Range(random, 0.3f, 0.6f), 0.06f, 0.008f, IceBlue, 0.02f);
                }
            }
            return wheel;
        }

        public static Sculpt SnowPine(int seed)
        {
            var tree = new Sculpt();
            var random = new System.Random(seed + 40);
            Color bark = new Color(0.36f, 0.26f, 0.18f);
            Color needle = seed % 2 == 0 ? new Color(0.12f, 0.3f, 0.24f) : new Color(0.15f, 0.34f, 0.22f);

            float height = 4.2f + (float)random.NextDouble() * 1.6f;
            tree.Limb(V(0f, -0.1f, 0f), V(0f, height * 0.55f, 0f), 0.24f, 0.14f, bark, 0.08f).noise = 0.02f;
            const int tiers = 4;
            for (int i = 0; i < tiers; i++)
            {
                float t = i / (float)(tiers - 1);
                float y = Mathf.Lerp(1.1f, height - 0.9f, t);
                float radius = Mathf.Lerp(1.7f, 0.55f, t);
                float rise = Mathf.Lerp(1.5f, 1f, t);
                tree.Limb(V(0f, y, 0f), V(0f, y + rise, 0f), radius, 0.08f, needle, 0.25f).noise = 0.12f;
                Paint(tree, tree.Ellipsoid(V(0f, y + rise * 0.55f, 0f), V(radius * 0.72f, 0.38f, radius * 0.72f), Snow, 0.3f));
                Paint(tree, tree.Torus(V(0f, y + 0.05f, 0f), radius * 0.92f, 0.12f, Snow, 0.2f));
            }
            Paint(tree, tree.Sphere(V(0f, height + 0.2f, 0f), 0.4f, Snow, 0.2f));
            tree.Ellipsoid(V(0f, -0.05f, 0f), V(1.3f, 0.2f, 1.3f), Snow, 0.25f).noise = 0.05f;
            return tree;
        }

        public static Sculpt SnowMound(int seed)
        {
            var mound = new Sculpt();
            var random = new System.Random(seed + 70);
            int blobs = 3 + random.Next(3);
            for (int i = 0; i < blobs; i++)
            {
                float angle = i * 2.1f;
                float spread = i == 0 ? 0f : 0.45f + (float)random.NextDouble() * 0.3f;
                float radius = i == 0 ? 0.65f : 0.4f + (float)random.NextDouble() * 0.2f;
                mound.Ellipsoid(V(Mathf.Cos(angle) * spread, radius * 0.4f, Mathf.Sin(angle) * spread), V(radius, radius * 0.75f, radius), Snow, 0.3f).noise = 0.08f;
            }
            if (seed % 2 == 1)
            {
                mound.Box(V(0.4f, 0.35f, -0.2f), V(0.5f, 0.5f, 0.45f), 0.08f, new Color(0.45f, 0.47f, 0.52f), 0.15f, V(15f, 30f, 10f)).noise = 0.04f;
                Paint(mound, mound.Ellipsoid(V(0.4f, 0.65f, -0.2f), V(0.35f, 0.12f, 0.3f), Snow, 0.1f));
            }
            return mound;
        }

        public static Sculpt IceCrystal()
        {
            var crystal = new Sculpt();
            var random = new System.Random(8);
            for (int i = 0; i < 6; i++)
            {
                float height = Range(random, 0.5f, 1.2f);
                Vector3 tilt = V(Range(random, -25f, 25f), Range(random, 0f, 360f), Range(random, -25f, 25f));
                Quaternion rotation = Quaternion.Euler(tilt);
                Vector3 center = rotation * V(0f, height * 0.5f, 0f) + V(Range(random, -0.2f, 0.2f), 0f, Range(random, -0.2f, 0.2f));
                Color color = Color.Lerp(IceBlue, new Color(0.6f, 0.95f, 1f), (float)random.NextDouble());
                crystal.Box(center, V(0.18f, height, 0.18f), 0.02f, color, 0.03f, tilt);
                crystal.Limb(center + rotation * V(0f, height * 0.5f, 0f), center + rotation * V(0f, height * 0.5f + 0.18f, 0f), 0.09f, 0.01f, color, 0.02f);
            }
            crystal.Ellipsoid(V(0f, 0f, 0f), V(0.45f, 0.12f, 0.45f), Snow, 0.1f);
            return crystal;
        }

        public static Sculpt Igloo()
        {
            var igloo = new Sculpt();
            Color line = new Color(0.72f, 0.82f, 0.92f);
            igloo.Ellipsoid(V(0f, 0f, 0f), V(1.4f, 1.2f, 1.4f), Snow, 0.03f);
            igloo.Cylinder(V(0f, 0.3f, 1.35f), 0.52f, 0.45f, 0.08f, Snow, 0.08f);
            Carve(igloo, igloo.Ellipsoid(V(0f, 0f, 0f), V(1.25f, 1.05f, 1.25f), Snow, 0.02f));
            Carve(igloo, igloo.Cylinder(V(0f, 0.25f, 1.45f), 0.36f, 0.7f, 0.05f, new Color(0.3f, 0.35f, 0.45f), 0.03f));
            Carve(igloo, igloo.Box(V(0f, -1f, 0f), V(4f, 2f, 4f), 0f, Snow, 0.01f));
            for (int i = 0; i < 3; i++)
            {
                float h = 0.3f + i * 0.3f;
                float r = 1.4f * Mathf.Sqrt(Mathf.Max(0f, 1f - (h / 1.2f) * (h / 1.2f)));
                Paint(igloo, igloo.Torus(V(0f, h, 0f), r, 0.025f, line, 0.01f));
            }
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f + (i % 2) * 0.2f;
                Paint(igloo, igloo.Box(V(Mathf.Cos(angle) * 1.2f, 0.45f + (i % 2) * 0.3f, Mathf.Sin(angle) * 1.2f), V(0.04f, 0.3f, 0.4f), 0f, line, 0.01f, V(0f, -angle * Mathf.Rad2Deg, 0f)));
            }
            return igloo;
        }

        public static Sculpt Snowman()
        {
            var snowman = new Sculpt();
            Color coal = new Color(0.1f, 0.1f, 0.12f);
            Color carrot = new Color(1f, 0.5f, 0.1f);
            Color stick = new Color(0.4f, 0.26f, 0.15f);
            Color scarf = new Color(0.85f, 0.2f, 0.22f);
            snowman.Sphere(V(0f, 0.45f, 0f), 0.52f, Snow, 0.1f).noise = 0.02f;
            snowman.Sphere(V(0f, 1.15f, 0f), 0.38f, Snow, 0.1f).noise = 0.015f;
            snowman.Sphere(V(0f, 1.68f, 0f), 0.27f, Snow, 0.08f);
            snowman.Limb(V(0f, 1.68f, 0.24f), V(0f, 1.64f, 0.52f), 0.05f, 0.01f, carrot, 0.02f);
            snowman.Mirror(s => snowman.Sphere(V(s * 0.1f, 1.76f, 0.23f), 0.035f, coal, 0.01f));
            for (int i = 0; i < 3; i++)
            {
                snowman.Sphere(V(0f, 0.95f + i * 0.17f, 0.36f - Mathf.Abs(i - 1) * 0.03f), 0.04f, coal, 0.01f);
            }
            snowman.Torus(V(0f, 1.45f, 0f), 0.26f, 0.06f, scarf, 0.04f);
            snowman.Limb(V(0.15f, 1.42f, 0.2f), V(0.25f, 1.1f, 0.32f), 0.06f, 0.05f, scarf, 0.03f);
            snowman.Mirror(s => snowman.Limb(V(s * 0.3f, 1.2f, 0f), V(s * 0.85f, 1.5f, 0.05f), 0.035f, 0.02f, stick, 0.02f));
            snowman.Cylinder(V(0f, 1.98f, 0f), 0.2f, 0.17f, 0.02f, coal, 0.02f, V(90f, 0f, 0f));
            snowman.Cylinder(V(0f, 1.86f, 0f), 0.3f, 0.025f, 0.01f, coal, 0.02f, V(90f, 0f, 0f));
            return snowman;
        }

        public static Sculpt Sled()
        {
            var sled = new Sculpt();
            Color wood = new Color(0.72f, 0.45f, 0.25f);
            Color metal = new Color(0.75f, 0.2f, 0.2f);
            sled.Mirror(s =>
            {
                sled.Limb(V(s * 0.3f, 0.05f, -0.6f), V(s * 0.3f, 0.05f, 0.5f), 0.03f, 0.03f, metal, 0.02f);
                sled.Limb(V(s * 0.3f, 0.05f, 0.5f), V(s * 0.3f, 0.25f, 0.68f), 0.03f, 0.03f, metal, 0.02f);
                return sled.Limb(V(s * 0.3f, 0.05f, -0.3f), V(s * 0.3f, 0.22f, -0.3f), 0.025f, 0.025f, metal, 0.01f);
            });
            for (int i = 0; i < 5; i++)
            {
                sled.Box(V(0f, 0.25f, -0.5f + i * 0.24f), V(0.75f, 0.04f, 0.18f), 0.015f, wood, 0.01f);
            }
            Paint(sled, sled.Ellipsoid(V(0f, 0.3f, 0f), V(0.3f, 0.06f, 0.4f), Snow, 0.05f));
            return sled;
        }

        public static Sculpt Palm(int seed)
        {
            var palm = new Sculpt();
            var random = new System.Random(seed + 90);
            Color bark = new Color(0.6f, 0.45f, 0.3f);
            Color ring = new Color(0.45f, 0.32f, 0.2f);
            Color[] greens = { new Color(0.25f, 0.6f, 0.25f), new Color(0.32f, 0.68f, 0.28f), new Color(0.2f, 0.52f, 0.22f) };
            Color coconut = new Color(0.42f, 0.28f, 0.15f);

            float height = 4.2f + (float)random.NextDouble() * 1.4f;
            float bend = Range(random, 0.6f, 1.4f) * (seed % 2 == 0 ? 1f : -1f);
            const int segments = 6;
            Vector3 previous = V(0f, -0.1f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 point = V(bend * t * t, height * t, bend * 0.3f * t * t);
                palm.Limb(previous, point, Mathf.Lerp(0.3f, 0.17f, t - 1f / segments), Mathf.Lerp(0.3f, 0.16f, t), bark, 0.06f).noise = 0.015f;
                Paint(palm, palm.Sphere(point, Mathf.Lerp(0.29f, 0.17f, t) + 0.02f, ring, 0.04f));
                previous = point;
            }
            Vector3 crown = previous;
            for (int c = 0; c < 3; c++)
            {
                float angle = c * 2.1f;
                palm.Sphere(crown + V(Mathf.Cos(angle) * 0.2f, -0.2f, Mathf.Sin(angle) * 0.2f), 0.15f, coconut, 0.04f);
            }
            int fronds = 7;
            for (int f = 0; f < fronds; f++)
            {
                float angle = f * Mathf.PI * 2f / fronds + Range(random, -0.2f, 0.2f);
                var direction = V(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Color green = greens[(f + seed) % greens.Length];
                float yaw = -angle * Mathf.Rad2Deg;
                for (int k = 1; k <= 9; k++)
                {
                    float d = k * 0.24f;
                    float u = k / 9f;
                    Vector3 position = crown + direction * d + V(0f, 0.25f - 1.1f * u * u, 0f);
                    float droop = Mathf.Atan2(2.2f * u, 2.2f) * Mathf.Rad2Deg;
                    palm.Ellipsoid(position, V(0.24f, 0.055f, Mathf.Lerp(0.34f, 0.1f, u)), green, 0.14f, V(0f, yaw, -droop));
                }
            }
            return palm;
        }

        public static Sculpt Umbrella(int seed)
        {
            var umbrella = new Sculpt();
            Color[] colors = { new Color(0.95f, 0.3f, 0.3f), new Color(0.2f, 0.55f, 0.95f), new Color(0.98f, 0.75f, 0.2f) };
            Color main = colors[seed % colors.Length];
            Color white = new Color(0.97f, 0.97f, 0.95f);
            Color towel = colors[(seed + 1) % colors.Length];
            umbrella.Limb(V(0f, -0.2f, 0f), V(0f, 2.3f, 0f), 0.035f, 0.035f, white, 0.01f);
            umbrella.Ellipsoid(V(0f, 2.15f, 0f), V(1.35f, 0.42f, 1.35f), main, 0.03f);
            Carve(umbrella, umbrella.Ellipsoid(V(0f, 1.95f, 0f), V(1.3f, 0.4f, 1.3f), main, 0.02f));
            for (int k = 0; k < 4; k++)
            {
                Paint(umbrella, umbrella.Box(V(0f, 2.2f, 0f), V(0.42f, 1.2f, 3f), 0f, white, 0.02f, V(0f, k * 45f, 0f)));
            }
            umbrella.Sphere(V(0f, 2.58f, 0f), 0.06f, white, 0.02f);
            umbrella.Box(V(0.9f, 0.02f, 0.5f), V(0.75f, 0.03f, 1.6f), 0.01f, towel, 0.01f, V(0f, 15f, 0f));
            for (int i = 0; i < 4; i++)
            {
                Paint(umbrella, umbrella.Box(V(0.9f, 0.03f, -0.1f + i * 0.4f), V(0.8f, 0.05f, 0.12f), 0f, white, 0.005f, V(0f, 15f, 0f)));
            }
            if (seed % 2 == 0)
            {
                umbrella.Sphere(V(-0.7f, 0.22f, 0.6f), 0.22f, white, 0.02f);
                Paint(umbrella, umbrella.Box(V(-0.7f, 0.22f, 0.6f), V(0.12f, 0.5f, 0.5f), 0f, colors[(seed + 2) % colors.Length], 0.01f));
                Paint(umbrella, umbrella.Box(V(-0.7f, 0.22f, 0.6f), V(0.5f, 0.5f, 0.12f), 0f, main, 0.01f));
            }
            return umbrella;
        }

        public static Sculpt LifeguardTower()
        {
            var tower = new Sculpt();
            Color wood = new Color(0.95f, 0.95f, 0.92f);
            Color red = new Color(0.9f, 0.25f, 0.2f);
            Color dark = new Color(0.35f, 0.3f, 0.28f);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    tower.Limb(V(x * 0.75f, -0.2f, z * 0.75f), V(x * 0.55f, 1.6f, z * 0.55f), 0.07f, 0.07f, wood, 0.02f);
                }
            }
            tower.Box(V(0f, 1.65f, 0f), V(1.5f, 0.12f, 1.5f), 0.02f, wood, 0.02f);
            tower.Box(V(0f, 2.2f, -0.15f), V(1.2f, 1.0f, 0.95f), 0.03f, wood, 0.02f);
            Carve(tower, tower.Box(V(0f, 2.25f, 0.3f), V(0.8f, 0.5f, 0.3f), 0.02f, dark, 0.01f));
            tower.Box(V(0f, 2.8f, -0.1f), V(1.5f, 0.14f, 1.3f), 0.03f, red, 0.02f, V(-8f, 0f, 0f));
            for (int i = 0; i < 3; i++)
            {
                Paint(tower, tower.Box(V(-0.4f + i * 0.4f, 2.0f, 0.33f), V(0.2f, 0.4f, 0.1f), 0f, red, 0.01f));
            }
            for (int i = 0; i < 6; i++)
            {
                tower.Box(V(0f, 0.15f + i * 0.25f, 0.95f - i * 0.06f), V(0.55f, 0.04f, 0.14f), 0.01f, wood, 0.01f);
            }
            tower.Mirror(s => tower.Limb(V(s * 0.3f, -0.2f, 1.0f), V(s * 0.3f, 1.6f, 0.62f), 0.035f, 0.035f, wood, 0.01f));
            tower.Limb(V(0.6f, 2.8f, 0.4f), V(0.6f, 3.6f, 0.4f), 0.025f, 0.025f, dark, 0.01f);
            tower.Box(V(0.85f, 3.45f, 0.4f), V(0.5f, 0.3f, 0.02f), 0f, red, 0.005f);
            Paint(tower, tower.Box(V(0.85f, 3.45f, 0.4f), V(0.5f, 0.1f, 0.05f), 0f, new Color(0.98f, 0.85f, 0.2f), 0.005f));
            return tower;
        }

        public static Sculpt Starfish()
        {
            var star = new Sculpt();
            Color orange = new Color(1f, 0.55f, 0.25f);
            Color dot = new Color(1f, 0.82f, 0.55f);
            star.Ellipsoid(V(0f, 0.03f, 0f), V(0.09f, 0.04f, 0.09f), orange, 0.03f);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * Mathf.PI * 2f / 5f;
                var direction = V(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                star.Limb(V(0f, 0.03f, 0f), direction * 0.24f + V(0f, 0.02f, 0f), 0.06f, 0.02f, orange, 0.03f);
                Paint(star, star.Sphere(direction * 0.12f + V(0f, 0.07f, 0f), 0.015f, dot, 0.005f));
            }
            return star;
        }

        public static Sculpt SandCastle()
        {
            var castle = new Sculpt();
            Color sand = new Color(0.92f, 0.8f, 0.55f);
            Color wet = new Color(0.78f, 0.65f, 0.42f);
            Color flag = new Color(0.95f, 0.3f, 0.3f);
            castle.Box(V(0f, 0.15f, 0f), V(1f, 0.3f, 1f), 0.05f, wet, 0.06f).noise = 0.02f;
            castle.Cylinder(V(0f, 0.45f, 0f), 0.25f, 0.2f, 0.03f, sand, 0.04f, V(90f, 0f, 0f));
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    castle.Cylinder(V(x * 0.38f, 0.42f, z * 0.38f), 0.12f, 0.15f, 0.02f, sand, 0.03f, V(90f, 0f, 0f));
                    castle.Limb(V(x * 0.38f, 0.56f, z * 0.38f), V(x * 0.38f, 0.72f, z * 0.38f), 0.11f, 0.01f, sand, 0.02f);
                }
            }
            castle.Limb(V(0f, 0.65f, 0f), V(0f, 1.0f, 0f), 0.012f, 0.012f, new Color(0.5f, 0.35f, 0.2f), 0.005f);
            castle.Box(V(0.09f, 0.94f, 0f), V(0.16f, 0.1f, 0.012f), 0f, flag, 0.005f);
            return castle;
        }
    }
}
