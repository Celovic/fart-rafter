using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public enum SwimWear
    {
        Trunks,
        Swimsuit,
        Bikini
    }

    public struct WinterLook
    {
        public PersonLook person;
        public Color coat;
        public Color scarf;
        public Color glove;
        public Color boot;
    }

    public struct BeachLook
    {
        public PersonLook person;
        public SwimWear wear;
        public Color swim;
        public Color sandal;
    }

    public static class BiomeCrowdSculptor
    {
        public static readonly WinterLook[] WinterPeople =
        {
            new WinterLook { person = new PersonLook { skin = new Color(0.94f, 0.76f, 0.62f), hair = new Color(0.18f, 0.12f, 0.08f), iris = new Color(0.3f, 0.2f, 0.12f), style = HairStyle.Beanie, hat = new Color(0.85f, 0.18f, 0.2f), mustache = true, belly = 1.1f, pants = new Color(0.2f, 0.22f, 0.3f) }, coat = new Color(0.15f, 0.35f, 0.7f), scarf = new Color(0.95f, 0.82f, 0.2f), glove = new Color(0.85f, 0.18f, 0.2f), boot = new Color(0.3f, 0.2f, 0.12f) },
            new WinterLook { person = new PersonLook { skin = new Color(0.98f, 0.84f, 0.74f), hair = new Color(0.62f, 0.38f, 0.18f), iris = new Color(0.25f, 0.45f, 0.3f), style = HairStyle.Beanie, hat = new Color(0.95f, 0.95f, 0.95f), belly = 0.9f, pants = new Color(0.3f, 0.25f, 0.35f) }, coat = new Color(0.85f, 0.25f, 0.45f), scarf = new Color(0.98f, 0.98f, 0.98f), glove = new Color(0.98f, 0.98f, 0.98f), boot = new Color(0.9f, 0.88f, 0.84f) },
            new WinterLook { person = new PersonLook { skin = new Color(0.7f, 0.5f, 0.36f), hair = new Color(0.1f, 0.08f, 0.07f), iris = new Color(0.22f, 0.14f, 0.08f), style = HairStyle.Beanie, hat = new Color(0.2f, 0.55f, 0.35f), belly = 1f, pants = new Color(0.18f, 0.18f, 0.2f) }, coat = new Color(0.95f, 0.55f, 0.15f), scarf = new Color(0.2f, 0.55f, 0.35f), glove = new Color(0.15f, 0.15f, 0.18f), boot = new Color(0.15f, 0.12f, 0.1f) },
            new WinterLook { person = new PersonLook { skin = new Color(0.9f, 0.72f, 0.6f), hair = new Color(0.75f, 0.73f, 0.7f), iris = new Color(0.35f, 0.4f, 0.5f), style = HairStyle.Beanie, hat = new Color(0.45f, 0.3f, 0.6f), mustache = true, belly = 1.3f, pants = new Color(0.35f, 0.3f, 0.25f) }, coat = new Color(0.25f, 0.45f, 0.3f), scarf = new Color(0.8f, 0.2f, 0.25f), glove = new Color(0.45f, 0.3f, 0.6f), boot = new Color(0.25f, 0.18f, 0.12f) }
        };

        public static readonly BeachLook[] BeachPeople =
        {
            new BeachLook { person = new PersonLook { skin = new Color(0.94f, 0.74f, 0.6f), hair = new Color(0.18f, 0.12f, 0.08f), iris = new Color(0.3f, 0.2f, 0.12f), style = HairStyle.Short, mustache = true, belly = 1.2f, sunglasses = true }, wear = SwimWear.Trunks, swim = new Color(0.95f, 0.45f, 0.15f), sandal = new Color(0.2f, 0.45f, 0.85f) },
            new BeachLook { person = new PersonLook { skin = new Color(0.98f, 0.82f, 0.7f), hair = new Color(0.85f, 0.65f, 0.3f), iris = new Color(0.25f, 0.45f, 0.6f), style = HairStyle.SunHat, hat = new Color(0.95f, 0.85f, 0.55f), shirt = new Color(0.9f, 0.3f, 0.4f), belly = 0.9f }, wear = SwimWear.Swimsuit, swim = new Color(0.15f, 0.6f, 0.75f), sandal = new Color(0.95f, 0.4f, 0.5f) },
            new BeachLook { person = new PersonLook { skin = new Color(0.7f, 0.5f, 0.36f), hair = new Color(0.1f, 0.08f, 0.07f), iris = new Color(0.22f, 0.14f, 0.08f), style = HairStyle.Curly, belly = 1f }, wear = SwimWear.Trunks, swim = new Color(0.2f, 0.65f, 0.35f), sandal = new Color(0.95f, 0.9f, 0.85f) },
            new BeachLook { person = new PersonLook { skin = new Color(0.9f, 0.7f, 0.58f), hair = new Color(0.55f, 0.25f, 0.15f), iris = new Color(0.3f, 0.22f, 0.15f), style = HairStyle.Long, belly = 0.95f, sunglasses = true }, wear = SwimWear.Bikini, swim = new Color(0.95f, 0.75f, 0.2f), sandal = new Color(0.85f, 0.2f, 0.3f) },
            new BeachLook { person = new PersonLook { skin = new Color(0.96f, 0.78f, 0.66f), hair = new Color(0.75f, 0.73f, 0.7f), iris = new Color(0.35f, 0.4f, 0.5f), style = HairStyle.SunHat, hat = new Color(0.92f, 0.82f, 0.5f), shirt = new Color(0.25f, 0.35f, 0.7f), mustache = true, belly = 1.3f }, wear = SwimWear.Trunks, swim = new Color(0.2f, 0.3f, 0.75f), sandal = new Color(0.4f, 0.28f, 0.18f) }
        };

        public static AvatarSculpture Winter(WinterLook look)
        {
            var r = new AvatarSculpture();
            PersonLook person = look.person;
            Sculpt b = r.body;
            float belly = person.belly;
            b.Ellipsoid(V(0f, 0.6f, 0.02f), V(0.35f * belly, 0.37f, 0.31f * belly), look.coat, 0.12f).noise = 0.01f;
            b.Ellipsoid(V(0f, 0.86f, 0f), V(0.33f, 0.21f, 0.25f), look.coat, 0.12f);
            for (int i = 0; i < 3; i++)
            {
                float y = 0.46f + i * 0.15f;
                Paint(b, b.Torus(V(0f, y, 0.02f), 0.33f * belly - Mathf.Abs(y - 0.6f) * 0.25f, 0.014f, SpectatorSculptor.Shade(look.coat, 0.78f), 0.01f));
            }
            Paint(b, b.Box(V(0f, 0.66f, 0.3f * belly), V(0.03f, 0.56f, 0.12f), 0.01f, SpectatorSculptor.Shade(look.coat, 0.55f), 0.01f));
            b.Ellipsoid(V(0f, 0.98f, 0.01f), V(0.11f, 0.08f, 0.1f), person.skin, 0.05f);
            b.Torus(V(0f, 0.97f, 0.01f), 0.13f, 0.055f, look.scarf, 0.04f);
            b.Limb(V(0.08f, 0.95f, 0.13f), V(0.12f, 0.7f, 0.25f), 0.05f, 0.045f, look.scarf, 0.03f);
            Paint(b, b.Box(V(0.12f, 0.72f, 0.25f), V(0.14f, 0.04f, 0.14f), 0.01f, SpectatorSculptor.Shade(look.scarf, 0.75f), 0.01f));
            Paint(b, b.Box(V(0f, 0.3f, 0.02f), V(1f, 0.26f, 1f), 0.02f, person.pants, 0.03f));
            Feet(b, person.pants, look.boot, 0.15f, 0.12f);
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.17f, 0.08f, 0.08f), V(0.15f, 0.13f, 0.2f), look.boot, 0.02f)));

            SpectatorSculptor.Head(r.head, person);

            Arm(r.armLeft, -1f, 0.34f, 0.09f, look.glove, look.glove, look.coat);
            Arm(r.armRight, 1f, 0.34f, 0.09f, look.glove, look.glove, look.coat);
            Eye(r, 0.042f, person.iris);

            r.headPivot = V(0f, 1f, 0.02f);
            r.shoulder = V(0.33f, 0.88f, 0f);
            r.eyePosition = V(0.085f, 0.29f, 0.19f);
            return r;
        }

        public static AvatarSculpture Beach(BeachLook look)
        {
            var r = new AvatarSculpture();
            PersonLook person = look.person;
            Sculpt b = r.body;
            float belly = person.belly;
            b.Ellipsoid(V(0f, 0.6f, 0.02f), V(0.3f * belly, 0.34f, 0.27f * belly), person.skin, 0.1f);
            b.Ellipsoid(V(0f, 0.84f, 0f), V(0.29f, 0.2f, 0.22f), person.skin, 0.1f);
            b.Ellipsoid(V(0f, 0.98f, 0.01f), V(0.11f, 0.08f, 0.1f), person.skin, 0.05f);
            Paint(b, b.Sphere(V(0f, 0.55f, 0.27f * belly), 0.015f, SpectatorSculptor.Shade(person.skin, 0.7f), 0.005f));
            switch (look.wear)
            {
                case SwimWear.Swimsuit:
                    Paint(b, b.Box(V(0f, 0.55f, 0.02f), V(1f, 0.48f, 1f), 0.02f, look.swim, 0.02f));
                    b.Mirror(s => Paint(b, b.Box(V(s * 0.12f, 0.88f, 0.02f), V(0.07f, 0.2f, 0.7f), 0.01f, look.swim, 0.01f)));
                    Paint(b, b.Box(V(0f, 0.3f, 0.02f), V(1f, 0.05f, 1f), 0.01f, look.swim, 0.01f));
                    break;
                case SwimWear.Bikini:
                    Paint(b, b.Box(V(0f, 0.76f, 0.02f), V(1f, 0.11f, 1f), 0.01f, look.swim, 0.015f));
                    b.Mirror(s => Paint(b, b.Box(V(s * 0.12f, 0.88f, 0.02f), V(0.05f, 0.18f, 0.7f), 0.01f, look.swim, 0.01f)));
                    Paint(b, b.Box(V(0f, 0.36f, 0.02f), V(1f, 0.15f, 1f), 0.01f, look.swim, 0.015f));
                    break;
                default:
                    Paint(b, b.Box(V(0f, 0.33f, 0.02f), V(1f, 0.3f, 1f), 0.02f, look.swim, 0.03f));
                    Paint(b, b.Box(V(0f, 0.47f, 0.02f), V(1f, 0.03f, 1f), 0.01f, Color.white, 0.01f));
                    b.Mirror(s => Paint(b, b.Box(V(s * 0.31f * belly, 0.33f, 0.02f), V(0.03f, 0.3f, 0.3f), 0.01f, Color.white, 0.01f)));
                    break;
            }
            Feet(b, person.skin, look.sandal, 0.15f, 0.1f);
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.17f, 0.012f, 0.1f), V(0.12f, 0.03f, 0.16f), look.sandal, 0.01f)));
            b.Mirror(s => Paint(b, b.Limb(V(s * 0.12f, 0.06f, 0.18f), V(s * 0.22f, 0.06f, 0.18f), 0.012f, 0.012f, look.sandal, 0.005f)));

            SpectatorSculptor.Head(r.head, person);

            Arm(r.armLeft, -1f, 0.34f, 0.075f, person.skin, person.skin);
            Arm(r.armRight, 1f, 0.34f, 0.075f, person.skin, person.skin);
            Eye(r, 0.042f, person.iris);

            r.headPivot = V(0f, 1f, 0.02f);
            r.shoulder = V(0.3f, 0.88f, 0f);
            r.eyePosition = V(0.085f, 0.29f, 0.19f);
            return r;
        }

        static void Flipper(Sculpt arm, float side, float length, float width, Color color, Color tip)
        {
            arm.Sphere(V(0f, 0f, 0f), width * 0.9f, color, 0.02f);
            arm.Ellipsoid(V(side * 0.02f, -length * 0.5f, 0.02f), V(width * 0.45f, length * 0.55f, width), color, 0.04f, V(0f, 0f, side * -8f));
            Paint(arm, arm.Ellipsoid(V(side * 0.03f, -length * 0.95f, 0.02f), V(width * 0.5f, length * 0.2f, width * 1.05f), tip, 0.03f));
        }

        public static AvatarSculpture Penguin()
        {
            var r = new AvatarSculpture();
            Color black = new Color(0.1f, 0.11f, 0.14f);
            Color white = new Color(0.96f, 0.96f, 0.94f);
            Color orange = new Color(1f, 0.6f, 0.15f);
            Color yellow = new Color(1f, 0.78f, 0.25f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.42f, 0f), V(0.26f, 0.42f, 0.23f), black, 0.1f);
            Paint(b, b.Ellipsoid(V(0f, 0.4f, 0.12f), V(0.2f, 0.35f, 0.15f), white, 0.05f));
            Paint(b, b.Ellipsoid(V(0f, 0.74f, 0.13f), V(0.12f, 0.07f, 0.1f), yellow, 0.06f));
            b.Mirror(s => b.Ellipsoid(V(s * 0.1f, 0.025f, 0.1f), V(0.075f, 0.025f, 0.11f), orange, 0.02f, V(0f, s * 15f, 0f)));
            b.Ellipsoid(V(0f, 0.1f, -0.2f), V(0.08f, 0.04f, 0.1f), black, 0.04f, V(30f, 0f, 0f));

            Sculpt h = r.head;
            h.Sphere(V(0f, 0.13f, 0f), 0.17f, black, 0.05f);
            Paint(h, h.Ellipsoid(V(0f, 0.11f, 0.11f), V(0.12f, 0.1f, 0.08f), white, 0.04f));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.13f, 0.1f, 0.05f), V(0.04f, 0.07f, 0.05f), yellow, 0.03f)));
            h.Limb(V(0f, 0.09f, 0.15f), V(0f, 0.06f, 0.29f), 0.045f, 0.012f, orange, 0.015f);
            Paint(h, h.Limb(V(0f, 0.08f, 0.15f), V(0f, 0.06f, 0.27f), 0.02f, 0.006f, SpectatorSculptor.Shade(orange, 0.75f), 0.005f));

            Flipper(r.armLeft, -1f, 0.32f, 0.07f, black, black);
            Flipper(r.armRight, 1f, 0.32f, 0.07f, black, black);
            Eye(r, 0.03f, new Color(0.12f, 0.1f, 0.08f), 0.8f);

            r.headPivot = V(0f, 0.8f, 0.02f);
            r.shoulder = V(0.23f, 0.62f, 0f);
            r.eyePosition = V(0.065f, 0.17f, 0.125f);
            return r;
        }

        public static AvatarSculpture Seal(Color fur, Color light)
        {
            var r = new AvatarSculpture();
            Color dark = SpectatorSculptor.Shade(fur, 0.65f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.3f, -0.05f), V(0.27f, 0.32f, 0.34f), fur, 0.12f);
            b.Ellipsoid(V(0f, 0.6f, 0.03f), V(0.19f, 0.24f, 0.18f), fur, 0.1f);
            Paint(b, b.Ellipsoid(V(0f, 0.38f, 0.15f), V(0.18f, 0.3f, 0.14f), light, 0.06f));
            b.Mirror(s => b.Ellipsoid(V(s * 0.12f, 0.04f, -0.4f), V(0.11f, 0.035f, 0.15f), dark, 0.03f, V(0f, s * 30f, 0f)));
            var random = new System.Random(3);
            for (int i = 0; i < 7; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                Paint(b, b.Sphere(V(Mathf.Cos(angle) * 0.22f, 0.25f + (float)random.NextDouble() * 0.35f, -0.12f + Mathf.Sin(angle) * 0.1f), 0.035f, dark, 0.01f));
            }

            Sculpt h = r.head;
            h.Sphere(V(0f, 0.12f, 0.02f), 0.155f, fur, 0.05f);
            h.Ellipsoid(V(0f, 0.07f, 0.14f), V(0.095f, 0.065f, 0.075f), light, 0.04f);
            h.Sphere(V(0f, 0.1f, 0.215f), 0.028f, Ink, 0.008f);
            h.Mirror(s =>
            {
                for (int k = 0; k < 3; k++)
                {
                    Paint(h, h.Sphere(V(s * (0.04f + k * 0.018f), 0.06f - k * 0.012f, 0.19f - k * 0.012f), 0.008f, Ink, 0.003f));
                }
                return h.Limb(V(s * 0.05f, 0.07f, 0.18f), V(s * 0.16f, 0.06f, 0.2f), 0.004f, 0.003f, light, 0.002f);
            });
            Smile(h, V(0f, 0.03f, 0.18f), 0.04f, 0.012f, 0.012f, 0.03f, Mouth);

            Flipper(r.armLeft, -1f, 0.24f, 0.07f, fur, dark);
            Flipper(r.armRight, 1f, 0.24f, 0.07f, fur, dark);
            Eye(r, 0.036f, new Color(0.08f, 0.06f, 0.05f), 0.88f);

            r.headPivot = V(0f, 0.8f, 0.05f);
            r.shoulder = V(0.19f, 0.55f, 0.08f);
            r.eyePosition = V(0.062f, 0.17f, 0.115f);
            return r;
        }

        public static AvatarSculpture FurBall(Color fur, Color light, Color nose, bool pointedEars)
        {
            var r = new AvatarSculpture();
            Color shade = SpectatorSculptor.Shade(fur, 0.88f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.42f, -0.05f), V(0.27f, 0.31f, 0.28f), fur, 0.1f).noise = 0.015f;
            b.Ellipsoid(V(0f, 0.68f, 0.02f), V(0.21f, 0.2f, 0.18f), fur, 0.1f).noise = 0.01f;
            Paint(b, b.Ellipsoid(V(0f, 0.55f, 0.16f), V(0.15f, 0.22f, 0.1f), light, 0.05f));
            b.Mirror(s => b.Ellipsoid(V(s * 0.17f, 0.12f, 0.02f), V(0.12f, 0.12f, 0.2f), fur, 0.06f));
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.17f, 0.03f, 0.16f), V(0.09f, 0.04f, 0.08f), shade, 0.02f)));
            if (pointedEars)
            {
                b.Limb(V(0f, 0.25f, -0.28f), V(0.05f, 0.5f, -0.55f), 0.1f, 0.13f, fur, 0.08f).noise = 0.025f;
                Paint(b, b.Sphere(V(0.06f, 0.55f, -0.6f), 0.1f, Color.white, 0.04f));
            }
            else
            {
                b.Sphere(V(0f, 0.22f, -0.3f), 0.07f, fur, 0.04f);
            }

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.2f, 0f), V(0.2f, 0.19f, 0.19f), fur, 0.05f).noise = 0.008f;
            h.Ellipsoid(V(0f, 0.12f, pointedEars ? 0.19f : 0.16f), pointedEars ? V(0.08f, 0.07f, 0.13f) : V(0.1f, 0.08f, 0.1f), light, 0.06f);
            h.Sphere(V(0f, 0.15f, pointedEars ? 0.31f : 0.26f), 0.035f, nose, 0.01f);
            Smile(h, V(0f, 0.07f, pointedEars ? 0.25f : 0.22f), 0.05f, 0.015f, 0.014f, 0.04f, Mouth);
            if (pointedEars)
            {
                h.Mirror(s => h.Limb(V(s * 0.12f, 0.3f, 0f), V(s * 0.16f, 0.5f, -0.02f), 0.075f, 0.012f, fur, 0.03f));
                h.Mirror(s => Paint(h, h.Limb(V(s * 0.125f, 0.32f, 0.03f), V(s * 0.155f, 0.46f, 0.02f), 0.04f, 0.008f, light, 0.01f)));
            }
            else
            {
                h.Mirror(s => h.Sphere(V(s * 0.15f, 0.34f, -0.02f), 0.065f, fur, 0.03f));
                h.Mirror(s => Paint(h, h.Sphere(V(s * 0.15f, 0.34f, 0.02f), 0.035f, shade, 0.01f)));
            }

            Arm(r.armLeft, -1f, 0.26f, 0.065f, fur, light);
            Arm(r.armRight, 1f, 0.26f, 0.065f, fur, light);
            Eye(r, 0.035f, new Color(0.15f, 0.1f, 0.08f), 0.85f);

            r.headPivot = V(0f, 0.82f, 0.05f);
            r.shoulder = V(0.19f, 0.68f, 0.06f);
            r.eyePosition = V(0.075f, 0.24f, 0.15f);
            return r;
        }

        public static AvatarSculpture Crab()
        {
            var r = new AvatarSculpture();
            Color shell = new Color(0.9f, 0.3f, 0.18f);
            Color light = new Color(1f, 0.6f, 0.4f);
            Color dark = new Color(0.65f, 0.18f, 0.1f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.24f, 0f), V(0.33f, 0.15f, 0.25f), shell, 0.06f).noise = 0.006f;
            Paint(b, b.Ellipsoid(V(0f, 0.17f, 0.05f), V(0.3f, 0.07f, 0.22f), light, 0.04f));
            b.Mirror(s =>
            {
                for (int k = 0; k < 3; k++)
                {
                    float z = -0.12f + k * 0.11f;
                    b.Limb(V(s * 0.25f, 0.2f, z), V(s * 0.46f, 0.26f, z - 0.02f), 0.04f, 0.035f, shell, 0.03f);
                    b.Limb(V(s * 0.46f, 0.26f, z - 0.02f), V(s * 0.54f, 0.01f, z + 0.03f), 0.035f, 0.02f, dark, 0.02f);
                }
                return null;
            });

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.01f, 0.04f), V(0.12f, 0.06f, 0.07f), shell, 0.04f);
            h.Mirror(s => h.Limb(V(s * 0.06f, 0f, 0.05f), V(s * 0.09f, 0.14f, 0.07f), 0.022f, 0.02f, shell, 0.02f));
            Smile(h, V(0f, -0.01f, 0.1f), 0.05f, 0.012f, 0.012f, 0.03f, Mouth);

            for (int side = -1; side <= 1; side += 2)
            {
                Sculpt arm = side < 0 ? r.armLeft : r.armRight;
                arm.Sphere(V(0f, 0f, 0f), 0.05f, shell, 0.02f);
                arm.Limb(V(0f, 0f, 0f), V(side * 0.1f, -0.08f, 0.1f), 0.045f, 0.04f, shell, 0.03f);
                arm.Ellipsoid(V(side * 0.13f, -0.1f, 0.19f), V(0.08f, 0.065f, 0.11f), shell, 0.03f);
                Carve(arm, arm.Box(V(side * 0.13f, -0.1f, 0.28f), V(0.2f, 0.035f, 0.12f), 0.01f, dark, 0.01f));
                Paint(arm, arm.Ellipsoid(V(side * 0.13f, -0.1f, 0.27f), V(0.085f, 0.06f, 0.04f), dark, 0.02f));
            }
            Eye(r, 0.038f, new Color(0.1f, 0.08f, 0.06f), 0.8f);

            r.headPivot = V(0f, 0.36f, 0.12f);
            r.shoulder = V(0.22f, 0.27f, 0.16f);
            r.eyePosition = V(0.09f, 0.17f, 0.075f);
            return r;
        }

        public static AvatarSculpture Gull()
        {
            var r = new AvatarSculpture();
            Color white = new Color(0.97f, 0.97f, 0.96f);
            Color grey = new Color(0.62f, 0.66f, 0.72f);
            Color beak = new Color(1f, 0.82f, 0.2f);
            Color leg = new Color(1f, 0.62f, 0.25f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.36f, -0.02f), V(0.17f, 0.22f, 0.25f), white, 0.08f, V(-20f, 0f, 0f));
            b.Ellipsoid(V(0f, 0.32f, -0.3f), V(0.1f, 0.05f, 0.14f), grey, 0.05f, V(-30f, 0f, 0f));
            Paint(b, b.Ellipsoid(V(0f, 0.4f, -0.2f), V(0.16f, 0.12f, 0.16f), grey, 0.05f));
            b.Mirror(s => b.Limb(V(s * 0.07f, 0.18f, 0f), V(s * 0.08f, 0.02f, 0.03f), 0.025f, 0.02f, leg, 0.02f));
            b.Mirror(s => b.Ellipsoid(V(s * 0.09f, 0.012f, 0.08f), V(0.06f, 0.012f, 0.08f), leg, 0.01f));

            Sculpt h = r.head;
            h.Sphere(V(0f, 0.12f, 0f), 0.12f, white, 0.05f);
            h.Limb(V(0f, 0.1f, 0.1f), V(0f, 0.07f, 0.24f), 0.03f, 0.014f, beak, 0.012f);
            Paint(h, h.Sphere(V(0f, 0.065f, 0.2f), 0.014f, new Color(0.9f, 0.2f, 0.15f), 0.004f));

            Flipper(r.armLeft, -1f, 0.26f, 0.08f, grey, Ink);
            Flipper(r.armRight, 1f, 0.26f, 0.08f, grey, Ink);
            Eye(r, 0.025f, new Color(0.95f, 0.8f, 0.3f), 0.55f);

            r.headPivot = V(0f, 0.6f, 0.05f);
            r.shoulder = V(0.16f, 0.46f, 0f);
            r.eyePosition = V(0.06f, 0.15f, 0.08f);
            return r;
        }

        public static AvatarSculpture Octopus()
        {
            var r = new AvatarSculpture();
            Color skin = new Color(0.62f, 0.32f, 0.72f);
            Color light = new Color(0.9f, 0.62f, 0.85f);
            Color spot = new Color(0.45f, 0.2f, 0.55f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.2f, 0f), V(0.24f, 0.15f, 0.22f), skin, 0.08f);
            for (int k = 0; k < 6; k++)
            {
                float angle = (k / 6f) * Mathf.PI * 2f + 0.3f;
                var direction = V(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 a = V(0f, 0.12f, 0f) + direction * 0.15f;
                Vector3 m = V(0f, 0.03f, 0f) + direction * 0.34f;
                Vector3 e = V(0f, 0.1f, 0f) + direction * 0.44f + V(0f, 0f, 0f);
                b.Limb(a, m, 0.07f, 0.045f, skin, 0.04f);
                b.Limb(m, e, 0.045f, 0.02f, skin, 0.03f);
                Paint(b, b.Sphere(m + V(0f, -0.03f, 0f), 0.03f, light, 0.01f));
            }

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.22f, -0.05f), V(0.25f, 0.3f, 0.25f), skin, 0.05f);
            var random = new System.Random(9);
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                Paint(h, h.Sphere(V(Mathf.Cos(angle) * 0.2f, 0.28f + (float)random.NextDouble() * 0.15f, -0.05f + Mathf.Sin(angle) * 0.2f), 0.035f, spot, 0.01f));
            }
            Smile(h, V(0f, 0.05f, 0.18f), 0.06f, 0.016f, 0.015f, 0.04f, Mouth);

            for (int side = -1; side <= 1; side += 2)
            {
                Sculpt arm = side < 0 ? r.armLeft : r.armRight;
                arm.Sphere(V(0f, 0f, 0f), 0.055f, skin, 0.02f);
                arm.Limb(V(0f, 0f, 0f), V(side * 0.06f, -0.18f, 0.06f), 0.055f, 0.04f, skin, 0.03f);
                arm.Limb(V(side * 0.06f, -0.18f, 0.06f), V(side * 0.02f, -0.3f, 0.14f), 0.04f, 0.015f, skin, 0.02f);
                Paint(arm, arm.Sphere(V(side * 0.05f, -0.2f, 0.1f), 0.022f, light, 0.006f));
            }
            Eye(r, 0.045f, new Color(0.2f, 0.12f, 0.05f), 0.7f);

            r.headPivot = V(0f, 0.3f, 0f);
            r.shoulder = V(0.18f, 0.27f, 0.12f);
            r.eyePosition = V(0.09f, 0.22f, 0.17f);
            return r;
        }

        public static AvatarSculpture Turtle()
        {
            var r = new AvatarSculpture();
            Color shell = new Color(0.32f, 0.5f, 0.25f);
            Color plate = new Color(0.48f, 0.62f, 0.3f);
            Color skin = new Color(0.55f, 0.72f, 0.45f);
            Color belly = new Color(0.92f, 0.86f, 0.6f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.42f, -0.1f), V(0.3f, 0.36f, 0.2f), shell, 0.06f).noise = 0.008f;
            for (int i = 0; i < 5; i++)
            {
                float y = 0.28f + (i % 3) * 0.12f;
                float x = (i - 2) * 0.1f;
                Paint(b, b.Ellipsoid(V(x, y, -0.28f), V(0.07f, 0.07f, 0.05f), plate, 0.02f));
            }
            b.Ellipsoid(V(0f, 0.42f, 0.04f), V(0.24f, 0.3f, 0.11f), belly, 0.05f);
            for (int i = 0; i < 3; i++)
            {
                Paint(b, b.Box(V(0f, 0.3f + i * 0.12f, 0.15f), V(0.4f, 0.012f, 0.05f), 0f, SpectatorSculptor.Shade(belly, 0.8f), 0.005f));
            }
            Feet(b, skin, SpectatorSculptor.Shade(skin, 0.8f), 0.13f, 0.09f);

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.12f, 0.03f), V(0.14f, 0.13f, 0.15f), skin, 0.05f);
            Smile(h, V(0f, 0.06f, 0.16f), 0.05f, 0.014f, 0.013f, 0.035f, Mouth);
            Paint(h, h.Sphere(V(0f, 0.2f, 0.06f), 0.06f, SpectatorSculptor.Shade(skin, 0.85f), 0.03f));

            Arm(r.armLeft, -1f, 0.22f, 0.06f, skin, skin);
            Arm(r.armRight, 1f, 0.22f, 0.06f, skin, skin);
            Eye(r, 0.032f, new Color(0.15f, 0.1f, 0.05f), 0.8f);

            r.headPivot = V(0f, 0.74f, 0.05f);
            r.shoulder = V(0.23f, 0.6f, 0.04f);
            r.eyePosition = V(0.06f, 0.17f, 0.12f);
            return r;
        }
    }
}
