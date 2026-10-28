using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public enum HatKind
    {
        None,
        Crown,
        SwimCap,
        ChefHat,
        CowboyHat,
        PartyHat,
        Headband,
        VikingHelmet,
        PirateHat,
        FlowerCrown,
        WizardHat,
        NightCap,
        SunHat,
        TopHat,
        BucketHat,
        CaptainHat
    }

    public enum OutfitKind
    {
        None,
        Cape,
        LifeVest,
        BowTie,
        Scarf,
        Apron,
        Lei,
        Medal,
        Sash
    }

    public struct CostumeLook
    {
        public string name;
        public HatKind hat;
        public OutfitKind outfit;
        public Color main;
        public Color accent;
    }

    public struct CostumeFit
    {
        public Vector3 head;
        public Vector3 headRadii;
        public Vector3 chest;
        public Vector3 chestRadii;
        public float neck;
    }

    public static class CostumeSculptor
    {
        static readonly Color Gold = new Color(1f, 0.8f, 0.22f);
        static readonly Color White = new Color(0.97f, 0.97f, 0.95f);
        static readonly Color Black = new Color(0.1f, 0.1f, 0.12f);

        public static CostumeFit Fit(AvatarKind kind)
        {
            switch (kind)
            {
                case AvatarKind.Bear:
                    return Make(V(0f, 0.24f, 0f), V(0.32f, 0.29f, 0.29f), V(0f, 0.75f, 0f), V(0.38f, 0.36f, 0.34f), 1.04f);
                case AvatarKind.Tombik:
                    return Make(V(0f, 0.27f, 0f), V(0.32f, 0.31f, 0.3f), V(0f, 0.85f, 0f), V(0.34f, 0.26f, 0.3f), 1.04f);
                case AvatarKind.Ogre:
                    return Make(V(0f, 0.28f, -0.02f), V(0.27f, 0.27f, 0.27f), V(0f, 0.8f, -0.02f), V(0.5f, 0.32f, 0.36f), 1.06f);
                case AvatarKind.Donkey:
                    return Make(V(0f, 0.3f, -0.02f), V(0.23f, 0.25f, 0.24f), V(0f, 0.76f, 0f), V(0.36f, 0.32f, 0.33f), 1.04f);
                case AvatarKind.Sloth:
                    return Make(V(0f, 0.25f, 0f), V(0.29f, 0.27f, 0.27f), V(0f, 0.78f, 0f), V(0.33f, 0.32f, 0.3f), 1.03f);
                case AvatarKind.Uncle:
                    return Make(V(0f, 0.27f, 0f), V(0.3f, 0.32f, 0.29f), V(0f, 0.86f, 0f), V(0.4f, 0.26f, 0.3f), 1.04f);
                case AvatarKind.Dad:
                    return Make(V(0f, 0.27f, 0f), V(0.3f, 0.31f, 0.29f), V(0f, 0.86f, 0f), V(0.37f, 0.26f, 0.3f), 1.04f);
                default:
                    return Make(V(0f, 0.2f, -0.02f), V(0.33f, 0.28f, 0.3f), V(0f, 0.72f, 0f), V(0.42f, 0.36f, 0.38f), 1f);
            }
        }

        static CostumeFit Make(Vector3 head, Vector3 headRadii, Vector3 chest, Vector3 chestRadii, float neck)
        {
            return new CostumeFit { head = head, headRadii = headRadii, chest = chest, chestRadii = chestRadii, neck = neck };
        }

        public static CostumeLook[] For(AvatarKind kind)
        {
            switch (kind)
            {
                case AvatarKind.Bear:
                    return new[] { Look("Aşçı", HatKind.ChefHat, OutfitKind.Apron, White, new Color(0.85f, 0.2f, 0.2f)), Look("Kovboy", HatKind.CowboyHat, OutfitKind.Scarf, new Color(0.55f, 0.35f, 0.2f), new Color(0.85f, 0.2f, 0.2f)) };
                case AvatarKind.Tombik:
                    return new[] { Look("Parti", HatKind.PartyHat, OutfitKind.BowTie, new Color(0.3f, 0.6f, 0.95f), new Color(1f, 0.85f, 0.25f)), Look("Süper Kahraman", HatKind.Headband, OutfitKind.Cape, new Color(0.85f, 0.15f, 0.2f), Gold) };
                case AvatarKind.Ogre:
                    return new[] { Look("Viking", HatKind.VikingHelmet, OutfitKind.Cape, new Color(0.55f, 0.55f, 0.6f), new Color(0.5f, 0.32f, 0.18f)), Look("Korsan", HatKind.PirateHat, OutfitKind.Sash, Black, new Color(0.85f, 0.2f, 0.2f)) };
                case AvatarKind.Donkey:
                    return new[] { Look("Çiçek Taçlı", HatKind.FlowerCrown, OutfitKind.Lei, new Color(0.35f, 0.7f, 0.3f), new Color(1f, 0.5f, 0.7f)), Look("Sihirbaz", HatKind.WizardHat, OutfitKind.Cape, new Color(0.35f, 0.25f, 0.7f), Gold) };
                case AvatarKind.Sloth:
                    return new[] { Look("Uykucu", HatKind.NightCap, OutfitKind.Scarf, new Color(0.35f, 0.45f, 0.85f), White), Look("Turist", HatKind.SunHat, OutfitKind.Lei, new Color(0.95f, 0.85f, 0.55f), new Color(1f, 0.45f, 0.3f)) };
                case AvatarKind.Uncle:
                    return new[] { Look("Beyefendi", HatKind.TopHat, OutfitKind.BowTie, Black, new Color(0.8f, 0.15f, 0.2f)), Look("Balıkçı", HatKind.BucketHat, OutfitKind.LifeVest, new Color(0.45f, 0.55f, 0.35f), new Color(1f, 0.5f, 0.1f)) };
                case AvatarKind.Dad:
                    return new[] { Look("Mangalcı", HatKind.ChefHat, OutfitKind.Apron, White, new Color(0.25f, 0.45f, 0.8f)), Look("Kaptan", HatKind.CaptainHat, OutfitKind.Medal, White, Gold) };
                default:
                    return new[] { Look("Kral", HatKind.Crown, OutfitKind.Cape, Gold, new Color(0.75f, 0.12f, 0.2f)), Look("Yüzücü", HatKind.SwimCap, OutfitKind.LifeVest, new Color(0.95f, 0.35f, 0.5f), new Color(1f, 0.55f, 0.1f)) };
            }
        }

        static CostumeLook Look(string name, HatKind hat, OutfitKind outfit, Color main, Color accent)
        {
            return new CostumeLook { name = name, hat = hat, outfit = outfit, main = main, accent = accent };
        }

        static Vector3 Top(CostumeFit fit)
        {
            return fit.head + V(0f, fit.headRadii.y, 0f);
        }

        static float HeadRadius(CostumeFit fit)
        {
            return Mathf.Min(fit.headRadii.x, fit.headRadii.z);
        }

        public static Sculpt Hat(CostumeLook look, CostumeFit fit)
        {
            var hat = new Sculpt();
            Vector3 top = Top(fit);
            float r = HeadRadius(fit);
            Color main = look.main;
            Color accent = look.accent;
            switch (look.hat)
            {
                case HatKind.Crown:
                    {
                        Vector3 band = top - V(0f, 0.06f, 0f);
                        hat.Cylinder(band, r * 0.62f, 0.07f, 0.02f, main, 0.02f, V(90f, 0f, 0f));
                        Carve(hat, hat.Cylinder(band + V(0f, 0.02f, 0f), r * 0.54f, 0.09f, 0.01f, main, 0.01f, V(90f, 0f, 0f)));
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = i * Mathf.PI / 3f;
                            Vector3 rim = band + V(Mathf.Cos(angle) * r * 0.58f, 0.06f, Mathf.Sin(angle) * r * 0.58f);
                            hat.Limb(rim, rim + V(0f, 0.12f, 0f), 0.035f, 0.008f, main, 0.015f);
                            hat.Sphere(rim + V(0f, 0.13f, 0f), 0.02f, main, 0.005f);
                            Paint(hat, hat.Sphere(band + V(Mathf.Cos(angle + 0.5f) * r * 0.62f, 0f, Mathf.Sin(angle + 0.5f) * r * 0.62f), 0.03f, i % 2 == 0 ? accent : new Color(0.2f, 0.5f, 0.95f), 0.008f));
                        }
                        break;
                    }
                case HatKind.SwimCap:
                    hat.Ellipsoid(fit.head + V(0f, 0.03f, -0.02f), fit.headRadii * 1.06f, main, 0.02f);
                    Carve(hat, hat.Box(fit.head + V(0f, -0.35f + fit.headRadii.y * 0.1f, 0.1f), V(1.5f, 0.7f, 1.5f), 0f, main, 0.03f, V(-25f, 0f, 0f)));
                    Paint(hat, hat.Torus(fit.head + V(0f, fit.headRadii.y * 0.45f, -0.02f), fit.headRadii.x * 0.92f, 0.015f, White, 0.008f));
                    break;
                case HatKind.ChefHat:
                    hat.Cylinder(top - V(0f, 0.02f, 0f), r * 0.68f, 0.07f, 0.02f, main, 0.02f, V(90f, 0f, 0f));
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = i * Mathf.PI * 0.4f;
                        hat.Sphere(top + V(Mathf.Cos(angle) * r * 0.35f, 0.2f, Mathf.Sin(angle) * r * 0.35f), r * 0.5f, main, 0.08f).noise = 0.01f;
                    }
                    hat.Sphere(top + V(0f, 0.26f, 0f), r * 0.5f, main, 0.08f);
                    break;
                case HatKind.CowboyHat:
                    hat.Ellipsoid(top + V(0f, -0.04f, 0f), V(r * 1.7f, 0.02f, r * 1.5f), main, 0.03f);
                    Paint(hat, hat.Torus(top + V(0f, -0.02f, 0f), r * 1.55f, 0.03f, accent, 0.01f));
                    hat.Ellipsoid(top + V(0f, 0.09f, 0f), V(r * 0.68f, 0.14f, r * 0.62f), main, 0.04f);
                    Carve(hat, hat.Ellipsoid(top + V(0f, 0.24f, 0f), V(0.04f, 0.06f, r * 0.4f), main, 0.03f));
                    Paint(hat, hat.Torus(top + V(0f, 0.0f, 0f), r * 0.68f, 0.03f, accent, 0.01f));
                    break;
                case HatKind.PartyHat:
                    hat.Limb(top - V(0f, 0.03f, 0f), top + V(0.02f, 0.42f, 0f), r * 0.45f, 0.012f, main, 0.02f);
                    for (int i = 0; i < 3; i++)
                    {
                        Paint(hat, hat.Torus(top + V(0f, 0.06f + i * 0.11f, 0f), r * (0.4f - i * 0.11f), 0.02f, accent, 0.008f));
                    }
                    hat.Sphere(top + V(0.02f, 0.45f, 0f), 0.05f, accent, 0.01f).noise = 0.01f;
                    break;
                case HatKind.Headband:
                    hat.Torus(fit.head + V(0f, fit.headRadii.y * 0.55f, 0f), fit.headRadii.x * 0.9f, 0.04f, main, 0.02f, V(-12f, 0f, 0f));
                    hat.Limb(fit.head + V(0.05f, fit.headRadii.y * 0.55f, -fit.headRadii.z * 0.92f), fit.head + V(0.18f, fit.headRadii.y * 0.25f, -fit.headRadii.z * 1.25f), 0.03f, 0.02f, main, 0.02f);
                    hat.Limb(fit.head + V(-0.05f, fit.headRadii.y * 0.55f, -fit.headRadii.z * 0.92f), fit.head + V(-0.16f, fit.headRadii.y * 0.18f, -fit.headRadii.z * 1.2f), 0.03f, 0.02f, main, 0.02f);
                    break;
                case HatKind.VikingHelmet:
                    hat.Ellipsoid(fit.head + V(0f, 0.04f, 0f), fit.headRadii * 1.08f, main, 0.02f);
                    Carve(hat, hat.Box(fit.head + V(0f, -0.3f + fit.headRadii.y * 0.25f, 0f), V(1.5f, 0.6f, 1.5f), 0f, main, 0.02f));
                    Paint(hat, hat.Torus(fit.head + V(0f, fit.headRadii.y * 0.28f, 0f), fit.headRadii.x * 1.02f, 0.035f, accent, 0.01f));
                    Paint(hat, hat.Box(top + V(0f, -0.08f, 0f), V(0.06f, 0.4f, fit.headRadii.z * 2.4f), 0f, accent, 0.01f));
                    hat.Mirror(s =>
                    {
                        Vector3 root = fit.head + V(s * fit.headRadii.x * 0.92f, fit.headRadii.y * 0.5f, 0f);
                        Vector3 mid = root + V(s * 0.14f, 0.1f, 0.02f);
                        hat.Limb(root, mid, 0.06f, 0.045f, new Color(0.95f, 0.9f, 0.78f), 0.02f);
                        return hat.Limb(mid, mid + V(s * 0.04f, 0.16f, 0.04f), 0.045f, 0.008f, new Color(0.95f, 0.9f, 0.78f), 0.015f);
                    });
                    break;
                case HatKind.PirateHat:
                    hat.Ellipsoid(top + V(0f, 0.04f, 0f), V(r * 1.45f, 0.16f, r * 0.55f), main, 0.03f);
                    Carve(hat, hat.Ellipsoid(top + V(0f, -0.08f, 0f), V(r * 1.1f, 0.1f, r * 0.9f), main, 0.02f));
                    Paint(hat, hat.Ellipsoid(top + V(0f, 0.18f, 0f), V(r * 1.5f, 0.03f, r * 0.6f), Gold, 0.02f));
                    Paint(hat, hat.Sphere(top + V(0f, 0.08f, r * 0.5f), 0.045f, White, 0.008f));
                    break;
                case HatKind.FlowerCrown:
                    hat.Torus(top - V(0f, 0.08f, 0.02f), r * 0.68f, 0.035f, main, 0.02f, V(-8f, 0f, 0f));
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * Mathf.PI / 4f;
                        Vector3 p = top - V(0f, 0.06f, 0.02f) + V(Mathf.Cos(angle) * r * 0.68f, 0f, Mathf.Sin(angle) * r * 0.68f);
                        Color petal = i % 3 == 0 ? accent : i % 3 == 1 ? new Color(1f, 0.9f, 0.35f) : White;
                        hat.Sphere(p + V(0f, 0.03f, 0f), 0.055f, petal, 0.02f).noise = 0.01f;
                        Paint(hat, hat.Sphere(p + V(0f, 0.07f, 0f), 0.02f, Gold, 0.005f));
                    }
                    break;
                case HatKind.WizardHat:
                    hat.Cylinder(top - V(0f, 0.04f, 0f), r * 1.25f, 0.02f, 0.015f, main, 0.03f, V(90f, 0f, 0f));
                    hat.Limb(top, top + V(0f, 0.38f, -0.04f), r * 0.62f, 0.1f, main, 0.05f);
                    hat.Limb(top + V(0f, 0.38f, -0.04f), top + V(0.02f, 0.55f, -0.16f), 0.1f, 0.015f, main, 0.04f);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = i * 1.7f;
                        Paint(hat, hat.Sphere(top + V(Mathf.Cos(angle) * r * 0.45f, 0.12f + i * 0.06f, Mathf.Sin(angle) * r * 0.45f), 0.035f, accent, 0.008f));
                    }
                    break;
                case HatKind.NightCap:
                    hat.Ellipsoid(top + V(0f, -0.02f, -0.02f), V(r * 0.85f, 0.12f, r * 0.82f), main, 0.04f);
                    hat.Limb(top + V(0f, 0.06f, -0.02f), top + V(0.18f, 0.22f, -0.08f), r * 0.55f, 0.08f, main, 0.05f);
                    hat.Limb(top + V(0.18f, 0.22f, -0.08f), top + V(0.36f, 0.06f, -0.1f), 0.08f, 0.03f, main, 0.04f);
                    hat.Sphere(top + V(0.38f, 0.02f, -0.1f), 0.06f, accent, 0.01f).noise = 0.012f;
                    for (int i = 0; i < 3; i++)
                    {
                        Paint(hat, hat.Torus(top + V(0.06f * i, 0.04f + i * 0.06f, -0.03f), r * (0.6f - i * 0.12f), 0.02f, accent, 0.008f, V(0f, 0f, -20f)));
                    }
                    break;
                case HatKind.SunHat:
                    hat.Cylinder(top - V(0f, 0.04f, 0f), r * 1.6f, 0.014f, 0.01f, main, 0.03f, V(90f, 0f, 0f));
                    hat.Ellipsoid(top + V(0f, 0.04f, 0f), V(r * 0.7f, 0.12f, r * 0.68f), main, 0.04f);
                    Paint(hat, hat.Torus(top + V(0f, 0f, 0f), r * 0.7f, 0.03f, accent, 0.01f));
                    break;
                case HatKind.TopHat:
                    hat.Cylinder(top - V(0f, 0.04f, 0f), r * 1.05f, 0.015f, 0.01f, main, 0.02f, V(90f, 0f, 0f));
                    hat.Cylinder(top + V(0f, 0.15f, 0f), r * 0.6f, 0.19f, 0.02f, main, 0.02f, V(90f, 0f, 0f));
                    Paint(hat, hat.Torus(top + V(0f, 0.01f, 0f), r * 0.6f, 0.03f, accent, 0.01f));
                    break;
                case HatKind.BucketHat:
                    hat.Limb(top - V(0f, 0.04f, 0f), top + V(0f, 0.12f, 0f), r * 0.8f, r * 0.6f, main, 0.04f);
                    hat.Ellipsoid(top - V(0f, 0.05f, 0f), V(r * 1.15f, 0.03f, r * 1.1f), main, 0.04f, V(4f, 0f, 0f));
                    Paint(hat, hat.Torus(top + V(0f, 0.0f, 0f), r * 0.76f, 0.025f, SpectatorSculptor.Shade(main, 0.7f), 0.01f));
                    break;
                case HatKind.CaptainHat:
                    hat.Cylinder(top + V(0f, 0.02f, 0f), r * 0.72f, 0.07f, 0.03f, main, 0.03f, V(90f, 0f, 0f));
                    hat.Ellipsoid(top + V(0f, 0.09f, -0.02f), V(r * 0.85f, 0.05f, r * 0.82f), main, 0.03f);
                    hat.Ellipsoid(top + V(0f, -0.03f, r * 0.62f), V(r * 0.5f, 0.015f, r * 0.3f), Black, 0.02f, V(15f, 0f, 0f));
                    Paint(hat, hat.Torus(top + V(0f, -0.02f, 0f), r * 0.72f, 0.025f, Black, 0.01f));
                    Paint(hat, hat.Sphere(top + V(0f, 0.04f, r * 0.72f), 0.04f, accent, 0.008f));
                    break;
            }
            return hat;
        }

        public static Sculpt Outfit(CostumeLook look, CostumeFit fit)
        {
            var outfit = new Sculpt();
            Vector3 c = fit.chest;
            Vector3 R = fit.chestRadii;
            Color main = look.outfit == OutfitKind.Cape || look.outfit == OutfitKind.Sash ? look.accent : look.main;
            Color accent = look.outfit == OutfitKind.Cape || look.outfit == OutfitKind.Sash ? look.main : look.accent;
            switch (look.outfit)
            {
                case OutfitKind.Cape:
                    outfit.Ellipsoid(V(0f, c.y - 0.12f, -R.z - 0.04f), V(R.x * 1.05f, 0.42f, 0.05f), main, 0.03f, V(-8f, 0f, 0f));
                    outfit.Torus(V(0f, fit.neck - 0.08f, 0f), R.x * 0.55f, 0.045f, accent, 0.02f);
                    break;
                case OutfitKind.LifeVest:
                    outfit.Ellipsoid(c, R * 1.08f, accent, 0.02f);
                    Carve(outfit, outfit.Ellipsoid(c, R * 1.01f, accent, 0.01f));
                    Carve(outfit, outfit.Box(c + V(0f, R.y * 0.95f, 0f), V(2f, R.y * 0.8f, 2f), 0f, accent, 0.02f));
                    Carve(outfit, outfit.Box(c + V(0f, R.y * 0.25f, R.z), V(0.12f, R.y * 1.4f, 0.4f), 0f, accent, 0.01f));
                    Paint(outfit, outfit.Torus(c + V(0f, -R.y * 0.2f, 0f), R.x * 1.05f, 0.03f, White, 0.01f));
                    break;
                case OutfitKind.BowTie:
                    {
                        Vector3 knot = V(0f, fit.neck - 0.13f, R.z * 0.85f);
                        outfit.Mirror(s => outfit.Ellipsoid(knot + V(s * 0.08f, 0f, 0f), V(0.08f, 0.05f, 0.03f), main, 0.02f, V(0f, 0f, s * 12f)));
                        outfit.Sphere(knot, 0.035f, accent, 0.01f);
                        break;
                    }
                case OutfitKind.Scarf:
                    outfit.Torus(V(0f, fit.neck - 0.1f, 0.01f), R.x * 0.52f, 0.06f, main, 0.04f);
                    outfit.Limb(V(R.x * 0.25f, fit.neck - 0.12f, R.z * 0.75f), V(R.x * 0.32f, fit.neck - 0.4f, R.z * 0.95f), 0.055f, 0.05f, main, 0.03f);
                    Paint(outfit, outfit.Box(V(R.x * 0.32f, fit.neck - 0.38f, R.z * 0.95f), V(0.14f, 0.04f, 0.14f), 0f, accent, 0.01f));
                    break;
                case OutfitKind.Apron:
                    outfit.Ellipsoid(c + V(0f, -0.08f, 0f), R * 1.06f, main, 0.02f);
                    Carve(outfit, outfit.Ellipsoid(c + V(0f, -0.08f, 0f), R * 1.0f, main, 0.01f));
                    Carve(outfit, outfit.Box(c + V(0f, 0f, -R.z), V(2f, 2f, R.z * 2.2f), 0f, main, 0.02f));
                    Carve(outfit, outfit.Box(c + V(0f, R.y * 1.05f, 0f), V(2f, R.y * 0.6f, 2f), 0f, main, 0.02f));
                    Paint(outfit, outfit.Box(c + V(0f, -0.15f, R.z), V(0.25f, 0.12f, 0.3f), 0.01f, accent, 0.01f));
                    break;
                case OutfitKind.Lei:
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = i * Mathf.PI / 6f;
                        Vector3 p = V(Mathf.Cos(angle) * R.x * 0.6f, fit.neck - 0.12f - Mathf.Max(0f, Mathf.Sin(angle)) * 0.08f, Mathf.Sin(angle) * R.z * 0.75f);
                        outfit.Sphere(p, 0.05f, i % 2 == 0 ? accent : new Color(1f, 0.85f, 0.3f), 0.02f).noise = 0.01f;
                    }
                    break;
                case OutfitKind.Medal:
                    outfit.Limb(V(-0.08f, fit.neck - 0.08f, R.z * 0.7f), V(0f, fit.neck - 0.28f, R.z * 0.98f), 0.02f, 0.02f, new Color(0.2f, 0.45f, 0.85f), 0.01f);
                    outfit.Limb(V(0.08f, fit.neck - 0.08f, R.z * 0.7f), V(0f, fit.neck - 0.28f, R.z * 0.98f), 0.02f, 0.02f, new Color(0.85f, 0.2f, 0.2f), 0.01f);
                    outfit.Cylinder(V(0f, fit.neck - 0.32f, R.z * 1.0f), 0.065f, 0.012f, 0.008f, accent, 0.01f);
                    break;
                case OutfitKind.Sash:
                    outfit.Torus(V(0f, c.y - R.y * 0.75f, 0f), R.x * 1.0f, 0.05f, main, 0.03f);
                    outfit.Limb(V(R.x * 0.5f, c.y - R.y * 0.75f, R.z * 0.85f), V(R.x * 0.65f, c.y - R.y * 1.3f, R.z * 0.9f), 0.04f, 0.03f, main, 0.02f);
                    break;
            }
            return outfit;
        }
    }
}
