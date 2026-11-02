using CrateBoss.Localization;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The look of one world (Ana Belge §2). Rule: things you can push are warm or saturated,
    /// things you cannot (container stacks, concrete) are cool and muted.
    /// </summary>
    public sealed class WorldTheme
    {
        public string Id;
        public string Name;
        /// <summary>Name in the player's language ("world.&lt;id&gt;"; the Turkish <see cref="Name"/> is the source).</summary>
        public string DisplayName => Loc.T("world." + Id, Name);
        public Color SkyTop, SkyBottom;
        public Color AmbientSky, AmbientEquator, AmbientGround;
        public float SunTemperature = 5200f;
        public float SunIntensity = 1.3f;
        public Color FloorLight, FloorDark;
        public Color Wall, WallTop;
        /// <summary>Walls drawn as corrugated container blocks in these mixed tints (null = plain blocks).</summary>
        public Color[] WallTints;
        public Color PedestalTop, PedestalBottom;
        public Color Below;            // the ground far under the board, or the sea
        public float BelowHeight = -4.5f;
        /// <summary>Port: the board is a pier standing in the sea, with rubber fenders on its front.</summary>
        public bool Sea;
        public Color SeaDeep;
        public Color Silhouette;
        public SilhouetteKind Silhouettes;
        public AmbientParticleKind Particles;
        public Color ParticleColor;
        public bool Lanterns;
        // Colour grading (stands in for a per-world LUT).
        public float Saturation, Contrast, PostExposure;
        public Color ColorFilter = Color.white;

        // Real places (Ana Belge §2.5): the yard sits level with the district's ground, fenced by a yellow barrier.
        public DistrictKind District;
        /// <summary>Pavement around the yard and under the outer walls.</summary>
        public Color Sidewalk;
        public Color Road, RoadLine;
        /// <summary>Painted lines on the yard floor (yellow in the city and port, white on the apron).</summary>
        public Color YardLine;
        /// <summary>Inner obstacles: matt, cool tones so they never read as pushable crates.</summary>
        public Color[] ObstacleTints;
        public Color Grass;
    }
}
