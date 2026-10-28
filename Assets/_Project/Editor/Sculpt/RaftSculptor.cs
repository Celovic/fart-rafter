using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public static class RaftSculptor
    {
        public static Sculpt InflatableBoat()
        {
            var boat = new Sculpt();
            Color tube = new Color(0.55f, 0.9f, 0.8f);
            Color stripe = new Color(1f, 0.55f, 0.25f);
            Color floor = new Color(0.45f, 0.72f, 0.3f);
            Color rope = new Color(0.95f, 0.9f, 0.75f);
            boat.Torus(V(0f, -0.02f, 0f), 1.42f, 0.32f, tube, 0.03f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                Paint(boat, boat.Box(V(Mathf.Cos(angle) * 1.42f, 0f, Mathf.Sin(angle) * 1.42f), V(0.18f, 0.8f, 0.8f), 0f, stripe, 0.02f, V(0f, -angle * Mathf.Rad2Deg, 0f)));
            }
            boat.Cylinder(V(0f, 0.08f, 0f), 1.22f, 0.05f, 0.04f, floor, 0.04f, V(90f, 0f, 0f));
            for (int i = 0; i < 5; i++)
            {
                Paint(boat, boat.Box(V(-0.8f + i * 0.4f, 0.13f, 0f), V(0.04f, 0.04f, 2.3f), 0f, SpectatorSculptor.Shade(floor, 0.8f), 0.01f));
            }
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f + 0.26f;
                Vector3 p = V(Mathf.Cos(angle) * 1.42f, 0.3f, Mathf.Sin(angle) * 1.42f);
                boat.Torus(p, 0.12f, 0.025f, rope, 0.01f, V(90f, -angle * Mathf.Rad2Deg, 0f));
            }
            boat.Sphere(V(1.3f, 0.25f, 0.5f), 0.07f, new Color(0.95f, 0.3f, 0.3f), 0.02f);
            return boat;
        }

        public static Sculpt Rowboat()
        {
            var boat = new Sculpt();
            Color hull = new Color(0.62f, 0.4f, 0.24f);
            Color plank = new Color(0.48f, 0.3f, 0.18f);
            Color trim = new Color(0.25f, 0.45f, 0.7f);
            Color seat = new Color(0.8f, 0.6f, 0.38f);
            boat.Ellipsoid(V(0f, 0f, 0f), V(1.35f, 0.55f, 1.85f), hull, 0.04f).noise = 0.01f;
            Carve(boat, boat.Ellipsoid(V(0f, 0.12f, 0f), V(1.2f, 0.5f, 1.7f), hull, 0.03f));
            Carve(boat, boat.Box(V(0f, 0.88f, 0f), V(4f, 1f, 5f), 0f, hull, 0.02f));
            for (int i = 0; i < 4; i++)
            {
                Paint(boat, boat.Torus(V(0f, 0.05f - i * 0.12f, 0f), 1.3f - i * 0.08f, 0.012f, plank, 0.006f));
            }
            Paint(boat, boat.Ellipsoid(V(0f, 0.36f, 0f), V(1.6f, 0.035f, 2.2f), trim, 0.01f));
            boat.Box(V(0f, 0.27f, 0.55f), V(2.0f, 0.07f, 0.35f), 0.02f, seat, 0.02f);
            boat.Box(V(0f, 0.27f, -0.75f), V(2.0f, 0.07f, 0.35f), 0.02f, seat, 0.02f);
            boat.Ellipsoid(V(0f, 0.17f, 0f), V(1.08f, 0.05f, 1.54f), plank, 0.03f);
            for (int i = 0; i < 4; i++)
            {
                Paint(boat, boat.Box(V(-0.6f + i * 0.4f, 0.21f, 0f), V(0.03f, 0.04f, 2.9f), 0f, SpectatorSculptor.Shade(plank, 0.75f), 0.01f));
            }
            boat.Mirror(s => boat.Limb(V(s * 1.3f, 0.25f, 0.1f), V(s * 2.0f, -0.25f, 0.4f), 0.035f, 0.035f, seat, 0.01f));
            boat.Mirror(s => boat.Ellipsoid(V(s * 2.05f, -0.28f, 0.42f), V(0.05f, 0.03f, 0.18f), seat, 0.01f, V(0f, s * 25f, 0f)));
            return boat;
        }

        public static Sculpt BigRing()
        {
            var ring = new Sculpt();
            Color red = new Color(0.95f, 0.3f, 0.3f);
            Color white = new Color(0.97f, 0.97f, 0.95f);
            ring.Torus(V(0f, -0.1f, 0f), 1.22f, 0.48f, white, 0.04f);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f;
                Paint(ring, ring.Box(V(Mathf.Cos(angle) * 1.22f, -0.1f, Mathf.Sin(angle) * 1.22f), V(0.55f, 1.2f, 1.2f), 0f, red, 0.03f, V(0f, -angle * Mathf.Rad2Deg, 0f)));
            }
            ring.Cylinder(V(0f, 0.08f, 0f), 0.86f, 0.05f, 0.03f, new Color(0.3f, 0.65f, 0.9f), 0.05f, V(90f, 0f, 0f));
            ring.Sphere(V(0f, 0.25f, 1.6f), 0.08f, new Color(1f, 0.85f, 0.25f), 0.02f);
            return ring;
        }

        public static Sculpt Bathtub()
        {
            var tub = new Sculpt();
            Color enamel = new Color(0.97f, 0.97f, 0.98f);
            Color gold = new Color(1f, 0.8f, 0.25f);
            Color duck = new Color(1f, 0.85f, 0.2f);
            Color foam = new Color(0.9f, 0.95f, 1f);
            tub.Box(V(0f, 0f, 0f), V(2.7f, 0.95f, 3.1f), 0.35f, enamel, 0.05f);
            Carve(tub, tub.Box(V(0f, 0.52f, 0f), V(2.4f, 0.95f, 2.8f), 0.3f, enamel, 0.04f));
            Paint(tub, tub.Torus(V(0f, 0.45f, 0f), 1.35f, 0.05f, new Color(0.85f, 0.9f, 0.95f), 0.03f));
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    tub.Limb(V(x * 1.05f, -0.4f, z * 1.25f), V(x * 1.2f, -0.65f, z * 1.4f), 0.1f, 0.07f, gold, 0.03f);
                }
            }
            tub.Limb(V(0f, 0.45f, -1.35f), V(0f, 0.85f, -1.35f), 0.05f, 0.05f, gold, 0.01f);
            tub.Limb(V(0f, 0.85f, -1.35f), V(0f, 0.85f, -1.05f), 0.05f, 0.04f, gold, 0.01f);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 1.05f;
                tub.Sphere(V(Mathf.Cos(angle) * 0.9f, 0.16f, Mathf.Sin(angle) * 1.1f), 0.16f, foam, 0.06f).noise = 0.02f;
            }
            tub.Ellipsoid(V(0.95f, 0.55f, 1.05f), V(0.14f, 0.11f, 0.17f), duck, 0.03f);
            tub.Sphere(V(0.95f, 0.72f, 1.15f), 0.09f, duck, 0.03f);
            tub.Ellipsoid(V(0.95f, 0.7f, 1.26f), V(0.05f, 0.02f, 0.05f), new Color(1f, 0.5f, 0.1f), 0.01f);
            return tub;
        }
    }
}
