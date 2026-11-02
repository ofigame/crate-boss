using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Sits on a placeholder environment prefab: builds the simple-shape stand-in when the piece is spawned.
    /// A real model prefab has no such component and is used as it is.
    /// </summary>
    public class EnvPlaceholder : MonoBehaviour
    {
        public DistrictKind district;
        public EnvPiece piece;

        public void Build(int variant) => EnvShapes.Build(district, piece, transform, variant);
    }
}
