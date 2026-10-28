using UnityEngine;
using static FartRafters.EditorTools.Sculpt.AvatarSculptor;

namespace FartRafters.EditorTools.Sculpt
{
    public class CostumeParts
    {
        public string name;
        public Sculpt head;
        public Sculpt body;
        public Sculpt armLeft;
        public Sculpt armRight;
    }

    public static class WardrobeSculptor
    {
        public const int Count = 3;

        static readonly Color Gold = new Color(1f, 0.8f, 0.22f);
        static readonly Color White = new Color(0.97f, 0.97f, 0.95f);
        static readonly Color Black = new Color(0.1f, 0.1f, 0.12f);
        static readonly Color Silver = new Color(0.78f, 0.8f, 0.84f);
        static readonly Color Leather = new Color(0.36f, 0.22f, 0.12f);
        static readonly Color Denim = new Color(0.22f, 0.36f, 0.62f);
        static readonly Color Pink = new Color(1f, 0.62f, 0.78f);
        static readonly Color Red = new Color(0.86f, 0.16f, 0.2f);

        struct Dims
        {
            public float armLength;
            public float armRadius;
            public float footSpread;
            public float footSize;
        }

        static Dims DimsFor(AvatarKind kind)
        {
            switch (kind)
            {
                case AvatarKind.Bear: return new Dims { armLength = 0.4f, armRadius = 0.1f, footSpread = 0.2f, footSize = 0.14f };
                case AvatarKind.Tombik: return new Dims { armLength = 0.38f, armRadius = 0.095f, footSpread = 0.21f, footSize = 0.13f };
                case AvatarKind.Ogre: return new Dims { armLength = 0.42f, armRadius = 0.12f, footSpread = 0.24f, footSize = 0.155f };
                case AvatarKind.Donkey: return new Dims { armLength = 0.38f, armRadius = 0.09f, footSpread = 0.2f, footSize = 0.13f };
                case AvatarKind.Sloth: return new Dims { armLength = 0.5f, armRadius = 0.085f, footSpread = 0.19f, footSize = 0.13f };
                case AvatarKind.Uncle: return new Dims { armLength = 0.4f, armRadius = 0.1f, footSpread = 0.22f, footSize = 0.14f };
                case AvatarKind.Dad: return new Dims { armLength = 0.38f, armRadius = 0.095f, footSpread = 0.21f, footSize = 0.135f };
                default: return new Dims { armLength = 0.36f, armRadius = 0.11f, footSpread = 0.24f, footSize = 0.15f };
            }
        }

        public static string[] Names(AvatarKind kind)
        {
            switch (kind)
            {
                case AvatarKind.Bear: return new[] { "Aşçı", "Motorcu", "Bal Arısı" };
                case AvatarKind.Tombik: return new[] { "Smokin", "Astronot", "Süper Kahraman" };
                case AvatarKind.Ogre: return new[] { "Viking", "Korsan", "Peri" };
                case AvatarKind.Donkey: return new[] { "Sihirbaz", "Kovboy", "Tek Boynuzlu At" };
                case AvatarKind.Sloth: return new[] { "Turist", "Ninja", "Uykucu" };
                case AvatarKind.Uncle: return new[] { "Beyefendi", "Balıkçı", "Bebek" };
                case AvatarKind.Dad: return new[] { "Mangalcı", "Kaptan", "Dinozor" };
                default: return new[] { "Kral", "Dalgıç", "Balerin" };
            }
        }

        public static CostumeParts Build(AvatarKind kind, int index)
        {
            var t = new Tailor(kind, AvatarSculptor.Create(kind), CostumeSculptor.Fit(kind), DimsFor(kind));
            t.parts.name = Names(kind)[index];
            switch (kind)
            {
                case AvatarKind.Bear:
                    if (index == 0) BearChef(t); else if (index == 1) BearBiker(t); else BearBee(t);
                    break;
                case AvatarKind.Tombik:
                    if (index == 0) TombikTux(t); else if (index == 1) TombikAstronaut(t); else TombikHero(t);
                    break;
                case AvatarKind.Ogre:
                    if (index == 0) OgreViking(t); else if (index == 1) OgrePirate(t); else OgreFairy(t);
                    break;
                case AvatarKind.Donkey:
                    if (index == 0) DonkeyWizard(t); else if (index == 1) DonkeyCowboy(t); else DonkeyUnicorn(t);
                    break;
                case AvatarKind.Sloth:
                    if (index == 0) SlothTourist(t); else if (index == 1) SlothNinja(t); else SlothSleepy(t);
                    break;
                case AvatarKind.Uncle:
                    if (index == 0) UncleGentleman(t); else if (index == 1) UncleFisher(t); else UncleBaby(t);
                    break;
                case AvatarKind.Dad:
                    if (index == 0) DadGrill(t); else if (index == 1) DadCaptain(t); else DadDino(t);
                    break;
                default:
                    if (index == 0) HippoKing(t); else if (index == 1) HippoDiver(t); else HippoBallerina(t);
                    break;
            }
            return t.parts;
        }

        static void HippoKing(Tailor t)
        {
            Color robe = new Color(0.62f, 0.1f, 0.22f);
            t.Hat(HatKind.Crown, Gold, Red);
            t.Outfit(robe, robe, Gold, 0.5f);
            t.Ermine(t.Neckline - 0.02f, 0.05f);
            t.Ermine(0.24f, 0.045f);
            t.Buttons(Gold, 3, 0.45f, 0.8f);
            t.Cape(new Color(0.5f, 0.06f, 0.16f), 0.62f, 0.06f);
            t.Sleeves(robe, 0.85f);
            t.Cuffs(White, 0.85f);
            t.Scepter(Gold, new Color(0.3f, 0.55f, 1f));
        }

        static void HippoDiver(Tailor t)
        {
            Color suit = new Color(0.12f, 0.14f, 0.2f);
            Color stripe = new Color(1f, 0.82f, 0.15f);
            t.Outfit(suit, suit, suit, 0.5f);
            t.Band(0.62f, 0.035f, 0.03f, stripe);
            t.Band(0.36f, 0.035f, 0.03f, stripe);
            t.Flippers(new Color(1f, 0.82f, 0.15f));
            t.SwimRing(new Color(1f, 0.45f, 0.15f), White, 0.5f);
            t.Sleeves(suit, 0.95f);
            t.Goggles(new Color(1f, 0.82f, 0.15f), new Color(0.55f, 0.85f, 1f));
            t.Snorkel(new Color(1f, 0.82f, 0.15f));
        }

        static void HippoBallerina(Tailor t)
        {
            t.Outfit(Pink, Pink, Pink, 0.45f, 0.016f);
            t.Tutu(Pink, White, 0.42f, 0.2f);
            t.BalletShoes(Pink);
            t.Sleeves(Pink, 0.25f);
            t.HairBun(new Color(0.42f, 0.28f, 0.2f), Pink);
        }

        static void BearChef(Tailor t)
        {
            Color check = new Color(0.2f, 0.2f, 0.25f);
            t.Hat(HatKind.ChefHat, White, Red);
            t.Outfit(White, check, Black, 0.42f);
            t.Checker(White, 0.42f);
            t.DoubleButtons(new Color(0.25f, 0.25f, 0.3f));
            t.Apron(White, Red, 0.62f, 0.2f);
            t.Scarf(Red);
            t.Sleeves(White, 0.75f);
            t.Ladle(Silver);
        }

        static void BearBiker(Tailor t)
        {
            Color jacket = new Color(0.12f, 0.11f, 0.12f);
            t.Outfit(jacket, Denim, Black, 0.48f, 0.024f);
            t.Zipper(Silver, 0.5f);
            t.Band(0.48f, 0.04f, 0.03f, Leather);
            t.Buckle(Silver, 0.48f);
            t.Boots(Black, 0.24f);
            t.Sleeves(jacket, 0.95f);
            t.Gloves(Black);
            t.Helmet(Red, White, new Color(0.15f, 0.15f, 0.18f));
        }

        static void BearBee(Tailor t)
        {
            Color yellow = new Color(1f, 0.82f, 0.12f);
            t.Outfit(yellow, yellow, Black, 0.3f, 0.02f);
            for (int i = 0; i < 4; i++)
            {
                t.Band(0.36f + i * 0.17f, 0.045f, 0.022f, Black);
            }
            t.Sleeves(yellow, 0.9f);
            t.SleeveStripes(Black, 3);
            t.Wings(new Color(0.88f, 0.95f, 1f), 0.8f, 0.36f);
            t.Stinger(Black);
            t.Antennae(Black, yellow);
        }

        static void TombikTux(Tailor t)
        {
            t.Hat(HatKind.TopHat, Black, Red);
            t.Outfit(Black, Black, Black, 0.5f);
            t.ShirtFront(White, Black);
            t.BowTie(Black);
            t.Lapels(new Color(0.2f, 0.2f, 0.24f));
            t.Sleeves(Black, 0.92f);
            t.Cuffs(White, 0.92f);
            t.Shoes(new Color(0.05f, 0.05f, 0.06f), White);
        }

        static void TombikAstronaut(Tailor t)
        {
            Color suit = new Color(0.94f, 0.94f, 0.96f);
            t.Outfit(suit, suit, new Color(0.6f, 0.6f, 0.66f), 0.45f, 0.03f);
            t.Band(0.46f, 0.04f, 0.04f, new Color(0.25f, 0.45f, 0.85f));
            t.ChestPanel(new Color(0.3f, 0.3f, 0.35f), Red, new Color(0.3f, 0.9f, 0.4f));
            t.Backpack(suit, new Color(0.6f, 0.6f, 0.66f));
            t.Boots(new Color(0.6f, 0.6f, 0.66f), 0.22f);
            t.Sleeves(suit, 0.95f);
            t.Gloves(new Color(0.6f, 0.6f, 0.66f));
            t.SpaceHelmet(suit, new Color(0.25f, 0.45f, 0.85f));
        }

