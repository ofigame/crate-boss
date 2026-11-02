using NUnit.Framework;
using CrateBoss.View;
using UnityEngine;

namespace CrateBoss.Tests
{
    /// <summary>Readability on a phone (Ana Belge §3.1 "Görünüm").</summary>
    public class FramingTests
    {
        [Test]
        public void LargestAllowedYard_CellIsAtLeast24PixelsWide_On390pxScreen()
        {
            var go = new GameObject("TestCamera");
            try
            {
                var cam = go.AddComponent<Camera>();
                cam.aspect = 390f / 844f; // reference phone in portrait
                var framer = go.AddComponent<CameraFramer>();
                framer.SetInsets(0.16f, 0.2f);
                // Largest yard the doc allows: 8 × 11 playable cells.
                framer.Frame(-0.45f, 7.45f, -0.45f, 10.45f); // + the barrier allowance GameController adds
                float px = framer.CellPixels(390f);
                TestContext.WriteLine($"8 x 11 yard: one cell = {px:0.0} px on a 390 px wide screen");
                // Owner 2026-10-31: diagonal camera (yaw 25°) so the city shows; the largest yard then gets 24 px per cell, accepted.
                Assert.GreaterOrEqual(px, 24f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
