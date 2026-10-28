using UnityEngine;

namespace FartRafters.EditorTools
{
    public static class LevelPresets
    {
        public static readonly string[] Files = { "01_IlkGaz", "02_Zikzak", "03_AkintiYolu", "04_Kapilar", "05_BuyukTur" };
        const float WidthScale = 1.4f;
        const float CrocodileSpeedScale = 0.6f;

        public static void Apply(int index, LevelDefinition level)
        {
            level.rocks = new RockSpec[0];
            level.thorns = new ThornSpec[0];
            level.pickups = new PickupSpec[0];
            level.damageMultiplier = 0.8f;
            switch (index)
            {
                case 0:
                    FirstFart(level);
                    break;
                case 1:
                    Zigzag(level);
                    break;
                case 2:
                    CurrentRoad(level);
                    break;
                case 3:
                    Gates(level);
                    break;
                default:
                    GrandTour(level);
                    break;
            }
        }

        static void FirstFart(LevelDefinition level)
        {
            level.id = "ilk_gaz";
            level.displayName = "İlk Gaz";
            level.nodes = new[]
            {
                Node(0f, -6f, 16f), Node(0f, 0f, 16f), Node(0f, 30f, 16f), Node(6f, 55f, 16f), Node(14f, 80f, 16f),
                Node(14f, 105f, 16f), Node(6f, 130f, 16f), Node(0f, 150f, 16f), Node(0f, 157f, 16f)
            };
            level.startNode = 1;
            level.checkpointNodes = new[] { 1, 4, 7 };
            level.currents = new CurrentSpec[0];
            level.obstacles = new ObstacleSpec[0];
            level.hazards = new[] { Hazard(10f, 68f, 10f), Hazard(10f, 118f, 10f) };
            level.parTime = 45f;
        }

        static void Zigzag(LevelDefinition level)
        {
            level.id = "zikzak";
            level.displayName = "Zikzak";
            level.nodes = new[]
            {
                Node(0f, -6f, 12f), Node(0f, 0f, 12f), Node(0f, 25f, 12f), Node(10f, 45f, 12f), Node(10f, 60f, 12f),
                Node(-2f, 80f, 12f), Node(-2f, 95f, 12f), Node(10f, 115f, 12f), Node(10f, 130f, 12f), Node(0f, 150f, 12f),
                Node(0f, 170f, 12f), Node(0f, 177f, 12f)
            };
            level.startNode = 1;
            level.checkpointNodes = new[] { 1, 4, 7, 10 };
            level.currents = new[] { Current(-2f, 87f, 11f, 14f, new Vector3(1f, 0f, 0f), 2.5f) };
            level.obstacles = new ObstacleSpec[0];
            level.hazards = new[]
            {
                Hazard(5f, 35f, 10f), Hazard(4f, 70f, 10f), Hazard(-2f, 87f, 9f), Hazard(4f, 105f, 10f), Hazard(5f, 140f, 10f)
            };
            level.parTime = 65f;
        }

        static void CurrentRoad(LevelDefinition level)
        {
            level.id = "akinti_yolu";
            level.displayName = "Akıntı Yolu";
            level.nodes = new[]
            {
                Node(0f, -6f, 14f), Node(0f, 0f, 14f), Node(0f, 30f, 14f), Node(0f, 38f, 6f), Node(0f, 56f, 6f),
                Node(0f, 64f, 12f), Node(14f, 88f, 12f), Node(14f, 100f, 14f), Node(14f, 126f, 14f), Node(0f, 148f, 12f),
                Node(0f, 160f, 14f), Node(0f, 185f, 14f), Node(0f, 192f, 14f)
            };
            level.startNode = 1;
            level.checkpointNodes = new[] { 1, 5, 7, 9, 11 };
            level.currents = new[] { Current(14f, 113f, 13f, 26f, new Vector3(-1f, 0f, 0.15f), 3.5f) };
            level.obstacles = new[] { Obstacle(0f, 160f, 4f, 3f) };
            level.hazards = new[]
            {
                Hazard(0f, 47f, 11f), Hazard(7f, 76f, 12f), Hazard(14f, 113f, 14f), Hazard(7f, 137f, 12f), Hazard(0f, 160f, 8f)
            };
            level.parTime = 65f;
        }

        static void Gates(LevelDefinition level)
        {
            level.id = "kapilar";
            level.displayName = "Kapılar";
            level.nodes = new[]
            {
                Node(0f, -6f, 14f), Node(0f, 0f, 14f), Node(0f, 35f, 14f), Node(8f, 60f, 14f), Node(8f, 95f, 14f),
                Node(0f, 120f, 14f), Node(0f, 155f, 14f), Node(0f, 162f, 14f)
            };
            level.startNode = 1;
            level.checkpointNodes = new[] { 1, 3, 5, 6 };
            level.currents = new CurrentSpec[0];
            level.obstacles = new[] { Obstacle(0f, 25f, 4f, 3f), Obstacle(8f, 78f, 4f, 3.5f), Obstacle(0f, 140f, 4f, 4f) };
            level.hazards = new[] { Hazard(0f, 25f, 8f), Hazard(8f, 78f, 8f), Hazard(0f, 140f, 8f) };
            level.parTime = 60f;
        }

