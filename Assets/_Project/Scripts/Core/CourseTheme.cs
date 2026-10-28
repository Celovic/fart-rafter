using UnityEngine;

namespace FartRafters
{
    [CreateAssetMenu(menuName = "FartRaft/Parkur Teması", fileName = "CourseTheme")]
    public class CourseTheme : ScriptableObject
    {
        public static CourseTheme Active { get; set; }

        [Header("Biyom")]
        public Biome biome;
        public string predatorName = "Timsah";
        [Min(0f)] public float predatorLength;
        public string caughtMessage = "Timsaha yakalandı!";
        public string[] extraCheers;
        public string[] extraJeers;
        public Material skybox;
        public Color sunColor = new Color(1f, 0.93f, 0.82f);
        [Min(0f)] public float sunIntensity = 1.6f;
        public Vector3 sunEuler = new Vector3(42f, -40f, 0f);
        public Color fogColor = new Color(0.68f, 0.78f, 0.86f);
        [Min(0f)] public float fogDensity = 0.0085f;
        public Color ambientTint = Color.white;
        public UnityEngine.Rendering.VolumeProfile postFx;
        [Min(0f)] public float waveAmplitude = 0.15f;
        [Min(0.1f)] public float waveLength = 8f;
        public float waveSpeed = 1.2f;
        public bool snow;
        public Material snowMaterial;
        [Range(-1, 1)] public int openSide;
        public Material iceMaterial;
        public Material whirlpoolMaterial;
        public Material rockMaterial;
        public Mesh[] rockMeshes;
        public Material thornMaterial;
        public Material millMaterial;
        public Mesh[] shoreProps;
        [Range(0f, 1f)] public float shorePropChance = 0.12f;
        public Mesh jumperMesh;
        public AudioClip[] crowdAnimal;
        [Range(0f, 1f)] public float animalShare = 0.25f;
        public AudioClip ambience;
        [Range(0f, 1f)] public float ambienceVolume = 0.35f;
        public bool elevated;
        public PhysicsMaterial rockPhysics;
        public Mesh[] mesaMeshes;
        public Mesh pillarMesh;
        public Mesh gatePoleMesh;
        public Mesh spinnerMesh;
        public Material cloudMaterial;
        public string gateLabel;
        public string startLabel;
        [Range(0f, 45f)] public float predatorLunge = 18f;
        public float predatorSpin;
        public Mesh driftMesh;
        [Min(0f)] public float driftLength;
        public Mesh eelMesh;
        [Min(0f)] public float eelLength;
        public Mesh barrelMesh;
        public Mesh icicleMesh;
        public Color rampColor = new Color(0.62f, 0.42f, 0.24f);
        public Color rampStripe = new Color(0.9f, 0.82f, 0.6f);
        public string driftName = "Tomruk";
        public string dropName = "Buz sarkıtı";
        public bool raftLight;

        [Header("Kovalamaca")]
        public Mesh chaseMesh;
        public string chaseName = "Dev Timsah";
        [Min(1f)] public float chaseSize = 9f;
        [Min(1)] public int chaseCount = 1;
        public float chaseHeight;

        [Header("Parkur")]
        public Material water;
        public Material wall;
        public Material obstacle;
        public Material checkpoint;
        public Material checkpointPassed;
        public Material finish;
        public Material buoy;
        public Material bubbles;
        public PhysicsMaterial physics;
        [Min(0f)] public float waterMargin = 20f;
        [Min(0.5f)] public float waterCellSize = 1.5f;

        [Header("Kıyı")]
        public Material grass;
        public Material rock;
        public Material trunk;
        public Material leaves;
        public Material leavesDark;
        public Material pine;
        public Material reed;
        public Material flower;
        public Material flowerAlt;
        public Material bank;
        [Min(10f)] public float landWidth = 45f;
        [Min(0f)] public float landHeight;
        [Min(0f)] public float bankSpread = 1f;
        [Min(0.5f)] public float decorationSpacing = 2.5f;

        [Header("Doğa modelleri")]
        public Mesh[] treeMeshes;
        public Mesh[] pineMeshes;
        public Mesh[] bushMeshes;
        public GameObject[] treeModels;
        public GameObject[] pineModels;
        public GameObject[] bushModels;
        public GameObject[] rockModels;
        public GameObject[] grassModels;
        public GameObject[] reedModels;
        public GameObject[] flowerModels;
        public GameObject[] plantModels;
        public GameObject[] pebbleModels;

        [Header("Sal")]
        public Material raftLog;
        public Material rope;
        public Material flag;
        public Mesh raftMesh;
        public GameObject[] raftProps;

        [Header("Engeller")]
        public Mesh crocodileMesh;
        public Mesh buoyMesh;
        public Mesh brambleMesh;
        public Mesh millWheelMesh;
        public Mesh frogMesh;
        public Mesh lilyPadMesh;
        public Mesh birdBodyMesh;
        public Mesh birdWingMesh;
        public Mesh[] pickupMeshes;
        public Material sculptMaterial;
        public Material foliageMaterial;
        public Material raftMaterial;
        public Material eyeMaterial;

        [Header("Seyirci")]
        public SpectatorLook[] spectators;
        public Mesh signMesh;
        public Mesh[] snackMeshes;
        public Mesh fishMesh;
        public Mesh duckMesh;
        public Mesh butterflyWingMesh;
        public Mesh turtleMesh;
        public AudioClip[] crowdBoo;
        public AudioClip[] crowdCheer;
        public AudioClip[] crowdClap;
        public AudioClip[] crowdChant;
        public AudioClip[] crowdChew;

        [Header("Karakter")]
        public Material accent;
        public Material eyeWhite;
        public Material blush;
        public Material[] playerMarkers;

        [Header("Efekt")]
        public Material puff;
        public Material splash;
        public Material confetti;
    }
}
