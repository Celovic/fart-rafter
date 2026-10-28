using UnityEngine;

namespace FartRafters.EditorTools.Sculpt
{
    public class AvatarSculpture
    {
        public Sculpt body = new Sculpt();
        public Sculpt head = new Sculpt();
        public Sculpt armLeft = new Sculpt();
        public Sculpt armRight = new Sculpt();
        public Sculpt eye = new Sculpt();
        public Vector3 headPivot;
        public Vector3 shoulder;
        public Vector3 eyePosition;
        public Vector3 puff;
    }

    public static class AvatarSculptor
    {
        internal static readonly Color EyeWhite = new Color(0.97f, 0.96f, 0.93f);
        internal static readonly Color Ink = new Color(0.08f, 0.06f, 0.07f);
        internal static readonly Color Mouth = new Color(0.32f, 0.1f, 0.12f);
        internal static readonly Color Tongue = new Color(0.86f, 0.38f, 0.42f);
        internal static readonly Color Tooth = new Color(0.98f, 0.96f, 0.88f);
        internal static readonly Color Blush = new Color(0.96f, 0.5f, 0.52f, 0.55f);

        public static AvatarSculpture Create(AvatarKind kind)
        {
            switch (kind)
            {
                case AvatarKind.Bear:
                    return Bear();
                case AvatarKind.Tombik:
                    return Tombik();
                case AvatarKind.Ogre:
                    return Ogre();
                case AvatarKind.Donkey:
                    return Donkey();
                case AvatarKind.Sloth:
                    return Sloth();
                case AvatarKind.Uncle:
                    return Uncle();
                case AvatarKind.Dad:
                    return Dad();
                default:
                    return Hippo();
            }
        }

        internal static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        internal static SdfShape Paint(Sculpt sculpt, SdfShape shape)
        {
            shape.op = SdfOp.Paint;
            return shape;
        }

        internal static SdfShape Carve(Sculpt sculpt, SdfShape shape)
        {
            shape.op = SdfOp.Subtract;
            return shape;
        }

        internal static void Smile(Sculpt h, Vector3 center, float halfWidth, float lift, float thickness, float depth, Color color)
        {
            for (int i = -3; i <= 3; i++)
            {
                float t = i / 3f;
                float slope = Mathf.Atan2(2f * lift * t, halfWidth) * Mathf.Rad2Deg;
                Vector3 position = center + V(t * halfWidth, lift * t * t, -Mathf.Abs(t) * depth * 0.3f);
                Carve(h, h.Ellipsoid(position, V(halfWidth * 0.3f, thickness * (1.1f - Mathf.Abs(t) * 0.55f), depth), color, 0.02f, V(0f, 0f, slope)));
            }
        }

        internal static void Eye(AvatarSculpture result, float radius, Color iris, float irisSize = 0.62f)
        {
            Sculpt eye = result.eye;
            eye.Sphere(Vector3.zero, radius, EyeWhite, 0.001f);
            Paint(eye, eye.Sphere(V(0f, -radius * 0.02f, radius * 0.95f), radius * irisSize, iris, radius * 0.06f));
            Paint(eye, eye.Sphere(V(0f, -radius * 0.02f, radius * 1.02f), radius * irisSize * 0.5f, Ink, radius * 0.05f));
            Paint(eye, eye.Sphere(V(radius * 0.28f, radius * 0.3f, radius * 0.95f), radius * 0.16f, Color.white, radius * 0.03f));
            Paint(eye, eye.Sphere(V(-radius * 0.18f, -radius * 0.22f, radius * 0.98f), radius * 0.07f, Color.white, radius * 0.02f));
        }

