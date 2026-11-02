namespace CrateBoss.Puzzle
{
    /// <summary>How the colour crates stand (brief §4 "PATLATMA YOĞUNLUĞU").</summary>
    public sealed class CandyMeasures
    {
        public int Candy, Pushable;
        /// <summary>Colour crates per pushable crate.</summary>
        public double Share;
        /// <summary>Colour crates with no other colour crate within 2 cells.</summary>
        public int Lonely;
        /// <summary>Touching groups of 4 or more colour crates, and how many of them mix 3 colours.</summary>
        public int Clusters, MixedClusters;
        /// <summary>Colours with fewer than 3 crates.</summary>
        public int ThinColours;
        /// <summary>A line of three ready at the start.</summary>
        public bool ReadyTriple;
        /// <summary>Triples whose last crate stands right beside its place at the start.</summary>
        public int NextToPlace;
    }
}
