using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Sound and vibration for every event (Ana Belge §2). Clips are made in code as placeholders;
    /// assign real AudioClips to the override fields and they are used instead.
    /// </summary>
    public class GameFeedback : MonoBehaviour
    {
        // A pentatonic scale: each newly filled target plays the next note.
        private static readonly float[] Scale = { 523.25f, 587.33f, 659.25f, 783.99f, 880f, 1046.5f, 1174.7f, 1318.5f };

        public AudioClip stepClip, woodClip, iceSlideClip, iceStopClip, heavyClip, fallClip, settleClip, bumpClip, starClip, clickClip;

        private readonly Dictionary<SoundId, AudioClip> clips = new Dictionary<SoundId, AudioClip>();
        private readonly Dictionary<int, AudioClip> notes = new Dictionary<int, AudioClip>();
        private AudioClip chord;
        private AudioSource source;

        public bool SoundOn { get; set; } = true;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        public void Step() => Play(SoundId.Step, stepClip, 0.6f);
        public void WoodPush() { Play(SoundId.WoodPush, woodClip); Haptics.Play(HapticStrength.Light); }
        public void IceSlide() => Play(SoundId.IceSlide, iceSlideClip);
        public void IceStop() { Play(SoundId.IceStop, iceStopClip); Haptics.Play(HapticStrength.Medium); }
        public void HeavyPush() { Play(SoundId.HeavyPush, heavyClip); Haptics.Play(HapticStrength.Strong); }
        public void Fall() { Play(SoundId.Fall, fallClip); Haptics.Play(HapticStrength.Medium); }
        public void Settle() => Play(SoundId.Settle, settleClip);
        public void Bump() { Play(SoundId.Bump, bumpClip); Haptics.Play(HapticStrength.Light); }
        public void Click() => Play(SoundId.Click, clickClip);
        public void CraneMove() => Play(SoundId.CraneMotor, null, 0.5f);
        public void CraneWarn() => Play(SoundId.CraneBeep, null);
        public void CraneAlarm() { Play(SoundId.CraneBeepDouble, null); Haptics.Play(HapticStrength.Light); }
        public void CraneOff() => Play(SoundId.CraneOff, null, 0.6f);
        public void CraneRewind() => Play(SoundId.CraneRewind, null);
        public void Pop() { Play(SoundId.Pop, null, 0.8f); Haptics.Play(HapticStrength.Light); }
        public void Boing() => Play(SoundId.Boing, null, 0.7f);

        /// <summary>A match (Ana Belge §16): three rising notes, then a pop. Longer rows climb higher.</summary>
        public System.Collections.IEnumerator MatchNotes(int crates)
        {
            for (int i = 0; i < 3; i++)
            {
                TargetLocked(Mathf.Min(Scale.Length - 1, 2 + i + (crates - 3)));
                yield return new WaitForSeconds(0.07f);
            }
            Pop();
        }
        public void Horn() => Play(SoundId.Horn, null, 0.7f);

        /// <summary>A crane drop lands: metal boom for a container, a wooden knock for a crate.</summary>
        public void CraneLanded(Core.BoxType type)
        {
            if (type == Core.BoxType.Container || type == Core.BoxType.Heavy) Play(SoundId.ContainerThud, heavyClip);
            else Play(SoundId.WoodPush, woodClip);
            Haptics.Play(HapticStrength.Medium);
        }

        public void Star(int index) => PlayClip(starClip != null ? starClip : Get(SoundId.Star), 1f, 1f + index * 0.12f);

        /// <summary>The n-th filled target (0-based) plays the n-th note of the scale.</summary>
        public void TargetLocked(int index)
        {
            int i = Mathf.Clamp(index, 0, Scale.Length - 1);
            if (!notes.TryGetValue(i, out var clip)) notes[i] = clip = SfxSynth.Note(Scale[i]);
            PlayClip(clip);
            Haptics.Play(HapticStrength.Light);
        }

        /// <summary>The notes come together as a chord.</summary>
        public void Win()
        {
            if (chord == null) chord = SfxSynth.Chord(new[] { Scale[0], Scale[2], Scale[3], Scale[5] });
            PlayClip(chord);
            Haptics.Play(HapticStrength.Success);
        }

        private AudioClip failClip;

        /// <summary>Out of moves: a short falling pair of notes.</summary>
        public void Fail()
        {
            if (failClip == null) failClip = SfxSynth.Chord(new[] { Scale[2] * 0.5f, Scale[1] * 0.5f }, 0.9f);
            PlayClip(failClip, 0.8f);
            Haptics.Play(HapticStrength.Medium);
        }

        private void Play(SoundId id, AudioClip custom, float volume = 1f) => PlayClip(custom != null ? custom : Get(id), volume);

        private AudioClip Get(SoundId id)
        {
            if (!clips.TryGetValue(id, out var clip)) clips[id] = clip = SfxSynth.Make(id);
            return clip;
        }

        private void PlayClip(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (!SoundOn || clip == null) return;
            source.pitch = pitch;
            source.PlayOneShot(clip, volume);
        }
    }
}