        internal static void Feet(Sculpt body, Color skin, Color sole, float spread, float size, Color? toes = null)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                body.Limb(V(s * spread, 0.34f, 0.0f), V(s * (spread + 0.02f), 0.1f, 0.05f), size * 1.05f, size * 0.9f, skin, 0.08f);
                body.Ellipsoid(V(s * (spread + 0.02f), 0.075f, 0.1f), V(size, size * 0.5f, size * 1.25f), skin, 0.06f);
                Paint(body, body.Ellipsoid(V(s * (spread + 0.02f), 0.0f, 0.1f), V(size * 1.1f, size * 0.22f, size * 1.4f), sole, 0.02f));
                if (toes.HasValue)
                {
                    for (int t = -1; t <= 1; t++)
                    {
                        Paint(body, body.Sphere(V(s * (spread + 0.02f) + t * size * 0.42f, 0.07f, 0.1f + size * 1.2f), size * 0.2f, toes.Value, 0.015f));
                    }
                }
            }
        }

        internal static void Arm(Sculpt arm, float side, float length, float radius, Color skin, Color hand, Color? sleeve = null, Color? claws = null, int fingers = 3)
        {
            Vector3 elbow = V(side * 0.04f, -length * 0.5f, 0.02f);
            Vector3 wrist = V(side * 0.07f, -length, 0.06f);
            arm.Sphere(V(0f, 0f, 0f), radius * 1.15f, sleeve ?? skin, 0.02f);
            arm.Limb(V(0f, 0f, 0f), elbow, radius * 1.05f, radius * 0.9f, sleeve ?? skin, 0.05f);
            arm.Limb(elbow, wrist, radius * 0.92f, radius * 0.82f, skin, 0.05f);
            arm.Ellipsoid(wrist + V(side * 0.01f, -radius * 0.7f, radius * 0.15f), V(radius * 1.05f, radius * 1.1f, radius * 0.95f), hand, 0.05f);
            arm.Limb(wrist + V(-side * radius * 0.55f, -radius * 0.35f, radius * 0.45f), wrist + V(-side * radius * 0.95f, -radius * 0.75f, radius * 0.85f), radius * 0.34f, radius * 0.28f, hand, 0.03f);
            if (claws.HasValue)
            {
                for (int f = 0; f < fingers; f++)
                {
                    float offset = (f - (fingers - 1) * 0.5f) * radius * 0.55f;
                    Vector3 root = wrist + V(side * 0.01f + offset, -radius * 1.55f, radius * 0.35f);
                    arm.Limb(root, root + V(0f, -radius * 0.45f, radius * 0.4f), radius * 0.18f, radius * 0.05f, claws.Value, 0.015f);
                }
            }
            if (sleeve.HasValue)
            {
                Paint(arm, arm.Sphere(V(0f, -length * 0.08f, 0f), radius * 1.25f, sleeve.Value, 0.02f));
            }
        }

        static AvatarSculpture Hippo()
        {
            var r = new AvatarSculpture();
            Color skin = new Color(0.58f, 0.52f, 0.7f);
            Color pink = new Color(0.94f, 0.7f, 0.78f);
            Color dark = new Color(0.36f, 0.3f, 0.45f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.5f, -0.02f), V(0.5f, 0.4f, 0.46f), skin, 0.1f);
            b.Ellipsoid(V(0f, 0.72f, 0.0f), V(0.42f, 0.36f, 0.38f), skin, 0.12f);
            b.Ellipsoid(V(0f, 0.95f, 0.04f), V(0.26f, 0.18f, 0.24f), skin, 0.1f);
            Paint(b, b.Ellipsoid(V(0f, 0.6f, 0.33f), V(0.33f, 0.36f, 0.16f), pink, 0.06f));
            Paint(b, b.Sphere(V(0f, 0.6f, 0.47f), 0.02f, dark, 0.01f));
            Feet(b, skin, dark, 0.24f, 0.15f, new Color(0.86f, 0.84f, 0.88f));
            b.Limb(V(0f, 0.4f, -0.43f), V(0f, 0.28f, -0.55f), 0.06f, 0.035f, skin, 0.05f);

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.2f, -0.02f), V(0.33f, 0.28f, 0.3f), skin, 0.05f);
            h.Ellipsoid(V(0f, 0.08f, 0.28f), V(0.37f, 0.23f, 0.27f), skin, 0.12f);
            Paint(h, h.Ellipsoid(V(0f, 0.05f, 0.42f), V(0.36f, 0.22f, 0.2f), pink, 0.08f));
            h.Mirror(s => h.Ellipsoid(V(s * 0.12f, 0.21f, 0.49f), V(0.08f, 0.06f, 0.06f), skin, 0.03f));
            h.Mirror(s => Carve(h, h.Sphere(V(s * 0.12f, 0.2f, 0.53f), 0.045f, Mouth, 0.015f)));
            Smile(h, V(0f, -0.01f, 0.53f), 0.22f, 0.06f, 0.04f, 0.1f, Mouth);
            h.Mirror(s => h.Box(V(s * 0.055f, 0.015f, 0.5f), V(0.06f, 0.07f, 0.04f), 0.015f, Tooth, 0.005f));
            h.Mirror(s => h.Ellipsoid(V(s * 0.15f, 0.36f, 0.16f), V(0.1f, 0.06f, 0.08f), skin, 0.05f));
            h.Mirror(s => h.Ellipsoid(V(s * 0.22f, 0.44f, -0.08f), V(0.075f, 0.09f, 0.045f), skin, 0.03f, V(0f, 0f, s * -20f)));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.22f, 0.44f, -0.04f), V(0.045f, 0.06f, 0.02f), pink, 0.012f)));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.25f, 0.12f, 0.3f), V(0.07f, 0.04f, 0.04f), Blush, 0.03f)));

            Arm(r.armLeft, -1f, 0.36f, 0.11f, skin, skin, null, new Color(0.86f, 0.84f, 0.88f));
            Arm(r.armRight, 1f, 0.36f, 0.11f, skin, skin, null, new Color(0.86f, 0.84f, 0.88f));
            Eye(r, 0.075f, new Color(0.35f, 0.24f, 0.18f));

            r.headPivot = V(0f, 1.0f, 0.05f);
            r.shoulder = V(0.4f, 0.86f, 0.02f);
            r.eyePosition = V(0.15f, 0.36f, 0.2f);
            r.puff = V(0f, 0.42f, -0.5f);
            return r;
        }

        static AvatarSculpture Bear()
        {
            var r = new AvatarSculpture();
            Color fur = new Color(0.5f, 0.32f, 0.19f);
            Color tan = new Color(0.88f, 0.71f, 0.5f);
            Color nose = new Color(0.16f, 0.1f, 0.08f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.5f, 0f), V(0.43f, 0.36f, 0.38f), fur, 0.1f);
            b.Ellipsoid(V(0f, 0.75f, 0f), V(0.38f, 0.36f, 0.34f), fur, 0.12f);
            b.Ellipsoid(V(0f, 1.0f, 0.02f), V(0.22f, 0.16f, 0.2f), fur, 0.1f);
            Paint(b, b.Ellipsoid(V(0f, 0.62f, 0.3f), V(0.27f, 0.32f, 0.14f), tan, 0.06f));
            Feet(b, fur, tan, 0.2f, 0.14f, nose);
            b.Sphere(V(0f, 0.38f, -0.38f), 0.08f, fur, 0.04f);

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.24f, 0f), V(0.32f, 0.29f, 0.29f), fur, 0.05f);
            h.Mirror(s => h.Ellipsoid(V(s * 0.17f, 0.12f, 0.12f), V(0.14f, 0.12f, 0.14f), fur, 0.08f));
            h.Ellipsoid(V(0f, 0.12f, 0.25f), V(0.16f, 0.12f, 0.14f), tan, 0.06f);
            h.Ellipsoid(V(0f, 0.2f, 0.38f), V(0.07f, 0.05f, 0.045f), nose, 0.025f);
            Paint(h, h.Sphere(V(0.02f, 0.225f, 0.42f), 0.015f, Color.white, 0.008f));
            Smile(h, V(0f, 0.06f, 0.37f), 0.08f, 0.03f, 0.025f, 0.06f, Mouth);
            Paint(h, h.Ellipsoid(V(0f, 0.045f, 0.37f), V(0.05f, 0.015f, 0.04f), Tongue, 0.01f));
            h.Mirror(s => h.Sphere(V(s * 0.24f, 0.47f, -0.04f), 0.1f, fur, 0.04f));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.24f, 0.47f, 0.04f), V(0.06f, 0.06f, 0.03f), tan, 0.015f)));
            h.Mirror(s => h.Ellipsoid(V(s * 0.12f, 0.41f, 0.23f), V(0.07f, 0.02f, 0.03f), nose, 0.02f, V(0f, 0f, s * -10f)));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.2f, 0.17f, 0.25f), V(0.06f, 0.035f, 0.04f), Blush, 0.03f)));

            Arm(r.armLeft, -1f, 0.4f, 0.1f, fur, fur, null, new Color(0.25f, 0.18f, 0.12f));
            Arm(r.armRight, 1f, 0.4f, 0.1f, fur, fur, null, new Color(0.25f, 0.18f, 0.12f));
            Eye(r, 0.06f, new Color(0.3f, 0.18f, 0.1f));

            r.headPivot = V(0f, 1.04f, 0.03f);
            r.shoulder = V(0.36f, 0.9f, 0.02f);
            r.eyePosition = V(0.12f, 0.32f, 0.24f);
            r.puff = V(0f, 0.42f, -0.44f);
            return r;
        }

        static AvatarSculpture Tombik()
        {
            var r = new AvatarSculpture();
            Color skin = new Color(0.98f, 0.77f, 0.63f);
            Color shirt = new Color(0.98f, 0.62f, 0.18f);
            Color stripe = new Color(0.98f, 0.92f, 0.8f);
            Color shorts = new Color(0.2f, 0.36f, 0.62f);
            Color shoe = new Color(0.92f, 0.92f, 0.94f);
            Color hair = new Color(0.32f, 0.2f, 0.11f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.6f, 0.04f), V(0.45f, 0.42f, 0.42f), shirt, 0.1f);
            b.Ellipsoid(V(0f, 0.85f, 0f), V(0.34f, 0.26f, 0.3f), shirt, 0.12f);
            b.Ellipsoid(V(0f, 1.02f, 0.02f), V(0.15f, 0.1f, 0.14f), skin, 0.06f);
            Paint(b, b.Box(V(0f, 0.36f, 0.02f), V(1.2f, 0.32f, 1.2f), 0.02f, shorts, 0.03f));
            Paint(b, b.Torus(V(0f, 0.66f, 0.04f), 0.44f, 0.035f, stripe, 0.02f));
            Paint(b, b.Torus(V(0f, 0.8f, 0.02f), 0.38f, 0.035f, stripe, 0.02f));
            Paint(b, b.Sphere(V(0f, 0.55f, 0.47f), 0.018f, new Color(0.8f, 0.5f, 0.15f), 0.01f));
            Feet(b, skin, new Color(0.85f, 0.22f, 0.2f), 0.21f, 0.13f);
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.23f, 0.08f, 0.1f), V(0.17f, 0.11f, 0.22f), shoe, 0.02f)));
            b.Mirror(s => Paint(b, b.Box(V(s * 0.22f, 0.26f, 0.03f), V(0.3f, 0.12f, 0.3f), 0.02f, shorts, 0.03f)));

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.27f, 0f), V(0.32f, 0.31f, 0.3f), skin, 0.05f);
            h.Mirror(s => h.Ellipsoid(V(s * 0.17f, 0.12f, 0.15f), V(0.14f, 0.12f, 0.12f), skin, 0.1f));
            h.Ellipsoid(V(0f, 0.07f, 0.12f), V(0.18f, 0.1f, 0.16f), skin, 0.1f);
            h.Sphere(V(0f, 0.22f, 0.31f), 0.05f, new Color(0.96f, 0.68f, 0.56f), 0.03f);
            Smile(h, V(0f, 0.1f, 0.3f), 0.1f, 0.04f, 0.035f, 0.07f, Mouth);
            Paint(h, h.Ellipsoid(V(0f, 0.085f, 0.3f), V(0.05f, 0.015f, 0.04f), Tongue, 0.01f));
            h.Mirror(s => h.Ellipsoid(V(s * 0.31f, 0.24f, 0f), V(0.05f, 0.08f, 0.06f), skin, 0.03f));
            h.Ellipsoid(V(0f, 0.43f, -0.04f), V(0.33f, 0.2f, 0.31f), hair, 0.04f);
            for (int i = -2; i <= 2; i++)
            {
                h.Ellipsoid(V(i * 0.09f, 0.47f - Mathf.Abs(i) * 0.025f, 0.22f - Mathf.Abs(i) * 0.05f), V(0.085f, 0.06f, 0.06f), hair, 0.03f);
            }
            h.Limb(V(0f, 0.58f, 0f), V(0.03f, 0.67f, -0.05f), 0.035f, 0.012f, hair, 0.03f);
            h.Mirror(s => h.Ellipsoid(V(s * 0.12f, 0.43f, 0.25f), V(0.07f, 0.018f, 0.03f), hair, 0.015f, V(0f, 0f, s * -8f)));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.2f, 0.15f, 0.24f), V(0.07f, 0.04f, 0.05f), Blush, 0.03f)));
            h.Mirror(s => Paint(h, h.Sphere(V(s * 0.16f, 0.2f, 0.26f), 0.012f, new Color(0.82f, 0.5f, 0.38f), 0.006f)));

            Arm(r.armLeft, -1f, 0.38f, 0.095f, skin, skin, shirt);
            Arm(r.armRight, 1f, 0.38f, 0.095f, skin, skin, shirt);
            Eye(r, 0.065f, new Color(0.25f, 0.55f, 0.45f));

            r.headPivot = V(0f, 1.04f, 0.04f);
            r.shoulder = V(0.37f, 0.9f, 0.02f);
            r.eyePosition = V(0.12f, 0.3f, 0.25f);
            r.puff = V(0f, 0.42f, -0.46f);
            return r;
        }

        static AvatarSculpture Ogre()
        {
            var r = new AvatarSculpture();
            Color skin = new Color(0.48f, 0.62f, 0.3f);
            Color dark = new Color(0.3f, 0.42f, 0.18f);
            Color cloth = new Color(0.55f, 0.4f, 0.24f);
            Color rope = new Color(0.82f, 0.7f, 0.45f);
            Color tusk = new Color(0.96f, 0.92f, 0.78f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.5f, 0.04f), V(0.48f, 0.38f, 0.44f), skin, 0.1f);
            b.Ellipsoid(V(0f, 0.8f, -0.02f), V(0.5f, 0.32f, 0.36f), skin, 0.14f);
            b.Ellipsoid(V(0f, 1.0f, 0f), V(0.3f, 0.16f, 0.24f), skin, 0.1f);
            Paint(b, b.Box(V(0f, 0.36f, 0.02f), V(1.2f, 0.36f, 1.2f), 0.02f, cloth, 0.03f));
            Paint(b, b.Torus(V(0f, 0.55f, 0.04f), 0.47f, 0.04f, rope, 0.015f));
            Paint(b, b.Limb(V(-0.28f, 0.98f, 0.2f), V(0.3f, 0.5f, 0.42f), 0.05f, 0.05f, cloth, 0.02f));
            Paint(b, b.Limb(V(-0.28f, 0.98f, -0.2f), V(0.3f, 0.5f, -0.42f), 0.05f, 0.05f, cloth, 0.02f));
            Paint(b, b.Sphere(V(0.02f, 0.66f, 0.47f), 0.02f, dark, 0.01f));
            Feet(b, skin, dark, 0.24f, 0.155f, dark);

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.28f, -0.02f), V(0.27f, 0.27f, 0.27f), skin, 0.05f);
            h.Ellipsoid(V(0f, 0.1f, 0.1f), V(0.32f, 0.19f, 0.25f), skin, 0.1f);
            h.Ellipsoid(V(0f, 0.38f, 0.17f), V(0.24f, 0.05f, 0.09f), dark, 0.05f);
            h.Ellipsoid(V(0f, 0.25f, 0.29f), V(0.08f, 0.07f, 0.07f), skin, 0.04f);
            h.Mirror(s => Carve(h, h.Sphere(V(s * 0.035f, 0.21f, 0.35f), 0.018f, Mouth, 0.008f)));
            Smile(h, V(0f, 0.08f, 0.33f), 0.15f, 0.045f, 0.03f, 0.08f, Mouth);
            h.Mirror(s => h.Limb(V(s * 0.12f, 0.07f, 0.3f), V(s * 0.14f, 0.19f, 0.33f), 0.03f, 0.008f, tusk, 0.01f));
            h.Mirror(s => h.Limb(V(s * 0.25f, 0.27f, 0f), V(s * 0.4f, 0.33f, -0.06f), 0.06f, 0.012f, skin, 0.04f));
            h.Mirror(s => Paint(h, h.Limb(V(s * 0.28f, 0.28f, 0.02f), V(s * 0.37f, 0.32f, -0.02f), 0.03f, 0.008f, new Color(0.7f, 0.52f, 0.44f), 0.01f)));
            h.Limb(V(0f, 0.52f, -0.06f), V(0f, 0.62f, -0.1f), 0.05f, 0.03f, new Color(0.18f, 0.12f, 0.08f), 0.03f);
            h.Sphere(V(0f, 0.64f, -0.11f), 0.045f, new Color(0.18f, 0.12f, 0.08f), 0.02f);
            h.Mirror(s => Paint(h, h.Sphere(V(s * 0.16f, 0.48f, 0.12f), 0.025f, dark, 0.02f)));

            Arm(r.armLeft, -1f, 0.42f, 0.12f, skin, skin, null, dark);
            Arm(r.armRight, 1f, 0.42f, 0.12f, skin, skin, null, dark);
            Eye(r, 0.055f, new Color(0.82f, 0.62f, 0.15f), 0.66f);

            r.headPivot = V(0f, 1.06f, 0.04f);
            r.shoulder = V(0.47f, 0.9f, 0f);
            r.eyePosition = V(0.11f, 0.31f, 0.21f);
            r.puff = V(0f, 0.42f, -0.5f);
            return r;
        }

        static AvatarSculpture Donkey()
        {
            var r = new AvatarSculpture();
            Color coat = new Color(0.64f, 0.52f, 0.42f);
            Color cream = new Color(0.94f, 0.89f, 0.8f);
            Color mane = new Color(0.3f, 0.2f, 0.14f);
            Color hoof = new Color(0.22f, 0.17f, 0.14f);
            Color bandana = new Color(0.84f, 0.2f, 0.2f);
            Color pink = new Color(0.9f, 0.66f, 0.66f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.5f, 0f), V(0.42f, 0.36f, 0.4f), coat, 0.1f);
            b.Ellipsoid(V(0f, 0.76f, 0f), V(0.36f, 0.32f, 0.33f), coat, 0.12f);
            b.Ellipsoid(V(0f, 1.0f, 0.02f), V(0.2f, 0.16f, 0.19f), coat, 0.08f);
            Paint(b, b.Ellipsoid(V(0f, 0.6f, 0.3f), V(0.26f, 0.32f, 0.14f), cream, 0.06f));
            b.Torus(V(0f, 0.98f, 0.02f), 0.2f, 0.045f, bandana, 0.03f, V(-10f, 0f, 0f));
            b.Limb(V(0f, 0.96f, 0.22f), V(0f, 0.84f, 0.3f), 0.07f, 0.02f, bandana, 0.03f);
            Feet(b, coat, hoof, 0.2f, 0.13f);
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.22f, 0.06f, 0.1f), V(0.16f, 0.09f, 0.2f), hoof, 0.02f)));
            b.Limb(V(0f, 0.42f, -0.38f), V(0f, 0.22f, -0.5f), 0.03f, 0.02f, coat, 0.03f);
            b.Ellipsoid(V(0f, 0.18f, -0.52f), V(0.04f, 0.08f, 0.04f), mane, 0.02f);

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.3f, -0.02f), V(0.23f, 0.25f, 0.24f), coat, 0.05f);
            h.Limb(V(0f, 0.24f, 0.1f), V(0f, 0.12f, 0.36f), 0.18f, 0.17f, coat, 0.08f);
            Paint(h, h.Ellipsoid(V(0f, 0.1f, 0.38f), V(0.2f, 0.17f, 0.18f), cream, 0.06f));
            h.Mirror(s => Carve(h, h.Ellipsoid(V(s * 0.08f, 0.16f, 0.52f), V(0.025f, 0.035f, 0.03f), Mouth, 0.01f)));
            Smile(h, V(0f, 0.0f, 0.48f), 0.13f, 0.05f, 0.04f, 0.09f, Mouth);
            h.Box(V(0f, 0.03f, 0.48f), V(0.13f, 0.05f, 0.04f), 0.012f, Tooth, 0.005f);
            Carve(h, h.Box(V(0f, 0.03f, 0.505f), V(0.006f, 0.05f, 0.02f), 0f, new Color(0.7f, 0.68f, 0.6f), 0.002f));
            h.Mirror(s => h.Limb(V(s * 0.1f, 0.48f, -0.06f), V(s * 0.24f, 0.86f, -0.14f), 0.07f, 0.035f, coat, 0.04f));
            h.Mirror(s => Paint(h, h.Limb(V(s * 0.11f, 0.52f, -0.01f), V(s * 0.23f, 0.82f, -0.1f), 0.045f, 0.02f, pink, 0.012f)));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.23f, 0.86f, -0.14f), V(0.05f, 0.06f, 0.05f), mane, 0.02f)));
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                h.Ellipsoid(V(0f, 0.5f - t * 0.32f, 0.06f - t * 0.34f), V(0.05f, 0.09f, 0.07f), mane, 0.03f, V(-30f - t * 30f, 0f, 0f));
            }
            h.Ellipsoid(V(0f, 0.51f, 0.12f), V(0.07f, 0.06f, 0.07f), mane, 0.03f, V(30f, 0f, 0f));
            h.Mirror(s => h.Ellipsoid(V(s * 0.11f, 0.42f, 0.17f), V(0.06f, 0.018f, 0.03f), mane, 0.015f, V(0f, 0f, s * 14f)));

            Arm(r.armLeft, -1f, 0.38f, 0.09f, coat, hoof);
            Arm(r.armRight, 1f, 0.38f, 0.09f, coat, hoof);
            Eye(r, 0.06f, new Color(0.35f, 0.22f, 0.12f));

            r.headPivot = V(0f, 1.04f, 0.04f);
            r.shoulder = V(0.34f, 0.88f, 0.02f);
            r.eyePosition = V(0.12f, 0.33f, 0.17f);
            r.puff = V(0f, 0.42f, -0.46f);
            return r;
        }

        static AvatarSculpture Sloth()
        {
            var r = new AvatarSculpture();
            Color fur = new Color(0.6f, 0.5f, 0.38f);
            Color face = new Color(0.93f, 0.86f, 0.72f);
            Color mask = new Color(0.3f, 0.22f, 0.15f);
            Color claw = new Color(0.92f, 0.88f, 0.76f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.5f, 0f), V(0.4f, 0.38f, 0.37f), fur, 0.1f);
            b.Ellipsoid(V(0f, 0.78f, 0f), V(0.33f, 0.32f, 0.3f), fur, 0.12f);
            b.Ellipsoid(V(0f, 1.0f, 0.02f), V(0.2f, 0.14f, 0.18f), fur, 0.08f);
            Paint(b, b.Ellipsoid(V(0f, 0.62f, 0.28f), V(0.24f, 0.3f, 0.14f), new Color(0.72f, 0.62f, 0.48f), 0.08f));
            for (int i = 0; i < 6; i++)
            {
                float a = i * 1.05f;
                b.Ellipsoid(V(Mathf.Sin(a) * 0.3f, 0.92f + (i % 2) * 0.05f, Mathf.Cos(a) * 0.26f), V(0.09f, 0.06f, 0.09f), fur, 0.06f);
            }
            Feet(b, fur, mask, 0.19f, 0.13f, claw);

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.25f, 0f), V(0.29f, 0.27f, 0.27f), fur, 0.05f);
            h.Ellipsoid(V(0f, 0.22f, 0.12f), V(0.24f, 0.2f, 0.18f), face, 0.06f);
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.12f, 0.24f, 0.28f), V(0.1f, 0.045f, 0.06f), mask, 0.025f, V(0f, 0f, s * -24f))));
            h.Ellipsoid(V(0f, 0.19f, 0.31f), V(0.06f, 0.045f, 0.04f), mask, 0.02f);
            Smile(h, V(0f, 0.09f, 0.28f), 0.08f, 0.03f, 0.014f, 0.05f, Mouth);
            h.Mirror(s => h.Ellipsoid(V(s * 0.115f, 0.31f, 0.255f), V(0.07f, 0.03f, 0.05f), fur, 0.015f, V(12f, 0f, s * -10f)));
            for (int i = 0; i < 5; i++)
            {
                float a = (i - 2) * 0.45f;
                h.Ellipsoid(V(Mathf.Sin(a) * 0.18f, 0.5f - Mathf.Abs(a) * 0.06f, Mathf.Cos(a) * 0.08f - 0.04f), V(0.08f, 0.05f, 0.08f), fur, 0.05f);
            }
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.17f, 0.13f, 0.24f), V(0.05f, 0.03f, 0.04f), Blush, 0.03f)));

            Arm(r.armLeft, -1f, 0.5f, 0.085f, fur, fur, null, claw, 3);
            Arm(r.armRight, 1f, 0.5f, 0.085f, fur, fur, null, claw, 3);
            Eye(r, 0.05f, new Color(0.28f, 0.18f, 0.1f), 0.72f);

            r.headPivot = V(0f, 1.03f, 0.04f);
            r.shoulder = V(0.33f, 0.9f, 0.02f);
            r.eyePosition = V(0.115f, 0.27f, 0.25f);
            r.puff = V(0f, 0.42f, -0.43f);
            return r;
        }

        static AvatarSculpture Uncle()
        {
            var r = new AvatarSculpture();
            Color skin = new Color(0.93f, 0.72f, 0.58f);
            Color tank = new Color(0.95f, 0.94f, 0.9f);
            Color trousers = new Color(0.4f, 0.3f, 0.22f);
            Color suspender = new Color(0.82f, 0.2f, 0.18f);
            Color hair = new Color(0.32f, 0.29f, 0.28f);
            Color shoe = new Color(0.16f, 0.12f, 0.1f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.58f, 0.06f), V(0.46f, 0.42f, 0.44f), tank, 0.1f);
            b.Ellipsoid(V(0f, 0.86f, 0f), V(0.4f, 0.26f, 0.3f), tank, 0.12f);
            b.Ellipsoid(V(0f, 1.02f, 0.02f), V(0.16f, 0.1f, 0.15f), skin, 0.06f);
            Paint(b, b.Box(V(0f, 0.34f, 0.02f), V(1.2f, 0.36f, 1.2f), 0.02f, trousers, 0.03f));
            b.Mirror(s => Paint(b, b.Limb(V(s * 0.18f, 1.0f, 0.2f), V(s * 0.2f, 0.5f, 0.5f), 0.035f, 0.035f, suspender, 0.012f)));
            b.Mirror(s => Paint(b, b.Limb(V(s * 0.18f, 1.0f, -0.2f), V(s * 0.16f, 0.5f, -0.46f), 0.035f, 0.035f, suspender, 0.012f)));
            b.Mirror(s => Paint(b, b.Sphere(V(s * 0.2f, 0.52f, 0.5f), 0.03f, new Color(0.85f, 0.75f, 0.3f), 0.01f)));
            Paint(b, b.Ellipsoid(V(0f, 0.98f, 0.24f), V(0.11f, 0.06f, 0.06f), skin, 0.02f));
            Feet(b, trousers, shoe, 0.22f, 0.14f);
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.24f, 0.07f, 0.1f), V(0.17f, 0.1f, 0.22f), shoe, 0.02f)));

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.27f, 0f), V(0.3f, 0.32f, 0.29f), skin, 0.05f);
            h.Ellipsoid(V(0f, 0.1f, 0.08f), V(0.27f, 0.17f, 0.22f), skin, 0.1f);
            h.Ellipsoid(V(0f, 0.22f, 0.3f), V(0.075f, 0.07f, 0.075f), new Color(0.92f, 0.62f, 0.52f), 0.04f);
            h.Mirror(s => h.Limb(V(s * 0.02f, 0.15f, 0.32f), V(s * 0.17f, 0.08f, 0.26f), 0.05f, 0.022f, hair, 0.03f));
            h.Mirror(s => h.Limb(V(s * 0.15f, 0.09f, 0.27f), V(s * 0.2f, 0.02f, 0.22f), 0.022f, 0.01f, hair, 0.015f));
            Smile(h, V(0f, 0.04f, 0.27f), 0.08f, 0.02f, 0.02f, 0.05f, Mouth);
            h.Mirror(s => h.Ellipsoid(V(s * 0.12f, 0.42f, 0.22f), V(0.08f, 0.028f, 0.04f), hair, 0.02f, V(0f, 0f, s * -6f)));
            h.Mirror(s => h.Ellipsoid(V(s * 0.3f, 0.22f, 0f), V(0.05f, 0.08f, 0.06f), skin, 0.03f));
            h.Ellipsoid(V(0f, 0.26f, -0.12f), V(0.3f, 0.14f, 0.2f), hair, 0.05f);
            h.Mirror(s => h.Ellipsoid(V(s * 0.26f, 0.3f, -0.02f), V(0.07f, 0.1f, 0.12f), hair, 0.04f));
            Paint(h, h.Ellipsoid(V(0f, 0.6f, 0.02f), V(0.12f, 0.03f, 0.1f), new Color(1f, 0.85f, 0.75f), 0.05f));
            h.Mirror(s => Paint(h, h.Ellipsoid(V(s * 0.2f, 0.15f, 0.22f), V(0.07f, 0.04f, 0.05f), Blush, 0.03f)));

            Arm(r.armLeft, -1f, 0.4f, 0.1f, skin, skin);
            Arm(r.armRight, 1f, 0.4f, 0.1f, skin, skin);
            Eye(r, 0.055f, new Color(0.3f, 0.22f, 0.15f));

            r.headPivot = V(0f, 1.04f, 0.04f);
            r.shoulder = V(0.42f, 0.9f, 0.02f);
            r.eyePosition = V(0.11f, 0.34f, 0.24f);
            r.puff = V(0f, 0.42f, -0.48f);
            return r;
        }

        static AvatarSculpture Dad()
        {
            var r = new AvatarSculpture();
            Color skin = new Color(0.98f, 0.8f, 0.68f);
            Color shirt = new Color(0.72f, 0.84f, 0.96f);
            Color vest = new Color(0.86f, 0.64f, 0.24f);
            Color pants = new Color(0.68f, 0.6f, 0.44f);
            Color shoe = new Color(0.42f, 0.26f, 0.15f);
            Color hair = new Color(0.4f, 0.26f, 0.15f);
            Color frame = new Color(0.12f, 0.1f, 0.1f);

            Sculpt b = r.body;
            b.Ellipsoid(V(0f, 0.6f, 0.05f), V(0.44f, 0.42f, 0.42f), vest, 0.1f);
            b.Ellipsoid(V(0f, 0.86f, 0f), V(0.37f, 0.26f, 0.3f), vest, 0.12f);
            b.Ellipsoid(V(0f, 1.02f, 0.02f), V(0.16f, 0.1f, 0.15f), shirt, 0.06f);
            Paint(b, b.Box(V(0f, 0.34f, 0.02f), V(1.2f, 0.34f, 1.2f), 0.02f, pants, 0.03f));
            Paint(b, b.Ellipsoid(V(0f, 0.95f, 0.26f), V(0.14f, 0.13f, 0.1f), shirt, 0.02f, V(-20f, 0f, 0f)));
            Paint(b, b.Limb(V(0f, 0.82f, 0.36f), V(0f, 0.5f, 0.47f), 0.012f, 0.012f, new Color(0.6f, 0.42f, 0.14f), 0.008f));
            for (int i = 0; i < 3; i++)
            {
                Paint(b, b.Sphere(V(0.03f, 0.78f - i * 0.12f, 0.4f + i * 0.03f), 0.015f, new Color(0.45f, 0.3f, 0.12f), 0.006f));
            }
            Feet(b, pants, shoe, 0.21f, 0.135f);
            b.Mirror(s => Paint(b, b.Ellipsoid(V(s * 0.23f, 0.07f, 0.1f), V(0.17f, 0.1f, 0.22f), shoe, 0.02f)));

            Sculpt h = r.head;
            h.Ellipsoid(V(0f, 0.27f, 0f), V(0.3f, 0.31f, 0.29f), skin, 0.05f);
            h.Ellipsoid(V(0f, 0.08f, 0.1f), V(0.26f, 0.17f, 0.21f), hair, 0.1f);
            Paint(h, h.Ellipsoid(V(0f, 0.25f, 0.2f), V(0.24f, 0.11f, 0.16f), skin, 0.04f));
            h.Ellipsoid(V(0f, 0.22f, 0.3f), V(0.06f, 0.06f, 0.06f), new Color(0.95f, 0.7f, 0.6f), 0.03f);
            Smile(h, V(0f, 0.09f, 0.3f), 0.08f, 0.025f, 0.022f, 0.05f, Mouth);
            h.Ellipsoid(V(0f, 0.47f, -0.04f), V(0.31f, 0.15f, 0.3f), hair, 0.05f);
            h.Ellipsoid(V(0.08f, 0.52f, 0.14f), V(0.18f, 0.07f, 0.12f), hair, 0.04f, V(0f, 0f, -12f));
            h.Mirror(s => h.Ellipsoid(V(s * 0.3f, 0.24f, 0f), V(0.05f, 0.08f, 0.06f), skin, 0.03f));
            h.Mirror(s => h.Ellipsoid(V(s * 0.11f, 0.42f, 0.24f), V(0.07f, 0.02f, 0.03f), hair, 0.015f, V(0f, 0f, s * 8f)));
            h.Mirror(s => h.Torus(V(s * 0.11f, 0.33f, 0.3f), 0.07f, 0.012f, frame, 0.004f, V(90f, 0f, 0f)));
            h.Limb(V(-0.04f, 0.34f, 0.31f), V(0.04f, 0.34f, 0.31f), 0.01f, 0.01f, frame, 0.004f);
            h.Mirror(s => h.Limb(V(s * 0.18f, 0.34f, 0.28f), V(s * 0.29f, 0.3f, 0.04f), 0.01f, 0.01f, frame, 0.004f));

            Arm(r.armLeft, -1f, 0.38f, 0.095f, skin, skin, shirt);
            Arm(r.armRight, 1f, 0.38f, 0.095f, skin, skin, shirt);
            Eye(r, 0.05f, new Color(0.3f, 0.45f, 0.6f));

            r.headPivot = V(0f, 1.04f, 0.04f);
            r.shoulder = V(0.39f, 0.9f, 0.02f);
            r.eyePosition = V(0.11f, 0.33f, 0.25f);
            r.puff = V(0f, 0.42f, -0.46f);
            return r;
        }
    }
}
