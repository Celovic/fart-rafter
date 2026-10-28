using System;

namespace FartRafters
{
    public static class SoundSynth
    {
        public const int Rate = 22050;
        const float TwoPi = MathF.PI * 2f;

        enum Wave
        {
            Sine,
            Triangle,
            Square,
            Soft
        }

        public static float[] FartLoop(int variant)
        {
            float baseFreq = variant == 0 ? 62f : variant == 1 ? 84f : 120f;
            float flutter = variant == 0 ? 17f : variant == 1 ? 23f : 30f;
            float cutoff = variant == 0 ? 650f : variant == 1 ? 850f : 1150f;
            int loop = Rate;
            int fade = Rate / 6;
            float[] raw = FartCore(loop + fade, baseFreq, flutter, cutoff, 0.1f, 0f, 0.7f, 101 + variant);
            float[] data = MakeLoop(raw, loop);
            Normalize(data, 0.85f);
            return data;
        }

        public static float[] FartTail()
        {
            float[] data = FartCore(Samples(0.38f), 95f, 26f, 900f, 0.05f, 0.55f, 1.2f, 7);
            Envelope(data, 0.005f, 0.3f);
            Normalize(data, 0.8f);
            return data;
        }

        public static float[] Wheeze()
        {
            float[] data = FartCore(Samples(0.7f), 340f, 38f, 2400f, 0.15f, 0.65f, 0.9f, 13);
            Envelope(data, 0.01f, 0.35f);
            Normalize(data, 0.7f);
            return data;
        }

        public static float[] Splash()
        {
            var random = new Random(21);
            var data = new float[Samples(0.7f)];
            float low = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                float cutoff = 300f + 3700f * MathF.Exp(-t * 6f);
                low += OnePole(cutoff) * (Noise(random) - low);
                float envelope = t < 0.005f ? t / 0.005f : MathF.Exp(-t * 5.5f);
                data[i] = low * envelope * 1.8f;
            }

            for (int k = 0; k < 7; k++)
            {
                int start = Samples(0.04f + (float)random.NextDouble() * 0.45f);
                float freq = 900f + (float)random.NextDouble() * 1400f;
                float phase = 0f;
                for (int j = 0; j < Samples(0.06f) && start + j < data.Length; j++)
                {
                    float t = j / (float)Rate;
                    phase += freq * (1f + t * 8f) / Rate;
                    data[start + j] += MathF.Sin(TwoPi * phase) * MathF.Exp(-t * 60f) * 0.25f;
                }
            }
            Normalize(data, 0.8f);
            return data;
        }

