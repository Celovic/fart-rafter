using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FartRafters
{
    public class CourseManager : MonoBehaviour
    {
        public Raft raft;
        public Checkpoint[] checkpoints;
        public Transform[] waypoints;
        [Min(0f)] public float spawnHeight = 0.6f;
        [Min(0f)] public float waypointReachRadius = 3f;
        [Min(0f)] public float startSpeedThreshold = 0.5f;

        float flippedTime;

        public event Action<Checkpoint> CheckpointReached;
        public event Action RaceFinished;
        public event Action Respawned;

        public int LastCheckpoint { get; private set; }
        public int WaypointIndex { get; private set; }
        public bool Running { get; private set; }
        public bool Finished { get; private set; }
        public float ElapsedTime { get; private set; }
        public int RespawnCount { get; private set; }
        public int CheckpointGoal => Mathf.Max(0, checkpoints.Length - 1);
        public bool NetworkClient { get; set; }

        public Vector3 CurrentWaypoint => WaypointAt(0);
        public Vector3 NextWaypoint => WaypointAt(1);

        public Vector3 WaypointAt(int offset)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                return raft.Body.position;
            }
            return waypoints[Mathf.Clamp(WaypointIndex + offset, 0, waypoints.Length - 1)].position;
        }

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = GraphicsQuality.TargetFps;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        void Start()
        {
            foreach (Checkpoint checkpoint in checkpoints)
            {
                checkpoint.course = this;
            }
            if (checkpoints.Length > 0)
            {
                WaypointIndex = checkpoints[0].nextWaypointIndex;
                checkpoints[0].MarkPassed();
            }
        }

        void Update()
        {
            if (raft == null)
            {
                return;
            }

            if (!Running && !Finished && raft.Body.linearVelocity.magnitude > startSpeedThreshold)
            {
                Running = true;
            }
            if (Running && !Finished)
            {
                ElapsedTime += Time.deltaTime;
            }

            AdvanceWaypoint();
            if (!NetworkClient)
            {
                WatchRaft(Time.deltaTime);
            }
        }

        void AdvanceWaypoint()
        {
            if (waypoints == null)
            {
                return;
            }

            while (WaypointIndex < waypoints.Length - 1)
            {
                Vector3 point = waypoints[WaypointIndex].position;
                Vector3 segment = waypoints[WaypointIndex + 1].position - point;
                Vector3 offset = raft.Body.position - point;
                segment.y = 0f;
                offset.y = 0f;

                if (offset.magnitude < waypointReachRadius || Vector3.Dot(offset, segment) > 0f)
                {
                    WaypointIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        void WatchRaft(float deltaTime)
        {
            RaftTuning tuning = raft.tuning;
            if (tuning == null)
            {
                return;
            }

            if (raft.Body.position.y < tuning.killHeight + CourseProfile.Height(raft.Body.position.x, raft.Body.position.z))
            {
                Respawn();
                return;
            }

            if (raft.IsFlipped)
            {
                flippedTime += deltaTime;
                if (flippedTime >= tuning.flippedRespawnDelay)
                {
                    Respawn();
                }
            }
            else
            {
                flippedTime = 0f;
            }
        }

        public void SetNetworkTime(float elapsed, bool running)
        {
            ElapsedTime = elapsed;
            Running = running || Running;
        }

        public void NetworkRespawn()
        {
            flippedTime = 0f;
            RespawnCount++;
            Respawned?.Invoke();
        }

        public void NetworkFinish()
        {
            if (Finished)
            {
                return;
            }
            Finished = true;
            RaceFinished?.Invoke();
        }

        public void Respawn()
        {
            if (checkpoints.Length == 0 || NetworkClient)
            {
                return;
            }

            Checkpoint checkpoint = checkpoints[LastCheckpoint];
            raft.Teleport(checkpoint.SpawnPosition + Vector3.up * spawnHeight, checkpoint.SpawnRotation);
            WaypointIndex = checkpoint.nextWaypointIndex;
            flippedTime = 0f;
            RespawnCount++;
            Respawned?.Invoke();
        }

        public void OnCheckpointTouched(Checkpoint checkpoint, Collider other)
        {
            if (raft == null || other.attachedRigidbody != raft.Body)
            {
                return;
            }

            int index = Array.IndexOf(checkpoints, checkpoint);
            if (index <= LastCheckpoint)
            {
                return;
            }

            LastCheckpoint = index;
            WaypointIndex = Mathf.Max(WaypointIndex, checkpoint.nextWaypointIndex);
            checkpoint.MarkPassed();
            if (checkpoint.isFinish)
            {
                Finished = true;
                RaceFinished?.Invoke();
            }
            else
            {
                CheckpointReached?.Invoke(checkpoint);
            }
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
