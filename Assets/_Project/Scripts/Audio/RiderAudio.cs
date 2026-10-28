using UnityEngine;

namespace FartRafters
{
    [RequireComponent(typeof(Rider))]
    public class RiderAudio : MonoBehaviour
    {
        [Range(0f, 1f)] public float playerVolume = 0.75f;
        [Range(0f, 1f)] public float botVolume = 0.45f;
        [Min(0f)] public float chainOverlap = 0.06f;

        Rider rider;
        AudioSource source;
        float level;
        float pitch = 1f;
        bool wasFiring;
        bool wasExhausted;

        void Awake()
        {
            rider = GetComponent<Rider>();
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }

        void OnDisable()
        {
            if (source != null)
            {
                source.Stop();
                source.volume = 0f;
            }
            level = 0f;
            wasFiring = false;
        }

        void Update()
        {
            AvatarDefinition avatar = rider.avatar;
            if (avatar == null)
            {
                return;
            }

            bool firing = rider.IsFiring && GameAudio.SfxOn;
            float volume = (rider.IsBot ? botVolume : playerVolume) * avatar.fartVolume;
            bool sampled = avatar.HasFartClips;

            if (firing && !wasFiring)
            {
                pitch = (sampled ? 1f : avatar.voicePitch) * Random.Range(0.94f, 1.06f);
                if (sampled)
                {
                    PlaySample(avatar);
                }
                else
                {
                    source.loop = true;
                    source.clip = GameAudio.FartLoop((int)avatar.kind);
                    if (!source.isPlaying)
                    {
                        source.Play();
                    }
                    source.time = Random.Range(0f, source.clip.length * 0.9f);
                }
            }
            else if (firing && sampled && SampleEnding())
            {
                pitch = Random.Range(0.94f, 1.06f);
                PlaySample(avatar);
            }

            if (!firing && wasFiring && !sampled)
            {
                GameAudio.Play(GameAudio.Sfx.FartTail, volume * 0.8f, pitch);
            }

            bool exhausted = rider.Gas.Exhausted;
            if (exhausted && !wasExhausted)
            {
                GameAudio.Play(GameAudio.Sfx.Wheeze, volume, avatar.voicePitch);
            }
            wasExhausted = exhausted;
            wasFiring = firing;

            float release = sampled ? 9f : 12f;
            level = Mathf.MoveTowards(level, firing ? 1f : 0f, Time.deltaTime * (firing ? 30f : release));
            source.volume = level * volume;
            source.pitch = sampled ? pitch : pitch * (1f + Mathf.Sin(Time.time * 3.1f) * 0.04f);
            if (level <= 0f && source.isPlaying)
            {
                source.Stop();
            }
        }

        void PlaySample(AvatarDefinition avatar)
        {
            AudioClip clip = avatar.RandomFart(source.clip);
            if (clip == null)
            {
                return;
            }
            source.loop = false;
            source.clip = clip;
            source.time = 0f;
            source.Play();
        }

        bool SampleEnding()
        {
            if (source.clip == null || !source.isPlaying)
            {
                return true;
            }
            return source.time >= source.clip.length - chainOverlap;
        }
    }
}
