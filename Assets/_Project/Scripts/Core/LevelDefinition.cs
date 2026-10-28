using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FartRafters
{
    [Serializable]
    public struct CourseNode
    {
        public Vector2 position;
        [Min(2f)] public float width;

        public CourseNode(float x, float z, float width)
        {
            position = new Vector2(x, z);
            this.width = width;
        }
    }

    [Serializable]
    public struct CurrentSpec
    {
        public Vector3 position;
        public Vector3 size;
        public Vector3 direction;
        [Min(0f)] public float strength;
        public float yaw;
    }

    public enum ObstacleKind
    {
        Predator,
        Drift,
        Eel
    }

    [Serializable]
    public struct ObstacleSpec
    {
        public Vector3 position;
        public Vector3 size;
        public Vector3 travel;
        [Min(0f)] public float speed;
        public ObstacleKind kind;
    }

    [Serializable]
    public struct GeyserSpec
    {
        public Vector3 position;
        [Min(0.5f)] public float radius;
        [Min(1f)] public float period;
        public float phase;
    }

    [Serializable]
    public struct BarrelSpec
    {
        public Vector3 position;
    }

    [Serializable]
    public struct RampSpec
    {
        public Vector3 position;
        public float yaw;
        [Min(2f)] public float width;
    }

    [Serializable]
    public struct IcicleSpec
    {
        public Vector3 position;
        [Min(1f)] public float period;
        public float phase;
    }

    [Serializable]
    public struct SpecialSpec
    {
        public Vector3 position;
        public float yaw;
        [Min(1f)] public float reach;
    }

    [Serializable]
    public struct HazardSpec
    {
        public Vector3 position;
        [Min(0f)] public float radius;
    }

    public enum PickupKind
    {
        Beans,
        Pastirma,
        Onion,
        Chickpea,
        Pepper,
        Egg,
        Repair,
        Bubble,
        Magnet
    }

    [Serializable]
    public struct RockSpec
    {
        public Vector3 position;
        [Min(0.2f)] public float radius;
    }

    [Serializable]
    public struct MillSpec
    {
        public Vector3 position;
        public float yaw;
        [Min(1f)] public float radius;
        public float speed;
    }

    [Serializable]
    public struct ThornSpec
    {
        public int startNode;
        public int endNode;
        public float side;
    }

    [Serializable]
    public struct IceSpec
    {
        public Vector3 position;
        public float yaw;
        public Vector2 size;
    }

    [Serializable]
    public struct WhirlpoolSpec
    {
        public Vector3 position;
        [Min(1f)] public float radius;
        [Min(0f)] public float swirl;
        public float direction;
    }

    [Serializable]
    public struct PickupSpec
    {
        public Vector3 position;
        public PickupKind kind;
    }

    [CreateAssetMenu(menuName = "FartRaft/Seviye", fileName = "Level")]
    public class LevelDefinition : ScriptableObject
    {
        public string id = "level";
        public string displayName = "Seviye";

        [Header("Parkur")]
        public CourseNode[] nodes = Array.Empty<CourseNode>();
        [Min(0)] public int startNode = 1;
        public int[] checkpointNodes = Array.Empty<int>();

        [Header("Engeller")]
        public CurrentSpec[] currents = Array.Empty<CurrentSpec>();
        public ObstacleSpec[] obstacles = Array.Empty<ObstacleSpec>();
        public HazardSpec[] hazards = Array.Empty<HazardSpec>();
        public RockSpec[] rocks = Array.Empty<RockSpec>();
        public ThornSpec[] thorns = Array.Empty<ThornSpec>();
        public PickupSpec[] pickups = Array.Empty<PickupSpec>();
        public MillSpec[] mills = Array.Empty<MillSpec>();
        public IceSpec[] ice = Array.Empty<IceSpec>();
        public WhirlpoolSpec[] whirlpools = Array.Empty<WhirlpoolSpec>();
        public float[] heights = Array.Empty<float>();
        public GeyserSpec[] geysers = Array.Empty<GeyserSpec>();
        public BarrelSpec[] barrels = Array.Empty<BarrelSpec>();
        public RampSpec[] ramps = Array.Empty<RampSpec>();
        public IcicleSpec[] icicles = Array.Empty<IcicleSpec>();
        public SpecialSpec[] specials = Array.Empty<SpecialSpec>();
        [Range(0f, 1f)] public float fallChance;
        [Min(0f)] public float damageMultiplier = 1f;

        [Header("Puan")]
        [FormerlySerializedAs("threeStarTime")] [Min(1f)] public float parTime = 60f;

        public int NodeCount => nodes.Length;
        public bool IsThorn(int segment, float side)
        {
            foreach (ThornSpec thorn in thorns)
            {
                if (segment >= thorn.startNode && segment < thorn.endNode && Mathf.Approximately(Mathf.Sign(thorn.side), Mathf.Sign(side)))
                {
                    return true;
                }
            }
            return false;
        }

        public int FirstWaypointNode => startNode + 1;
        public int FinishNode => checkpointNodes.Length > 0 ? checkpointNodes[checkpointNodes.Length - 1] : nodes.Length - 1;

        public Vector3 Center(int index)
        {
            Vector2 p = nodes[Mathf.Clamp(index, 0, nodes.Length - 1)].position;
            return new Vector3(p.x, 0f, p.y);
        }

        public float Height(int index)
        {
            return heights != null && heights.Length == nodes.Length && nodes.Length > 0 ? heights[Mathf.Clamp(index, 0, nodes.Length - 1)] : 0f;
        }

        public Vector3 Surface(int index)
        {
            return Center(index) + Vector3.up * Height(index);
        }

        public float Width(int index)
        {
            return nodes[Mathf.Clamp(index, 0, nodes.Length - 1)].width;
        }

        public Vector3 Direction(int index)
        {
            Vector3 previous = Center(Mathf.Max(index - 1, 0));
            Vector3 next = Center(Mathf.Min(index + 1, nodes.Length - 1));
            return (next - previous).normalized;
        }

        public Vector3 Edge(int index, float side)
        {
            Vector3 perpendicular = Vector3.Cross(Vector3.up, Direction(index)).normalized;
            return Center(index) + perpendicular * (side * Width(index) * 0.5f);
        }
    }
}
