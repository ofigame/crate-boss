namespace CrateBoss.EditorTools
{
    /// <summary>How a model is turned before it is measured, found from its own vertices.</summary>
    public enum ModelOrient
    {
        None,
        /// <summary>Plates: the thinnest axis points up, the more detailed face on top.</summary>
        ThinAxisUp,
        /// <summary>Forklift: the low, thin end (the forks) points along +Z.</summary>
        LowEndForward,
        /// <summary>Barrier: the longest footprint side runs along X.</summary>
        LongAlongX,
        /// <summary>Tower crane: the tower stands on the origin, the jib points along +Z.</summary>
        CraneJib,
        /// <summary>L-shaped barrier corner: the joint at (-x, -z), legs along +X and +Z.</summary>
        LCorner,
        /// <summary>Barrier end post: the rail stubs point along +X, the post's centre on the origin.</summary>
        EndPost,
        /// <summary>Ice crate: its snowflake is painted on a side (X) face; turned 90° about Z so it faces up.</summary>
        SideDecalUp,
    }
}
