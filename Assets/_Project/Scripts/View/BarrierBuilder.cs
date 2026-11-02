using System;
using System.Collections.Generic;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The yellow steel barrier around the yard (Ana Belge §2.5). Every edge between a yard cell and an
    /// outer boundary wall gets two rails and posts, all the same thickness, pushed just outside the edge into the wall
    /// cell so nothing reaches into a playable cell. Where two edges turn, the shifted rails meet on one corner post;
    /// where the barrier stops (at the sea, at an opening) a post closes it. Only the picture changes: the rules still see '#'.
    /// </summary>
    public static class BarrierBuilder
    {
        private const float PostRadius = 0.055f, RailRadius = 0.035f;
        private const float PostHeight = 0.48f, TopRail = 0.42f, LowRail = 0.22f;
        /// <summary>How far the barrier's centre line sits outside the cell edge.</summary>
        private const float Inset = PostRadius + 0.03f;

        // Grid corner points: vertex (i, j) is the top-left corner of cell (i, j).
        private struct Edge : IEquatable<Edge>
        {
            public readonly Vector2Int A, B; // A < B
            public Edge(Vector2Int a, Vector2Int b)
            {
                bool swap = a.x > b.x || (a.x == b.x && a.y > b.y);
                A = swap ? b : a;
                B = swap ? a : b;
            }
            public bool Horizontal => A.y == B.y;
            public bool Equals(Edge o) => A == o.A && B == o.B;
            public override bool Equals(object o) => o is Edge e && Equals(e);
            public override int GetHashCode() => A.GetHashCode() * 397 ^ B.GetHashCode();
        }

        public static void Build(GameState state, bool[] outer, Transform parent, WorldTheme theme)
        {
            var root = new GameObject("Barrier").transform;
            root.SetParent(parent, false);
            int w = state.Width, h = state.Height;
            Vector3 World(Vector2Int v) => new Vector3(v.x - 0.5f, 0f, h - 0.5f - v.y);

            // Each edge remembers which way the wall is (world direction), so the barrier can sit on the wall side.
            var edges = new Dictionary<Edge, Vector3>();
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!BoundaryMap.IsYard(state.GetTile(new GridPos(x, y)))) continue;
                bool Wall(int nx, int ny) => nx >= 0 && ny >= 0 && nx < w && ny < h && outer[ny * w + nx];
                if (Wall(x, y - 1)) edges[new Edge(new Vector2Int(x, y), new Vector2Int(x + 1, y))] = Vector3.forward;
                if (Wall(x, y + 1)) edges[new Edge(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1))] = Vector3.back;
                if (Wall(x - 1, y)) edges[new Edge(new Vector2Int(x, y), new Vector2Int(x, y + 1))] = Vector3.left;
                if (Wall(x + 1, y)) edges[new Edge(new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1))] = Vector3.right;
            }

            // The truck backs up to the yard (Ana Belge §5.5): no barrier across its cargo bed, only as wide as the bed.
            foreach (var bay in state.Bays)
            {
                var outward = TruckView.Outward(bay, c => new Vector3(c.X, 0f, h - 1 - c.Y), w, h);
                var open = new List<Edge>();
                foreach (var kv in edges)
                {
                    if (kv.Value != outward) continue;
                    foreach (var c in bay.Cells)
                    {
                        // The edge on the truck's side of a bay cell.
                        var side = outward == Vector3.forward ? new Edge(new Vector2Int(c.X, c.Y), new Vector2Int(c.X + 1, c.Y))
                            : outward == Vector3.back ? new Edge(new Vector2Int(c.X, c.Y + 1), new Vector2Int(c.X + 1, c.Y + 1))
                            : outward == Vector3.left ? new Edge(new Vector2Int(c.X, c.Y), new Vector2Int(c.X, c.Y + 1))
                            : new Edge(new Vector2Int(c.X + 1, c.Y), new Vector2Int(c.X + 1, c.Y + 1));
                        if (kv.Key.Equals(side)) open.Add(kv.Key);
                    }
                }
                foreach (var e in open) edges.Remove(e);
            }

            var at = new Dictionary<Vector2Int, List<Edge>>();
            foreach (var e in edges.Keys)
            {
                if (!at.TryGetValue(e.A, out var la)) at[e.A] = la = new List<Edge>();
                if (!at.TryGetValue(e.B, out var lb)) at[e.B] = lb = new List<Edge>();
                la.Add(e);
                lb.Add(e);
            }

            // Where an edge's shifted line ends at a grid point: on the corner post if a perpendicular edge meets it there.
            Vector3 End(Edge e, Vector2Int v)
            {
                var p = World(v) + edges[e] * Inset;
                foreach (var other in at[v])
                    if (other.Horizontal != e.Horizontal) { p += edges[other] * Inset; break; }
                return p;
            }

            var yellow = MaterialLibrary.Shared(new Color(0.96f, 0.74f, 0.16f), 0.45f, 0.25f);
            var band = MaterialLibrary.Shared(new Color(0.13f, 0.13f, 0.14f), 0.3f);
            var posts = new HashSet<Vector2Int>();
            void Post(Vector3 p)
            {
                var key = new Vector2Int(Mathf.RoundToInt(p.x * 100f), Mathf.RoundToInt(p.z * 100f));
                if (!posts.Add(key)) return;
                Shapes.Make(PrimitiveType.Cylinder, root, p + Vector3.up * PostHeight / 2f, new Vector3(PostRadius * 2f, PostHeight / 2f, PostRadius * 2f), yellow, "Post");
                Shapes.Make(PrimitiveType.Cylinder, root, p + Vector3.up * 0.06f, new Vector3(PostRadius * 2.1f, 0.025f, PostRadius * 2.1f), band, "PostBand");
                Shapes.SmallSphere(root, p + Vector3.up * PostHeight, Vector3.one * PostRadius * 2f, yellow, "PostCap");
            }

            foreach (var e in edges.Keys)
            {
                var a = End(e, e.A);
                var b = End(e, e.B);
                Post(a);
                Post(b);
                Post((a + b) / 2f);
                Rail(root, a, b, TopRail, yellow);
                Rail(root, a, b, LowRail, yellow);
            }
            MeshMerger.Merge(root);
        }

        private static void Rail(Transform root, Vector3 a, Vector3 b, float height, Material mat)
        {
            var dir = b - a;
            float len = dir.magnitude;
            if (len < 0.01f) return;
            var rail = Shapes.Make(PrimitiveType.Cylinder, root, (a + b) / 2f + Vector3.up * height, new Vector3(RailRadius * 2f, len / 2f, RailRadius * 2f), mat, "Rail");
            rail.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir / len);
        }
    }
}
