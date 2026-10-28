using UnityEngine;

namespace FartRafters
{
    public enum AvatarKind
    {
        Hippo,
        Bear,
        Tombik,
        Ogre,
        Donkey,
        Sloth,
        Uncle,
        Dad
    }

    public enum VoiceLine
    {
        Hit,
        Fall,
        Board,
        Eliminated,
        Cheer,
        Pickup,
        Caught
    }

    [System.Serializable]
    public class VoiceSet
    {
        public AudioClip[] hit;
        public AudioClip[] fall;
        public AudioClip[] board;
        public AudioClip[] eliminated;
        public AudioClip[] cheer;
        public AudioClip[] pickup;
        public AudioClip[] caught;
        public AudioClip[] caughtPolar;
        public AudioClip[] caughtOcean;
        public AudioClip[] caughtFun;

        public AudioClip[] Get(VoiceLine line)
        {
            switch (line)
            {
                case VoiceLine.Hit:
                    return hit;
                case VoiceLine.Fall:
                    return fall;
                case VoiceLine.Board:
                    return board;
                case VoiceLine.Eliminated:
                    return eliminated;
                case VoiceLine.Cheer:
                    return cheer;
                case VoiceLine.Pickup:
                    return pickup;
                default:
                    CourseTheme theme = CourseTheme.Active;
                    if (theme != null && theme.biome == Biome.Polar && caughtPolar != null && caughtPolar.Length > 0)
                    {
                        return caughtPolar;
                    }
                    if (theme != null && theme.biome == Biome.Ocean && caughtOcean != null && caughtOcean.Length > 0)
                    {
                        return caughtOcean;
                    }
                    if (theme != null && (theme.biome == Biome.Fun || theme.biome == Biome.Lava || theme.biome == Biome.Night) && caughtFun != null && caughtFun.Length > 0)
                    {
                        return caughtFun;
                    }
                    return caught;
            }
        }
    }

    [System.Serializable]
    public class Costume
    {
        public string name;
        public Mesh head;
        public Mesh body;
        public Mesh armLeft;
        public Mesh armRight;
    }

    [CreateAssetMenu(menuName = "FartRaft/Avatar", fileName = "Avatar")]
    public class AvatarDefinition : ScriptableObject
    {
        [Header("Görünüş")]
        public AvatarKind kind;
        public string displayName = "Avatar";
        public Color accentColor = Color.white;

        [Header("Model")]
        public Material material;
        public Mesh bodyMesh;
        public Mesh headMesh;
        public Mesh armLeftMesh;
        public Mesh armRightMesh;
        public Mesh eyeMesh;
        public Vector3 headPivot = new Vector3(0f, 1f, 0.04f);
        public Vector3 shoulder = new Vector3(0.4f, 0.88f, 0f);
        public Vector3 eyePosition = new Vector3(0.12f, 0.3f, 0.24f);
        public Vector3 puffOffset = new Vector3(0f, 0.42f, -0.46f);

        [Header("Ses")]
        [Range(0.5f, 2f)] public float voicePitch = 1f;
        public AudioClip[] fartClips;
        [Range(0f, 2f)] public float fartVolume = 1f;
        public VoiceSet voice = new VoiceSet();

        [Header("Kostüm")]
        public Costume[] costumes = new Costume[0];

        public const int CostumeCode = 100;

        public int CostumeCount => 1 + (costumes != null ? costumes.Length : 0);

        public Costume GetCostume(int index)
        {
            return costumes != null && index > 0 && index <= costumes.Length ? costumes[index - 1] : null;
        }

        public string CostumeName(int index)
        {
            Costume costume = GetCostume(index);
            return costume != null ? costume.name : "Klasik";
        }

        [Header("Özellikler")]
        [Min(1f)] public float thrustForce = 450f;
        [Min(0.1f)] public float gasCapacitySeconds = 2.5f;
        [Min(0.01f)] public float refillPerSecond = 0.9f;
        [Min(0f)] public float refillDelay = 0.35f;
        [Range(0f, 1f)] public float recoverThreshold = 0.25f;
        [Min(1f)] public float turnSpeed = 720f;

        public bool HasFartClips => fartClips != null && fartClips.Length > 0;

        public AudioClip SignatureFart => HasFartClips ? fartClips[0] : null;

        public AudioClip RandomFart(AudioClip avoid = null)
        {
            if (!HasFartClips)
            {
                return null;
            }
            AudioClip clip = fartClips[Random.Range(0, fartClips.Length)];
            if (clip == avoid && fartClips.Length > 1)
            {
                clip = fartClips[(System.Array.IndexOf(fartClips, clip) + 1 + Random.Range(0, fartClips.Length - 1)) % fartClips.Length];
            }
            return clip;
        }
    }
}
