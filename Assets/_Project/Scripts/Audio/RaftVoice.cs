using System.Collections;
using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Raft))]
    public class RaftVoice : MonoBehaviour
    {
        public CourseManager course;
        [Range(0f, 1f)] public float volume = 0.95f;
        [Min(0f)] public float hitCooldown = 2.5f;
        [Range(0f, 1f)] public float hitChance = 0.6f;
        [Range(0f, 1f)] public float pickupChance = 0.7f;

        Raft raft;
        RaftHealth health;
        RaftBoosts boosts;
        float nextHit;

        void Start()
        {
            raft = GetComponent<Raft>();
            health = GetComponent<RaftHealth>();
            boosts = GetComponent<RaftBoosts>();
            if (health != null)
            {
                health.Damaged += OnDamaged;
                health.Caught += OnCaught;
            }
            if (boosts != null)
            {
                boosts.Collected += OnCollected;
            }
            if (course != null)
            {
                course.RaceFinished += OnFinished;
            }
            Rider.StateChanged += OnRiderState;
        }

        void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Caught -= OnCaught;
            }
            if (boosts != null)
            {
                boosts.Collected -= OnCollected;
            }
            if (course != null)
            {
                course.RaceFinished -= OnFinished;
            }
            Rider.StateChanged -= OnRiderState;
        }

        void OnDamaged(float amount)
        {
            if (Time.time < nextHit || Random.value > hitChance)
            {
                return;
            }
            nextHit = Time.time + hitCooldown;
            Say(RandomRider(), VoiceLine.Hit);
        }

        void OnCaught()
        {
            Say(RandomRider(), VoiceLine.Caught);
        }

        void OnCollected(PickupKind kind)
        {
            if (Random.value <= pickupChance)
            {
                Say(RandomRider(), VoiceLine.Pickup);
            }
        }

        void OnFinished()
        {
            StartCoroutine(Cheer());
        }

        IEnumerator Cheer()
        {
            foreach (Rider rider in raft.riders)
            {
                if (rider == null || !rider.isActiveAndEnabled)
                {
                    continue;
                }
                yield return new WaitForSeconds(Random.Range(0.25f, 0.6f));
                AudioClip[] clips = rider.avatar != null ? rider.avatar.voice.Get(VoiceLine.Cheer) : null;
                if (clips != null && clips.Length > 0)
                {
                    AudioClip clip = Random.value < 0.5f ? clips[clips.Length - 1] : clips[Random.Range(0, clips.Length)];
                    GameAudio.PlayClip(clip, volume);
                }
            }
        }

        void OnRiderState(Rider rider, RiderEvent state)
        {
            if (rider == null || rider.raft != raft)
            {
                return;
            }
            switch (state)
            {
                case RiderEvent.Fell:
                    Say(rider, VoiceLine.Fall);
                    break;
                case RiderEvent.Boarded:
                    Say(rider, VoiceLine.Board);
                    break;
                case RiderEvent.Eliminated:
                    Say(rider, VoiceLine.Eliminated);
                    break;
            }
        }

        Rider RandomRider()
        {
            if (raft == null || raft.riders == null || raft.riders.Length == 0)
            {
                return null;
            }
            int start = Random.Range(0, raft.riders.Length);
            for (int k = 0; k < raft.riders.Length; k++)
            {
                Rider rider = raft.riders[(start + k) % raft.riders.Length];
                if (rider != null && rider.isActiveAndEnabled && !rider.InWater)
                {
                    return rider;
                }
            }
            return null;
        }

        void Say(Rider rider, VoiceLine line)
        {
            if (rider == null || rider.avatar == null || rider.avatar.voice == null)
            {
                return;
            }
            AudioClip[] clips = rider.avatar.voice.Get(line);
            if (clips == null || clips.Length == 0)
            {
                return;
            }
            GameAudio.PlayClip(clips[Random.Range(0, clips.Length)], volume);
        }
    }
}
