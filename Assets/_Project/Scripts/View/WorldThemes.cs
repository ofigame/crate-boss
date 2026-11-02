using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The one look of the game: the toy city (Ana Belge §2). There are no districts or worlds any more (§20); ranks only
    /// change the crates and obstacles (§7).
    /// </summary>
    public static class WorldThemes
    {
        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        private static readonly Color[] CityObstacles = { Hex("#7D8790"), Hex("#6F7B84"), Hex("#8A9399"), Hex("#5F6B73") };

        private static readonly WorldTheme City = new WorldTheme
        {
            Id = "sehir_deposu",
            Name = "Şehir Deposu",
            District = DistrictKind.City,
            SkyTop = Hex("#9CCBEA"),
            SkyBottom = Hex("#F6E6CC"),
            AmbientSky = new Color(0.76f, 0.84f, 0.94f),
            AmbientEquator = new Color(0.68f, 0.70f, 0.70f),
            AmbientGround = new Color(0.48f, 0.50f, 0.46f),
            SunTemperature = 4600f,
            SunIntensity = 1.45f,
            FloorLight = Hex("#D3D6D8"),
            FloorDark = Hex("#C8CCCF"),
            Wall = Hex("#7D8790"),
            WallTop = Hex("#69737B"),
            PedestalTop = Hex("#9AA0A5"),
            PedestalBottom = Hex("#5D646B"),
            Below = Hex("#3A3F45"),
            Silhouette = new Color(0.80f, 0.87f, 0.93f),
            Silhouettes = SilhouetteKind.None,
            Particles = AmbientParticleKind.Dust,
            ParticleColor = new Color(1f, 0.98f, 0.92f, 0.45f),
            Saturation = 8f, Contrast = 6f, PostExposure = 0.05f,
            ColorFilter = new Color(1f, 0.99f, 0.98f),
            Sidewalk = Hex("#C4BBAE"),
            Road = Hex("#4C5258"),
            RoadLine = Hex("#F2F2EA"),
            YardLine = Hex("#F2C232"),
            ObstacleTints = CityObstacles,
            Grass = Hex("#7DB65A"),
        };

        /// <summary>The city in the toy palette, the look of every level and of the home screen.</summary>
        public static WorldTheme Current => Toy(City);

        // ---------- toy look (Ana Belge §2.2) ----------

        private static readonly System.Collections.Generic.HashSet<WorldTheme> toyed = new System.Collections.Generic.HashSet<WorldTheme>();

        // Candy tones for the one-cell obstacles (Ana Belge §2.3).
        private static readonly Color[] CandyTints = { Hex("#F2558A"), Hex("#9B5DE5"), Hex("#3EC1D3"), Hex("#FFD23F"), Hex("#FF8C42"), Hex("#5AA9F0") };

        /// <summary>
        /// The city in the toy palette: clean light-grey city ground and parking-lot yard with white lines, dark-blue
        /// asphalt with white dashes, pale sky, bright soft low-contrast light, a little more saturation. Done once.
        /// </summary>
        private static WorldTheme Toy(WorldTheme t)
        {
            if (!toyed.Add(t)) return t;
            t.FloorLight = Hex("#ECEEF1");
            t.FloorDark = Hex("#E3E6EA");
            t.YardLine = Hex("#FFFFFF");
            t.Sidewalk = Hex("#E4E6EA");
            t.Road = Hex("#4E5462");
            t.RoadLine = Hex("#FFFFFF");
            // The city ground is light grey, not grass.
            t.Grass = Hex("#D6D9DE");
            t.SkyTop = Hex("#DDEBF5");
            t.SkyBottom = Hex("#F5F8FA");
            t.AmbientSky = Hex("#DDEBF5");
            t.AmbientEquator = Hex("#E4E6EA") * 0.92f;
            t.AmbientGround = Hex("#C9CDD3") * 0.8f;
            t.SunTemperature = 5600f;
            t.SunIntensity = 1.2f;
            t.Saturation = 10f;
            t.Contrast = -6f;
            t.PostExposure = 0.08f;
            t.ColorFilter = Color.white;
            t.ObstacleTints = CandyTints;
            return t;
        }
    }
}
