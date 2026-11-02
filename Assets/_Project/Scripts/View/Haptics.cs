using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Vibration in a few strengths. Android uses the system vibrator with a length and amplitude;
    /// iOS has no light taps without a plugin, so only strong ones vibrate there.
    /// </summary>
    public static class Haptics
    {
        public static bool Enabled = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject vibrator;
        private static int sdk;
#endif

        public static void Play(HapticStrength strength)
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (vibrator == null)
                {
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                        vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    using (var version = new AndroidJavaClass("android.os.Build$VERSION")) sdk = version.GetStatic<int>("SDK_INT");
                }
                if (strength == HapticStrength.Success) { OneShot(20, 120); OneShot(40, 255); return; }
                long ms = strength == HapticStrength.Light ? 12 : strength == HapticStrength.Medium ? 25 : 45;
                int amplitude = strength == HapticStrength.Light ? 80 : strength == HapticStrength.Medium ? 160 : 255;
                OneShot(ms, amplitude);
            }
            catch (System.Exception) { Enabled = false; }
#elif UNITY_IOS && !UNITY_EDITOR
            if (strength == HapticStrength.Strong || strength == HapticStrength.Success) Handheld.Vibrate();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void OneShot(long ms, int amplitude)
        {
            if (sdk >= 26)
            {
                using (var effect = new AndroidJavaClass("android.os.VibrationEffect"))
                using (var e = effect.CallStatic<AndroidJavaObject>("createOneShot", ms, amplitude))
                    vibrator.Call("vibrate", e);
            }
            else vibrator.Call("vibrate", ms);
        }
#endif
    }
}
