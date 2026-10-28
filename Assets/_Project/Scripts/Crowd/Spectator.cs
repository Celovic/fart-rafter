using UnityEngine;

namespace FartRafters
{
    public enum SpectatorAct
    {
        Cheer,
        Clap,
        Boo,
        Sign,
        Eat
    }

    public class Spectator : MonoBehaviour
    {
        public Transform root;
        public Transform head;
        public Transform armLeft;
        public Transform armRight;
        public Transform[] eyes;
        public Transform sign;
        public TextMesh signText;
        public SpectatorAct act;
        public float signBaseHeight = 1.1f;

        float excitement;
        float target;
        float seed;
        float blink;
        float nextBlink;
        float biteStart = -10f;
        bool fitted;
        int stride = 1;
        int phase;
        float nextCheck;

        public void Excite(float level)
        {
            target = level;
        }

        void Start()
        {
            seed = Random.value * 50f;
            phase = Random.Range(0, 6);
            nextBlink = Random.Range(1f, 4f);
        }

        void Update()
        {
            if (Time.time >= nextCheck)
            {
                nextCheck = Time.time + 0.5f;
                Camera view = Camera.main;
                float distance = view != null ? Vector3.Distance(view.transform.position, transform.position) : 0f;
                stride = distance > 45f ? 6 : distance > 30f ? 3 : 1;
            }
            if ((Time.frameCount + phase) % stride != 0)
            {
                return;
            }
            Animate(Time.time, Time.deltaTime * stride);
        }

        public void Pose(float time, float level)
        {
            excitement = target = level;
            Animate(time, 0f);
        }

        void Animate(float time, float dt)
        {
            float t = time + seed;
            excitement = Mathf.MoveTowards(excitement, target, dt * 1.5f);
            FitSign();
            FaceRaft(dt);

            float breathe = Mathf.Sin(t * 2.2f) * 0.02f;
            Vector3 position = Vector3.zero;
            Vector3 rootEuler = Vector3.zero;
            Vector3 headEuler = new Vector3(0f, Mathf.Sin(t * 0.6f) * 15f, Mathf.Sin(t * 0.9f) * 4f);
            float leftX = Mathf.Sin(t * 1.7f) * 4f, leftZ = -8f, rightX = -Mathf.Sin(t * 1.7f) * 4f, rightZ = 8f;
            float e = excitement;

            switch (act)
            {
                case SpectatorAct.Cheer:
                    position.y = Mathf.Abs(Mathf.Sin(t * 6.5f)) * 0.16f * e;
                    leftZ = Mathf.Lerp(leftZ, -150f + Mathf.Sin(t * 13f) * 20f, e);
                    rightZ = Mathf.Lerp(rightZ, 150f - Mathf.Sin(t * 13f + 1f) * 20f, e);
                    headEuler = Vector3.Lerp(headEuler, new Vector3(-18f, 0f, Mathf.Sin(t * 6.5f) * 8f), e);
                    break;
                case SpectatorAct.Clap:
                    float clap = Mathf.Abs(Mathf.Sin(t * 9f));
                    leftX = Mathf.Lerp(leftX, -72f, e);
                    rightX = Mathf.Lerp(rightX, -72f, e);
                    leftZ = Mathf.Lerp(leftZ, 12f + clap * 26f, e);
                    rightZ = Mathf.Lerp(rightZ, -12f - clap * 26f, e);
                    position.y = Mathf.Abs(Mathf.Sin(t * 4.5f)) * 0.04f * e;
                    headEuler = Vector3.Lerp(headEuler, new Vector3(-8f, Mathf.Sin(t * 2f) * 10f, 0f), e);
                    break;
                case SpectatorAct.Boo:
                    float shake = Mathf.Sin(t * 22f) * 12f;
                    leftX = Mathf.Lerp(leftX, -55f, e);
                    rightX = Mathf.Lerp(rightX, -55f, e);
                    leftZ = Mathf.Lerp(leftZ, 18f + shake, e);
                    rightZ = Mathf.Lerp(rightZ, -18f - shake, e);
                    rootEuler.x = 9f * e;
                    headEuler = Vector3.Lerp(headEuler, new Vector3(8f, Mathf.Sin(t * 7f) * 28f, 0f), e);
                    break;
                case SpectatorAct.Sign:
                    float pump = Mathf.Sin(t * 5f);
                    leftZ = Mathf.Lerp(-25f, -165f, Mathf.Max(0.25f, e));
                    rightZ = Mathf.Lerp(25f, 165f, Mathf.Max(0.25f, e));
                    leftX = rightX = pump * 8f * e;
                    position.y = Mathf.Max(0f, pump) * 0.08f * e;
                    if (sign != null)
                    {
                        sign.localPosition = new Vector3(0f, signBaseHeight + pump * 0.07f * e, 0.08f);
                        sign.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.5f) * 6f * e);
                    }
                    break;
                case SpectatorAct.Eat:
                    float cycle = (t * 0.55f) % 1f;
                    float bite = cycle < 0.35f ? Mathf.SmoothStep(0f, 1f, cycle / 0.35f) : cycle < 0.55f ? 1f : Mathf.SmoothStep(1f, 0f, (cycle - 0.55f) / 0.3f);
                    rightX = -30f - bite * 115f;
                    rightZ = 10f - bite * 28f;
                    leftX = -35f;
                    leftZ = 18f;
                    float chew = bite > 0.9f ? Mathf.Sin(t * 20f) * 4f : 0f;
                    headEuler = new Vector3(6f - bite * 10f + chew, Mathf.Sin(t * 0.6f) * 8f, 0f);
                    if (e > 0.5f && cycle > 0.6f)
                    {
                        rightZ = Mathf.Lerp(rightZ, 140f, e - 0.5f);
                    }
                    break;
            }

            if (root != null)
            {
                root.localPosition = position;
                root.localRotation = Quaternion.Euler(rootEuler);
                root.localScale = new Vector3(1f + breathe * 0.4f, 1f - breathe, 1f + breathe * 0.4f);
            }
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(headEuler);
            }
            if (armLeft != null)
            {
                armLeft.localRotation = Quaternion.Euler(leftX, 0f, leftZ);
            }
            if (armRight != null)
            {
                armRight.localRotation = Quaternion.Euler(rightX, 0f, rightZ);
            }
            Blink(dt);
        }

        void FaceRaft(float dt)
        {
            Raft raft = Raft.Current;
            if (raft == null || excitement < 0.05f)
            {
                return;
            }
            Vector3 toRaft = raft.transform.position - transform.position;
            toRaft.y = 0f;
            if (toRaft.sqrMagnitude < 0.01f)
            {
                return;
            }
            Quaternion look = Quaternion.LookRotation(toRaft.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 120f * dt * excitement);
        }

        void FitSign()
        {
            if (fitted || signText == null)
            {
                return;
            }
            var renderer = signText.GetComponent<MeshRenderer>();
            Vector3 size = renderer.localBounds.size;
            if (size.x <= 0.0001f)
            {
                return;
            }
            fitted = true;
            float scale = Mathf.Min(0.78f / size.x, 0.36f / Mathf.Max(0.0001f, size.y));
            signText.transform.localScale = Vector3.one * scale;
        }

        void Blink(float dt)
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
            float open = 1f - Mathf.Sin(blink * Mathf.PI);
            foreach (Transform eye in eyes)
            {
                if (eye != null)
                {
                    eye.localScale = new Vector3(1f, Mathf.Max(0.08f, open), 1f);
                }
            }
        }
    }
}
