using UnityEngine;

namespace CrateBoss.UI
{
    /// <summary>
    /// Rounded bold title font + readable body font, both with Turkish letters (Nunito, OFL licence).
    /// Arabic and Korean fallbacks are set on the font importers by Tools > Crate Boss > Setup Project.
    /// </summary>
    public static class UiFonts
    {
        private static Font title, body;

        public static Font Title => title ??= Load("Fonts/Nunito-ExtraBold");
        public static Font Body => body ??= Load("Fonts/Nunito-SemiBold");

        private static Font Load(string path)
        {
            var f = Resources.Load<Font>(path);
            return f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
