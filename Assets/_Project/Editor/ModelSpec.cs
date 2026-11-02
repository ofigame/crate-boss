namespace CrateBoss.EditorTools
{
    /// <summary>One static Tripo model: where it lives, what prefab key it becomes and how it is sized (Ana Belge §19).</summary>
    public sealed class ModelSpec
    {
        public string Name;        // folder and file name under Art/Models
        public string Key;         // Resources/Models/<Key>.prefab, read by VisualCatalog
        public ModelFit Fit;
        public float Target;       // size for the fit (units, 1 = one cell)
        public string ScaleFrom;   // ModelFit.SameScale: use this model's scale
        public ModelOrient Orient;
        public bool PivotTop;      // origin at the top centre (things that hang) instead of the bottom centre
        public int TextureSize;
        public float Alpha = 1f;   // < 1 = transparent surface
        public int MaxTriangles;   // budget from the doc, for the report
    }
}
