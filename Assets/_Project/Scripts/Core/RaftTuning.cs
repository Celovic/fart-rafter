using UnityEngine;

namespace FartRafters
{
    [CreateAssetMenu(menuName = "FartRaft/Raft Tuning", fileName = "RaftTuning")]
    public class RaftTuning : ScriptableObject
    {
        [Header("Yüzdürme")]
        [Min(0.1f)] public float buoyancyStrength = 2.5f;
        [Min(0.05f)] public float fullSubmergeDepth = 0.5f;
        [Min(0f)] public float verticalDamping = 2.5f;

        [Header("Su sürtünmesi")]
        [Min(0f)] public float waterLinearDrag = 0.8f;
        [Min(0f)] public float waterAngularDrag = 1.5f;
        [Range(0f, 1f)] public float iceLinearDrag = 0.1f;
        [Range(0f, 1f)] public float iceAngularDrag = 0.3f;
        [Min(1f)] public float iceMaxSpeed = 8f;
        [Min(0f)] public float slopePull = 0.6f;

        [Header("Genel çarpanlar")]
        [Min(0f)] public float thrustMultiplier = 1f;
        [Min(0.1f)] public float gasMultiplier = 1f;
        [Range(0f, 1f)] public float crewCompensation = 0.6f;

        [Header("Yeniden doğma")]
        [Range(-1f, 1f)] public float flippedUpThreshold = 0.3f;
        [Min(0f)] public float flippedRespawnDelay = 1.5f;
        public float killHeight = -2.5f;
    }
}
