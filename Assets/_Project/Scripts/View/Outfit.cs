using CrateBoss.Meta;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// What the worker wears (Ana Belge §13.2 wardrobe), read from the save when a yard is built and drawn by
    /// <see cref="OutfitDresser"/>. Null = the model's own look.
    /// </summary>
    public static class Outfit
    {
        public static Color? Hat { get; private set; }
        /// <summary>The hat is the flat cap, not a hard hat.</summary>
        public static bool Cap { get; private set; }
        public static Color? Vest { get; private set; }
        public static Color? Gloves { get; private set; }
        public static Color? CrateSkin { get; private set; }

        public static void Set(SaveData data)
        {
            // Owner 2026-11-02: the wardrobe is gone; the worker always wears his own clothes (bought pieces are ignored).
            Hat = Vest = Gloves = null;
            CrateSkin = null;
            Cap = false;
        }

        private static Color? ColorOf(SaveData data, string slot)
        {
            var p = data != null ? WardrobeCatalog.Worn(data, slot) : null;
            return p != null && ColorUtility.TryParseHtmlString(p.Color, out var c) ? c : (Color?)null;
        }
    }
}
