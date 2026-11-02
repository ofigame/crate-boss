using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Calm looping music per world, built in code as four layers that play in sync.
    /// Every box sitting on a target turns one more layer on (Ana Belge §2), so the music grows as the level fills.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        private const float LoopSeconds = 8f;
        private const float Volume = 0.35f;

        private readonly AudioSource[] layers = new AudioSource[4];
        private readonly float[] targetVolume = new float[4];
        private string world;

        public bool Muted { get; set; }

        private void Awake()
        {
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = gameObject.AddComponent<AudioSource>();
                layers[i].loop = true;
                layers[i].playOnAwake = false;
                layers[i].volume = 0f;
            }
        }

        public void PlayWorld(string worldId)
        {
            if (world == worldId) return;
            world = worldId;
            float root;
            int[] chords; // scale degrees of the four chord roots
            bool minor;
            switch (worldId)
            {
                case "liman": root = 246.94f; chords = new[] { 0, 5, 9, 7 }; minor = false; break;
                case "soguk_depo": root = 293.66f; chords = new[] { 0, 7, 9, 4 }; minor = false; break;
                case "santiye": root = 220f; chords = new[] { 0, 8, 3, 10 }; minor = true; break;
                case "havalimani": root = 196f; chords = new[] { 0, 5, 3, 7 }; minor = true; break;
                case "gumruk": root = 220f; chords = new[] { 0, 8, 3, 10 }; minor = true; break;
                default: root = 261.63f; chords = new[] { 0, 9, 5, 7 }; minor = false; break;
            }
            var clips = Build(root, chords, minor);
            double start = AudioSettings.dspTime + 0.1;
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].Stop();
                layers[i].clip = clips[i];
                layers[i].PlayScheduled(start);
            }
        }

        /// <summary>How many targets are filled right now; 0 keeps only the base pad.</summary>
        public void SetIntensity(int filledTargets)
        {
            for (int i = 0; i < targetVolume.Length; i++) targetVolume[i] = i == 0 || filledTargets >= i ? 1f : 0f;
        }

        private void Update()
        {
            for (int i = 0; i < layers.Length; i++)
            {
                float goal = Muted ? 0f : targetVolume[i] * Volume * (i == 0 ? 1f : 0.8f);
                layers[i].volume = Mathf.MoveTowards(layers[i].volume, goal, Time.deltaTime * 0.6f);
            }
        }

        private static AudioClip[] Build(float root, int[] chords, bool minor)
        {
            float Freq(int semitones) => root * Mathf.Pow(2f, semitones / 12f);
            int third = minor ? 3 : 4;
            float bar = LoopSeconds / chords.Length;

            int ChordAt(float t) => chords[Mathf.Min(chords.Length - 1, (int)(t / bar))];

            // Pad: soft sustained triad per bar.
            var pad = SfxSynth.Clip("music_pad", LoopSeconds, t =>
            {
                int c = ChordAt(t);
                float local = t % bar;
                float env = Mathf.Min(1f, local / 0.4f) * Mathf.Min(1f, (bar - local) / 0.4f);
                return (SfxSynth.Sine(t, Freq(c - 12)) + SfxSynth.Sine(t, Freq(c - 12 + third)) * 0.7f + SfxSynth.Sine(t, Freq(c - 5)) * 0.6f) * env * 0.16f;
            });
            // Bass: a soft pulse on every beat.
            var bass = SfxSynth.Clip("music_bass", LoopSeconds, t =>
            {
                int c = ChordAt(t);
                float beat = t % 0.5f;
                return SfxSynth.Sine(t, Freq(c - 24)) * SfxSynth.Env(beat, 0.01f, 0.45f) * 0.35f;
            });
            // Arpeggio: plucked chord tones in eighths.
            var arp = SfxSynth.Clip("music_arp", LoopSeconds, t =>
            {
                int c = ChordAt(t);
                int step = (int)(t / 0.25f) % 4;
                int[] tones = { 0, third, 7, 12 };
                float local = t % 0.25f;
                return SfxSynth.Sine(t, Freq(c + tones[step])) * SfxSynth.Env(local, 0.003f, 0.22f) * 0.14f;
            });
            // Bells: sparse high notes.
            var bells = SfxSynth.Clip("music_bells", LoopSeconds, t =>
            {
                int c = ChordAt(t);
                float local = t % 1f;
                int which = (int)t % 3;
                float f = Freq(c + 24 + (which == 0 ? 0 : which == 1 ? 7 : third));
                return (SfxSynth.Sine(t, f) + SfxSynth.Sine(t, f * 2.01f) * 0.2f) * SfxSynth.Env(local, 0.002f, 0.9f) * 0.09f;
            });
            return new[] { pad, bass, arp, bells };
        }
    }
}
