using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public static class ChaseSculptor
    {
        public const float PolarBearLength = 5.6f;
        public const float ToyDragonLength = 6.2f;
        public const float SalamanderLength = 5.8f;
        public const float WolfLength = 3.2f;
        public const float ScorpionLength = 5.4f;
        public const float CatfishLength = 5.6f;
        public const float OctopusLength = 5.0f;

        static readonly Color EyeWhite = new Color(0.98f, 0.98f, 0.96f);

        static void Eyes(Sculpt sculpt, Vector3 right, float radius, Color iris, Vector3 look)
        {
            sculpt.Mirror(s => sculpt.Sphere(V(s * right.x, right.y, right.z), radius, EyeWhite, 0.03f));
            sculpt.Mirror(s => Paint(sculpt, sculpt.Sphere(V(s * right.x, right.y, right.z) + look * radius * 0.75f, radius * 0.55f, iris, 0.01f)));
            sculpt.Mirror(s => Paint(sculpt, sculpt.Sphere(V(s * right.x, right.y, right.z) + look * radius * 0.92f, radius * 0.3f, Ink, 0.005f)));
        }

        static void Chain(Sculpt sculpt, Vector3[] points, float startRadius, float endRadius, Color color, float blend = 0.08f)
        {
            int segments = points.Length - 1;
            for (int i = 0; i < segments; i++)
            {
                float a = Mathf.Lerp(startRadius, endRadius, i / (float)segments);
                float b = Mathf.Lerp(startRadius, endRadius, (i + 1) / (float)segments);
                sculpt.Limb(points[i], points[i + 1], a, b, color, blend);
            }
        }

        public static Sculpt PolarBear()
        {
            var bear = new Sculpt();
            Color fur = new Color(0.96f, 0.95f, 0.9f);
            Color shade = new Color(0.86f, 0.85f, 0.8f);
            Color nose = new Color(0.07f, 0.07f, 0.08f);

            bear.Ellipsoid(V(0f, -0.45f, -0.4f), V(1f, 0.85f, 2f), fur, 0.3f);
            bear.Sphere(V(0f, -0.1f, -2.55f), 0.25f, fur, 0.15f);
            bear.Limb(V(0f, 0.2f, 1.2f), V(0f, 0.55f, 1.9f), 0.6f, 0.5f, fur, 0.3f);
            bear.Ellipsoid(V(0f, 0.65f, 2.15f), V(0.55f, 0.5f, 0.55f), fur, 0.2f);
            bear.Ellipsoid(V(0f, 0.5f, 2.55f), V(0.28f, 0.24f, 0.25f), fur, 0.12f);
            bear.Sphere(V(0f, 0.6f, 2.72f), 0.08f, nose, 0.03f);
            Carve(bear, bear.Ellipsoid(V(0f, 0.38f, 2.62f), V(0.18f, 0.05f, 0.12f), Mouth, 0.03f));
            bear.Mirror(s => bear.Sphere(V(s * 0.38f, 1.08f, 2.05f), 0.14f, fur, 0.06f));
            bear.Mirror(s => Paint(bear, bear.Sphere(V(s * 0.38f, 1.1f, 2.12f), 0.08f, shade, 0.02f)));
            bear.Mirror(s => Paint(bear, bear.Sphere(V(s * 0.22f, 0.82f, 2.6f), 0.07f, Ink, 0.01f)));
            bear.Mirror(s => Paint(bear, bear.Sphere(V(s * 0.2f, 0.85f, 2.65f), 0.025f, EyeWhite, 0.005f)));
            bear.Mirror(s => bear.Limb(V(s * 0.75f, -0.1f, 1f), V(s * 0.95f, -0.3f, 1.7f), 0.28f, 0.22f, fur, 0.15f));
            bear.Mirror(s => bear.Ellipsoid(V(s * 0.95f, -0.3f, 1.7f), V(0.25f, 0.15f, 0.3f), fur, 0.08f));
            bear.Mirror(s =>
            {
                for (int i = -1; i <= 1; i++)
                {
                    Paint(bear, bear.Sphere(V(s * 0.95f + i * 0.09f, -0.18f, 1.97f), 0.04f, nose, 0.005f));
                }
                return Paint(bear, bear.Ellipsoid(V(s * 0.95f, -0.42f, 1.7f), V(0.15f, 0.04f, 0.18f), shade, 0.02f));
            });
            Paint(bear, bear.Ellipsoid(V(0f, -0.1f, 1.5f), V(0.55f, 0.4f, 0.45f), shade, 0.2f));
            return bear;
        }

        public static Sculpt ToyDragon()
        {
            var dragon = new Sculpt();
            Color purple = new Color(0.62f, 0.38f, 0.9f);
            Color pink = new Color(0.98f, 0.5f, 0.75f);
            Color yellow = new Color(1f, 0.86f, 0.3f);
            Color mint = new Color(0.5f, 0.92f, 0.78f);

            dragon.Ellipsoid(V(0f, -0.2f, 0f), V(1f, 0.9f, 1.6f), purple, 0.3f);
            Paint(dragon, dragon.Ellipsoid(V(0f, -0.45f, 0.35f), V(0.75f, 0.6f, 1.25f), yellow, 0.15f));
            for (int i = 0; i < 6; i++)
            {
                Paint(dragon, dragon.Torus(V(0f, -0.55f + i * 0.12f, 0.95f - i * 0.04f), 0.55f - Mathf.Abs(i - 2.5f) * 0.06f, 0.025f, new Color(0.95f, 0.72f, 0.22f), 0.01f, V(70f, 0f, 0f)));
            }
            dragon.Limb(V(0f, 0.3f, 1.1f), V(0f, 1f, 1.9f), 0.5f, 0.45f, purple, 0.25f);
            dragon.Ellipsoid(V(0f, 1.25f, 2.3f), V(0.65f, 0.6f, 0.65f), purple, 0.2f);
            dragon.Ellipsoid(V(0f, 1.05f, 2.85f), V(0.35f, 0.28f, 0.25f), purple, 0.15f);
            dragon.Mirror(s => Paint(dragon, dragon.Sphere(V(s * 0.12f, 1.12f, 3.07f), 0.05f, Ink, 0.01f)));
            Paint(dragon, dragon.Limb(V(-0.18f, 0.9f, 2.98f), V(0.18f, 0.9f, 2.98f), 0.035f, 0.035f, Mouth, 0.01f));
            dragon.Mirror(s => dragon.Sphere(V(s * 0.32f, 1.5f, 2.65f), 0.16f, EyeWhite, 0.03f));
            dragon.Mirror(s => Paint(dragon, dragon.Sphere(V(s * 0.32f, 1.5f, 2.78f), 0.08f, Ink, 0.01f)));
            dragon.Mirror(s => Paint(dragon, dragon.Sphere(V(s * 0.29f, 1.54f, 2.8f), 0.025f, EyeWhite, 0.005f)));
            dragon.Mirror(s => Paint(dragon, dragon.Sphere(V(s * 0.45f, 1.18f, 2.62f), 0.09f, pink, 0.04f)));
            dragon.Mirror(s => dragon.Limb(V(s * 0.28f, 1.75f, 2.2f), V(s * 0.38f, 2.15f, 2f), 0.1f, 0.06f, yellow, 0.05f));
            for (int i = 0; i < 5; i++)
            {
                float z = 1.7f - i * 0.65f;
                float y = 0.75f + Mathf.Sin(i * 0.7f) * 0.1f - i * 0.05f;
                dragon.Limb(V(0f, y - 0.1f, z), V(0f, y + 0.3f, z - 0.12f), 0.16f, 0.07f, i % 2 == 0 ? pink : mint, 0.05f);
            }
            Chain(dragon, new[] { V(0f, -0.1f, -1.4f), V(0.25f, -0.05f, -2f), V(0.1f, 0.1f, -2.5f), V(0f, 0.3f, -2.85f) }, 0.45f, 0.12f, purple, 0.15f);
            dragon.Ellipsoid(V(0f, 0.45f, -2.95f), V(0.3f, 0.25f, 0.15f), pink, 0.05f);
            dragon.Mirror(s => dragon.Ellipsoid(V(s * 0.95f, 0.7f, -0.2f), V(0.7f, 0.08f, 0.45f), pink, 0.12f, V(0f, 0f, s * 30f)));
            dragon.Mirror(s => Paint(dragon, dragon.Ellipsoid(V(s * 1.2f, 0.85f, -0.2f), V(0.3f, 0.1f, 0.3f), mint, 0.05f, V(0f, 0f, s * 30f))));
            dragon.Mirror(s => dragon.Ellipsoid(V(s * 0.55f, -0.85f, 0.9f), V(0.25f, 0.22f, 0.3f), purple, 0.12f));
            dragon.Mirror(s => dragon.Ellipsoid(V(s * 0.6f, -0.85f, -0.9f), V(0.28f, 0.24f, 0.32f), purple, 0.12f));
            return dragon;
        }

        public static Sculpt Salamander()
        {
            var salamander = new Sculpt();
            Color black = new Color(0.1f, 0.08f, 0.09f);
            Color orange = new Color(1f, 0.48f, 0.08f);
            Color yellow = new Color(1f, 0.82f, 0.15f);
            Color glow = new Color(1f, 0.6f, 0.18f);
            var random = new System.Random(23);

            salamander.Ellipsoid(V(0f, -0.25f, 0.2f), V(0.75f, 0.4f, 1.7f), black, 0.3f);
            salamander.Ellipsoid(V(0f, -0.15f, 2.2f), V(0.7f, 0.35f, 0.7f), black, 0.25f);
            Chain(salamander, new[] { V(0f, -0.25f, -1.3f), V(0.35f, -0.25f, -2f), V(-0.15f, -0.25f, -2.5f), V(0.1f, -0.25f, -2.8f) }, 0.42f, 0.1f, black, 0.15f);
            Paint(salamander, salamander.Ellipsoid(V(0f, -0.55f, 0.2f), V(0.65f, 0.2f, 1.6f), glow, 0.15f));
            Paint(salamander, salamander.Ellipsoid(V(0f, -0.4f, 2.2f), V(0.55f, 0.12f, 0.55f), glow, 0.1f));
            salamander.Mirror(s => salamander.Limb(V(s * 0.6f, -0.3f, 1.2f), V(s * 1.15f, -0.35f, 1.55f), 0.18f, 0.14f, black, 0.1f));
            salamander.Mirror(s => salamander.Ellipsoid(V(s * 1.2f, -0.38f, 1.6f), V(0.2f, 0.07f, 0.18f), black, 0.05f));
            salamander.Mirror(s => salamander.Limb(V(s * 0.6f, -0.3f, -0.8f), V(s * 1.1f, -0.35f, -1.3f), 0.18f, 0.14f, black, 0.1f));
            salamander.Mirror(s => salamander.Ellipsoid(V(s * 1.15f, -0.38f, -1.35f), V(0.2f, 0.07f, 0.18f), black, 0.05f));
            for (int i = 0; i < 18; i++)
            {
                float z = Mathf.Lerp(-2.3f, 2.3f, (float)random.NextDouble());
                float side = ((float)random.NextDouble() - 0.5f) * 0.9f;
                float lateral = z < -1.3f ? side * 0.5f + Mathf.Sin(z * 2.2f) * 0.2f : side;
                float height = z > 1.6f ? 0.15f : z < -1.3f ? -0.05f : 0.08f;
                float radius = 0.08f + (float)random.NextDouble() * 0.1f;
                Paint(salamander, salamander.Sphere(V(lateral, height, z), radius, i % 3 == 0 ? orange : yellow, 0.02f));
            }
            salamander.Mirror(s => salamander.Sphere(V(s * 0.4f, 0.15f, 2.45f), 0.15f, EyeWhite, 0.04f));
            salamander.Mirror(s => Paint(salamander, salamander.Sphere(V(s * 0.42f, 0.18f, 2.55f), 0.08f, Ink, 0.01f)));
            Carve(salamander, salamander.Ellipsoid(V(0f, -0.22f, 2.75f), V(0.5f, 0.05f, 0.15f), Mouth, 0.03f));
            return salamander;
        }

        public static Sculpt Wolf()
        {
            var wolf = new Sculpt();
            Color grey = new Color(0.5f, 0.52f, 0.56f);
            Color light = new Color(0.8f, 0.8f, 0.8f);
            Color dark = new Color(0.3f, 0.31f, 0.34f);
            Color amber = new Color(1f, 0.78f, 0.15f);

            wolf.Ellipsoid(V(0f, 0f, -0.1f), V(0.42f, 0.45f, 0.95f), grey, 0.2f);
            Paint(wolf, wolf.Ellipsoid(V(0f, -0.15f, 0.35f), V(0.36f, 0.32f, 0.5f), light, 0.12f));
            Paint(wolf, wolf.Ellipsoid(V(0f, 0.38f, -0.2f), V(0.3f, 0.12f, 0.8f), dark, 0.12f));
            wolf.Limb(V(0f, 0.15f, 0.6f), V(0f, 0.4f, 0.95f), 0.33f, 0.3f, grey, 0.15f);
            wolf.Ellipsoid(V(0f, 0.5f, 1.1f), V(0.32f, 0.3f, 0.34f), grey, 0.12f);
            wolf.Limb(V(0f, 0.42f, 1.25f), V(0f, 0.38f, 1.52f), 0.16f, 0.08f, grey, 0.08f);
            wolf.Limb(V(0f, 0.25f, 1.2f), V(0f, 0.24f, 1.45f), 0.1f, 0.06f, light, 0.05f);
            Carve(wolf, wolf.Ellipsoid(V(0f, 0.32f, 1.42f), V(0.1f, 0.05f, 0.14f), Mouth, 0.02f));
            wolf.Mirror(s => wolf.Limb(V(s * 0.06f, 0.33f, 1.38f), V(s * 0.06f, 0.29f, 1.39f), 0.025f, 0.01f, Tooth, 0.005f));
            wolf.Sphere(V(0f, 0.43f, 1.54f), 0.05f, Ink, 0.02f);
            wolf.Mirror(s => wolf.Limb(V(s * 0.18f, 0.75f, 1f), V(s * 0.22f, 0.98f, 0.95f), 0.09f, 0.04f, grey, 0.04f));
            wolf.Mirror(s => Paint(wolf, wolf.Limb(V(s * 0.18f, 0.78f, 1.04f), V(s * 0.21f, 0.92f, 1f), 0.05f, 0.02f, dark, 0.01f)));
            wolf.Mirror(s => Paint(wolf, wolf.Sphere(V(s * 0.15f, 0.6f, 1.38f), 0.06f, amber, 0.01f)));
            wolf.Mirror(s => Paint(wolf, wolf.Sphere(V(s * 0.15f, 0.6f, 1.42f), 0.03f, Ink, 0.005f)));
            wolf.Limb(V(0f, 0.1f, -0.95f), V(0f, 0.25f, -1.45f), 0.17f, 0.12f, grey, 0.08f);
            wolf.Sphere(V(0f, 0.27f, -1.5f), 0.1f, light, 0.05f);
            wolf.Mirror(s => wolf.Limb(V(s * 0.2f, -0.2f, 0.5f), V(s * 0.22f, -0.35f, 1f), 0.12f, 0.09f, grey, 0.06f));
            wolf.Mirror(s => wolf.Limb(V(s * 0.22f, -0.2f, -0.7f), V(s * 0.24f, -0.45f, -1.1f), 0.13f, 0.09f, grey, 0.06f));
            return wolf;
        }

        public static Sculpt Scorpion()
        {
            var scorpion = new Sculpt();
            Color amber = new Color(0.86f, 0.6f, 0.28f);
            Color dark = new Color(0.55f, 0.34f, 0.14f);
            Color sting = new Color(0.25f, 0.12f, 0.08f);

            scorpion.Ellipsoid(V(0f, -0.05f, 1.1f), V(0.8f, 0.38f, 0.6f), amber, 0.15f);
            for (int i = 0; i < 3; i++)
            {
                float z = 0.4f - i * 0.6f;
                scorpion.Ellipsoid(V(0f, -0.05f + i * 0.02f, z), V(0.85f - i * 0.08f, 0.4f, 0.5f), amber, 0.1f);
                Paint(scorpion, scorpion.Torus(V(0f, -0.05f, z - 0.42f), 0.6f - i * 0.06f, 0.04f, dark, 0.02f, V(90f, 0f, 0f)));
            }
            Chain(scorpion, new[] { V(0f, 0f, -1.3f), V(0f, 0.6f, -2.1f), V(0f, 1.6f, -2.415f), V(0f, 2.5f, -2.2f), V(0f, 2.9f, -1.5f) }, 0.35f, 0.22f, amber, 0.1f);
            scorpion.Ellipsoid(V(0f, 2.85f, -1.2f), V(0.24f, 0.24f, 0.3f), amber, 0.08f);
            scorpion.Limb(V(0f, 2.8f, -1f), V(0f, 2.45f, -0.7f), 0.14f, 0.05f, sting, 0.04f);
            scorpion.Mirror(s => scorpion.Limb(V(s * 0.7f, 0f, 1.3f), V(s * 1.2f, 0.1f, 1.9f), 0.2f, 0.18f, amber, 0.08f));
            scorpion.Mirror(s => scorpion.Limb(V(s * 1.2f, 0.1f, 1.9f), V(s * 0.95f, 0.15f, 2.25f), 0.18f, 0.18f, amber, 0.08f));
            scorpion.Mirror(s => scorpion.Ellipsoid(V(s * 0.85f, 0.15f, 2.4f), V(0.3f, 0.2f, 0.3f), amber, 0.08f));
            scorpion.Mirror(s => scorpion.Limb(V(s * 0.95f, 0.15f, 2.5f), V(s * 0.95f, 0.15f, 2.62f), 0.09f, 0.06f, dark, 0.04f));
            scorpion.Mirror(s => scorpion.Limb(V(s * 0.72f, 0.15f, 2.5f), V(s * 0.72f, 0.15f, 2.62f), 0.09f, 0.06f, dark, 0.04f));
            scorpion.Mirror(s =>
            {
                for (int i = 0; i < 4; i++)
                {
                    float z = 0.9f - i * 0.55f;
                    scorpion.Limb(V(s * 0.7f, -0.05f, z), V(s * 1.35f, 0.25f, z - 0.1f), 0.1f, 0.08f, dark, 0.05f);
                    scorpion.Limb(V(s * 1.35f, 0.25f, z - 0.1f), V(s * 1.6f, -0.5f, z - 0.15f), 0.08f, 0.06f, dark, 0.04f);
                }
                return scorpion.Sphere(V(s * 0.25f, 0.35f, 1.45f), 0.12f, EyeWhite, 0.03f);
            });
            scorpion.Mirror(s => Paint(scorpion, scorpion.Sphere(V(s * 0.25f, 0.37f, 1.53f), 0.06f, Ink, 0.01f)));
            scorpion.Mirror(s => Paint(scorpion, scorpion.Sphere(V(s * 0.1f, 0.33f, 1.6f), 0.04f, Ink, 0.01f)));
            return scorpion;
        }

        public static Sculpt Catfish()
        {
            var catfish = new Sculpt();
            Color body = new Color(0.27f, 0.3f, 0.2f);
            Color dark = new Color(0.16f, 0.18f, 0.12f);
            Color belly = new Color(0.68f, 0.66f, 0.5f);
            Color lip = new Color(0.45f, 0.42f, 0.3f);
            const float y = -0.35f;

            catfish.Ellipsoid(V(0f, y, 0f), V(0.85f, 0.6f, 2.2f), body, 0.3f);
            catfish.Ellipsoid(V(0f, y + 0.05f, 1.9f), V(0.95f, 0.55f, 0.8f), body, 0.3f);
            catfish.Ellipsoid(V(0f, y - 0.03f, 2.6f), V(0.7f, 0.2f, 0.2f), lip, 0.15f);
            Carve(catfish, catfish.Ellipsoid(V(0f, y - 0.05f, 2.7f), V(0.55f, 0.08f, 0.15f), Mouth, 0.04f));
            Paint(catfish, catfish.Ellipsoid(V(0f, y - 0.45f, 0.4f), V(0.7f, 0.25f, 2f), belly, 0.2f));
            Paint(catfish, catfish.Ellipsoid(V(0f, y + 0.5f, -0.2f), V(0.5f, 0.15f, 1.9f), dark, 0.2f));
            catfish.Mirror(s => catfish.Limb(V(s * 0.6f, y + 0.05f, 2.5f), V(s * 1.6f, y - 0.15f, 2f), 0.07f, 0.05f, dark, 0.03f));
            catfish.Mirror(s => catfish.Limb(V(s * 1.6f, y - 0.15f, 2f), V(s * 1.75f, y - 0.2f, 1.3f), 0.05f, 0.05f, dark, 0.03f));
            catfish.Mirror(s => catfish.Limb(V(s * 0.35f, y - 0.2f, 2.5f), V(s * 0.6f, y - 0.6f, 2.2f), 0.06f, 0.05f, dark, 0.03f));
            catfish.Mirror(s => catfish.Sphere(V(s * 0.62f, y + 0.25f, 2.1f), 0.1f, new Color(0.95f, 0.85f, 0.3f), 0.02f));
            catfish.Mirror(s => Paint(catfish, catfish.Sphere(V(s * 0.66f, y + 0.27f, 2.16f), 0.05f, Ink, 0.01f)));
            catfish.Ellipsoid(V(0f, y + 0.05f, -2.48f), V(0.08f, 0.6f, 0.32f), dark, 0.1f);
            catfish.Limb(V(0f, y + 0.45f, 0.6f), V(0f, y + 0.95f, 0.3f), 0.1f, 0.06f, dark, 0.06f);
            catfish.Mirror(s => catfish.Ellipsoid(V(s * 0.85f, y - 0.3f, 1.2f), V(0.45f, 0.06f, 0.22f), dark, 0.1f, V(0f, s * 25f, s * -25f)));
            return catfish;
        }

        public static Sculpt GiantOctopus()
        {
            var octopus = new Sculpt();
            Color skin = new Color(0.72f, 0.22f, 0.45f);
            Color deep = new Color(0.5f, 0.14f, 0.38f);
            Color sucker = new Color(0.98f, 0.72f, 0.75f);

            octopus.Ellipsoid(V(0f, 0.9f, -0.6f), V(1.1f, 1.3f, 1.2f), skin, 0.3f);
            Paint(octopus, octopus.Ellipsoid(V(0f, 1.6f, -0.9f), V(0.8f, 0.5f, 0.7f), deep, 0.2f));
            var random = new System.Random(5);
            for (int i = 0; i < 10; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float height = 0.6f + (float)random.NextDouble() * 1.3f;
                Vector3 spot = V(Mathf.Cos(angle) * 1f, height, -0.6f + Mathf.Sin(angle) * 1.05f);
                Paint(octopus, octopus.Sphere(spot, 0.1f + (float)random.NextDouble() * 0.06f, deep, 0.02f));
            }
            Eyes(octopus, V(0.55f, 0.6f, 0.45f), 0.32f, new Color(1f, 0.82f, 0.25f), V(0f, 0f, 1f));
            Carve(octopus, octopus.Ellipsoid(V(0f, 0.15f, 0.55f), V(0.25f, 0.08f, 0.12f), Mouth, 0.03f));

            float[] angles = { -100f, -60f, -25f, 25f, 60f, 100f, 150f, -150f };
            float[] reach = { 2f, 2f, 2.43f, 2.43f, 2f, 2f, 1.42f, 1.42f };
            for (int i = 0; i < angles.Length; i++)
            {
                float a = angles[i] * Mathf.Deg2Rad;
                Vector3 direction = V(Mathf.Sin(a), 0f, Mathf.Cos(a));
                Vector3 root = V(direction.x * 0.75f, -0.2f, -0.5f + direction.z * 0.75f);
                float length = reach[i];
                Vector3 side = Vector3.Cross(Vector3.up, direction) * (i % 2 == 0 ? 0.15f : -0.15f);
                var points = new[]
                {
                    root,
                    root + direction * length * 0.45f + V(0f, -0.1f, 0f) + side,
                    root + direction * length * 0.8f + side * 0.5f,
                    root + direction * length + V(0f, 0.35f, 0f)
                };
                Chain(octopus, points, 0.32f, 0.12f, skin, 0.1f);
                for (int k = 1; k < 4; k++)
                {
                    Vector3 cup = Vector3.Lerp(root, points[3], k / 4f) + V(0f, -0.2f, 0f);
                    Paint(octopus, octopus.Sphere(cup, 0.09f - k * 0.015f, sucker, 0.01f));
                }
            }
            return octopus;
        }
    }
}
