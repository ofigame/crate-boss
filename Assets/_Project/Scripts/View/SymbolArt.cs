using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Match crate colours: the owner's three models in their own colours (2026-11-01): 1 red, 2 turquoise, 3 white. The
    /// levels use only these; 4–6 stay for old files. Colour alone
    /// tells the kinds apart (owner 2026-10-31), so the tones are kept far apart in hue.
    /// </summary>
    public static class SymbolArt
    {
        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        private static readonly Color[] Colors =
        {
            Color.white, Hex("#EF4444"), Hex("#3EC1D3"), Hex("#F7F7F9"), Hex("#FFD23F"), Hex("#9B5DE5"), Hex("#F2558A"),
            Hex("#2B2D42"), // 7: the rainbow crate, a colour no other crate has (owner 2026-11-02)
        };

        public static Color ColorOf(int symbol) => symbol >= 1 && symbol < Colors.Length ? Colors[symbol] : Color.white;
    }
}