        static void TombikHero(Tailor t)
        {
            Color tights = new Color(0.2f, 0.4f, 0.9f);
            t.Outfit(tights, tights, Red, 0.45f, 0.018f);
            t.Briefs(Red, 0.42f);
            t.Band(0.45f, 0.035f, 0.035f, Gold);
            t.Emblem(Gold, Red);
            t.Cape(Red, 0.75f, 0.05f);
            t.Boots(Red, 0.22f);
            t.Sleeves(tights, 0.95f);
            t.Gloves(Red);
            t.EyeMask(Red);
        }

        static void OgreViking(Tailor t)
        {
            Color tunic = new Color(0.32f, 0.42f, 0.62f);
            Color fur = new Color(0.55f, 0.4f, 0.26f);
            t.Hat(HatKind.VikingHelmet, new Color(0.55f, 0.55f, 0.6f), new Color(0.5f, 0.32f, 0.18f));
            t.Outfit(tunic, new Color(0.42f, 0.3f, 0.18f), Leather, 0.42f);
            t.FurCollar(fur);
            t.Band(0.44f, 0.05f, 0.035f, Leather);
            t.Buckle(Gold, 0.44f);
            t.Boots(Leather, 0.26f);
            t.FurTrim(fur, 0.26f);
            t.Sleeves(tunic, 0.6f);
            t.Bracers(Leather, Gold);
            t.Shield(new Color(0.75f, 0.2f, 0.18f), Gold, Silver);
        }

        static void OgrePirate(Tailor t)
        {
            t.Hat(HatKind.PirateHat, Black, Red);
            t.Outfit(White, Black, Black, 0.44f);
            t.Stripes(Red, 0.5f, 0.92f, 4);
            t.Vest(new Color(0.2f, 0.12f, 0.08f));
            t.Sash(Red, 0.46f);
            t.Boots(Black, 0.28f);
            t.Sleeves(White, 0.85f);
            t.Hook(Silver);
            t.EyePatch(Black);
        }

        static void OgreFairy(Tailor t)
        {
            Color dress = new Color(1f, 0.55f, 0.8f);
            t.Outfit(dress, dress, dress, 0.4f, 0.02f);
            t.Skirt(dress, new Color(1f, 0.85f, 0.95f), 0.42f, 0.3f);
            t.Wings(new Color(1f, 0.75f, 0.95f), 1.05f, 0.5f);
            t.BalletShoes(dress);
            t.Sleeves(dress, 0.3f);
            t.Tiara(Silver, new Color(0.6f, 0.9f, 1f));
            t.Wand(Gold, new Color(1f, 0.95f, 0.4f));
        }

        static void DonkeyWizard(Tailor t)
        {
            Color robe = new Color(0.3f, 0.2f, 0.62f);
            t.Hat(HatKind.WizardHat, robe, Gold);
            t.Outfit(robe, robe, new Color(0.25f, 0.15f, 0.4f), 0.3f, 0.024f);
            t.Stars(Gold, 9);
            t.Band(0.46f, 0.04f, 0.035f, Gold);
            t.Cape(new Color(0.2f, 0.12f, 0.45f), 0.72f, 0.06f);
            t.Sleeves(robe, 0.92f);
            t.Cuffs(Gold, 0.92f);
            t.Staff(Leather, new Color(0.4f, 0.9f, 1f));
        }

        static void DonkeyCowboy(Tailor t)
        {
            Color shirt = new Color(0.85f, 0.3f, 0.25f);
            t.Hat(HatKind.CowboyHat, new Color(0.55f, 0.35f, 0.2f), new Color(0.3f, 0.18f, 0.1f));
            t.Outfit(shirt, Denim, Leather, 0.44f);
            t.Plaid(new Color(0.6f, 0.15f, 0.12f), 0.44f);
            t.Vest(new Color(0.55f, 0.35f, 0.2f));
            t.Band(0.44f, 0.045f, 0.03f, Leather);
            t.Buckle(Gold, 0.44f);
            t.Boots(Leather, 0.3f);
            t.Spurs(Silver);
            t.Sleeves(shirt, 0.9f);
            t.Scarf(new Color(0.95f, 0.85f, 0.2f));
        }

        static void DonkeyUnicorn(Tailor t)
        {
            Color white = new Color(0.98f, 0.96f, 1f);
            t.Outfit(white, white, new Color(0.85f, 0.7f, 1f), 0.4f, 0.02f);
            t.Hooves(new Color(0.85f, 0.7f, 1f));
            t.Sleeves(white, 0.95f);
            t.RainbowMane();
            t.Horn(Gold);
            t.RainbowTail();
            t.Wings(new Color(0.95f, 0.85f, 1f), 0.8f, 0.4f);
        }

        static void SlothTourist(Tailor t)
        {
            Color shirt = new Color(0.25f, 0.65f, 0.85f);
            Color shorts = new Color(0.9f, 0.82f, 0.6f);
            t.Hat(HatKind.SunHat, new Color(0.95f, 0.85f, 0.55f), new Color(1f, 0.45f, 0.3f));
            t.Outfit(shirt, shorts, new Color(0.55f, 0.35f, 0.2f), 0.44f);
            t.Flowers(new Color(1f, 0.85f, 0.25f), new Color(1f, 0.45f, 0.55f), 0.44f);
            t.ShortsHem(0.26f);
            t.Sandals(new Color(0.55f, 0.35f, 0.2f));
            t.Sleeves(shirt, 0.4f);
            t.Camera(Black, Silver);
            t.Sunglasses(Black);
        }

        static void SlothNinja(Tailor t)
        {
            Color suit = new Color(0.1f, 0.1f, 0.13f);
            t.Outfit(suit, suit, suit, 0.46f, 0.022f);
            t.Sash(new Color(0.75f, 0.12f, 0.15f), 0.46f);
            t.Sleeves(suit, 0.95f);
            t.Wraps(new Color(0.2f, 0.2f, 0.25f));
            t.NinjaHood(suit, new Color(0.75f, 0.12f, 0.15f));
            t.Katana(Black, Silver);
        }

        static void SlothSleepy(Tailor t)
        {
            Color pj = new Color(0.45f, 0.6f, 0.95f);
            t.Hat(HatKind.NightCap, pj, White);
            t.Outfit(pj, pj, new Color(1f, 0.75f, 0.85f), 0.44f, 0.022f);
            t.Dots(White, 14);
            t.Buttons(White, 4, 0.42f, 0.95f);
            t.BunnySlippers(new Color(1f, 0.8f, 0.88f));
            t.Sleeves(pj, 0.92f);
            t.Cuffs(White, 0.92f);
            t.Pillow(White, new Color(0.65f, 0.75f, 1f));
        }

        static void UncleGentleman(Tailor t)
        {
            Color coat = new Color(0.12f, 0.12f, 0.16f);
            t.Hat(HatKind.TopHat, Black, Red);
            t.Outfit(coat, new Color(0.25f, 0.25f, 0.3f), Black, 0.5f);
            t.ShirtFront(White, coat);
            t.BowTie(Red);
            t.Lapels(new Color(0.2f, 0.2f, 0.26f));
            t.Tails(coat);
            t.Buttons(Gold, 2, 0.55f, 0.68f);
            t.Sleeves(coat, 0.92f);
            t.Cuffs(White, 0.92f);
            t.Shoes(Black, White);
            t.Monocle(Gold);
            t.Cane(Black, Gold);
        }

        static void UncleFisher(Tailor t)
        {
            Color vest = new Color(0.42f, 0.5f, 0.3f);
            t.Hat(HatKind.BucketHat, new Color(0.45f, 0.55f, 0.35f), new Color(1f, 0.5f, 0.1f));
            t.Outfit(new Color(0.85f, 0.85f, 0.8f), new Color(0.35f, 0.32f, 0.28f), new Color(1f, 0.82f, 0.1f), 0.48f);
            t.Vest(vest);
            t.Pockets(SpectatorSculptor.Shade(vest, 0.8f));
            t.Boots(new Color(1f, 0.82f, 0.1f), 0.36f);
            t.Sleeves(new Color(0.85f, 0.85f, 0.8f), 0.85f);
            t.FishingRod(Leather, Silver);
        }

        static void UncleBaby(Tailor t)
        {
            t.Diaper(White, new Color(0.6f, 0.85f, 1f));
            t.Bib(new Color(0.65f, 0.85f, 1f), White);
            t.Bonnet(new Color(0.65f, 0.85f, 1f), White);
            t.Pacifier(new Color(1f, 0.6f, 0.75f), White);
            t.Rattle(new Color(1f, 0.85f, 0.25f), new Color(1f, 0.5f, 0.6f));
            t.Booties(new Color(0.65f, 0.85f, 1f));
        }

        static void DadGrill(Tailor t)
        {
            Color polo = new Color(0.25f, 0.55f, 0.35f);
            t.Hat(HatKind.ChefHat, White, Red);
            t.Outfit(polo, new Color(0.85f, 0.75f, 0.55f), new Color(0.35f, 0.25f, 0.18f), 0.46f);
            t.Collar(SpectatorSculptor.Shade(polo, 0.8f));
            t.ShortsHem(0.26f);
            t.Apron(Red, White, 0.78f, 0.24f);
            t.Flame(new Color(1f, 0.85f, 0.2f), new Color(1f, 0.45f, 0.1f));
            t.Sleeves(polo, 0.35f);
            t.Tongs(Silver);
        }

