using UnityEngine;

namespace FartRafters
{
    public class GameFeedback : MonoBehaviour
    {
        public CourseManager course;
        public RaftEffects raftEffects;
        public CourseTheme theme;
        [Range(0f, 1f)] public float musicLevel = 0.3f;

        ParticleSystem confetti;
        RaftHealth health;

        void Start()
        {
            GameAudio.SetMusicLevel(musicLevel);
            if (theme != null)
            {
                confetti = EffectFactory.CreateConfetti(transform, theme.confetti);
            }
            if (course == null)
            {
                return;
            }
            course.CheckpointReached += OnCheckpoint;
            course.RaceFinished += OnFinished;
            course.Respawned += OnRespawned;
            health = course.raft != null ? course.raft.GetComponent<RaftHealth>() : null;
            if (health != null)
            {
                health.Damaged += OnDamaged;
                health.Broken += OnBroken;
                health.Caught += OnCaught;
                health.Shocked += OnShocked;
            }
        }

        void OnDestroy()
        {
            if (course == null)
            {
                return;
            }
            course.CheckpointReached -= OnCheckpoint;
            course.RaceFinished -= OnFinished;
            course.Respawned -= OnRespawned;
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Broken -= OnBroken;
                health.Caught -= OnCaught;
                health.Shocked -= OnShocked;
            }
        }

        void OnDamaged(float amount)
        {
            FollowCamera.Shake(Mathf.Clamp(amount * 0.02f, 0.05f, 0.35f));
            Haptics.Impact(amount / 25f);
        }

        void OnBroken()
        {
            GameAudio.Play(GameAudio.Sfx.Crack, 1f);
            FollowCamera.Shake(0.6f);
            Haptics.Break();
        }

        void OnShocked()
        {
            GameAudio.Play(GameAudio.Sfx.Wheeze, 0.9f, 0.5f);
            GameAudio.Play(GameAudio.Sfx.Crack, 0.5f, 1.6f);
            FollowCamera.Shake(0.35f);
            Haptics.Heavy();
        }

        void OnCaught()
        {
            GameAudio.Play(GameAudio.Sfx.Chomp, 1f);
            FollowCamera.Shake(0.5f);
            Haptics.Break();
        }

        void OnCheckpoint(Checkpoint checkpoint)
        {
            GameAudio.Play(GameAudio.Sfx.Checkpoint, 0.8f);
            Haptics.Medium();
            Burst(checkpoint.transform.position, 40);
        }

        void OnFinished()
        {
            GameAudio.Play(GameAudio.Sfx.Finish, 1f);
            Haptics.Celebrate();
            if (course.raft != null)
            {
                foreach (AvatarRig rig in course.raft.GetComponentsInChildren<AvatarRig>())
                {
                    rig.Celebrate();
                }
            }
            Checkpoint finish = course.checkpoints[course.checkpoints.Length - 1];
            Vector3 right = finish.transform.right * (finish.GetComponent<BoxCollider>().size.x * 0.4f);
            Burst(finish.transform.position + right, 90);
            Burst(finish.transform.position - right, 90);
            FollowCamera.Shake(0.25f);
        }

        void OnRespawned()
        {
            Haptics.Medium();
            GameAudio.Play(GameAudio.Sfx.Respawn, 0.8f);
            GameAudio.Play(GameAudio.Sfx.Splash, 0.6f, 0.9f);
            if (raftEffects != null && course.raft != null)
            {
                raftEffects.SplashAt(course.raft.transform.position, 30);
            }
        }

        void Burst(Vector3 position, int count)
        {
            if (confetti == null)
            {
                return;
            }
            confetti.transform.position = position + Vector3.up * 0.5f;
            confetti.Emit(count);
        }
    }
}
