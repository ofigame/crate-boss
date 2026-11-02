using System;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// What lies under the yard in the city look (Ana Belge §2.5): no pedestal, no earth layers. The yard sits on the
    /// district's ground; only where the map is open below do we draw something. In the port the front edge is a concrete
    /// quay wall going down into the sea, with rubber fenders; elsewhere a void cell is a deep loading pit with dark
    /// concrete sides and a yellow-black hazard edge.
    /// </summary>
    public static class CitySiteBuilder
    {
        private const float PitDepth = 3f;

        public static void Build(GameState state, Transform parent, WorldTheme theme, Func<GridPos, Vector3> toWorld)
        {
            var root = new GameObject("Site").transform;
            root.SetParent(parent, false);
            var concrete = MaterialLibrary.Shared(theme.Sidewalk * 0.82f, 0.1f);
            var dark = MaterialLibrary.Shared(new Color(0.16f, 0.17f, 0.19f), 0.05f);
            var hazardYellow = MaterialLibrary.Shared(new Color(0.95f, 0.76f, 0.12f), 0.3f);
            var hazardBlack = MaterialLibrary.Shared(new Color(0.12f, 0.12f, 0.13f), 0.3f);
            for (int y = 0; y < state.Height; y++)
            for (int x = 0; x < state.Width; x++)
            {
                var cell = new GridPos(x, y);
                if (state.GetTile(cell) != TileType.Void) continue;
                var p = toWorld(cell);
                for (int d = 0; d < 4; d++)
                {
                    var step = d == 0 ? new GridPos(0, -1) : d == 1 ? new GridPos(0, 1) : d == 2 ? new GridPos(-1, 0) : new GridPos(1, 0);
                    var n = cell + step;
                    if (state.GetTile(n) == TileType.Void) continue; // open on that side too
                    // Wall of the neighbour cell facing into the void.
                    var dir = toWorld(n) - p;
                    var center = p + dir * 0.5f;
                    var along = new Vector3(Mathf.Abs(dir.z), 0, Mathf.Abs(dir.x));
                    float depth = theme.Sea ? -theme.BelowHeight + 0.4f : PitDepth;
                    Shapes.Cube(root, center + Vector3.down * (depth / 2f + 0.02f) - dir * 0.02f, along * 1f + Vector3.up * depth + dir * 0.04f, theme.Sea ? concrete : dark, theme.Sea ? "QuayWall" : "PitWall");
                    if (!theme.Sea)
                    {
                        // Hazard edge: alternating yellow / black blocks along the rim.
                        for (int i = 0; i < 4; i++)
                        {
                            var at = center - dir * 0.04f + along * (-0.375f + i * 0.25f) + Vector3.up * 0.01f;
                            Shapes.NoShadows(Shapes.Cube(root, at, along * 0.25f + Vector3.up * 0.03f + dir * 0.08f, i % 2 == 0 ? hazardYellow : hazardBlack, "Hazard"));
                        }
                    }
                }
                if (!theme.Sea)
                    Shapes.Cube(root, p + Vector3.down * PitDepth, new Vector3(1f, 0.05f, 1f), MaterialLibrary.Shared(new Color(0.06f, 0.06f, 0.07f), 0f), "PitFloor");
            }
            if (theme.Sea)
            {
                // The quay face under the front row of each column, down into the water.
                float depth = -theme.BelowHeight + 0.4f;
                for (int x = 0; x < state.Width; x++)
                {
                    int front = -1;
                    for (int y = state.Height - 1; y >= 0 && front < 0; y--)
                        if (state.GetTile(new GridPos(x, y)) != TileType.Void) front = y;
                    if (front < 0 || front < state.Height - 1) continue; // a void row in front already got its wall
                    var p = toWorld(new GridPos(x, front));
                    Shapes.Cube(root, p + new Vector3(0, -depth / 2f - 0.02f, -0.48f), new Vector3(1f, depth, 0.04f), concrete, "QuayWall");
                }
                Fenders(state, root, toWorld);
            }
        }

        // Port quay: black rubber fenders on the sea-facing (camera) side, one every other column.
        private static void Fenders(GameState state, Transform parent, Func<GridPos, Vector3> toWorld)
        {
            var rubber = MaterialLibrary.Shared(new Color(0.118f, 0.129f, 0.149f), 0.15f);
            for (int x = 0; x < state.Width; x += 2)
            {
                int front = -1;
                for (int y = state.Height - 1; y >= 0 && front < 0; y--)
                    if (state.GetTile(new GridPos(x, y)) != TileType.Void) front = y;
                if (front < 0) continue;
                var p = toWorld(new GridPos(x, front)) + new Vector3(0f, -0.45f, -0.53f);
                var fender = Shapes.Make(PrimitiveType.Cylinder, parent, p, new Vector3(0.42f, 0.09f, 0.42f), rubber, "Fender");
                fender.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }
}