        static void DadCaptain(Tailor t)
        {
            t.Hat(HatKind.CaptainHat, White, Gold);
            t.Outfit(White, White, Black, 0.48f);
            t.DoubleButtons(Gold);
            t.Band(0.48f, 0.04f, 0.03f, Black);
            t.Buckle(Gold, 0.48f);
            t.Epaulettes(Gold);
            t.Medals(Gold, Red, new Color(0.2f, 0.45f, 0.85f));
            t.Sleeves(White, 0.92f);
            t.CuffStripes(Gold, 0.92f);
        }

        static void DadDino(Tailor t)
        {
            Color green = new Color(0.35f, 0.72f, 0.3f);
            t.Outfit(green, green, green, 0.4f, 0.026f);
            t.Belly(new Color(0.85f, 0.9f, 0.55f));
            t.Sleeves(green, 0.95f);
            t.Claws(White);
            t.DinoHood(green, White, new Color(1f, 0.55f, 0.15f));
            t.DinoTail(green, new Color(1f, 0.55f, 0.15f));
            t.Spikes(new Color(1f, 0.55f, 0.15f));
            t.DinoFeet(green, White);
        }

        class Tailor
        {
            public readonly CostumeParts parts = new CostumeParts();
            readonly AvatarSculpture avatar;
            readonly CostumeFit fit;
            readonly Dims dims;
            readonly AvatarKind kind;
            Color bottomColor = Color.gray;

            public Tailor(AvatarKind kind, AvatarSculpture avatar, CostumeFit fit, Dims dims)
            {
                this.kind = kind;
                this.avatar = avatar;
                this.fit = fit;
                this.dims = dims;
                avatar.body.Prepare();
                avatar.head.Prepare();
                avatar.armLeft.Prepare();
                avatar.armRight.Prepare();
            }

            public float Neckline => fit.neck - 0.07f;

            Sculpt Body => parts.body ??= new Sculpt();
            Sculpt Head => parts.head ??= new Sculpt();
            Sculpt ArmL => parts.armLeft ??= new Sculpt();
            Sculpt ArmR => parts.armRight ??= new Sculpt();

            static SdfShape Clip(SdfShape shape)
            {
                shape.op = SdfOp.Intersect;
                return shape;
            }

            float FrontZ(float y, float x = 0f)
            {
                for (float z = 1f; z > -1f; z -= 0.005f)
                {
                    if (avatar.body.Distance(V(x, y, z)) <= 0f)
                    {
                        return z;
                    }
                }
                return 0.3f;
            }

            float BackZ(float y, float x = 0f)
            {
                for (float z = -1f; z < 1f; z += 0.005f)
                {
                    if (avatar.body.Distance(V(x, y, z)) <= 0f)
                    {
                        return z;
                    }
                }
                return -0.3f;
            }

            float SideX(float y, float z = 0f)
            {
                for (float x = 1f; x > 0f; x -= 0.005f)
                {
                    if (avatar.body.Distance(V(x, y, z)) <= 0f)
                    {
                        return x;
                    }
                }
                return 0.3f;
            }

            Vector3 HeadFront(float y)
            {
                for (float z = 1f; z > -1f; z -= 0.004f)
                {
                    if (avatar.head.Distance(V(0f, y, z)) <= 0f)
                    {
                        return V(0f, y, z);
                    }
                }
                return V(0f, y, 0.3f);
            }

            Vector3 Hand(float side)
            {
                float L = dims.armLength;
                float r = dims.armRadius;
                return V(side * 0.08f, -L - r * 0.7f, 0.06f + r * 0.15f);
            }

            Sculpt Arm(float side)
            {
                return side < 0f ? ArmL : ArmR;
            }

            Sculpt ArmSource(float side)
            {
                return side < 0f ? avatar.armLeft : avatar.armRight;
            }

            public void Hat(HatKind hat, Color main, Color accent)
            {
                Head.Append(CostumeSculptor.Hat(new CostumeLook { hat = hat, main = main, accent = accent }, fit));
            }

            public void Outfit(Color top, Color bottom, Color shoes, float waist, float inflate = 0.02f)
            {
                bottomColor = bottom;
                Body.Shell(avatar.body, inflate, top, 0.01f);
                Carve(Body, Body.Box(V(0f, Neckline + 0.5f, 0f), V(3f, 1f, 3f), 0f, top, 0.01f));
                Paint(Body, Body.Box(V(0f, waist - 0.5f, 0f), V(3f, 1f, 3f), 0f, bottom, 0.006f));
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.07f, 0.11f);
                    Body.Ellipsoid(foot, V(dims.footSize * 1.12f, dims.footSize * 0.6f, dims.footSize * 1.38f), shoes, 0.03f);
                    Paint(Body, Body.Box(foot + V(0f, -0.075f, 0f), V(dims.footSize * 2.6f, 0.04f, dims.footSize * 3.2f), 0f, SpectatorSculptor.Shade(shoes, 0.55f), 0.008f));
                }
            }

            public void Band(float y, float height, float inflate, Color color)
            {
                var band = new Sculpt();
                band.Shell(avatar.body, inflate, color, 0.01f);
                Clip(band.Box(V(0f, y, 0f), V(3f, height, 3f), 0.01f, color, 0.008f));
                Body.Shell(band, 0f, color, 0.004f);
            }

            public void Buckle(Color color, float y)
            {
                float z = FrontZ(y) + 0.04f;
                Body.Box(V(0f, y, z), V(0.11f, 0.08f, 0.03f), 0.012f, color, 0.006f);
                Carve(Body, Body.Box(V(0f, y, z + 0.02f), V(0.06f, 0.035f, 0.03f), 0.006f, color, 0.004f));
            }

            public void Buttons(Color color, int count, float from, float to)
            {
                for (int i = 0; i < count; i++)
                {
                    float y = Mathf.Lerp(from, Mathf.Min(to, Neckline - 0.04f), count == 1 ? 0.5f : i / (float)(count - 1));
                    Body.Sphere(V(0f, y, FrontZ(y) + 0.025f), 0.028f, color, 0.006f);
                }
            }

