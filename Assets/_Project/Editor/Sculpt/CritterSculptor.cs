using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public static class CritterSculptor
    {
        public static Sculpt Fish()
        {
            var fish = new Sculpt();
            Color back = new Color(0.28f, 0.52f, 0.62f);
            Color belly = new Color(0.92f, 0.9f, 0.82f);
            Color fin = new Color(0.95f, 0.55f, 0.25f);
            fish.Ellipsoid(V(0f, 0f, 0f), V(0.09f, 0.13f, 0.3f), back, 0.04f);
            Paint(fish, fish.Ellipsoid(V(0f, -0.05f, 0.02f), V(0.085f, 0.08f, 0.26f), belly, 0.05f));
            fish.Ellipsoid(V(0f, 0f, -0.34f), V(0.02f, 0.13f, 0.08f), fin, 0.03f, V(0f, 0f, 0f));
            fish.Ellipsoid(V(0f, 0.13f, -0.02f), V(0.015f, 0.06f, 0.12f), fin, 0.02f);
            fish.Mirror(s => fish.Ellipsoid(V(s * 0.08f, -0.04f, 0.08f), V(0.05f, 0.012f, 0.05f), fin, 0.015f, V(0f, s * 30f, 0f)));
            fish.Mirror(s => Paint(fish, fish.Sphere(V(s * 0.07f, 0.03f, 0.2f), 0.025f, EyeWhite, 0.005f)));
            fish.Mirror(s => Paint(fish, fish.Sphere(V(s * 0.085f, 0.03f, 0.21f), 0.013f, Ink, 0.003f)));
            return fish;
        }

        public static Sculpt Duck()
        {
            var duck = new Sculpt();
            Color body = new Color(0.62f, 0.48f, 0.32f);
            Color head = new Color(0.12f, 0.45f, 0.25f);
            Color beak = new Color(1f, 0.75f, 0.2f);
            Color ring = new Color(0.97f, 0.97f, 0.95f);
            duck.Ellipsoid(V(0f, 0.12f, 0f), V(0.17f, 0.12f, 0.26f), body, 0.05f);
            duck.Ellipsoid(V(0f, 0.2f, -0.24f), V(0.08f, 0.06f, 0.1f), body, 0.05f, V(-30f, 0f, 0f));
            duck.Limb(V(0f, 0.18f, 0.15f), V(0f, 0.34f, 0.2f), 0.06f, 0.055f, head, 0.04f);
            duck.Sphere(V(0f, 0.38f, 0.22f), 0.08f, head, 0.04f);
            Paint(duck, duck.Torus(V(0f, 0.28f, 0.19f), 0.055f, 0.012f, ring, 0.006f, V(70f, 0f, 0f)));
            duck.Ellipsoid(V(0f, 0.36f, 0.32f), V(0.04f, 0.018f, 0.06f), beak, 0.015f);
            duck.Mirror(s => Paint(duck, duck.Sphere(V(s * 0.055f, 0.4f, 0.26f), 0.015f, Ink, 0.004f)));
            duck.Mirror(s => Paint(duck, duck.Ellipsoid(V(s * 0.14f, 0.14f, -0.02f), V(0.04f, 0.07f, 0.16f), new Color(0.42f, 0.32f, 0.22f), 0.03f)));
            return duck;
        }

        public static Sculpt ButterflyWing(Color color)
        {
            var wing = new Sculpt();
            Color spot = Color.Lerp(color, Color.white, 0.6f);
            Color edge = new Color(0.12f, 0.1f, 0.1f);
            wing.Ellipsoid(V(0.09f, 0f, 0.04f), V(0.1f, 0.006f, 0.08f), color, 0.01f, V(0f, -20f, 0f));
            wing.Ellipsoid(V(0.07f, 0f, -0.06f), V(0.07f, 0.006f, 0.06f), color, 0.01f, V(0f, 25f, 0f));
            Paint(wing, wing.Torus(V(0.09f, 0f, 0.04f), 0.085f, 0.012f, edge, 0.006f));
            Paint(wing, wing.Sphere(V(0.11f, 0f, 0.05f), 0.025f, spot, 0.006f));
            return wing;
        }

        public static Sculpt Turtle()
        {
            var turtle = new Sculpt();
            Color shell = new Color(0.35f, 0.42f, 0.22f);
            Color plate = new Color(0.52f, 0.55f, 0.28f);
            Color skin = new Color(0.55f, 0.62f, 0.38f);
            turtle.Ellipsoid(V(0f, 0.1f, 0f), V(0.26f, 0.12f, 0.32f), shell, 0.04f).noise = 0.01f;
            for (int i = 0; i < 5; i++)
            {
                float angle = i * Mathf.PI * 2f / 5f;
                Paint(turtle, turtle.Ellipsoid(V(Mathf.Cos(angle) * 0.13f, 0.19f, Mathf.Sin(angle) * 0.16f), V(0.07f, 0.03f, 0.07f), plate, 0.02f));
            }
            Paint(turtle, turtle.Ellipsoid(V(0f, 0.22f, 0f), V(0.08f, 0.03f, 0.09f), plate, 0.02f));
            turtle.Limb(V(0f, 0.06f, 0.28f), V(0f, 0.1f, 0.42f), 0.06f, 0.055f, skin, 0.03f);
            turtle.Sphere(V(0f, 0.11f, 0.45f), 0.07f, skin, 0.03f);
            turtle.Mirror(s => Paint(turtle, turtle.Sphere(V(s * 0.045f, 0.15f, 0.49f), 0.015f, Ink, 0.004f)));
            turtle.Mirror(s => turtle.Ellipsoid(V(s * 0.24f, 0.03f, 0.16f), V(0.09f, 0.03f, 0.05f), skin, 0.03f, V(0f, s * 30f, 0f)));
            turtle.Mirror(s => turtle.Ellipsoid(V(s * 0.22f, 0.03f, -0.18f), V(0.07f, 0.03f, 0.05f), skin, 0.03f, V(0f, s * -30f, 0f)));
            return turtle;
        }
    }
}
