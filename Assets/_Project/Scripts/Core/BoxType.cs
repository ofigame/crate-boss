namespace CrateBoss.Core
{
    public enum BoxType
    {
        Normal,
        Ice,
        Heavy,
        /// <summary>Dropped by the crane only: cannot be pushed and never counts as covering a target.</summary>
        Container,
        /// <summary>D: delivery crate (Ana Belge §5.1). Pushed like a normal crate; never matches; the deliver goal wants it on its area.</summary>
        Delivery,
    }
}