            public void DoubleButtons(Color color)
            {
                for (int i = 0; i < 3; i++)
                {
                    float y = 0.55f + i * 0.12f;
                    if (y > Neckline - 0.04f)
                    {
                        break;
                    }
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float x = s * 0.1f;
                        Body.Sphere(V(x, y, FrontZ(y, x) + 0.025f), 0.026f, color, 0.006f);
                    }
                }
            }

            public void Zipper(Color color, float waist)
            {
                for (float y = waist; y < Neckline - 0.02f; y += 0.03f)
                {
                    Paint(Body, Body.Box(V(0f, y, FrontZ(y) + 0.02f), V(0.02f, 0.025f, 0.08f), 0f, color, 0.003f));
                }
                Body.Box(V(0.02f, Neckline - 0.06f, FrontZ(Neckline - 0.06f) + 0.03f), V(0.03f, 0.06f, 0.015f), 0.005f, color, 0.004f);
            }

            public void Ermine(float y, float height)
            {
                Band(y, height, 0.04f, White);
                float r = SideX(y) + 0.04f;
                for (int i = 0; i < 12; i++)
                {
                    float a = i * Mathf.PI / 6f;
                    Paint(Body, Body.Sphere(V(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r * 0.95f), 0.018f, Black, 0.004f));
                }
            }

            public void Cape(Color color, float length, float inflate)
            {
                var cape = new Sculpt();
                cape.Shell(avatar.body, inflate, color, 0.01f);
                Carve(cape, cape.Shell(avatar.body, inflate - 0.025f, color, 0.004f));
                Clip(cape.Box(V(0f, 0f, -1.04f), V(3f, 6f, 2f), 0f, color, 0.02f));
                Clip(cape.Box(V(0f, Neckline - length * 0.5f, 0f), V(3f, length, 3f), 0f, color, 0.02f));
                Body.Shell(cape, 0f, color, 0.006f);
                Body.Torus(V(0f, Neckline + 0.01f, 0.02f), SideX(Neckline) * 0.82f, 0.03f, SpectatorSculptor.Shade(color, 1.25f), 0.015f);
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * SideX(Neckline) * 0.6f;
                    Body.Sphere(V(x, Neckline - 0.02f, FrontZ(Neckline - 0.02f, x) + 0.01f), 0.03f, Gold, 0.008f);
                }
            }

            public void Apron(Color color, Color trim, float top, float bottom)
            {
                var apron = new Sculpt();
                apron.Shell(avatar.body, 0.045f, color, 0.01f);
                Carve(apron, apron.Shell(avatar.body, 0.02f, color, 0.004f));
                Clip(apron.Box(V(0f, 0f, 1.12f), V(3f, 6f, 2f), 0f, color, 0.02f));
                Clip(apron.Box(V(0f, (top + bottom) * 0.5f, 0f), V(SideX(0.55f) * 1.5f, top - bottom, 3f), 0.02f, color, 0.02f));
                Body.Shell(apron, 0f, color, 0.006f);
                float py = Mathf.Lerp(bottom, top, 0.35f);
                Paint(Body, Body.Box(V(0f, py, FrontZ(py) + 0.04f), V(0.26f, 0.12f, 0.2f), 0.01f, trim, 0.006f));
                Band(Mathf.Lerp(bottom, top, 0.55f), 0.03f, 0.035f, trim);
            }

            public void Tutu(Color color, Color light, float y, float flare)
            {
                float r = SideX(y);
                for (int layer = 0; layer < 3; layer++)
                {
                    float ly = y - layer * 0.035f;
                    float lr = r + flare - layer * 0.03f;
                    Body.Ellipsoid(V(0f, ly, 0f), V(lr, 0.03f, lr * 0.95f), layer == 1 ? light : color, 0.02f);
                    for (int i = 0; i < 16; i++)
                    {
                        float a = i * Mathf.PI / 8f + layer * 0.2f;
                        Body.Sphere(V(Mathf.Cos(a) * lr, ly - 0.01f, Mathf.Sin(a) * lr * 0.95f), 0.04f, layer == 1 ? light : color, 0.02f).noise = 0.01f;
                    }
                }
            }

            public void Skirt(Color color, Color hem, float waist, float length)
            {
                float r = SideX(waist);
                Body.Limb(V(0f, waist, 0f), V(0f, waist - length, 0f), r + 0.03f, r + 0.16f, color, 0.04f);
                Carve(Body, Body.Cylinder(V(0f, waist - length - 0.8f, 0f), 1.2f, 0.8f, 0f, color, 0.01f, V(90f, 0f, 0f)));
                Body.Torus(V(0f, waist - length + 0.02f, 0f), r + 0.15f, 0.03f, hem, 0.02f);
            }

            public void Wings(Color color, float size, float y)
            {
                float z = BackZ(Mathf.Lerp(y, Neckline, 0.6f)) - 0.02f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Body.Ellipsoid(V(s * size * 0.32f, y + size * 0.42f, z - 0.05f), V(size * 0.3f, size * 0.38f, 0.025f), color, 0.02f, V(0f, s * 20f, s * -25f));
                    Body.Ellipsoid(V(s * size * 0.26f, y + size * 0.08f, z - 0.04f), V(size * 0.22f, size * 0.22f, 0.025f), SpectatorSculptor.Shade(color, 0.92f), 0.02f, V(0f, s * 20f, s * 35f));
                }
                Body.Sphere(V(0f, Mathf.Lerp(y, Neckline, 0.6f), z), 0.05f, SpectatorSculptor.Shade(color, 0.8f), 0.02f);
            }

            public void Stinger(Color color)
            {
                float y = 0.42f;
                float z = BackZ(y);
                Body.Limb(V(0f, y, z + 0.02f), V(0f, y - 0.04f, z - 0.16f), 0.05f, 0.005f, color, 0.02f);
            }

            public void BalletShoes(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.07f, 0.11f);
                    Body.Torus(foot + V(0f, 0.08f, -0.04f), dims.footSize * 0.75f, 0.015f, color, 0.01f);
                    Body.Mirror(k => Body.Limb(foot + V(k * 0.03f, 0.09f, dims.footSize), foot + V(k * 0.07f, 0.14f, dims.footSize * 1.2f), 0.02f, 0.012f, color, 0.01f));
                }
            }

            public void Flippers(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.03f), 0.03f, 0.25f);
                    Body.Ellipsoid(foot, V(dims.footSize * 1.1f, 0.025f, dims.footSize * 2.4f), color, 0.03f);
                    Paint(Body, Body.Box(foot + V(0f, 0f, dims.footSize * 1.2f), V(dims.footSize * 2.4f, 0.1f, 0.03f), 0f, SpectatorSculptor.Shade(color, 0.7f), 0.005f));
                }
            }

            public void SwimRing(Color color, Color stripe, float y)
            {
                float r = SideX(y) + 0.08f;
                Body.Torus(V(0f, y, 0.01f), r, 0.09f, color, 0.02f);
                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI * 0.5f + 0.4f;
                    Paint(Body, Body.Box(V(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r), V(0.14f, 0.3f, 0.14f), 0f, stripe, 0.005f, V(0f, -a * Mathf.Rad2Deg, 0f)));
                }
            }

            public void Checker(Color light, float waist)
            {
                for (float y = 0.12f; y < waist; y += 0.07f)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        if ((i + Mathf.RoundToInt(y / 0.07f)) % 2 == 0)
                        {
                            continue;
                        }
                        float a = i * Mathf.PI / 9f;
                        Paint(Body, Body.Box(V(Mathf.Cos(a) * 0.5f, y, Mathf.Sin(a) * 0.5f), V(0.09f, 0.07f, 0.6f), 0f, light, 0.002f, V(0f, -a * Mathf.Rad2Deg, 0f)));
                    }
                }
            }

            public void Scarf(Color color)
            {
                float r = SideX(Neckline) * 0.85f;
                Body.Torus(V(0f, Neckline + 0.01f, 0.02f), r, 0.05f, color, 0.03f);
                float z = FrontZ(Neckline - 0.05f, 0.08f);
                Body.Limb(V(0.08f, Neckline - 0.02f, z + 0.03f), V(0.13f, Neckline - 0.2f, z + 0.05f), 0.045f, 0.04f, color, 0.02f);
                Body.Ellipsoid(V(0.13f, Neckline - 0.22f, z + 0.05f), V(0.06f, 0.03f, 0.02f), color, 0.02f);
            }

            public void Boots(Color color, float top)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * (dims.footSpread + 0.01f);
                    Body.Limb(V(x, top, 0.0f), V(x + s * 0.01f, 0.1f, 0.05f), dims.footSize * 1.12f, dims.footSize * 1.0f, color, 0.03f);
                    Body.Torus(V(x, top, 0f), dims.footSize * 1.05f, 0.022f, SpectatorSculptor.Shade(color, 0.75f), 0.01f);
                }
            }

            public void FurTrim(Color color, float top)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * (dims.footSpread + 0.01f);
                    Body.Torus(V(x, top + 0.01f, 0f), dims.footSize * 1.12f, 0.04f, color, 0.02f).noise = 0.02f;
                }
            }

            public void Spurs(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 heel = V(s * (dims.footSpread + 0.02f), 0.1f, -0.06f);
                    Body.Limb(heel, heel + V(0f, 0f, -0.08f), 0.01f, 0.01f, color, 0.005f);
                    Body.Cylinder(heel + V(0f, 0f, -0.1f), 0.035f, 0.006f, 0.002f, color, 0.004f, V(0f, 90f, 0f));
                }
            }

            public void Shoes(Color color, Color sock)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.07f, 0.12f);
                    Body.Ellipsoid(foot, V(dims.footSize * 1.18f, dims.footSize * 0.62f, dims.footSize * 1.48f), color, 0.03f);
                    Paint(Body, Body.Sphere(foot + V(0.02f, 0.06f, dims.footSize * 0.6f), 0.03f, White, 0.01f));
                }
            }

            public void Sandals(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.01f, 0.11f);
                    Body.Ellipsoid(foot, V(dims.footSize * 1.15f, 0.02f, dims.footSize * 1.45f), color, 0.01f);
                }
            }

            public void Hooves(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.06f, 0.11f);
                    Body.Cylinder(foot, dims.footSize * 1.15f, 0.07f, 0.02f, color, 0.02f, V(90f, 0f, 0f));
                }
            }

            public void Booties(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.08f, 0.1f);
                    Body.Ellipsoid(foot, V(dims.footSize * 1.2f, dims.footSize * 0.8f, dims.footSize * 1.4f), color, 0.04f).noise = 0.01f;
                    Body.Sphere(foot + V(0f, dims.footSize * 0.7f, dims.footSize * 0.6f), 0.035f, White, 0.01f);
                }
            }

            public void BunnySlippers(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.08f, 0.12f);
                    Body.Ellipsoid(foot, V(dims.footSize * 1.25f, dims.footSize * 0.75f, dims.footSize * 1.5f), color, 0.04f).noise = 0.012f;
                    Body.Mirror(k => Body.Ellipsoid(foot + V(k * 0.05f, dims.footSize * 0.9f, dims.footSize * 0.7f), V(0.025f, 0.08f, 0.02f), color, 0.015f, V(-15f, 0f, k * -15f)));
                    Body.Mirror(k => Body.Sphere(foot + V(k * 0.04f, dims.footSize * 0.45f, dims.footSize * 1.35f), 0.014f, Black, 0.003f));
                }
            }

            public void DinoFeet(Color color, Color claw)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 foot = V(s * (dims.footSpread + 0.02f), 0.07f, 0.12f);
                    Body.Ellipsoid(foot, V(dims.footSize * 1.25f, dims.footSize * 0.7f, dims.footSize * 1.5f), color, 0.03f);
                    for (int k = -1; k <= 1; k++)
                    {
                        Body.Limb(foot + V(k * dims.footSize * 0.45f, 0f, dims.footSize * 1.3f), foot + V(k * dims.footSize * 0.5f, -0.02f, dims.footSize * 1.65f), 0.022f, 0.006f, claw, 0.008f);
                    }
                }
            }

            public void Sleeves(Color color, float coverage, float inflate = 0.016f)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Sculpt arm = Arm(s);
                    arm.Shell(ArmSource(s), inflate, color, 0.01f);
                    float cut = -dims.armLength * coverage;
                    Carve(arm, arm.Box(V(0f, cut - 0.5f, 0f), V(2f, 1f, 2f), 0f, color, 0.008f));
                }
            }

            public void Cuffs(Color color, float coverage)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float y = -dims.armLength * coverage;
                    Vector3 c = V(s * 0.07f * coverage, y + 0.02f, 0.06f * coverage);
                    Arm(s).Torus(c, dims.armRadius * 0.98f, 0.025f, color, 0.01f);
                }
            }

            public void CuffStripes(Color color, float coverage)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float y = -dims.armLength * coverage + 0.03f + i * 0.04f;
                        Paint(Arm(s), Arm(s).Box(V(0f, y, 0f), V(1f, 0.016f, 1f), 0f, color, 0.003f));
                    }
                }
            }

            public void SleeveStripes(Color color, int count)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int i = 0; i < count; i++)
                    {
                        float y = -dims.armLength * (0.2f + i * 0.25f);
                        Paint(Arm(s), Arm(s).Box(V(0f, y, 0f), V(1f, 0.05f, 1f), 0f, color, 0.004f));
                    }
                }
            }

            public void Gloves(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Sculpt arm = Arm(s);
                    var glove = new Sculpt();
                    glove.Shell(ArmSource(s), 0.014f, color, 0.01f);
                    Clip(glove.Box(V(0f, -dims.armLength - 0.5f + 0.04f, 0f), V(2f, 1f, 2f), 0f, color, 0.008f));
                    arm.Shell(glove, 0f, color, 0.004f);
                }
            }

            public void Bracers(Color color, Color stud)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Sculpt arm = Arm(s);
                    var bracer = new Sculpt();
                    bracer.Shell(ArmSource(s), 0.03f, color, 0.01f);
                    Clip(bracer.Box(V(0f, -dims.armLength * 0.82f, 0f), V(2f, dims.armLength * 0.3f, 2f), 0f, color, 0.008f));
                    arm.Shell(bracer, 0f, color, 0.004f);
                    arm.Sphere(V(s * 0.06f, -dims.armLength * 0.8f, 0.05f + dims.armRadius), 0.018f, stud, 0.004f);
                }
            }

            public void Wraps(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float y = -dims.armLength * (0.72f + i * 0.08f);
                        Arm(s).Torus(V(s * 0.06f, y, 0.05f), dims.armRadius * 0.95f, 0.014f, color, 0.006f, V(10f, 0f, 0f));
                    }
                }
            }

            public void Claws(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 hand = Hand(s);
                    for (int k = -1; k <= 1; k++)
                    {
                        Vector3 root = hand + V(k * dims.armRadius * 0.5f, -dims.armRadius * 0.6f, dims.armRadius * 0.4f);
                        Arm(s).Limb(root, root + V(0f, -0.06f, 0.05f), 0.02f, 0.004f, color, 0.006f);
                    }
                }
            }

            public void Scepter(Color gold, Color gem)
            {
                Vector3 hand = Hand(1f);
                Sculpt arm = ArmR;
                arm.Limb(hand + V(0f, -0.18f, 0f), hand + V(0f, 0.28f, 0.04f), 0.022f, 0.022f, gold, 0.008f);
                arm.Sphere(hand + V(0f, 0.33f, 0.045f), 0.055f, gem, 0.01f);
                arm.Torus(hand + V(0f, 0.28f, 0.04f), 0.04f, 0.012f, gold, 0.006f);
            }

            public void Ladle(Color color)
            {
                Vector3 hand = Hand(1f);
                ArmR.Limb(hand + V(0f, -0.05f, 0f), hand + V(0f, 0.3f, 0.06f), 0.014f, 0.012f, color, 0.006f);
                ArmR.Sphere(hand + V(0f, 0.36f, 0.12f), 0.07f, color, 0.01f);
                Carve(ArmR, ArmR.Sphere(hand + V(0f, 0.39f, 0.14f), 0.062f, color, 0.006f));
            }

            public void Wand(Color stick, Color star)
            {
                Vector3 hand = Hand(1f);
                ArmR.Limb(hand + V(0f, -0.04f, 0f), hand + V(0f, 0.26f, 0.08f), 0.012f, 0.01f, stick, 0.005f);
                Vector3 c = hand + V(0f, 0.31f, 0.09f);
                for (int i = 0; i < 5; i++)
                {
                    float a = i * Mathf.PI * 0.4f + Mathf.PI * 0.5f;
                    ArmR.Limb(c, c + V(Mathf.Cos(a) * 0.08f, Mathf.Sin(a) * 0.08f, 0f), 0.03f, 0.008f, star, 0.01f);
                }
            }

            public void Staff(Color wood, Color orb)
            {
                Vector3 hand = Hand(1f);
                ArmR.Limb(hand + V(0f, -0.35f, -0.02f), hand + V(0f, 0.42f, 0.06f), 0.022f, 0.02f, wood, 0.01f).noise = 0.01f;
                ArmR.Sphere(hand + V(0f, 0.5f, 0.07f), 0.07f, orb, 0.01f);
                ArmR.Torus(hand + V(0f, 0.44f, 0.06f), 0.05f, 0.012f, Gold, 0.006f);
            }

            public void Cane(Color stick, Color top)
            {
                Vector3 hand = Hand(1f);
                ArmR.Limb(hand + V(0f, 0.04f, 0f), hand + V(0f, -0.42f, 0.04f), 0.016f, 0.014f, stick, 0.006f);
                ArmR.Sphere(hand + V(0f, 0.06f, 0f), 0.035f, top, 0.008f);
            }

            public void Tongs(Color color)
            {
                Vector3 hand = Hand(1f);
                ArmR.Limb(hand + V(-0.01f, 0f, 0f), hand + V(-0.03f, 0.28f, 0.12f), 0.01f, 0.008f, color, 0.004f);
                ArmR.Limb(hand + V(0.01f, 0f, 0f), hand + V(0.03f, 0.28f, 0.12f), 0.01f, 0.008f, color, 0.004f);
            }

            public void Rattle(Color handle, Color head)
            {
                Vector3 hand = Hand(1f);
                ArmR.Limb(hand + V(0f, -0.04f, 0f), hand + V(0f, 0.18f, 0.04f), 0.016f, 0.014f, handle, 0.006f);
                ArmR.Sphere(hand + V(0f, 0.24f, 0.05f), 0.07f, head, 0.01f);
                Paint(ArmR, ArmR.Torus(hand + V(0f, 0.24f, 0.05f), 0.07f, 0.012f, handle, 0.004f));
            }

            public void FishingRod(Color rod, Color line)
            {
                Vector3 hand = Hand(1f);
                ArmR.Limb(hand + V(0f, -0.12f, -0.02f), hand + V(0f, 0.55f, 0.35f), 0.018f, 0.007f, rod, 0.006f);
                ArmR.Cylinder(hand + V(0.03f, 0.02f, 0.02f), 0.035f, 0.02f, 0.005f, line, 0.005f, V(0f, 90f, 0f));
                ArmR.Limb(hand + V(0f, 0.55f, 0.35f), hand + V(0f, 0.2f, 0.42f), 0.004f, 0.004f, line, 0.002f);
                ArmR.Sphere(hand + V(0f, 0.19f, 0.42f), 0.025f, Red, 0.005f);
            }

            public void Hook(Color color)
            {
                Vector3 hand = Hand(1f);
                ArmR.Cylinder(hand + V(0f, 0.02f, 0f), dims.armRadius * 1.05f, 0.04f, 0.01f, Leather, 0.01f, V(90f, 0f, 0f));
                ArmR.Limb(hand + V(0f, -0.02f, 0f), hand + V(0f, -0.12f, 0.02f), 0.015f, 0.015f, color, 0.005f);
                ArmR.Torus(hand + V(0f, -0.16f, 0.06f), 0.045f, 0.013f, color, 0.005f, V(0f, 90f, 0f));
                Carve(ArmR, ArmR.Box(hand + V(0f, -0.13f, 0.11f), V(0.1f, 0.06f, 0.06f), 0f, color, 0.004f));
            }

            public void Pillow(Color color, Color stripe)
            {
                Vector3 hand = Hand(-1f);
                ArmL.Box(hand + V(0.02f, 0.06f, 0.1f), V(0.12f, 0.24f, 0.32f), 0.06f, color, 0.03f).noise = 0.006f;
                Paint(ArmL, ArmL.Box(hand + V(0.02f, 0.06f, 0.1f), V(0.2f, 0.04f, 0.4f), 0f, stripe, 0.004f));
            }

            public void ShirtFront(Color shirt, Color coat)
            {
                var front = new Sculpt();
                front.Shell(avatar.body, 0.024f, shirt, 0.01f);
                Clip(front.Box(V(0f, 0f, 1.1f), V(3f, 6f, 2f), 0f, shirt, 0.01f));
                Clip(front.Box(V(0f, (Neckline + 0.5f) * 0.5f, 0f), V(0.2f, Neckline - 0.5f, 3f), 0f, shirt, 0.02f));
                Body.Shell(front, 0f, shirt, 0.004f);
                Buttons(SpectatorSculptor.Shade(shirt, 0.8f), 3, 0.56f, Neckline - 0.08f);
            }

            public void Lapels(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float y0 = Neckline - 0.02f;
                    float y1 = 0.58f;
                    float x0 = s * 0.11f;
                    Body.Limb(V(x0, y0, FrontZ(y0, x0) + 0.02f), V(s * 0.04f, y1, FrontZ(y1, s * 0.04f) + 0.02f), 0.035f, 0.015f, color, 0.006f);
                }
            }

            public void BowTie(Color color)
            {
                float y = Neckline - 0.02f;
                Vector3 knot = V(0f, y, FrontZ(y) + 0.04f);
                Body.Mirror(s => Body.Ellipsoid(knot + V(s * 0.065f, 0f, 0f), V(0.065f, 0.045f, 0.025f), color, 0.015f, V(0f, 0f, s * 12f)));
                Body.Sphere(knot, 0.028f, SpectatorSculptor.Shade(color, 0.7f), 0.008f);
            }

            public void Tails(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float y = 0.42f;
                    float x = s * 0.12f;
                    float z = BackZ(y, x) - 0.02f;
                    Body.Box(V(x, y - 0.12f, z), V(0.16f, 0.36f, 0.03f), 0.015f, color, 0.02f, V(-12f, 0f, s * 6f));
                }
            }

            public void Vest(Color color)
            {
                var vest = new Sculpt();
                vest.Shell(avatar.body, 0.036f, color, 0.01f);
                Carve(vest, vest.Shell(avatar.body, 0.018f, color, 0.004f));
                Clip(vest.Box(V(0f, (Neckline + 0.46f) * 0.5f, 0f), V(3f, Neckline - 0.46f, 3f), 0f, color, 0.01f));
                Carve(vest, vest.Box(V(0f, (Neckline + 0.5f) * 0.5f + 0.05f, 1.1f), V(0.18f, Neckline - 0.4f, 2f), 0f, color, 0.02f));
                Body.Shell(vest, 0f, color, 0.004f);
            }

            public void Pockets(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        float y = 0.56f + k * 0.14f;
                        float x = s * 0.17f;
                        Body.Box(V(x, y, FrontZ(y, x) + 0.04f), V(0.11f, 0.09f, 0.03f), 0.01f, color, 0.006f);
                    }
                }
            }

            public void Sash(Color color, float y)
            {
                Band(y, 0.07f, 0.034f, color);
                float x = SideX(y) * 0.6f;
                float z = FrontZ(y, x);
                Body.Limb(V(x, y - 0.02f, z + 0.02f), V(x + 0.06f, y - 0.2f, z + 0.03f), 0.035f, 0.03f, color, 0.015f);
            }

            public void Stripes(Color color, float from, float to, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    float y = Mathf.Lerp(from, Mathf.Min(to, Neckline - 0.04f), i / (float)Mathf.Max(1, count - 1));
                    Paint(Body, Body.Box(V(0f, y, 0f), V(3f, 0.035f, 3f), 0f, color, 0.004f));
                }
            }

            public void Plaid(Color color, float waist)
            {
                for (float y = waist + 0.04f; y < Neckline - 0.02f; y += 0.09f)
                {
                    Paint(Body, Body.Box(V(0f, y, 0f), V(3f, 0.022f, 3f), 0f, color, 0.003f));
                }
                for (int i = 0; i < 16; i++)
                {
                    float a = i * Mathf.PI / 8f;
                    Paint(Body, Body.Box(V(Mathf.Cos(a) * 0.5f, (waist + Neckline) * 0.5f, Mathf.Sin(a) * 0.5f), V(0.022f, Neckline - waist, 0.6f), 0f, color, 0.003f, V(0f, -a * Mathf.Rad2Deg, 0f)));
                }
                Paint(Body, Body.Box(V(0f, waist - 0.5f, 0f), V(3f, 1f, 3f), 0f, Denim, 0.004f));
            }

            public void Stars(Color color, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    float a = i * 2.39996f;
                    float y = 0.3f + (i * 0.37f % 1f) * (Neckline - 0.36f);
                    float r = SideX(y) + 0.03f;
                    Paint(Body, Body.Sphere(V(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r), 0.03f, color, 0.006f));
                }
            }

            public void Dots(Color color, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    float a = i * 2.39996f;
                    float y = 0.18f + (i * 0.618f % 1f) * (Neckline - 0.22f);
                    float r = SideX(y) + 0.03f;
                    Paint(Body, Body.Sphere(V(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r), 0.028f, color, 0.006f));
                }
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Paint(Arm(s), Arm(s).Sphere(V(s * 0.03f, -dims.armLength * (0.2f + i * 0.25f), 0.08f), 0.025f, color, 0.005f));
                    }
                }
            }

            public void Flowers(Color a1, Color a2, float waist)
            {
                for (int i = 0; i < 12; i++)
                {
                    float a = i * 2.39996f;
                    float y = waist + 0.04f + (i * 0.618f % 1f) * (Neckline - waist - 0.08f);
                    float r = SideX(y) + 0.03f;
                    Vector3 c = V(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
                    Color petal = i % 2 == 0 ? a1 : a2;
                    for (int k = 0; k < 5; k++)
                    {
                        float b = k * Mathf.PI * 0.4f;
                        Paint(Body, Body.Sphere(c + V(Mathf.Cos(b) * 0.025f, Mathf.Sin(b) * 0.025f, 0f), 0.018f, petal, 0.004f));
                    }
                    Paint(Body, Body.Sphere(c, 0.012f, White, 0.003f));
                }
            }

            public void ShortsHem(float y)
            {
                Color skin = avatar.head.Color(fit.head + V(fit.headRadii.x * 0.95f, 0f, 0f));
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * (dims.footSpread + 0.01f);
                    Paint(Body, Body.Limb(V(x, 0.13f, 0.03f), V(x, y - 0.02f, 0f), dims.footSize * 1.6f, dims.footSize * 1.6f, skin, 0.006f));
                    Body.Torus(V(x, y, 0.01f), dims.footSize * 1.15f, 0.022f, bottomColor, 0.01f);
                }
            }

            public void Collar(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * 0.09f;
                    float y = Neckline - 0.03f;
                    Body.Ellipsoid(V(x, y, FrontZ(y, x) + 0.02f), V(0.08f, 0.03f, 0.05f), color, 0.01f, V(0f, 0f, s * -25f));
                }
            }

            public void ChestPanel(Color panel, Color b1, Color b2)
            {
                float y = 0.66f;
                float z = FrontZ(y) + 0.03f;
                Body.Box(V(0f, y, z), V(0.22f, 0.14f, 0.04f), 0.015f, panel, 0.008f);
                Body.Sphere(V(-0.05f, y + 0.02f, z + 0.025f), 0.022f, b1, 0.004f);
                Body.Sphere(V(0.05f, y + 0.02f, z + 0.025f), 0.022f, b2, 0.004f);
                Body.Box(V(0f, y - 0.035f, z + 0.025f), V(0.14f, 0.025f, 0.01f), 0.004f, White, 0.003f);
            }

            public void Backpack(Color color, Color trim)
            {
                float y = 0.68f;
                float z = BackZ(y) - 0.09f;
                Body.Box(V(0f, y, z), V(0.42f, 0.42f, 0.18f), 0.06f, color, 0.02f);
                Body.Mirror(s => Body.Cylinder(V(s * 0.1f, y - 0.24f, z), 0.045f, 0.04f, 0.01f, trim, 0.01f, V(90f, 0f, 0f)));
            }

            public void Briefs(Color color, float waist)
            {
                var briefs = new Sculpt();
                briefs.Shell(avatar.body, 0.034f, color, 0.01f);
                Clip(briefs.Box(V(0f, (waist + 0.2f) * 0.5f, 0f), V(3f, waist - 0.2f, 3f), 0f, color, 0.02f));
                Body.Shell(briefs, 0f, color, 0.004f);
            }

            public void Emblem(Color star, Color back)
            {
                float y = 0.7f;
                Vector3 c = V(0f, y, FrontZ(y) + 0.03f);
                Body.Cylinder(c, 0.11f, 0.012f, 0.006f, star, 0.006f);
                for (int i = 0; i < 5; i++)
                {
                    float a = i * Mathf.PI * 0.4f + Mathf.PI * 0.5f;
                    Body.Limb(c + V(0f, 0f, 0.015f), c + V(Mathf.Cos(a) * 0.08f, Mathf.Sin(a) * 0.08f, 0.015f), 0.028f, 0.006f, back, 0.006f);
                }
            }

            public void FurCollar(Color color)
            {
                float r = SideX(Neckline - 0.04f);
                for (int i = 0; i < 18; i++)
                {
                    float a = i * Mathf.PI / 9f;
                    Body.Sphere(V(Mathf.Cos(a) * r * 0.95f, Neckline - 0.04f, Mathf.Sin(a) * r * 0.95f), 0.07f, color, 0.04f).noise = 0.025f;
                }
            }

            public void Shield(Color face, Color rim, Color boss)
            {
                float y = 0.62f;
                float z = BackZ(y) - 0.06f;
                Body.Cylinder(V(0f, y, z), 0.3f, 0.025f, 0.01f, face, 0.01f);
                Body.Torus(V(0f, y, z), 0.3f, 0.025f, rim, 0.006f, V(90f, 0f, 0f));
                Body.Sphere(V(0f, y, z - 0.03f), 0.06f, boss, 0.01f);
                Paint(Body, Body.Box(V(0f, y, z - 0.03f), V(0.6f, 0.05f, 0.02f), 0f, rim, 0.004f));
                Paint(Body, Body.Box(V(0f, y, z - 0.03f), V(0.05f, 0.6f, 0.02f), 0f, rim, 0.004f));
            }

            public void Epaulettes(Color color)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 shoulder = avatar.shoulder;
                    shoulder.x *= s;
                    Body.Ellipsoid(shoulder + V(-s * 0.05f, 0.08f, 0f), V(0.1f, 0.025f, 0.08f), color, 0.01f);
                    for (int i = 0; i < 4; i++)
                    {
                        Body.Limb(shoulder + V(-s * 0.03f + s * 0.03f, 0.07f, -0.06f + i * 0.04f), shoulder + V(s * 0.05f, 0.0f, -0.06f + i * 0.04f), 0.008f, 0.008f, color, 0.003f);
                    }
                }
            }

            public void Medals(Color gold, Color r1, Color r2)
            {
                for (int i = 0; i < 3; i++)
                {
                    float x = -0.2f + i * 0.06f;
                    float y = 0.74f;
                    float z = FrontZ(y, x) + 0.03f;
                    Body.Box(V(x, y + 0.04f, z), V(0.04f, 0.05f, 0.012f), 0.004f, i % 2 == 0 ? r1 : r2, 0.003f);
                    Body.Cylinder(V(x, y - 0.02f, z + 0.005f), 0.022f, 0.006f, 0.002f, gold, 0.003f);
                }
            }

            public void Belly(Color color)
            {
                var belly = new Sculpt();
                belly.Shell(avatar.body, 0.034f, color, 0.01f);
                Clip(belly.Ellipsoid(V(0f, 0.6f, 0.6f), V(0.28f, 0.34f, 0.5f), color, 0.02f));
                Body.Shell(belly, 0f, color, 0.004f);
                for (int i = 0; i < 4; i++)
                {
                    float y = 0.42f + i * 0.09f;
                    Paint(Body, Body.Box(V(0f, y, FrontZ(y) + 0.03f), V(0.5f, 0.012f, 0.12f), 0f, SpectatorSculptor.Shade(color, 0.85f), 0.003f));
                }
            }

            public void DinoTail(Color color, Color spike)
            {
                float y = 0.36f;
                float z = BackZ(y);
                Vector3 a = V(0f, y, z + 0.08f);
                Vector3 b = V(0.04f, 0.2f, z - 0.32f);
                Vector3 c = V(0.12f, 0.12f, z - 0.6f);
                Body.Limb(a, b, 0.17f, 0.11f, color, 0.06f);
                Body.Limb(b, c, 0.11f, 0.03f, color, 0.05f);
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = Vector3.Lerp(a, c, 0.2f + i * 0.2f);
                    float r = Mathf.Lerp(0.15f, 0.04f, 0.2f + i * 0.2f);
                    Body.Limb(p + V(0f, r * 0.8f, 0f), p + V(0f, r * 0.8f + 0.07f, -0.03f), 0.035f, 0.006f, spike, 0.01f);
                }
            }

            public void Spikes(Color color)
            {
                for (int i = 0; i < 4; i++)
                {
                    float y = 0.5f + i * 0.13f;
                    if (y > Neckline)
                    {
                        break;
                    }
                    float z = BackZ(y) - 0.02f;
                    Body.Limb(V(0f, y, z + 0.04f), V(0f, y + 0.03f, z - 0.1f), 0.05f, 0.008f, color, 0.012f);
                }
            }

            public void RainbowTail()
            {
                Color[] rainbow = Rainbow();
                float y = 0.42f;
                float z = BackZ(y);
                for (int i = 0; i < rainbow.Length; i++)
                {
                    float x = (i - 2.5f) * 0.03f;
                    Body.Limb(V(x, y, z + 0.03f), V(x * 1.6f, 0.12f, z - 0.24f), 0.04f, 0.02f, rainbow[i], 0.02f);
                }
            }

            static Color[] Rainbow()
            {
                return new[] { new Color(1f, 0.3f, 0.35f), new Color(1f, 0.6f, 0.2f), new Color(1f, 0.9f, 0.3f), new Color(0.4f, 0.85f, 0.4f), new Color(0.35f, 0.6f, 1f), new Color(0.7f, 0.45f, 0.95f) };
            }

            public void Hooded(Color color, float inflate, float faceDepth, out Sculpt hood)
            {
                hood = new Sculpt();
                hood.Shell(avatar.head, inflate, color, 0.01f);
                Carve(hood, hood.Ellipsoid(HeadFront(fit.head.y - fit.headRadii.y * 0.05f) + V(0f, 0f, 0.05f), V(fit.headRadii.x * 0.78f, fit.headRadii.y * 0.72f, faceDepth), color, 0.03f));
                Carve(hood, hood.Box(V(0f, fit.head.y - fit.headRadii.y - 0.45f, 0f), V(3f, 0.9f, 3f), 0f, color, 0.02f));
            }

            public void Helmet(Color shell, Color stripe, Color visor)
            {
                Hooded(shell, 0.05f, fit.headRadii.z * 0.9f, out Sculpt hood);
                Head.Shell(hood, 0f, shell, 0.004f);
                Paint(Head, Head.Box(V(0f, fit.head.y + fit.headRadii.y, 0f), V(0.08f, 0.6f, 2f), 0f, stripe, 0.004f));
                Vector3 front = HeadFront(fit.head.y + fit.headRadii.y * 0.35f);
                Head.Ellipsoid(front + V(0f, 0.02f, 0.04f), V(fit.headRadii.x * 0.85f, 0.035f, 0.07f), visor, 0.01f, V(-20f, 0f, 0f));
            }

            public void SpaceHelmet(Color shell, Color trim)
            {
                Hooded(shell, 0.06f, fit.headRadii.z * 1.1f, out Sculpt hood);
                Head.Shell(hood, 0f, shell, 0.004f);
                Vector3 face = HeadFront(fit.head.y);
                Head.Torus(face + V(0f, 0f, 0.0f), fit.headRadii.x * 0.78f, 0.035f, trim, 0.01f, V(90f, 0f, 0f));
                Head.Limb(V(fit.headRadii.x * 0.7f, fit.head.y + fit.headRadii.y * 0.6f, 0f), V(fit.headRadii.x * 0.85f, fit.head.y + fit.headRadii.y * 1.3f, 0f), 0.012f, 0.008f, Silver, 0.004f);
                Head.Sphere(V(fit.headRadii.x * 0.86f, fit.head.y + fit.headRadii.y * 1.33f, 0f), 0.025f, Red, 0.004f);
            }

            public void NinjaHood(Color color, Color band)
            {
                var hood = new Sculpt();
                hood.Shell(avatar.head, 0.03f, color, 0.01f);
                Vector3 eye = avatar.eyePosition;
                Carve(hood, hood.Box(V(0f, eye.y, eye.z + 0.3f), V(fit.headRadii.x * 1.7f, 0.12f, 0.6f), 0.03f, color, 0.02f));
                Carve(hood, hood.Box(V(0f, fit.head.y - fit.headRadii.y - 0.45f, 0f), V(3f, 0.9f, 3f), 0f, color, 0.02f));
                Head.Shell(hood, 0f, color, 0.004f);
                float y = eye.y + 0.1f;
                Head.Torus(V(0f, y, fit.head.z), fit.headRadii.x * 1.05f, 0.025f, band, 0.008f);
                Head.Limb(V(0.04f, y, fit.head.z - fit.headRadii.z * 1.02f), V(0.16f, y - 0.16f, fit.head.z - fit.headRadii.z * 1.35f), 0.03f, 0.02f, band, 0.01f);
                Head.Limb(V(-0.04f, y, fit.head.z - fit.headRadii.z * 1.02f), V(-0.12f, y - 0.2f, fit.head.z - fit.headRadii.z * 1.3f), 0.03f, 0.02f, band, 0.01f);
            }

            public void Katana(Color handle, Color blade)
            {
                float y = 0.66f;
                float z = BackZ(y) - 0.05f;
                Body.Limb(V(-0.24f, y - 0.24f, z), V(0.2f, y + 0.26f, z), 0.025f, 0.02f, Black, 0.006f);
                Body.Limb(V(0.2f, y + 0.26f, z), V(0.32f, y + 0.4f, z), 0.022f, 0.02f, handle, 0.006f);
                Body.Cylinder(V(0.2f, y + 0.26f, z), 0.045f, 0.01f, 0.004f, Gold, 0.004f, V(0f, 0f, 45f));
            }

            public void DinoHood(Color color, Color teeth, Color spikes)
            {
                Hooded(color, 0.05f, fit.headRadii.z * 1.0f, out Sculpt hood);
                Head.Shell(hood, 0f, color, 0.004f);
                Vector3 front = HeadFront(fit.head.y + fit.headRadii.y * 0.6f);
                for (int i = -3; i <= 3; i++)
                {
                    Head.Limb(front + V(i * 0.05f, 0.02f, 0.02f), front + V(i * 0.052f, -0.035f, 0.035f), 0.018f, 0.004f, teeth, 0.004f);
                }
                Head.Mirror(s => Head.Sphere(V(s * fit.headRadii.x * 0.45f, fit.head.y + fit.headRadii.y * 1.02f, fit.head.z + fit.headRadii.z * 0.35f), 0.05f, White, 0.01f));
                Head.Mirror(s => Head.Sphere(V(s * fit.headRadii.x * 0.45f, fit.head.y + fit.headRadii.y * 1.04f, fit.head.z + fit.headRadii.z * 0.35f + 0.035f), 0.025f, Black, 0.004f));
                for (int i = 0; i < 3; i++)
                {
                    float y = fit.head.y + fit.headRadii.y * (1.0f - i * 0.25f);
                    float z = fit.head.z - fit.headRadii.z * (0.2f + i * 0.45f);
                    Head.Limb(V(0f, y, z), V(0f, y + 0.08f, z - 0.03f), 0.04f, 0.006f, spikes, 0.01f);
                }
            }

            public void Goggles(Color frame, Color glass)
            {
                Vector3 eye = avatar.eyePosition;
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 p = V(s * eye.x, eye.y, eye.z + 0.035f);
                    Head.Torus(p, 0.07f, 0.02f, frame, 0.006f, V(90f, 0f, 0f));
                }
            }

            public void Snorkel(Color color)
            {
                Vector3 eye = avatar.eyePosition;
                Vector3 side = V(fit.headRadii.x * 0.95f, eye.y - 0.05f, fit.head.z + 0.05f);
                Head.Limb(side, side + V(0.04f, 0.35f, -0.05f), 0.025f, 0.025f, color, 0.006f);
                Head.Limb(side, side + V(-0.1f, -0.12f, 0.12f), 0.025f, 0.02f, color, 0.006f);
            }

            public void HairBun(Color hair, Color bow)
            {
                Vector3 top = fit.head + V(0f, fit.headRadii.y * 1.02f, -0.04f);
                Head.Sphere(top + V(0f, 0.06f, 0f), 0.1f, hair, 0.04f).noise = 0.01f;
                Head.Mirror(s => Head.Ellipsoid(top + V(s * 0.09f, 0.0f, 0.06f), V(0.08f, 0.05f, 0.03f), bow, 0.012f, V(0f, 0f, s * 20f)));
                Head.Sphere(top + V(0f, 0.0f, 0.07f), 0.03f, bow, 0.008f);
            }

            public void Antennae(Color stalk, Color ball)
            {
                Vector3 top = fit.head + V(0f, fit.headRadii.y * 0.95f, 0.02f);
                Head.Mirror(s =>
                {
                    Vector3 tip = top + V(s * 0.16f, 0.24f, 0.05f);
                    Head.Limb(top + V(s * 0.08f, -0.02f, 0f), tip, 0.014f, 0.01f, stalk, 0.006f);
                    return Head.Sphere(tip, 0.045f, ball, 0.008f);
                });
            }

            public void EyeMask(Color color)
            {
                Vector3 eye = avatar.eyePosition;
                var mask = new Sculpt();
                mask.Shell(avatar.head, 0.018f, color, 0.01f);
                Clip(mask.Box(V(0f, eye.y + 0.01f, fit.head.z + 0.5f), V(fit.headRadii.x * 2.4f, 0.1f, 1f), 0.02f, color, 0.02f));
                Head.Shell(mask, 0f, color, 0.004f);
                Head.Torus(V(0f, eye.y + 0.01f, fit.head.z), fit.headRadii.x * 1.02f, 0.012f, color, 0.004f);
            }

            public void EyePatch(Color color)
            {
                Vector3 eye = avatar.eyePosition;
                Head.Cylinder(V(eye.x, eye.y, eye.z + 0.04f), 0.07f, 0.012f, 0.008f, color, 0.006f, V(-10f, 0f, 0f));
                Head.Torus(V(0f, eye.y + 0.03f, fit.head.z), fit.headRadii.x * 1.03f, 0.01f, color, 0.004f, V(0f, 0f, -18f));
            }

            public void Tiara(Color metal, Color gem)
            {
                Vector3 top = fit.head + V(0f, fit.headRadii.y * 0.82f, 0.04f);
                Head.Torus(top, fit.headRadii.x * 0.62f, 0.018f, metal, 0.006f, V(-14f, 0f, 0f));
                Vector3 front = top + V(0f, 0.05f, fit.headRadii.x * 0.6f);
                for (int i = -2; i <= 2; i++)
                {
                    Head.Limb(front + V(i * 0.05f, -0.04f, -Mathf.Abs(i) * 0.02f), front + V(i * 0.05f, 0.04f - Mathf.Abs(i) * 0.02f, -Mathf.Abs(i) * 0.02f), 0.012f, 0.004f, metal, 0.004f);
                }
                Head.Sphere(front + V(0f, -0.01f, 0.01f), 0.03f, gem, 0.005f);
            }

            public void Bonnet(Color color, Color frill)
            {
                Hooded(color, 0.04f, fit.headRadii.z * 1.15f, out Sculpt hood);
                Head.Shell(hood, 0f, color, 0.004f);
                Vector3 face = HeadFront(fit.head.y);
                Head.Torus(face + V(0f, 0f, -0.04f), fit.headRadii.x * 0.86f, 0.03f, frill, 0.012f, V(90f, 0f, 0f)).noise = 0.01f;
                Head.Mirror(s => Head.Limb(V(s * fit.headRadii.x * 0.7f, fit.head.y - fit.headRadii.y * 0.5f, fit.head.z + 0.05f), V(s * 0.04f, fit.head.y - fit.headRadii.y * 1.05f, fit.head.z + fit.headRadii.z * 0.6f), 0.016f, 0.014f, frill, 0.006f));
            }

            public void Pacifier(Color shield, Color ring)
            {
                Vector3 mouth = HeadFront(fit.head.y - fit.headRadii.y * 0.45f);
                Head.Ellipsoid(mouth + V(0f, 0f, 0.02f), V(0.07f, 0.045f, 0.015f), shield, 0.008f);
                Head.Torus(mouth + V(0f, -0.02f, 0.06f), 0.035f, 0.01f, ring, 0.004f);
            }

            public void Diaper(Color color, Color pin)
            {
                var diaper = new Sculpt();
                diaper.Shell(avatar.body, 0.04f, color, 0.012f);
                Clip(diaper.Box(V(0f, 0.36f, 0f), V(3f, 0.36f, 3f), 0f, color, 0.03f));
                Body.Shell(diaper, 0f, color, 0.004f);
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * 0.2f;
                    float y = 0.48f;
                    Body.Limb(V(x - 0.04f, y, FrontZ(y, x) + 0.05f), V(x + 0.04f, y, FrontZ(y, x) + 0.05f), 0.012f, 0.012f, pin, 0.004f);
                }
            }

            public void Bib(Color color, Color trim)
            {
                float y = Neckline - 0.12f;
                Vector3 c = V(0f, y, FrontZ(y) + 0.03f);
                Body.Ellipsoid(c, V(0.17f, 0.15f, 0.025f), color, 0.02f, V(-10f, 0f, 0f));
                Body.Torus(c, 0.16f, 0.012f, trim, 0.006f, V(80f, 0f, 0f));
                Body.Torus(V(0f, Neckline + 0.01f, 0.02f), SideX(Neckline) * 0.8f, 0.018f, trim, 0.008f);
            }

            public void Sunglasses(Color color)
            {
                Vector3 eye = avatar.eyePosition;
                for (int s = -1; s <= 1; s += 2)
                {
                    Head.Ellipsoid(V(s * eye.x, eye.y, eye.z + 0.05f), V(0.085f, 0.06f, 0.015f), color, 0.008f);
                }
                Head.Limb(V(-eye.x + 0.06f, eye.y + 0.01f, eye.z + 0.05f), V(eye.x - 0.06f, eye.y + 0.01f, eye.z + 0.05f), 0.01f, 0.01f, color, 0.004f);
                Head.Torus(V(0f, eye.y + 0.01f, fit.head.z), fit.headRadii.x * 1.02f, 0.01f, color, 0.004f);
            }

            public void Monocle(Color color)
            {
                Vector3 eye = avatar.eyePosition;
                Head.Torus(V(eye.x, eye.y, eye.z + 0.05f), 0.06f, 0.01f, color, 0.003f, V(90f, 0f, 0f));
                Head.Limb(V(eye.x + 0.05f, eye.y - 0.04f, eye.z + 0.04f), V(eye.x + 0.1f, eye.y - 0.3f, eye.z), 0.004f, 0.004f, color, 0.002f);
            }

            public void Camera(Color body, Color lens)
            {
                float y = 0.66f;
                Vector3 c = V(0f, y, FrontZ(y) + 0.08f);
                Body.Box(c, V(0.2f, 0.13f, 0.08f), 0.02f, body, 0.008f);
                Body.Cylinder(c + V(0f, 0f, 0.06f), 0.045f, 0.04f, 0.008f, lens, 0.006f);
                Body.Torus(V(0f, (y + Neckline) * 0.5f + 0.05f, 0.02f), SideX(Neckline) * 0.95f, 0.01f, body, 0.004f, V(30f, 0f, 0f));
            }

            public void Flame(Color inner, Color outer)
            {
                float y = 0.5f;
                Vector3 c = V(0f, y, FrontZ(y) + 0.07f);
                Body.Ellipsoid(c, V(0.08f, 0.1f, 0.012f), outer, 0.008f);
                Body.Ellipsoid(c + V(0f, -0.015f, 0.008f), V(0.045f, 0.06f, 0.01f), inner, 0.006f);
            }

            public void Horn(Color color)
            {
                Vector3 root = fit.head + V(0f, fit.headRadii.y * 0.8f, fit.headRadii.z * 0.55f);
                Head.Limb(root, root + V(0f, 0.28f, 0.1f), 0.05f, 0.006f, color, 0.01f);
                for (int i = 0; i < 4; i++)
                {
                    Paint(Head, Head.Torus(root + V(0f, 0.05f + i * 0.055f, 0.02f + i * 0.02f), 0.045f - i * 0.009f, 0.008f, White, 0.003f, V(-20f, 0f, 25f)));
                }
            }

            public void RainbowMane()
            {
                Color[] rainbow = Rainbow();
                for (int i = 0; i < 9; i++)
                {
                    float t = i / 8f;
                    float y = fit.head.y + fit.headRadii.y * Mathf.Lerp(1.0f, -0.6f, t);
                    float z = fit.head.z - fit.headRadii.z * Mathf.Lerp(0.75f, 1.25f, t);
                    Head.Sphere(V(0f, y, z), 0.085f - t * 0.02f, rainbow[i % rainbow.Length], 0.03f).noise = 0.012f;
                }
            }
        }
    }
}
