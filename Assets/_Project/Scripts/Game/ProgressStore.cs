using System;
using UnityEngine;

namespace CrateBoss.Game
{
    /// <summary>Saved progress (PlayerPrefs): unlocked levels, best stars per level, daily free hints, sound.</summary>
    public static class ProgressStore
    {
        public const int DailyFreeHints = 3;

        private const string UnlockedKey = "crateboss.unlocked";
        private const string StarsKey = "crateboss.stars.";
        private const string HintDayKey = "crateboss.hint.day";
        private const string HintUsedKey = "crateboss.hint.used";
        private const string SoundKey = "crateboss.sound";

        private const string TutorialKey = "crateboss.tutorial.";

        /// <summary>True once the wordless tutorial of this object (push, fixed, crane) was shown, played or skipped.</summary>
        public static bool TutorialSeen(string kind) => PlayerPrefs.GetInt(TutorialKey + kind, 0) == 1;

        public static void MarkTutorial(string kind)
        {
            PlayerPrefs.SetInt(TutorialKey + kind, 1);
            PlayerPrefs.Save();
        }

        public static int Unlocked => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedKey, 1));

        public static void Unlock(int count)
        {
            if (count <= Unlocked) return;
            PlayerPrefs.SetInt(UnlockedKey, count);
            PlayerPrefs.Save();
        }

        public static int GetStars(int levelIndex) => PlayerPrefs.GetInt(StarsKey + levelIndex, 0);

        public static void SetStars(int levelIndex, int stars)
        {
            if (stars <= GetStars(levelIndex)) return;
            PlayerPrefs.SetInt(StarsKey + levelIndex, stars);
            PlayerPrefs.Save();
        }

        /// <summary>Free hints left today (the count resets every day).</summary>
        public static int HintsLeft
        {
            get
            {
                if (PlayerPrefs.GetString(HintDayKey, "") != Today) return DailyFreeHints;
                return Mathf.Max(0, DailyFreeHints - PlayerPrefs.GetInt(HintUsedKey, 0));
            }
        }

        public static bool UseHint()
        {
            if (HintsLeft <= 0) return false;
            int used = PlayerPrefs.GetString(HintDayKey, "") == Today ? PlayerPrefs.GetInt(HintUsedKey, 0) : 0;
            PlayerPrefs.SetString(HintDayKey, Today);
            PlayerPrefs.SetInt(HintUsedKey, used + 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool SoundOn
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set { PlayerPrefs.SetInt(SoundKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        private static string Today => DateTime.Now.ToString("yyyyMMdd");
    }
}
