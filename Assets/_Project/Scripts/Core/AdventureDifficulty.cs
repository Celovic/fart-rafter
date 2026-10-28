using UnityEngine;

namespace FartRafters
{
    [CreateAssetMenu(menuName = "FartRaft/Macera Zorluğu", fileName = "AdventureDifficulty")]
    public class AdventureDifficulty : ScriptableObject
    {
        public string id = "kolay";
        public string displayName = "Kolay";
        public string summary = "";

        [Header("Parkur")]
        [Min(100f)] public float length = 450f;
        [Min(8f)] public float riverWidth = 20f;
        [Min(30f)] public float checkpointSpacing = 75f;
        [Min(0)] public int checkpointCount;
        [Range(0f, 80f)] public float maxBend = 40f;
        [Range(0f, 1f)] public float narrowChance = 0.15f;
        [Range(0f, 1f)] public float zigzagChance = 0f;

        [Header("Engeller")]
        [Min(10f)] public float featureSpacing = 42f;
        [Range(0f, 1f)] public float crocodileWeight = 0.2f;
        [Range(0f, 1f)] public float rockWeight = 0.3f;
        [Range(0f, 1f)] public float currentWeight = 0.3f;
        [Range(0f, 1f)] public float thornWeight = 0.2f;
        [Range(0f, 1f)] public float millWeight = 0f;
        [Range(0f, 1f)] public float rockFieldWeight = 0f;
        [Range(0f, 1f)] public float driftWeight = 0f;
        [Range(0f, 1f)] public float geyserWeight = 0f;
        [Range(0f, 1f)] public float barrelWeight = 0f;
        [Range(0f, 1f)] public float rampWeight = 0f;
        [Range(0f, 1f)] public float specialWeight = 0f;
        [Min(0f)] public float millSpeed = 40f;
        [Range(0f, 1f)] public float ragingChance = 0f;
        [Min(0f)] public float crocodileSpeed = 1.2f;
        [Min(0f)] public float currentStrength = 2.5f;
        [Min(1)] public int maxRocksPerCluster = 2;
        [Min(0f)] public float damageMultiplier = 0.7f;
        [Range(0f, 1f)] public float fallChance = 0f;

        [Header("Yemekler")]
        [Min(10f)] public float pickupSpacing = 55f;

        [Header("Süre")]
        [Min(0.5f)] public float expectedSpeed = 3.2f;
    }
}
