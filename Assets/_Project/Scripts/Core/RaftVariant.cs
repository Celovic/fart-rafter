using UnityEngine;

namespace FartRafters
{
    [System.Serializable]
    public class RaftVariant
    {
        public string name = "Klasik Sal";
        public string description = "";
        public Mesh mesh;
        public Material material;
        public bool props = true;
        [Min(0.1f)] public float thrust = 1f;
        [Min(0.1f)] public float drag = 1f;
        [Min(0.1f)] public float turnDrag = 1f;
        [Range(1, 5)] public int durability = 3;
        [Range(1, 5)] public int acceleration = 3;
        public PhysicsMaterial physics;

        public const int MaxDurability = 5;

        public float DamageScale
        {
            get
            {
                switch (Mathf.Clamp(durability, 1, MaxDurability))
                {
                    case 1: return 1.25f;
                    case 2: return 1.1f;
                    case 3: return 1f;
                    case 4: return 0.8f;
                    default: return 0.6f;
                }
            }
        }

        public string DurabilityMarks => Marks(durability);
        public string AccelerationMarks => Marks(acceleration);

        static string Marks(int value)
        {
            int level = Mathf.Clamp(value, 1, MaxDurability);
            return new string('●', level) + new string('○', MaxDurability - level);
        }
    }
}