        static void GrandTour(LevelDefinition level)
        {
            level.id = "buyuk_tur";
            level.displayName = "Büyük Tur";
            level.nodes = new[]
            {
                Node(0f, -6f, 14f), Node(0f, 0f, 14f), Node(0f, 25f, 14f), Node(0f, 32f, 7f), Node(0f, 50f, 7f),
                Node(0f, 58f, 13f), Node(12f, 80f, 13f), Node(12f, 100f, 13f), Node(12f, 125f, 13f), Node(0f, 147f, 12f),
                Node(0f, 165f, 12f), Node(-12f, 187f, 12f), Node(-12f, 210f, 12f), Node(-12f, 218f, 7f), Node(-12f, 236f, 7f),
                Node(-12f, 244f, 12f), Node(0f, 266f, 12f), Node(0f, 290f, 12f), Node(0f, 315f, 13f), Node(0f, 322f, 7f),
                Node(0f, 340f, 7f), Node(0f, 348f, 13f), Node(-12f, 370f, 13f), Node(-12f, 390f, 13f), Node(-12f, 415f, 13f),
                Node(0f, 437f, 12f), Node(0f, 455f, 12f), Node(12f, 477f, 12f), Node(12f, 500f, 12f), Node(12f, 508f, 7f),
                Node(12f, 526f, 7f), Node(12f, 534f, 12f), Node(0f, 556f, 12f), Node(0f, 585f, 12f), Node(0f, 592f, 12f)
            };
            level.startNode = 1;
            level.checkpointNodes = new[] { 1, 5, 8, 12, 15, 18, 21, 24, 28, 31, 33 };
            level.currents = new[]
            {
                Current(12f, 112f, 12f, 24f, new Vector3(-1f, 0f, 0.15f), 3.5f),
                Current(-12f, 198f, 11f, 20f, new Vector3(1f, 0f, 0.1f), 3f),
                Current(-12f, 402f, 12f, 22f, new Vector3(1f, 0f, 0.15f), 3.5f),
                Current(0f, 446f, 11f, 16f, new Vector3(0f, 0f, 1f), 3f),
                Current(12f, 488f, 11f, 18f, new Vector3(0f, 0f, -1f), 2.4f)
            };
            level.obstacles = new[]
            {
                Obstacle(0f, 157f, 3.5f, 3f), Obstacle(0f, 280f, 3.5f, 3.5f), Obstacle(-12f, 380f, 4f, 3f), Obstacle(0f, 566f, 3.5f, 3.5f)
            };
            level.hazards = new[]
            {
                Hazard(0f, 41f, 11f), Hazard(6f, 69f, 12f), Hazard(12f, 112f, 14f), Hazard(6f, 136f, 12f), Hazard(0f, 157f, 8f),
                Hazard(-6f, 176f, 12f), Hazard(-12f, 198f, 12f), Hazard(-12f, 227f, 11f), Hazard(-6f, 255f, 12f), Hazard(0f, 280f, 8f),
                Hazard(0f, 331f, 11f), Hazard(-6f, 359f, 12f), Hazard(-12f, 380f, 8f), Hazard(-12f, 402f, 14f), Hazard(-6f, 426f, 12f),
                Hazard(12f, 488f, 10f), Hazard(12f, 517f, 11f), Hazard(6f, 545f, 12f), Hazard(0f, 566f, 8f)
            };
            level.rocks = new[] { Rock(5f, 300f, 1.2f), Rock(-6f, 306f, 1f), Rock(6f, 462f, 1.2f), Rock(-3f, 470f, 1f) };
            level.thorns = new[] { Thorn(22, 24, 1f), Thorn(27, 28, -1f) };
            level.pickups = new[]
            {
                Pickup(4f, 112f, PickupKind.Beans), Pickup(-14f, 205f, PickupKind.Pastirma), Pickup(-3f, 296f, PickupKind.Onion),
                Pickup(-14f, 395f, PickupKind.Beans), Pickup(15f, 492f, PickupKind.Pastirma), Pickup(3f, 575f, PickupKind.Beans)
            };
            level.parTime = 240f;
        }

        static CourseNode Node(float x, float z, float width)
        {
            return new CourseNode(x, z, width * WidthScale);
        }

        static RockSpec Rock(float x, float z, float radius)
        {
            return new RockSpec { position = new Vector3(x, 0f, z), radius = radius };
        }

        static ThornSpec Thorn(int start, int end, float side)
        {
            return new ThornSpec { startNode = start, endNode = end, side = side };
        }

        static PickupSpec Pickup(float x, float z, PickupKind kind)
        {
            return new PickupSpec { position = new Vector3(x, 0.35f, z), kind = kind };
        }

        static HazardSpec Hazard(float x, float z, float radius)
        {
            return new HazardSpec { position = new Vector3(x, 0f, z), radius = radius };
        }

        static CurrentSpec Current(float x, float z, float width, float length, Vector3 direction, float strength)
        {
            return new CurrentSpec
            {
                position = new Vector3(x, 0.5f, z),
                size = new Vector3(width * WidthScale, 4f, length),
                direction = direction,
                strength = strength
            };
        }

        static ObstacleSpec Obstacle(float x, float z, float reach, float speed)
        {
            return new ObstacleSpec
            {
                position = new Vector3(x, 0.4f, z),
                size = new Vector3(5f, 1.5f, 1.2f),
                travel = new Vector3(reach * WidthScale, 0f, 0f),
                speed = speed * CrocodileSpeedScale
            };
        }
    }
}
