using System.Collections.Generic;
using UnityEngine;

namespace FartRafters
{
    public enum CrowdMood
    {
        Fans,
        Hecklers,
        Foodies,
        Mixed
    }

    public class SpectatorGroup : MonoBehaviour
    {
        public CrowdMood mood;
        public CourseTheme theme;
        public float reactDistance = 28f;
        public float volume = 0.9f;
        public readonly List<Spectator> members = new List<Spectator>();

        AudioSource source;
        float nextSound;
        bool active;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 6f;
            source.maxDistance = 42f;
            source.dopplerLevel = 0f;
        }

        void Update()
        {
            Raft raft = Raft.Current;
            float distance = float.MaxValue;
            if (raft != null)
            {
                Vector3 offset = raft.transform.position - transform.position;
                offset.y = 0f;
                distance = offset.magnitude;
            }
            else if (Camera.main != null)
            {
                distance = Vector3.Distance(Camera.main.transform.position, transform.position) * 0.5f;
            }

            bool near = distance < reactDistance;
            if (near != active)
            {
                active = near;
                foreach (Spectator member in members)
                {
                    if (member != null)
                    {
                        member.Excite(near ? 1f : 0f);
                    }
                }
                nextSound = Time.time + Random.Range(0.1f, 0.8f);
            }

            if (active && Time.time >= nextSound && GameAudio.SfxOn)
            {
                AudioClip clip = PickClip();
                if (clip != null)
                {
                    source.pitch = Random.Range(0.94f, 1.06f);
                    source.PlayOneShot(clip, volume);
                    nextSound = Time.time + clip.length * Random.Range(0.8f, 1.4f) + Random.Range(0.3f, 1.5f);
                }
            }
        }

        AudioClip PickClip()
        {
            AudioClip[] pool;
            if (theme.crowdAnimal != null && theme.crowdAnimal.Length > 0 && Random.value < 0.3f)
            {
                return theme.crowdAnimal[Random.Range(0, theme.crowdAnimal.Length)];
            }
            float roll = Random.value;
            switch (mood)
            {
                case CrowdMood.Fans:
                    pool = roll < 0.4f ? theme.crowdChant : roll < 0.7f ? theme.crowdCheer : theme.crowdClap;
                    break;
                case CrowdMood.Hecklers:
                    pool = theme.crowdBoo;
                    break;
                case CrowdMood.Foodies:
                    pool = roll < 0.6f ? theme.crowdChew : theme.crowdClap;
                    break;
                default:
                    pool = roll < 0.3f ? theme.crowdBoo : roll < 0.6f ? theme.crowdCheer : roll < 0.8f ? theme.crowdChant : theme.crowdClap;
                    break;
            }
            return pool != null && pool.Length > 0 ? pool[Random.Range(0, pool.Length)] : null;
        }
    }
}
