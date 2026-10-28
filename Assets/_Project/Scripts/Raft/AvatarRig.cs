using UnityEngine;

namespace FartRafters
{
    public class AvatarRig : MonoBehaviour
    {
        public Transform root;
        public Transform head;
        public Transform armLeft;
        public Transform armRight;
        public Transform[] eyes;
        public Rider rider;
        public ParticleSystem puff;
        public bool demoFiring;
        public AvatarKind kind;
        [Min(0f)] public float puffRate = 45f;

        float fire;
        float tired;
        float swim;
        float blink;
        float nextBlink;
        float seed;
        bool wasDemoFiring;
        bool celebrating;
        float celebrateStart;
        int emote = -1;
        float emoteStart;
        float emoteUntil;
        public const float EmoteDuration = 2.6f;

        public bool Celebrating => celebrating;

        void Start()
        {
            seed = Random.value * 100f;
            nextBlink = Random.Range(1f, 4f);
        }

        public void Celebrate()
        {
            if (celebrating)
            {
                return;
            }
            celebrating = true;
            celebrateStart = Time.time + Random.Range(0f, 0.25f);
        }

        public void StopCelebrating()
        {
            celebrating = false;
        }

        public void PlayEmote(int move)
        {
            emote = move;
            emoteStart = Time.time;
            emoteUntil = Time.time + EmoteDuration;
        }

        void Update()
        {
            bool firing = rider != null ? rider.IsFiring : demoFiring;
            bool exhausted = rider != null && rider.Gas.Exhausted;
            bool swimming = rider != null && rider.InWater;
            float dt = Time.deltaTime;
            float t = Time.time + seed;

            fire = Mathf.MoveTowards(fire, firing ? 1f : 0f, dt * 8f);
            tired = Mathf.MoveTowards(tired, exhausted ? 1f : 0f, dt * 3f);
            swim = Mathf.MoveTowards(swim, swimming ? 1f : 0f, dt * 5f);

            if (emote >= 0 && Time.time < emoteUntil && swim < 0.01f)
            {
                Emote(Time.time - emoteStart);
            }
            else if (celebrating && Time.time >= celebrateStart)
            {
                Dance(Time.time - celebrateStart);
            }
            else if (swim > 0.01f)
            {
                Swim(t);
            }
            else
            {
                Idle(t);
            }

            UpdateEyes(dt);
            UpdateDemoPuff();
        }

        void Idle(float t)
        {
            float breathe = Mathf.Sin(t * 2.4f) * 0.025f;
            float wiggle = Mathf.Sin(t * 26f) * 5f * fire;
            float shake = Mathf.Sin(t * 41f) * 0.02f * fire;
            float hop = Mathf.Abs(Mathf.Sin(t * 3f)) * 0.02f * (1f - fire);

            if (root != null)
            {
                root.localScale = new Vector3(1f + breathe * 0.5f + fire * 0.06f, 1f - breathe - fire * 0.07f - tired * 0.06f, 1f + breathe * 0.5f + fire * 0.04f);
                root.localRotation = Quaternion.Euler(fire * 14f + tired * 6f, 0f, wiggle);
                root.localPosition = new Vector3(shake, hop, 0f);
            }
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(-fire * 10f + tired * 18f, Mathf.Sin(t * 0.7f) * 8f * (1f - fire), Mathf.Sin(t * 1.1f) * 4f);
                head.localScale = new Vector3(1f + fire * 0.08f, 1f + fire * 0.03f, 1f + fire * 0.05f);
            }
            float swing = Mathf.Sin(t * 2f) * 6f * (1f - fire);
            SetArms(swing - fire * 35f, -10f - fire * 25f + tired * 8f, -swing - fire * 35f, 10f + fire * 25f - tired * 8f);
        }

