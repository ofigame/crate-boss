using System;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Placeholder sounds made in code (doc Faz 2: "ses dosyası yoksa yer tutucu ses").
    /// Each one follows the material: wooden knock, icy hiss and glass ring, deep metal thud, ...
    /// Replace by dropping real clips into GameFeedback's fields.
    /// </summary>
    public static class SfxSynth
    {
        public const int Rate = 22050;
        private static readonly System.Random noise = new System.Random(1);

        public static AudioClip Make(SoundId id)
        {
            switch (id)
            {
                case SoundId.Step: return Clip("step", 0.06f, t => Noise() * Env(t, 0.002f, 0.05f) * 0.15f + Sine(t, 180) * Env(t, 0.002f, 0.04f) * 0.15f);
                case SoundId.WoodPush: return Clip("wood", 0.22f, t =>
                    (Sine(t, 140) * 0.6f + Sine(t, 290) * 0.3f) * Env(t, 0.001f, 0.08f) * 0.7f + Noise() * Env(t, 0.01f, 0.2f) * 0.12f);
                case SoundId.IceSlide: return Clip("ice_slide", 0.45f, t => Bandish(t) * Env(t, 0.03f, 0.42f) * 0.25f);
                case SoundId.IceStop: return Clip("ice_stop", 0.7f, t =>
                    (Sine(t, 1760) * 0.5f + Sine(t, 2637) * 0.3f + Sine(t, 3520) * 0.15f) * Env(t, 0.001f, 0.65f) * 0.35f);
                case SoundId.HeavyPush: return Clip("heavy", 0.5f, t =>
                    (Sine(t, 55 + 20 * Mathf.Exp(-t * 20)) * 0.9f + Sine(t, 110) * 0.3f) * Env(t, 0.002f, 0.45f) * 0.8f + Noise() * Env(t, 0.001f, 0.06f) * 0.2f);
                case SoundId.Fall: return Clip("fall", 0.35f, t => (Noise() * 0.4f + Sine(t, 70) * 0.8f) * Env(t, 0.005f, 0.3f) * 0.6f);
                case SoundId.Settle: return Clip("settle", 0.12f, t => Sine(t, 220) * Env(t, 0.001f, 0.1f) * 0.4f);
                case SoundId.Bump: return Clip("bump", 0.12f, t => Sine(t, 160 - t * 400) * Env(t, 0.001f, 0.1f) * 0.35f);
                case SoundId.Star: return Clip("star", 0.4f, t => (Sine(t, 1320) + Sine(t, 1980) * 0.4f) * Env(t, 0.001f, 0.38f) * 0.3f);
                case SoundId.CraneMotor: return Clip("crane_motor", 0.6f, t =>
                    (Saw(t, 90 + 30 * Mathf.Sin(t * 6f)) * 0.35f + Sine(t, 180) * 0.2f) * Env(t, 0.08f, 0.58f) * 0.4f);
                case SoundId.CraneBeep: return Clip("crane_beep", 0.16f, t => Sine(t, 1180) * Env(t, 0.005f, 0.15f) * 0.3f);
                case SoundId.CraneBeepDouble: return Clip("crane_beep2", 0.34f, t =>
                    Sine(t, 1320) * (Env(t, 0.005f, 0.13f) + Env(t - 0.18f, 0.005f, 0.14f)) * 0.32f);
                case SoundId.ContainerThud: return Clip("container", 0.7f, t =>
                    (Sine(t, 48 + 30 * Mathf.Exp(-t * 18)) * 0.9f + Sine(t, 213) * 0.25f * Mathf.Exp(-t * 6f) + Sine(t, 431) * 0.12f * Mathf.Exp(-t * 4f))
                    * Env(t, 0.002f, 0.65f) * 0.85f + Noise() * Env(t, 0.001f, 0.05f) * 0.25f);
                case SoundId.CraneOff: return Clip("crane_off", 0.9f, t => Saw(t, 110 - t * 70) * Env(t, 0.02f, 0.88f) * 0.25f);
                case SoundId.CraneRewind: return Clip("crane_rewind", 0.22f, t => Saw(t, 240 - t * 500) * Env(t, 0.01f, 0.2f) * 0.25f);
                case SoundId.Pop: return Clip("pop", 0.14f, t => Sine(t, 420 + t * 4200) * Env(t, 0.002f, 0.12f) * 0.45f);
                case SoundId.Boing: return Clip("boing", 0.45f, t => Sine(t, 240 + 70 * Mathf.Sin(t * 38f) * (1f - t / 0.45f) - t * 120) * Env(t, 0.004f, 0.42f) * 0.4f);
                case SoundId.Horn: return Clip("horn", 0.42f, t =>
                    (Saw(t, 420) * 0.5f + Saw(t, 528) * 0.5f) * (Env(t, 0.01f, 0.15f) + Env(t - 0.22f, 0.01f, 0.17f)) * 0.22f);
                default: return Clip("click", 0.05f, t => Sine(t, 900) * Env(t, 0.001f, 0.04f) * 0.25f);
            }
        }

        /// <summary>A soft bell note; targets play a rising scale, the win plays them together as a chord.</summary>
        public static AudioClip Note(float frequency, float length = 0.9f) =>
            Clip($"note_{frequency:0}", length, t =>
                (Sine(t, frequency) + Sine(t, frequency * 2f) * 0.35f + Sine(t, frequency * 3f) * 0.12f) * Env(t, 0.004f, length * 0.95f) * 0.28f);

        public static AudioClip Chord(float[] frequencies, float length = 1.6f) =>
            Clip("chord", length, t =>
            {
                float s = 0f;
                for (int i = 0; i < frequencies.Length; i++)
                    s += Sine(t, frequencies[i]) * Env(t - i * 0.06f, 0.01f, length - 0.2f);
                return s * 0.18f;
            });

        public static float Sine(float t, float f) => Mathf.Sin(2f * Mathf.PI * f * t);

        /// <summary>Attack/decay envelope: quick rise, exponential-looking fall to zero at <paramref name="length"/>.</summary>
        public static float Env(float t, float attack, float length)
        {
            if (t < 0f || t > length) return 0f;
            if (t < attack) return t / attack;
            float k = (t - attack) / (length - attack);
            return (1f - k) * (1f - k);
        }

        // Soft sawtooth (a few harmonics), for motors.
        private static float Saw(float t, float f) => Sine(t, f) + Sine(t, f * 2f) * 0.5f + Sine(t, f * 3f) * 0.33f;

        private static float Noise() => (float)(noise.NextDouble() * 2.0 - 1.0);

        // Hissy noise with a slow shimmer, for sliding on ice.
        private static float Bandish(float t) => Noise() * (0.6f + 0.4f * Mathf.Sin(t * 60f));

        public static AudioClip Clip(string name, float seconds, Func<float, float> wave)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(wave(i / (float)Rate), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
