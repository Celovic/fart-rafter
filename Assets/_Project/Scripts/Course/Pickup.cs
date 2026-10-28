using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(SphereCollider))]
    public class Pickup : MonoBehaviour
    {
        public PickupKind kind;
        public int index;
        public static event System.Action<Pickup> Taken;
        public static readonly System.Collections.Generic.List<Pickup> Active = new System.Collections.Generic.List<Pickup>();
        public Transform visual;
        public ParticleSystem burst;
        [Min(0f)] public float bobHeight = 0.15f;
        [Min(0f)] public float spinSpeed = 70f;

        Vector3 basePosition;
        float seed;

        public static string NameOf(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Beans:
                    return "Kuru Fasulye";
                case PickupKind.Pastirma:
                    return "Pastırma";
                case PickupKind.Onion:
                    return "Soğan";
                case PickupKind.Chickpea:
                    return "Nohut";
                case PickupKind.Pepper:
                    return "Acı Biber";
                case PickupKind.Repair:
                    return "Tamir Kiti";
                case PickupKind.Bubble:
                    return "Kalkan";
                case PickupKind.Magnet:
                    return "Mıknatıs";
                default:
                    return "Haşlanmış Yumurta";
            }
        }

        public static Color ColorOf(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Beans:
                    return new Color(0.95f, 0.55f, 0.25f);
                case PickupKind.Pastirma:
                    return new Color(0.9f, 0.3f, 0.3f);
                case PickupKind.Onion:
                    return new Color(0.75f, 0.5f, 0.95f);
                case PickupKind.Chickpea:
                    return new Color(0.95f, 0.85f, 0.45f);
                case PickupKind.Pepper:
                    return new Color(1f, 0.25f, 0.15f);
                case PickupKind.Repair:
                    return new Color(0.45f, 0.95f, 0.5f);
                case PickupKind.Bubble:
                    return new Color(0.45f, 0.85f, 1f);
                case PickupKind.Magnet:
                    return new Color(1f, 0.45f, 0.7f);
                default:
                    return new Color(0.95f, 0.95f, 0.85f);
            }
        }

        WaterFloater floater;

        void OnEnable()
        {
            Active.Add(this);
        }

        void OnDisable()
        {
            Active.Remove(this);
        }

        public void Pull(Vector3 delta)
        {
            if (floater == null)
            {
                floater = GetComponent<WaterFloater>();
            }
            if (floater != null)
            {
                floater.Shift(delta);
            }
            else
            {
                transform.position += delta;
            }
        }

        void Start()
        {
            seed = Random.value * 10f;
            if (visual != null)
            {
                basePosition = visual.localPosition;
            }
        }

        void Update()
        {
            if (visual == null)
            {
                return;
            }
            visual.localPosition = basePosition + Vector3.up * (Mathf.Sin(Time.time * 2.2f + seed) * bobHeight);
            visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (OnlineGame.IsClient)
            {
                return;
            }
            Rigidbody body = other.attachedRigidbody;
            RaftBoosts boosts = body != null ? body.GetComponent<RaftBoosts>() : null;
            if (boosts == null)
            {
                return;
            }
            Collect(boosts);
            Taken?.Invoke(this);
        }

        public void Collect(RaftBoosts boosts)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            if (boosts != null)
            {
                boosts.Apply(kind);
            }
            GameAudio.Play(GameAudio.Sfx.Pickup, 0.9f, Random.Range(0.95f, 1.05f));
            if (burst != null)
            {
                burst.transform.SetParent(null, true);
                burst.Emit(40);
                Destroy(burst.gameObject, 3f);
            }
            gameObject.SetActive(false);
        }
    }
}