        void Swim(float t)
        {
            float stroke = t * 6f;
            if (root != null)
            {
                root.localScale = Vector3.one;
                root.localRotation = Quaternion.Euler(70f * swim, 0f, Mathf.Sin(stroke) * 8f);
                root.localPosition = new Vector3(0f, 0.15f * swim, -0.3f * swim);
            }
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(-55f * swim, Mathf.Sin(stroke * 0.5f) * 10f, 0f);
                head.localScale = Vector3.one;
            }
            SetArms(-90f + Mathf.Sin(stroke) * 90f, -20f, -90f - Mathf.Sin(stroke) * 90f, 20f);
        }

        void Dance(float time)
        {
            Transform target = Camera.main != null ? Camera.main.transform : null;
            if (root != null)
            {
                root.localScale = Vector3.one;
                Quaternion face = Quaternion.identity;
                if (target != null)
                {
                    Vector3 toCamera = Vector3.ProjectOnPlane(target.position - root.position, Vector3.up);
                    if (toCamera.sqrMagnitude > 0.01f)
                    {
                        Quaternion world = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
                        face = Quaternion.Inverse(root.parent != null ? root.parent.rotation : Quaternion.identity) * world;
                    }
                }
                root.localRotation = Quaternion.Slerp(Quaternion.identity, face, Mathf.Clamp01(time * 3f));
            }

            float beat = time * Mathf.PI * 2f;
            float hop = 0f;
            Vector3 headEuler = Vector3.zero;
            float leftX = 0f, leftZ = -10f, rightX = 0f, rightZ = 10f;
            Vector3 scale = Vector3.one;

            switch (kind)
            {
                case AvatarKind.Hippo:
                    hop = Mathf.Abs(Mathf.Sin(beat * 1.2f)) * 0.12f;
                    float jiggle = Mathf.Sin(beat * 2.4f) * 0.08f;
                    scale = new Vector3(1f + jiggle, 1f - jiggle, 1f + jiggle);
                    leftZ = -60f + Mathf.Sin(beat * 1.2f) * 30f;
                    rightZ = 60f - Mathf.Sin(beat * 1.2f) * 30f;
                    headEuler = new Vector3(-15f, Mathf.Sin(beat * 0.6f) * 20f, 0f);
                    break;
                case AvatarKind.Bear:
                    bool leftHit = Mathf.Sin(beat * 1.5f) > 0f;
                    leftX = leftHit ? -80f : -30f;
                    rightX = leftHit ? -30f : -80f;
                    leftZ = 25f;
                    rightZ = -25f;
                    headEuler = new Vector3(-25f, 0f, Mathf.Sin(beat * 0.75f) * 8f);
                    hop = Mathf.Abs(Mathf.Sin(beat * 0.75f)) * 0.05f;
                    break;
                case AvatarKind.Tombik:
                    hop = Mathf.Abs(Mathf.Sin(beat * 1.4f)) * 0.35f;
                    leftZ = -160f + Mathf.Sin(beat * 2.8f) * 15f;
                    rightZ = 160f - Mathf.Sin(beat * 2.8f) * 15f;
                    headEuler = new Vector3(-20f, 0f, Mathf.Sin(beat * 1.4f) * 10f);
                    break;
                case AvatarKind.Ogre:
                    leftZ = -110f;
                    rightZ = 110f;
                    leftX = -20f;
                    rightX = -20f;
                    headEuler = new Vector3(-35f + Mathf.Sin(beat * 3f) * 4f, 0f, 0f);
                    hop = Mathf.Abs(Mathf.Sin(beat * 0.8f)) * 0.06f;
                    scale = Vector3.one * (1f + Mathf.Abs(Mathf.Sin(beat * 0.8f)) * 0.05f);
                    break;
                case AvatarKind.Donkey:
                    float kick = Mathf.Max(0f, Mathf.Sin(beat * 1.3f));
                    if (root != null)
                    {
                        root.localRotation *= Quaternion.Euler(kick * 25f, 0f, 0f);
                    }
                    headEuler = new Vector3(Mathf.Sin(beat * 2.6f) * 20f, 0f, 0f);
                    leftX = -40f;
                    rightX = -40f;
                    hop = kick * 0.1f;
                    break;
                case AvatarKind.Sloth:
                    rightZ = 150f;
                    rightX = Mathf.Sin(beat * 0.35f) * 25f;
                    leftZ = -15f;
                    headEuler = new Vector3(-10f, 0f, Mathf.Sin(beat * 0.25f) * 15f);
                    if (root != null)
                    {
                        root.localRotation *= Quaternion.Euler(0f, 0f, Mathf.Sin(beat * 0.25f) * 8f);
                    }
                    break;
                case AvatarKind.Uncle:
                    rightX = -95f + Mathf.Sin(beat * 2f) * 10f;
                    rightZ = -20f;
                    leftZ = -35f;
                    leftX = 20f;
                    headEuler = new Vector3(Mathf.Sin(beat) * 12f, Mathf.Sin(beat * 0.5f) * 15f, 0f);
                    break;
                default:
                    float pump = Mathf.Sin(beat * 1.6f);
                    rightZ = 140f + pump * 25f;
                    leftZ = -20f;
                    hop = Mathf.Max(0f, pump) * 0.12f;
                    headEuler = new Vector3(-15f, 0f, pump * 6f);
                    break;
            }

            if (root != null)
            {
                root.localPosition = new Vector3(0f, hop, 0f);
                root.localScale = scale;
            }
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(headEuler);
                head.localScale = Vector3.one;
            }
            SetArms(leftX, leftZ, rightX, rightZ);
        }

        void Emote(float time)
        {
            if (emote == 0)
            {
                Dance(time);
                return;
            }
            float beat = time * Mathf.PI * 2f;
            float hop = 0f;
            Vector3 headEuler = Vector3.zero;
            Vector3 scale = Vector3.one;
            float leftX = 0f, leftZ = -10f, rightX = 0f, rightZ = 10f;
            switch (emote)
            {
                case 1:
                    rightZ = 150f;
                    rightX = Mathf.Sin(beat * 2f) * 30f;
                    headEuler = new Vector3(-8f, 0f, Mathf.Sin(beat) * 10f);
                    break;
                case 2:
                    hop = Mathf.Abs(Mathf.Sin(beat * 1.4f)) * 0.45f;
                    leftZ = -150f;
                    rightZ = 150f;
                    headEuler = new Vector3(-20f, 0f, 0f);
                    float squash = Mathf.Max(0f, 0.15f - hop) * 0.6f;
                    scale = new Vector3(1f + squash, 1f - squash, 1f + squash);
                    break;
                case 3:
                    float clap = Mathf.Abs(Mathf.Sin(beat * 2f));
                    leftX = -80f;
                    rightX = -80f;
                    leftZ = -5f - clap * 30f;
                    rightZ = 5f + clap * 30f;
                    headEuler = new Vector3(-10f + Mathf.Sin(beat * 2f) * 6f, 0f, 0f);
                    break;
                default:
                    float pump = Mathf.Abs(Mathf.Sin(beat * 1.2f));
                    leftZ = -95f;
                    rightZ = 95f;
                    leftX = -50f - pump * 40f;
                    rightX = -50f - pump * 40f;
                    scale = Vector3.one * (1f + pump * 0.08f);
                    headEuler = new Vector3(-20f, 0f, 0f);
                    break;
            }
            if (root != null)
            {
                root.localRotation = Quaternion.identity;
                root.localPosition = new Vector3(0f, hop, 0f);
                root.localScale = scale;
            }
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(headEuler);
                head.localScale = Vector3.one;
            }
            SetArms(leftX, leftZ, rightX, rightZ);
        }

        void SetArms(float leftX, float leftZ, float rightX, float rightZ)
        {
            if (armLeft != null)
            {
                armLeft.localRotation = Quaternion.Euler(leftX, 0f, leftZ);
            }
            if (armRight != null)
            {
                armRight.localRotation = Quaternion.Euler(rightX, 0f, rightZ);
            }
        }

        void UpdateEyes(float dt)
        {
            if (eyes == null)
            {
                return;
            }

            nextBlink -= dt;
            if (nextBlink <= 0f)
            {
                blink = 1f;
                nextBlink = Random.Range(2f, 5f);
            }
            blink = Mathf.MoveTowards(blink, 0f, dt * 8f);

            float squint = celebrating ? 0.25f : Mathf.Max(fire, tired * 0.6f);
            float open = Mathf.Lerp(1f, 0.35f, squint) * (1f - Mathf.Sin(blink * Mathf.PI));
            foreach (Transform eye in eyes)
            {
                if (eye != null)
                {
                    eye.localScale = new Vector3(1f, Mathf.Max(0.08f, open), 1f);
                }
            }
        }

        void UpdateDemoPuff()
        {
            if (rider != null || puff == null)
            {
                return;
            }
            if (demoFiring && !wasDemoFiring)
            {
                puff.Emit(8);
            }
            wasDemoFiring = demoFiring;
            ParticleSystem.EmissionModule emission = puff.emission;
            emission.rateOverTime = demoFiring ? puffRate : 0f;
        }
    }
}