        public static float[] Bump()
        {
            var random = new Random(3);
            var data = new float[Samples(0.35f)];
            float phase = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                phase += (55f + 70f * MathF.Exp(-t * 18f)) / Rate;
                float body = MathF.Sin(TwoPi * phase) * MathF.Exp(-t * 12f);
                float knock = MathF.Sin(TwoPi * 230f * t) * MathF.Exp(-t * 30f) * 0.4f;
                float click = Noise(random) * MathF.Exp(-t * 150f) * 0.5f;
                data[i] = body + knock + click;
            }
            Normalize(data, 0.9f);
            return data;
        }

        public static float[] Ding()
        {
            var data = new float[Samples(1.3f)];
            Bell(data, 0f, 987.77f, 0.6f);
            Bell(data, 0.11f, 1318.51f, 0.6f);
            Normalize(data, 0.7f);
            return data;
        }

        public static float[] Fanfare()
        {
            var data = new float[Samples(1.5f)];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            for (int i = 0; i < notes.Length; i++)
            {
                bool last = i == notes.Length - 1;
                AddTone(data, i * 0.11f, last ? 0.8f : 0.1f, notes[i], 0.5f, Wave.Soft, 0.005f, 0.08f, last ? 0.012f : 0f, false);
            }
            AddTone(data, 0.33f, 0.8f, 261.63f, 0.22f, Wave.Triangle, 0.01f, 0.2f, 0f, false);
            AddTone(data, 0.33f, 0.8f, 329.63f, 0.22f, Wave.Triangle, 0.01f, 0.2f, 0f, false);
            AddTone(data, 0.33f, 0.8f, 392f, 0.22f, Wave.Triangle, 0.01f, 0.2f, 0f, false);
            Normalize(data, 0.75f);
            return data;
        }

        public static float[] Star()
        {
            var data = new float[Samples(0.35f)];
            float phase = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                phase += (700f + 700f * MathF.Min(1f, t / 0.12f)) / Rate;
                float envelope = MathF.Min(1f, t / 0.004f) * MathF.Exp(-t * 9f);
                data[i] = (MathF.Sin(TwoPi * phase) + 0.3f * MathF.Sin(2f * TwoPi * phase)) * envelope;
            }
            AddTone(data, 0.06f, 0.05f, 2800f, 0.2f, Wave.Sine, 0.002f, 0.15f, 0f, false);
            Normalize(data, 0.7f);
            return data;
        }

        public static float[] Pickup()
        {
            var data = new float[Samples(0.5f)];
            Bell(data, 0f, 880f, 0.5f);
            Bell(data, 0.07f, 1174.7f, 0.45f);
            Bell(data, 0.14f, 1568f, 0.45f);
            AddTone(data, 0.02f, 0.3f, 2600f, 0.12f, Wave.Sine, 0.002f, 0.25f, 0f, false);
            Normalize(data, 0.75f);
            return data;
        }

        public static float[] Crack()
        {
            var random = new Random(9);
            var data = new float[Samples(0.7f)];
            float lowpass = 0f;
            float phase = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                lowpass += (Noise(random) - lowpass) * OnePole(1800f);
                float splinter = lowpass * MathF.Exp(-t * 7f) * (0.6f + 0.4f * MathF.Sin(TwoPi * 37f * t));
                phase += (70f + 50f * MathF.Exp(-t * 10f)) / Rate;
                float thump = MathF.Sin(TwoPi * phase) * MathF.Exp(-t * 6f);
                data[i] = splinter * 1.2f + thump * 0.8f;
            }
            Normalize(data, 0.9f);
            return data;
        }

        public static float[] Chomp()
        {
            var random = new Random(13);
            var data = new float[Samples(0.45f)];
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                float first = MathF.Exp(-t * 60f);
                float second = t > 0.16f ? MathF.Exp(-(t - 0.16f) * 50f) : 0f;
                float clack = Noise(random) * (first + second) * 0.7f;
                float body = MathF.Sin(TwoPi * 180f * t) * (first + second * 1.2f);
                data[i] = clack + body;
            }
            Normalize(data, 0.85f);
            return data;
        }

        public static float[] Click()
        {
            var data = new float[Samples(0.06f)];
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                data[i] = MathF.Sin(TwoPi * 1400f * t) * MathF.Exp(-t * 70f) + 0.3f * MathF.Sin(TwoPi * 2800f * t) * MathF.Exp(-t * 90f);
            }
            Normalize(data, 0.6f);
            return data;
        }

        public static float[] Whoosh()
        {
            var random = new Random(17);
            const float duration = 0.7f;
            var data = new float[Samples(duration)];
            float high = 0f;
            float low = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                float shape = MathF.Sin(MathF.PI * t / duration);
                float center = 300f + 1700f * shape;
                float noise = Noise(random);
                high += OnePole(center * 1.5f) * (noise - high);
                low += OnePole(center * 0.5f) * (noise - low);
                data[i] = (high - low) * shape * shape * 3f;
            }
            Normalize(data, 0.6f);
            return data;
        }

        public static float[] WaterLoop()
        {
            var random = new Random(5);
            int loop = Rate * 4;
            int fade = Rate / 2;
            var raw = new float[loop + fade];
            var laps = new float[raw.Length];
            float time = 0f;
            float total = raw.Length / (float)Rate;
            while (time < total)
            {
                float duration = 0.4f + (float)random.NextDouble() * 0.3f;
                float amplitude = 0.4f + (float)random.NextDouble() * 0.6f;
                int start = Samples(time);
                int length = Samples(duration);
                for (int j = 0; j < length && start + j < laps.Length; j++)
                {
                    float x = MathF.Sin(MathF.PI * j / length);
                    laps[start + j] += amplitude * x * x;
                }
                time += 0.45f + (float)random.NextDouble() * 0.45f;
            }

            float low = 0f;
            float mid = 0f;
            float aLow = OnePole(420f);
            float aMid = OnePole(1300f);
            for (int i = 0; i < raw.Length; i++)
            {
                float white = Noise(random);
                low += aLow * (white - low);
                mid += aMid * (white - mid);
                raw[i] = low * 1.4f * (0.35f + 0.65f * laps[i]) + (mid - low) * 0.18f * laps[i];
            }

            float[] data = MakeLoop(raw, loop);
            Normalize(data, 0.5f);
            return data;
        }

        public static float[] Music()
        {
            const float bpm = 116f;
            const int bars = 8;
            float beat = 60f / bpm;
            float bar = beat * 4f;
            var data = new float[Samples(bar * bars)];
            var random = new Random(2026);

            int[] roots = { 48, 45, 41, 43, 48, 45, 41, 43 };
            bool[] minor = { false, true, false, false, false, true, false, false };
            int[] scale = { 72, 74, 76, 79, 81, 84 };
            int melody = 2;

            for (int b = 0; b < bars; b++)
            {
                float start = b * bar;
                int root = roots[b];
                int third = minor[b] ? 3 : 4;

                AddTone(data, start, beat * 0.45f, Midi(root), 0.45f, Wave.Soft, 0.004f, 0.05f, 0f, true);
                AddTone(data, start + beat * 2f, beat * 0.45f, Midi(root + 7), 0.4f, Wave.Soft, 0.004f, 0.05f, 0f, true);
                for (int k = 1; k <= 3; k += 2)
                {
                    float at = start + beat * k;
                    AddTone(data, at, beat * 0.3f, Midi(root + 12), 0.09f, Wave.Square, 0.004f, 0.05f, 0f, true);
                    AddTone(data, at, beat * 0.3f, Midi(root + 12 + third), 0.09f, Wave.Square, 0.004f, 0.05f, 0f, true);
                    AddTone(data, at, beat * 0.3f, Midi(root + 19), 0.09f, Wave.Square, 0.004f, 0.05f, 0f, true);
                }

                bool cadence = b % 4 == 3;
                for (int e = 0; e < 8; e++)
                {
                    float at = start + e * beat * 0.5f;
                    if (cadence)
                    {
                        if (e == 0 || e == 4)
                        {
                            melody = NearestChordTone(scale, melody, root, third, random);
                            AddTone(data, at, beat * (e == 0 ? 1.4f : 1.8f), Midi(scale[melody]), 0.17f, Wave.Soft, 0.01f, 0.1f, 0.006f, true);
                        }
                        continue;
                    }

                    bool strong = e % 2 == 0;
                    if (!strong && random.NextDouble() < 0.25)
                    {
                        continue;
                    }
                    melody = strong
                        ? NearestChordTone(scale, melody, root, third, random)
                        : Math.Clamp(melody + (random.Next(2) == 0 ? -1 : 1), 0, scale.Length - 1);
                    AddTone(data, at, beat * 0.42f, Midi(scale[melody]), 0.16f, Wave.Soft, 0.006f, 0.06f, 0.004f, true);
                }

                for (int k = 0; k < 4; k++)
                {
                    float at = start + beat * k;
                    if (k % 2 == 0)
                    {
                        AddKick(data, at, 0.6f);
                    }
                    else
                    {
                        AddSnare(data, at, 0.3f, random);
                    }
                    AddHat(data, at, 0.06f, random);
                    AddHat(data, at + beat * 0.5f, 0.1f, random);
                }
            }

            Normalize(data, 0.8f);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = MathF.Tanh(data[i] * 1.1f);
            }
            return data;
        }

        static float[] FartCore(int length, float baseFreq, float flutter, float cutoff, float wobble, float pitchDrop, float noiseAmount, int seed)
        {
            var random = new Random(seed);
            var data = new float[length];
            float phase = 0f;
            float flap = 0f;
            float low1 = 0f;
            float low2 = 0f;
            float noiseLow = 0f;
            float jitter = 0f;
            float a = OnePole(cutoff);
            float aNoise = OnePole(cutoff * 1.8f);

            for (int i = 0; i < length; i++)
            {
                float t = i / (float)Rate;
                float progress = i / (float)length;
                jitter = jitter * 0.995f + Noise(random) * 0.01f;
                float freq = baseFreq * (1f + wobble * MathF.Sin(TwoPi * 1.7f * t) + jitter) * (1f - pitchDrop * progress);
                phase += freq / Rate;
                phase -= MathF.Floor(phase);
                float pulse = phase < 0.3f ? 1f : -0.43f;

                flap += flutter * (1f + jitter * 3f) / Rate;
                flap -= MathF.Floor(flap);
                float lips = 0.5f + 0.5f * MathF.Sin(TwoPi * flap);
                float amplitude = 0.3f + 0.7f * lips * lips;

                noiseLow += aNoise * (Noise(random) - noiseLow);
                float sample = (pulse + noiseLow * noiseAmount) * amplitude;
                low1 += a * (sample - low1);
                low2 += a * (low1 - low2);
                data[i] = MathF.Tanh(low2 * 2.2f);
            }
            return data;
        }

        static void Bell(float[] data, float start, float freq, float volume)
        {
            float[] ratios = { 1f, 2f, 3f, 4.2f };
            float[] amplitudes = { 1f, 0.45f, 0.22f, 0.12f };
            float[] decays = { 3.5f, 5.5f, 8f, 12f };
            int offset = Samples(start);
            for (int i = offset; i < data.Length; i++)
            {
                float t = (i - offset) / (float)Rate;
                float sample = 0f;
                for (int k = 0; k < ratios.Length; k++)
                {
                    sample += amplitudes[k] * MathF.Sin(TwoPi * freq * ratios[k] * t) * MathF.Exp(-t * decays[k]);
                }
                data[i] += sample * volume * MathF.Min(1f, t / 0.002f);
            }
        }

        static void AddTone(float[] data, float start, float duration, float freq, float volume, Wave wave, float attack, float release, float vibrato, bool wrap)
        {
            int begin = Samples(start);
            int length = Samples(duration + release);
            float phase = 0f;
            for (int j = 0; j < length; j++)
            {
                float t = j / (float)Rate;
                float envelope = t < attack ? t / attack : 1f - 0.25f * MathF.Min(1f, (t - attack) / MathF.Max(duration, 0.001f));
                if (t > duration)
                {
                    envelope *= MathF.Max(0f, 1f - (t - duration) / MathF.Max(release, 0.001f));
                }

                phase += freq * (1f + vibrato * MathF.Sin(TwoPi * 5.5f * t)) / Rate;
                phase -= MathF.Floor(phase);

                int index = begin + j;
                if (wrap)
                {
                    index %= data.Length;
                }
                else if (index >= data.Length)
                {
                    break;
                }
                data[index] += Oscillator(wave, phase) * envelope * volume;
            }
        }

        static void AddKick(float[] data, float start, float volume)
        {
            int begin = Samples(start);
            float phase = 0f;
            for (int j = 0; j < Samples(0.18f); j++)
            {
                float t = j / (float)Rate;
                phase += (45f + 90f * MathF.Exp(-t * 30f)) / Rate;
                data[(begin + j) % data.Length] += MathF.Sin(TwoPi * phase) * MathF.Exp(-t * 18f) * volume;
            }
        }

        static void AddSnare(float[] data, float start, float volume, Random random)
        {
            int begin = Samples(start);
            float low = 0f;
            float a = OnePole(1500f);
            for (int j = 0; j < Samples(0.15f); j++)
            {
                float t = j / (float)Rate;
                float noise = Noise(random);
                low += a * (noise - low);
                float sample = (noise - low) * MathF.Exp(-t * 25f) + MathF.Sin(TwoPi * 190f * t) * MathF.Exp(-t * 30f) * 0.5f;
                data[(begin + j) % data.Length] += sample * volume;
            }
        }

        static void AddHat(float[] data, float start, float volume, Random random)
        {
            int begin = Samples(start);
            float low = 0f;
            float a = OnePole(6000f);
            for (int j = 0; j < Samples(0.04f); j++)
            {
                float t = j / (float)Rate;
                float noise = Noise(random);
                low += a * (noise - low);
                data[(begin + j) % data.Length] += (noise - low) * MathF.Exp(-t * 90f) * volume;
            }
        }

        static int NearestChordTone(int[] scale, int from, int root, int third, Random random)
        {
            int best = from;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < scale.Length; i++)
            {
                int pitch = scale[i] % 12;
                if (pitch != root % 12 && pitch != (root + third) % 12 && pitch != (root + 7) % 12)
                {
                    continue;
                }
                int distance = Math.Abs(i - from) * 2 + random.Next(0, 3);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
            return best;
        }

        static float Oscillator(Wave wave, float phase)
        {
            switch (wave)
            {
                case Wave.Triangle:
                    return 1f - 4f * MathF.Abs(phase - 0.5f);
                case Wave.Square:
                    return phase < 0.5f ? 1f : -1f;
                case Wave.Soft:
                    return 0.8f * (MathF.Sin(TwoPi * phase) + MathF.Sin(3f * TwoPi * phase) / 3f + MathF.Sin(5f * TwoPi * phase) / 5f);
                default:
                    return MathF.Sin(TwoPi * phase);
            }
        }

        static float[] MakeLoop(float[] raw, int loopLength)
        {
            int fade = raw.Length - loopLength;
            var result = new float[loopLength];
            Array.Copy(raw, result, loopLength);
            for (int i = 0; i < fade; i++)
            {
                float w = i / (float)fade;
                result[i] = raw[i] * w + raw[loopLength + i] * (1f - w);
            }
            return result;
        }

        static void Envelope(float[] data, float attack, float release)
        {
            int attackSamples = Math.Max(1, Samples(attack));
            int releaseSamples = Math.Max(1, Samples(release));
            for (int i = 0; i < data.Length; i++)
            {
                float gain = 1f;
                if (i < attackSamples)
                {
                    gain = i / (float)attackSamples;
                }
                int fromEnd = data.Length - i;
                if (fromEnd < releaseSamples)
                {
                    gain *= fromEnd / (float)releaseSamples;
                }
                data[i] *= gain;
            }
        }

        static void Normalize(float[] data, float peak)
        {
            float max = 0f;
            foreach (float sample in data)
            {
                max = MathF.Max(max, MathF.Abs(sample));
            }
            if (max < 0.0001f)
            {
                return;
            }
            float gain = peak / max;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] *= gain;
            }
        }

        static float Midi(int note)
        {
            return 440f * MathF.Pow(2f, (note - 69) / 12f);
        }

        static float OnePole(float cutoff)
        {
            return 1f - MathF.Exp(-TwoPi * cutoff / Rate);
        }

        static float Noise(Random random)
        {
            return (float)(random.NextDouble() * 2.0 - 1.0);
        }

        static int Samples(float seconds)
        {
            return (int)(seconds * Rate);
        }
    }
}
