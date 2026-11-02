using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Colours that do not change between worlds. Pushable things are warm or saturated (Ana Belge §2);
    /// world colours (floor, walls, pedestal) live in WorldTheme.
    /// </summary>
    public static class Palette
    {
        public static readonly Color Target = new Color(0.35f, 0.95f, 0.70f);
        public static readonly Color TargetDone = new Color(1.00f, 0.78f, 0.25f);
        public static readonly Color TargetPlate = new Color(0.55f, 0.58f, 0.60f);
        public static readonly Color TargetHollow = new Color(0.16f, 0.20f, 0.22f);
        public static readonly Color Pit = new Color(0.03f, 0.03f, 0.05f);
        public static readonly Color PitWall = new Color(0.13f, 0.12f, 0.14f);
        public static readonly Color RimStone = new Color(0.46f, 0.44f, 0.44f);

        public static readonly Color Wood = new Color(0.93f, 0.60f, 0.26f);
        public static readonly Color WoodDark = new Color(0.72f, 0.41f, 0.17f);
        public static readonly Color WoodScuff = new Color(0.98f, 0.76f, 0.48f);
        public static readonly Color Ice = new Color(0.16f, 0.80f, 0.90f, 0.72f);
        public static readonly Color IceCore = new Color(0.85f, 0.98f, 1.00f, 0.45f);
        public static readonly Color Frost = new Color(0.95f, 0.99f, 1.00f, 0.9f);
        public static readonly Color IceReflection = new Color(0.25f, 0.75f, 1.00f, 0.35f);
        public static readonly Color Iron = new Color(0.20f, 0.21f, 0.24f);
        public static readonly Color IronEdge = new Color(0.32f, 0.33f, 0.36f);
        public static readonly Color Hazard = new Color(1.00f, 0.80f, 0.08f);
        public static readonly Color HazardDark = new Color(0.08f, 0.08f, 0.09f);
        public static readonly Color Rust = new Color(0.55f, 0.28f, 0.14f);
        // Crane container (Ana Belge §10): cold steel blue, same family as the container walls ("this is a wall now").
        public static readonly Color Container = new Color(0.435f, 0.561f, 0.659f);
        public static readonly Color ContainerDark = new Color(0.204f, 0.298f, 0.376f);
        public static readonly Color CraneYellow = new Color(1.00f, 0.79f, 0.24f);
        public static readonly Color CraneYellowDark = new Color(0.88f, 0.60f, 0.07f);
        public static readonly Color CraneWarning = new Color(1.00f, 0.416f, 0.239f);
        public static readonly Color CraneAlarm = new Color(0.95f, 0.18f, 0.16f);
        public static readonly Color Crack = new Color(0.15f, 0.12f, 0.10f, 0.6f);

        public static readonly Color Rubber = new Color(0.12f, 0.12f, 0.14f);
        public static readonly Color AntennaTip = new Color(1.00f, 0.30f, 0.35f);

        public static Color ForBox(BoxType type)
        {
            switch (type)
            {
                case BoxType.Ice: return Ice;
                case BoxType.Heavy: return Iron;
                case BoxType.Container: return Container;
                default: return Wood;
            }
        }
    }
}
