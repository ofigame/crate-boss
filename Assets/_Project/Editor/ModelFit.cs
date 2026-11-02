namespace CrateBoss.EditorTools
{
    /// <summary>How a model is scaled. Always uniform: a model is never stretched in one direction (Ana Belge §19).</summary>
    public enum ModelFit
    {
        /// <summary>The longest side of the footprint becomes Target.</summary>
        Base,
        /// <summary>The height becomes Target.</summary>
        Height,
        /// <summary>The same scale as another model (barrier pieces line up with the straight piece).</summary>
        SameScale,
        /// <summary>Tower crane: the jib's underside ends up Target above the tower foot.</summary>
        CraneJib,
    }
}
