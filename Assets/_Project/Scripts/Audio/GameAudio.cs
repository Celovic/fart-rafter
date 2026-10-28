using System.Threading.Tasks;
using UnityEngine;

namespace FartRafters
{
    public class GameAudio : MonoBehaviour
    {
        public enum Sfx
        {
            Click,
            Splash,
            Bump,
            Checkpoint,
            Finish,
            Star,
            Respawn,
            Wheeze,
            FartTail,
            Pickup,
            Crack,
            Chomp
        }

        const string SfxKey = "audio.sfx";
        const string MusicKey = "audio.music";
        const int VoiceCount = 10;
        const float AmbienceVolume = 0.22f;

        static GameAudio instance;

        AudioClip[] clips;
        AudioClip[] fartLoops;
        AudioSource[] voices;
        AudioSource musicSource;
        AudioSource ambienceSource;
        Task<float[]> musicTask;
        int nextVoice;
        float musicLevel = 0.5f;
        bool sfxOn;
        bool musicOn;

        public static GameAudio Instance
        {
            get
            {
                EnsureInstance();
                return instance;
            }
        }

        public static bool SfxOn => Instance.sfxOn;
        public static bool MusicOn => Instance.musicOn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            EnsureInstance();
        }

        static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }
            var go = new GameObject("GameAudio");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameAudio>();
            instance.Initialize();
        }

        public static void SetSfx(bool on)
        {
            PlayerPrefs.SetInt(SfxKey, on ? 1 : 0);
            PlayerPrefs.Save();
            Instance.sfxOn = on;
            Instance.ApplyAmbience();
        }

        public static void SetMusic(bool on)
        {
            PlayerPrefs.SetInt(MusicKey, on ? 1 : 0);
            PlayerPrefs.Save();
            Instance.musicOn = on;
        }

        public static void SetMusicLevel(float level)
        {
            Instance.musicLevel = Mathf.Clamp01(level);
        }

        public static void Play(Sfx sfx, float volume = 1f, float pitch = 1f)
        {
            GameAudio audio = Instance;
            if (!audio.sfxOn)
            {
                return;
            }
            AudioClip clip = audio.clips[(int)sfx];
            AudioSource voice = audio.voices[audio.nextVoice];
            audio.nextVoice = (audio.nextVoice + 1) % audio.voices.Length;
            voice.pitch = pitch;
            voice.PlayOneShot(clip, volume);
        }

        public const float HearingRange = 25f;

        public static void PlayAt(Sfx sfx, Vector3 position, float volume = 1f, float pitch = 1f, float range = HearingRange)
        {
            Raft raft = Raft.Current;
            Camera view = Camera.main;
            if (raft == null && view == null)
            {
                return;
            }
            Vector3 listener = raft != null ? raft.transform.position : view.transform.position;
            float distance = Vector3.Distance(listener, position);
            if (distance >= range)
            {
                return;
            }
            float falloff = 1f - distance / range;
            Play(sfx, volume * falloff * falloff, pitch);
        }

        public static void PlayClip(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            GameAudio audio = Instance;
            if (!audio.sfxOn || clip == null)
            {
                return;
            }
            AudioSource voice = audio.voices[audio.nextVoice];
            audio.nextVoice = (audio.nextVoice + 1) % audio.voices.Length;
            voice.pitch = pitch;
            voice.PlayOneShot(clip, volume);
        }

        public static void PlayFart(AvatarDefinition avatar, float volume = 1f, bool signature = false)
        {
            if (avatar == null)
            {
                return;
            }
            AudioClip clip = signature ? avatar.SignatureFart : avatar.RandomFart();
            if (clip != null)
            {
                PlayClip(clip, volume * avatar.fartVolume, signature ? 1f : Random.Range(0.96f, 1.04f));
            }
            else
            {
                Play(Sfx.FartTail, volume, avatar.voicePitch);
            }
        }

        public static AudioClip FartLoop(int variant)
        {
            AudioClip[] loops = Instance.fartLoops;
            return loops[Mathf.Clamp(variant, 0, loops.Length - 1)];
        }

        void Initialize()
        {
            sfxOn = PlayerPrefs.GetInt(SfxKey, 1) == 1;
            musicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;

            clips = new AudioClip[12];
            clips[(int)Sfx.Click] = Clip("Click", SoundSynth.Click());
            clips[(int)Sfx.Splash] = Clip("Splash", SoundSynth.Splash());
            clips[(int)Sfx.Bump] = Clip("Bump", SoundSynth.Bump());
            clips[(int)Sfx.Checkpoint] = Clip("Checkpoint", SoundSynth.Ding());
            clips[(int)Sfx.Finish] = Clip("Finish", SoundSynth.Fanfare());
            clips[(int)Sfx.Star] = Clip("Star", SoundSynth.Star());
            clips[(int)Sfx.Respawn] = Clip("Respawn", SoundSynth.Whoosh());
            clips[(int)Sfx.Wheeze] = Clip("Wheeze", SoundSynth.Wheeze());
            clips[(int)Sfx.FartTail] = Clip("FartTail", SoundSynth.FartTail());
            clips[(int)Sfx.Pickup] = Clip("Pickup", SoundSynth.Pickup());
            clips[(int)Sfx.Crack] = Clip("Crack", SoundSynth.Crack());
            clips[(int)Sfx.Chomp] = Clip("Chomp", SoundSynth.Chomp());

            fartLoops = new AudioClip[3];
            for (int i = 0; i < fartLoops.Length; i++)
            {
                fartLoops[i] = Clip("FartLoop" + i, SoundSynth.FartLoop(i));
            }

            voices = new AudioSource[VoiceCount];
            for (int i = 0; i < voices.Length; i++)
            {
                voices[i] = CreateSource(false);
            }

            ambienceSource = CreateSource(true);
            ambienceSource.clip = Clip("Water", SoundSynth.WaterLoop());
            ApplyAmbience();

            musicSource = CreateSource(true);
            musicSource.volume = 0f;
            musicTask = Task.Run(() => SoundSynth.Music());
        }

        void Update()
        {
            if (musicTask != null && musicTask.IsCompleted)
            {
                if (musicTask.Status == TaskStatus.RanToCompletion)
                {
                    musicSource.clip = Clip("Music", musicTask.Result);
                    musicSource.Play();
                }
                musicTask = null;
            }

            float target = musicOn ? musicLevel : 0f;
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, target, Time.unscaledDeltaTime * 0.6f);
        }

        void ApplyAmbience()
        {
            if (ambienceSource == null)
            {
                return;
            }
            ambienceSource.volume = AmbienceVolume;
            if (sfxOn && !ambienceSource.isPlaying)
            {
                ambienceSource.Play();
            }
            else if (!sfxOn && ambienceSource.isPlaying)
            {
                ambienceSource.Stop();
            }
        }

        AudioSource CreateSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        static AudioClip Clip(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SoundSynth.Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
