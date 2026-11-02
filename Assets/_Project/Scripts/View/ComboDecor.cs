using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// What marks a combo colour crate (owner 2026-11-02), one icon on the lid: a red-and-white striped candy stick lying
    /// along the row or column it clears, a black bomb with a lit fuse, or a ball ringed with every colour (rainbow, on a crate
    /// of the colour that made it).
    /// </summary>
    public static class ComboDecor
    {
        private static Material white, black, fuse, spark, red;

        /// <summary>
        /// The colour model a combo crate is built from; a rainbow has a colour of its own, unlike every other crate (owner
        /// 2026-11-02: "hepsinden farklı olsun, rengi de farklı olsun").
        /// </summary>
        public static int ModelSymbol(Box box) => box.Special == CandySpecial.Rainbow || box.Symbol == CandySystem.RainbowSymbol ? CandySystem.RainbowSymbol : box.Look > 0 ? box.Look : box.Symbol;

        public static void Apply(Transform crate, CandySpecial kind)
        {
            white ??= MaterialLibrary.Create(new Color(1f, 1f, 1f), 0.7f);
            black ??= MaterialLibrary.Create(new Color(0.12f, 0.12f, 0.15f), 0.6f);
            fuse ??= MaterialLibrary.Create(new Color(0.55f, 0.4f, 0.25f), 0.2f);
            spark ??= MaterialLibrary.Unlit(new Color(1f, 0.8f, 0.2f));
            red ??= MaterialLibrary.Create(new Color(0.93f, 0.25f, 0.25f), 0.6f);
            var root = new GameObject("Combo").transform;
            root.SetParent(crate, false);
            switch (kind)
            {
                case CandySpecial.StripeRow:
                case CandySpecial.StripeColumn:
                {
                    // One icon on the lid like the bomb (owner 2026-11-02): a striped candy stick lying along the line it clears.
                    var stick = new GameObject("Stick").transform;
                    stick.SetParent(root, false);
                    stick.localPosition = new Vector3(0f, 1.02f, 0f);
                    stick.localRotation = kind == CandySpecial.StripeRow ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(90f, 0f, 0f);
                    Shapes.Make(PrimitiveType.Capsule, stick, Vector3.zero, new Vector3(0.24f, 0.3f, 0.24f), white, "Candy");
                    foreach (var at in new[] { -0.17f, 0f, 0.17f })
                        Shapes.Make(PrimitiveType.Cylinder, stick, new Vector3(0f, at, 0f), new Vector3(0.26f, 0.025f, 0.26f), red, "Band");
                    break;
                }
                case CandySpecial.Bomb:
                    Shapes.Make(PrimitiveType.Sphere, root, new Vector3(0f, 1.05f, 0f), Vector3.one * 0.42f, black, "Bomb");
                    Shapes.Cube(root, new Vector3(0.08f, 1.3f, 0f), new Vector3(0.05f, 0.16f, 0.05f), fuse, "Fuse");
                    Shapes.Make(PrimitiveType.Sphere, root, new Vector3(0.1f, 1.4f, 0f), Vector3.one * 0.09f, spark, "Spark");
                    break;
                case CandySpecial.Rainbow:
                {
                    Shapes.Make(PrimitiveType.Sphere, root, new Vector3(0f, 1.05f, 0f), Vector3.one * 0.36f, white, "Ball");
                    Color[] all = { new Color(0.94f, 0.27f, 0.27f), new Color(1f, 0.82f, 0.25f), new Color(0.24f, 0.8f, 0.42f),
                        new Color(0.24f, 0.76f, 0.83f), new Color(0.61f, 0.36f, 0.9f), new Color(0.95f, 0.33f, 0.54f) };
                    for (int i = 0; i < all.Length; i++)
                    {
                        float a = i * Mathf.PI * 2f / all.Length;
                        Shapes.Make(PrimitiveType.Sphere, root, new Vector3(Mathf.Cos(a) * 0.22f, 1.05f, Mathf.Sin(a) * 0.22f), Vector3.one * 0.13f,
                            MaterialLibrary.Shared(all[i], 0.6f), "Dot");
                    }
                    break;
                }
            }
            foreach (var c in root.GetComponentsInChildren<Collider>()) Object.Destroy(c);
        }
    }
}
