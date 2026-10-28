using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public enum HairStyle
    {
        Short,
        Bald,
        Long,
        Cap,
        Scarf,
        Curly,
        Beanie,
        SunHat
    }

    public struct PersonLook
    {
        public Color skin;
        public Color shirt;
        public Color pants;
        public Color hair;
        public Color shoe;
        public Color iris;
        public HairStyle style;
        public bool mustache;
        public float belly;
        public Color hat;
        public bool sunglasses;
    }

    public static class SpectatorSculptor
    {
        public static readonly PersonLook[] People =
        {
            new PersonLook { skin = new Color(0.94f, 0.76f, 0.62f), shirt = new Color(0.85f, 0.2f, 0.18f), pants = new Color(0.22f, 0.27f, 0.42f), hair = new Color(0.18f, 0.12f, 0.08f), shoe = new Color(0.15f, 0.12f, 0.1f), iris = new Color(0.3f, 0.2f, 0.12f), style = HairStyle.Short, mustache = true, belly = 1.15f },
            new PersonLook { skin = new Color(0.98f, 0.82f, 0.7f), shirt = new Color(0.98f, 0.82f, 0.25f), pants = new Color(0.35f, 0.3f, 0.26f), hair = new Color(0.55f, 0.32f, 0.15f), shoe = new Color(0.45f, 0.25f, 0.15f), iris = new Color(0.25f, 0.45f, 0.3f), style = HairStyle.Long, mustache = false, belly = 0.9f },
            new PersonLook { skin = new Color(0.72f, 0.5f, 0.36f), shirt = new Color(0.2f, 0.55f, 0.35f), pants = new Color(0.25f, 0.25f, 0.28f), hair = new Color(0.1f, 0.08f, 0.07f), shoe = new Color(0.9f, 0.9f, 0.88f), iris = new Color(0.22f, 0.14f, 0.08f), style = HairStyle.Cap, mustache = false, belly = 1f },
            new PersonLook { skin = new Color(0.9f, 0.7f, 0.58f), shirt = new Color(0.55f, 0.3f, 0.62f), pants = new Color(0.4f, 0.22f, 0.3f), hair = new Color(0.82f, 0.35f, 0.28f), shoe = new Color(0.2f, 0.15f, 0.15f), iris = new Color(0.3f, 0.22f, 0.15f), style = HairStyle.Scarf, mustache = false, belly = 1.05f },
            new PersonLook { skin = new Color(0.96f, 0.78f, 0.66f), shirt = new Color(0.3f, 0.5f, 0.85f), pants = new Color(0.6f, 0.52f, 0.38f), hair = new Color(0.75f, 0.73f, 0.7f), shoe = new Color(0.3f, 0.2f, 0.12f), iris = new Color(0.35f, 0.4f, 0.5f), style = HairStyle.Bald, mustache = true, belly = 1.3f },
            new PersonLook { skin = new Color(0.62f, 0.42f, 0.3f), shirt = new Color(0.95f, 0.55f, 0.2f), pants = new Color(0.2f, 0.3f, 0.5f), hair = new Color(0.12f, 0.09f, 0.08f), shoe = new Color(0.85f, 0.2f, 0.2f), iris = new Color(0.2f, 0.12f, 0.08f), style = HairStyle.Curly, mustache = false, belly = 0.95f }
        };

        internal static Color Shade(Color color, float amount)
        {
            return new Color(color.r * amount, color.g * amount, color.b * amount, 1f);
        }

        public static AvatarSculpture Person(PersonLook look)
        {
            var r = new AvatarSculpture();
            Sculpt b = r.body;
            float belly = look.belly;
            b.Ellipsoid(V(0f, 0.6f, 0.02f), V(0.3f * belly, 0.34f, 0.27f * belly), look.shirt, 0.1f);
            b.Ellipsoid(V(0f, 0.84f, 0f), V(0.29f, 0.2f, 0.22f), look.shirt, 0.1f);
            b.Ellipsoid(V(0f, 0.98f, 0.01f), V(0.11f, 0.08f, 0.1f), look.skin, 0.05f);
            Paint(b, b.Box(V(0f, 0.33f, 0.02f), V(1f, 0.3f, 1f), 0.02f, look.pants, 0.03f));
            Paint(b, b.Box(V(0f, 0.49f, 0.02f), V(1f, 0.035f, 1f), 0.01f, Shade(look.pants, 0.75f), 0.01f));
            Feet(b, look.pants, look.shoe, 0.15f, 0.11f);
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.17f, 0.06f, 0.08f), V(0.13f, 0.08f, 0.18f), look.shoe, 0.02f)));
            Paint(b, b.Ellipsoid(V(0f, 0.86f, 0.2f), V(0.08f, 0.1f, 0.05f), Shade(look.shirt, 0.8f), 0.02f, V(-15f, 0f, 0f)));

            Head(r.head, look);

            Arm(r.armLeft, -1f, 0.34f, 0.075f, look.skin, look.skin, look.shirt);
            Arm(r.armRight, 1f, 0.34f, 0.075f, look.skin, look.skin, look.shirt);
            Eye(r, 0.042f, look.iris);

            r.headPivot = V(0f, 1f, 0.02f);
            r.shoulder = V(0.3f, 0.88f, 0f);
            r.eyePosition = V(0.085f, 0.29f, 0.19f);
            return r;
        }

        internal static void Head(Sculpt h, PersonLook look)
        {
            h.Ellipsoid(V(0f, 0.25f, 0f), V(0.23f, 0.26f, 0.23f), look.skin, 0.05f);
            h.Ellipsoid(V(0f, 0.11f, 0.06f), V(0.2f, 0.14f, 0.17f), look.skin, 0.08f);
            h.Ellipsoid(V(0f, 0.21f, 0.24f), V(0.05f, 0.055f, 0.055f), Shade(look.skin, 0.95f), 0.03f);
            h.Mirror(s => h.Ellipsoid(V(s * 0.23f, 0.22f, 0f), V(0.04f, 0.065f, 0.05f), look.skin, 0.03f));
            Smile(h, V(0f, 0.08f, 0.21f), 0.065f, 0.018f, 0.017f, 0.045f, Mouth);
            h.Mirror(s => h.Ellipsoid(V(s * 0.09f, 0.37f, 0.19f), V(0.055f, 0.016f, 0.025f), look.hair, 0.012f, V(0f, 0f, s * -8f)));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.15f, 0.15f, 0.18f), V(0.05f, 0.03f, 0.04f), Blush, 0.03f)));
            if (look.mustache)
            {
                h.Mirror(s => h.Limb(V(s * 0.015f, 0.14f, 0.255f), V(s * 0.12f, 0.1f, 0.21f), 0.035f, 0.016f, look.hair, 0.02f));
            }
            Hair(h, look);
            if (look.sunglasses)
            {
                h.Mirror(s => h.Ellipsoid(V(s * 0.085f, 0.29f, 0.236f), V(0.078f, 0.052f, 0.03f), Ink, 0.01f));
                h.Limb(V(-0.02f, 0.3f, 0.25f), V(0.02f, 0.3f, 0.25f), 0.012f, 0.012f, Ink, 0.005f);
                h.Mirror(s => h.Limb(V(s * 0.16f, 0.3f, 0.2f), V(s * 0.225f, 0.3f, 0.02f), 0.012f, 0.012f, Ink, 0.005f));
            }
        }

        static void Hair(Sculpt h, PersonLook look)
        {
            switch (look.style)
            {
                case HairStyle.Short:
                    h.Ellipsoid(V(0f, 0.36f, -0.04f), V(0.24f, 0.17f, 0.23f), look.hair, 0.04f).noise = 0.01f;
                    h.Ellipsoid(V(0.05f, 0.44f, 0.1f), V(0.16f, 0.06f, 0.1f), look.hair, 0.04f, V(0f, 0f, -10f));
                    break;
                case HairStyle.Bald:
                    h.Ellipsoid(V(0f, 0.22f, -0.12f), V(0.22f, 0.1f, 0.14f), look.hair, 0.04f);
                    h.Mirror(s => h.Ellipsoid(V(s * 0.2f, 0.26f, -0.02f), V(0.05f, 0.08f, 0.1f), look.hair, 0.03f));
                    break;
                case HairStyle.Long:
                    h.Ellipsoid(V(0f, 0.37f, -0.03f), V(0.25f, 0.18f, 0.24f), look.hair, 0.04f);
                    h.Ellipsoid(V(0f, 0.12f, -0.14f), V(0.24f, 0.3f, 0.12f), look.hair, 0.06f).noise = 0.012f;
                    h.Mirror(s => h.Ellipsoid(V(s * 0.21f, 0.14f, -0.02f), V(0.05f, 0.22f, 0.09f), look.hair, 0.04f));
                    h.Ellipsoid(V(0.07f, 0.43f, 0.14f), V(0.14f, 0.05f, 0.08f), look.hair, 0.03f, V(0f, 0f, -18f));
                    break;
                case HairStyle.Cap:
                    h.Ellipsoid(V(0f, 0.38f, -0.02f), V(0.245f, 0.16f, 0.24f), Shade(look.shirt, 0.85f), 0.03f);
                    h.Box(V(0f, 0.36f, 0.24f), V(0.3f, 0.025f, 0.18f), 0.01f, Shade(look.shirt, 0.7f), 0.02f, V(-8f, 0f, 0f));
                    h.Sphere(V(0f, 0.53f, -0.02f), 0.025f, Shade(look.shirt, 0.7f), 0.01f);
                    break;
                case HairStyle.Scarf:
                    h.Ellipsoid(V(0f, 0.34f, -0.07f), V(0.26f, 0.25f, 0.21f), look.hair, 0.04f);
                    h.Mirror(s => h.Ellipsoid(V(s * 0.2f, 0.17f, -0.01f), V(0.07f, 0.18f, 0.12f), look.hair, 0.04f));
                    h.Ellipsoid(V(0f, 0.02f, -0.04f), V(0.2f, 0.07f, 0.15f), look.hair, 0.05f);
                    h.Ellipsoid(V(0f, 0.42f, 0.12f), V(0.2f, 0.07f, 0.1f), look.hair, 0.04f);
                    Paint(h, h.Ellipsoid(V(0f, 0.38f, 0.17f), V(0.2f, 0.02f, 0.06f), Shade(look.hair, 0.7f), 0.01f));
                    break;
                case HairStyle.Beanie:
                    h.Ellipsoid(V(0f, 0.37f, -0.02f), V(0.255f, 0.2f, 0.25f), look.hat, 0.04f).noise = 0.006f;
                    h.Torus(V(0f, 0.3f, -0.01f), 0.235f, 0.04f, Shade(look.hat, 0.8f), 0.03f);
                    h.Sphere(V(0f, 0.6f, -0.03f), 0.08f, Color.Lerp(look.hat, Color.white, 0.75f), 0.02f).noise = 0.015f;
                    h.Mirror(s => h.Ellipsoid(V(s * 0.21f, 0.2f, -0.02f), V(0.05f, 0.1f, 0.08f), look.hat, 0.03f));
                    break;
                case HairStyle.SunHat:
                    h.Ellipsoid(V(0f, 0.33f, -0.05f), V(0.24f, 0.15f, 0.23f), look.hair, 0.04f);
                    h.Cylinder(V(0f, 0.4f, 0f), 0.42f, 0.014f, 0.01f, look.hat, 0.02f, V(90f, 0f, 0f));
                    h.Ellipsoid(V(0f, 0.47f, -0.01f), V(0.22f, 0.13f, 0.21f), look.hat, 0.03f);
                    Paint(h, h.Torus(V(0f, 0.43f, -0.01f), 0.215f, 0.025f, Shade(look.shirt, 0.9f), 0.01f));
                    break;
                default:
                    for (int i = 0; i < 9; i++)
                    {
                        float angle = i * 0.7f;
                        h.Sphere(V(Mathf.Cos(angle) * 0.15f, 0.42f + (i % 3) * 0.03f, Mathf.Sin(angle) * 0.13f - 0.04f), 0.1f, look.hair, 0.03f).noise = 0.01f;
                    }
                    break;
            }
        }

        public static AvatarSculpture Dog()
        {
            var r = new AvatarSculpture();
            Color fur = new Color(0.82f, 0.62f, 0.38f);
            Color light = new Color(0.97f, 0.9f, 0.78f);
            Color dark = new Color(0.38f, 0.24f, 0.14f);
            Color nose = new Color(0.1f, 0.08f, 0.08f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.42f, -0.05f), V(0.26f, 0.3f, 0.28f), fur, 0.1f).noise = 0.01f;
            b.Ellipsoid(V(0f, 0.68f, 0.02f), V(0.2f, 0.2f, 0.18f), fur, 0.1f);
            Paint(b, b.Ellipsoid(V(0f, 0.55f, 0.16f), V(0.15f, 0.22f, 0.1f), light, 0.05f));
            b.Mirror(s => b.Ellipsoid(V(s * 0.17f, 0.12f, 0.02f), V(0.12f, 0.12f, 0.2f), fur, 0.06f));
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.17f, 0.03f, 0.16f), V(0.09f, 0.04f, 0.08f), light, 0.02f)));
            b.Limb(V(0f, 0.25f, -0.28f), V(0f, 0.55f, -0.5f), 0.05f, 0.03f, fur, 0.05f);
            Paint(b, b.Sphere(V(0f, 0.56f, -0.5f), 0.04f, light, 0.02f));
            Paint(b, b.Torus(V(0f, 0.78f, 0f), 0.13f, 0.02f, new Color(0.85f, 0.15f, 0.15f), 0.01f));

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.2f, 0f), V(0.2f, 0.19f, 0.19f), fur, 0.05f);
            h.Ellipsoid(V(0f, 0.12f, 0.17f), V(0.1f, 0.08f, 0.12f), light, 0.06f);
            h.Sphere(V(0f, 0.16f, 0.29f), 0.035f, nose, 0.01f);
            Smile(h, V(0f, 0.07f, 0.24f), 0.05f, 0.015f, 0.014f, 0.04f, Mouth);
            h.Mirror(s => h.Ellipsoid(V(s * 0.2f, 0.18f, -0.02f), V(0.05f, 0.14f, 0.09f), dark, 0.03f, V(0f, 0f, s * 18f)));
            Paint(h, h.Ellipsoid(V(0.08f, 0.3f, 0.1f), V(0.08f, 0.07f, 0.09f), dark, 0.03f));

            Arm(r.armLeft, -1f, 0.26f, 0.06f, fur, light);
            Arm(r.armRight, 1f, 0.26f, 0.06f, fur, light);
            Eye(r, 0.035f, new Color(0.25f, 0.15f, 0.08f), 0.8f);

            r.headPivot = V(0f, 0.82f, 0.05f);
            r.shoulder = V(0.18f, 0.68f, 0.06f);
            r.eyePosition = V(0.075f, 0.24f, 0.15f);
            return r;
        }

        public static AvatarSculpture Sheep()
        {
            var r = new AvatarSculpture();
            Color wool = new Color(0.95f, 0.94f, 0.9f);
            Color face = new Color(0.2f, 0.17f, 0.16f);
            Color inner = new Color(0.86f, 0.6f, 0.6f);

            Sculpt b = r.body;
            var random = new System.Random(5);
            for (int i = 0; i < 14; i++)
            {
                float angle = i * 2.4f;
                float y = 0.3f + (i % 5) * 0.12f;
                float radius = 0.18f + (float)random.NextDouble() * 0.05f;
                b.Sphere(V(Mathf.Cos(angle) * 0.18f, y, Mathf.Sin(angle) * 0.16f), radius, wool, 0.08f).noise = 0.015f;
            }
            b.Mirror(s => b.Limb(V(s * 0.12f, 0.25f, 0f), V(s * 0.13f, 0.02f, 0.04f), 0.05f, 0.045f, face, 0.03f));

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.16f, 0.04f), V(0.13f, 0.17f, 0.15f), face, 0.05f);
            h.Ellipsoid(V(0f, 0.08f, 0.15f), V(0.08f, 0.07f, 0.07f), face, 0.05f);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 1.05f;
                h.Sphere(V(Mathf.Cos(angle) * 0.1f, 0.32f, Mathf.Sin(angle) * 0.08f - 0.02f), 0.08f, wool, 0.04f).noise = 0.01f;
            }
            h.Mirror(s => h.Ellipsoid(V(s * 0.16f, 0.2f, 0f), V(0.09f, 0.035f, 0.05f), face, 0.02f, V(0f, 0f, s * -20f)));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.17f, 0.2f, 0.02f), V(0.06f, 0.02f, 0.03f), inner, 0.01f, V(0f, 0f, s * -20f))));
            Smile(h, V(0f, 0.04f, 0.2f), 0.04f, 0.012f, 0.012f, 0.03f, Mouth);

            Arm(r.armLeft, -1f, 0.24f, 0.05f, face, face, wool);
            Arm(r.armRight, 1f, 0.24f, 0.05f, face, face, wool);
            Eye(r, 0.032f, new Color(0.55f, 0.45f, 0.2f), 0.7f);

            r.headPivot = V(0f, 0.78f, 0.12f);
            r.shoulder = V(0.22f, 0.62f, 0.05f);
            r.eyePosition = V(0.065f, 0.2f, 0.14f);
            return r;
        }

        public static AvatarSculpture Duck()
        {
            var r = new AvatarSculpture();
            Color white = new Color(0.97f, 0.97f, 0.94f);
            Color beak = new Color(1f, 0.62f, 0.12f);
            Color shade = new Color(0.85f, 0.86f, 0.84f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.38f, -0.04f), V(0.24f, 0.28f, 0.28f), white, 0.1f);
            b.Ellipsoid(V(0f, 0.64f, 0.02f), V(0.13f, 0.18f, 0.13f), white, 0.1f);
            b.Ellipsoid(V(0f, 0.42f, -0.32f), V(0.1f, 0.08f, 0.1f), shade, 0.06f, V(-30f, 0f, 0f));
            b.Mirror(s => b.Limb(V(s * 0.1f, 0.14f, 0f), V(s * 0.11f, 0.03f, 0.03f), 0.03f, 0.03f, beak, 0.02f));
            b.Mirror(s => b.Ellipsoid(V(s * 0.12f, 0.015f, 0.1f), V(0.08f, 0.015f, 0.1f), beak, 0.01f));

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.15f, 0f), V(0.15f, 0.15f, 0.15f), white, 0.05f);
            h.Ellipsoid(V(0f, 0.1f, 0.17f), V(0.08f, 0.03f, 0.1f), beak, 0.02f);
            h.Ellipsoid(V(0f, 0.07f, 0.16f), V(0.07f, 0.02f, 0.08f), Shade(beak, 0.9f), 0.02f);
            Paint(h, h.Sphere(V(0f, 0.31f, -0.02f), 0.05f, shade, 0.03f));

            Arm(r.armLeft, -1f, 0.22f, 0.07f, white, shade);
            Arm(r.armRight, 1f, 0.22f, 0.07f, white, shade);
            Eye(r, 0.03f, new Color(0.15f, 0.1f, 0.08f), 0.8f);

            r.headPivot = V(0f, 0.76f, 0.05f);
            r.shoulder = V(0.2f, 0.55f, 0.02f);
            r.eyePosition = V(0.075f, 0.19f, 0.11f);
            return r;
        }

        public static Sculpt Sign()
        {
            var sign = new Sculpt();
            Color board = new Color(0.97f, 0.94f, 0.86f);
            Color frame = new Color(0.55f, 0.36f, 0.2f);
            sign.Limb(V(0f, -0.1f, -0.03f), V(0f, 0.75f, -0.03f), 0.025f, 0.022f, frame, 0.01f);
            sign.Box(V(0f, 0.75f, 0f), V(0.9f, 0.46f, 0.035f), 0.02f, board, 0.01f);
            Paint(sign, sign.Box(V(0f, 0.75f, 0f), V(0.92f, 0.48f, 0.05f), 0.02f, frame, 0.005f));
            Paint(sign, sign.Box(V(0f, 0.75f, 0f), V(0.84f, 0.4f, 0.07f), 0.01f, board, 0.005f));
            return sign;
        }

        public static Sculpt BeanBowl()
        {
            var bowl = new Sculpt();
            Color ceramic = new Color(0.88f, 0.42f, 0.2f);
            Color bean = new Color(0.62f, 0.2f, 0.12f);
            bowl.Ellipsoid(V(0f, 0f, 0f), V(0.13f, 0.07f, 0.13f), ceramic, 0.02f);
            Carve(bowl, bowl.Ellipsoid(V(0f, 0.05f, 0f), V(0.11f, 0.06f, 0.11f), ceramic, 0.02f));
            for (int i = 0; i < 9; i++)
            {
                float angle = i * 2.4f;
                float radius = 0.08f * Mathf.Sqrt((i + 0.5f) / 9f);
                bowl.Ellipsoid(V(Mathf.Cos(angle) * radius, 0.035f, Mathf.Sin(angle) * radius), V(0.022f, 0.014f, 0.014f), bean, 0.008f, V(0f, i * 40f, 0f));
            }
            Paint(bowl, bowl.Torus(V(0f, 0.04f, 0f), 0.12f, 0.008f, Color.white, 0.004f));
            return bowl;
        }

        public static Sculpt PastirmaBread()
        {
            var bread = new Sculpt();
            Color crust = new Color(0.82f, 0.58f, 0.3f);
            Color meat = new Color(0.62f, 0.12f, 0.12f);
            Color spice = new Color(0.68f, 0.28f, 0.12f);
            bread.Ellipsoid(V(0f, 0f, 0f), V(0.07f, 0.045f, 0.16f), crust, 0.02f);
            bread.Ellipsoid(V(0f, 0.045f, 0f), V(0.075f, 0.02f, 0.15f), meat, 0.01f);
            Paint(bread, bread.Ellipsoid(V(0f, 0.05f, 0f), V(0.08f, 0.01f, 0.155f), spice, 0.006f));
            bread.Ellipsoid(V(0f, 0.08f, 0f), V(0.068f, 0.035f, 0.15f), crust, 0.02f);
            return bread;
        }

        public static Sculpt Simit()
        {
            var simit = new Sculpt();
            Color crust = new Color(0.66f, 0.36f, 0.16f);
            Color sesame = new Color(0.96f, 0.88f, 0.62f);
            simit.Torus(Vector3.zero, 0.1f, 0.035f, crust, 0.01f, V(90f, 0f, 0f));
            for (int i = 0; i < 18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f;
                Vector3 p = V(Mathf.Cos(angle) * 0.1f, Mathf.Sin(angle) * 0.1f, 0.03f + (i % 2) * 0.005f);
                Paint(simit, simit.Sphere(p, 0.008f, sesame, 0.002f));
            }
            return simit;
        }
    }
}
