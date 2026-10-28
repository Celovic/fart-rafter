using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Rigidbody))]
    public class Buoyancy : MonoBehaviour
    {
        public RaftTuning tuning;
        public WaterSurface surface;
        public Transform[] floatPoints;

        Rigidbody body;
        Raft raft;

        public bool IsInWater { get; private set; }
        public float Submersion { get; private set; }
        public bool OnIce { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            raft = GetComponent<Raft>();
        }

        void FixedUpdate()
        {
            WaterSurface water = surface != null ? surface : WaterSurface.Instance;
            if (water == null || tuning == null || floatPoints == null || floatPoints.Length == 0)
            {
                return;
            }

            int pointCount = floatPoints.Length;
            float massShare = body.mass / pointCount;
            float weightShare = massShare * Physics.gravity.magnitude;
            int wetPoints = 0;
            float totalSubmersion = 0f;
            float ice = IceZone.Amount(body.position);
            OnIce = ice > 0.5f;

            foreach (Transform point in floatPoints)
            {
                Vector3 position = point.position;
                float surfaceHeight = water.GetHeight(position);
                if (ice > 0f)
                {
                    surfaceHeight = Mathf.Lerp(surfaceHeight, water.baseHeight + IceZone.FloatLevel, ice);
                }
                float depth = surfaceHeight - position.y;
                if (depth <= 0f)
                {
                    continue;
                }

                wetPoints++;
                float submersion = Mathf.Clamp01(depth / tuning.fullSubmergeDepth);
                totalSubmersion += submersion;

                float lift = weightShare * tuning.buoyancyStrength * submersion;
                float damping = -body.GetPointVelocity(position).y * tuning.verticalDamping * massShare;
                body.AddForceAtPosition(Vector3.up * (lift + damping), position, ForceMode.Force);
            }

            IsInWater = wetPoints > 0;
            Submersion = totalSubmersion / pointCount;

            if (!IsInWater)
            {
                return;
            }

            float wetRatio = wetPoints / (float)pointCount;
            Vector3 velocity = body.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            float dragScale = raft != null ? raft.DragScale : 1f;
            float turnScale = raft != null ? raft.TurnDragScale : 1f;
            body.AddForce(-horizontal * (tuning.waterLinearDrag * dragScale * wetRatio * Mathf.Lerp(1f, tuning.iceLinearDrag, ice)), ForceMode.Acceleration);
            body.AddTorque(-body.angularVelocity * (tuning.waterAngularDrag * turnScale * wetRatio * Mathf.Lerp(1f, tuning.iceAngularDrag, ice)), ForceMode.Acceleration);
            if (CourseProfile.Active)
            {
                Vector2 slope = CourseProfile.Gradient(body.position.x, body.position.z);
                body.AddForce(new Vector3(-slope.x, 0f, -slope.y) * (Physics.gravity.magnitude * tuning.slopePull * wetRatio), ForceMode.Acceleration);
            }
            float speed = horizontal.magnitude;
            if (ice > 0f && speed > tuning.iceMaxSpeed)
            {
                body.AddForce(-horizontal / speed * ((speed - tuning.iceMaxSpeed) * 1.5f * ice), ForceMode.Acceleration);
            }
        }
    }
}
