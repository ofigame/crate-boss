using System.Collections;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>One ground cell. Floor top sits at y = 0.</summary>
    public class TileView : MonoBehaviour
    {
        private const float SlabHeight = 0.2f;
        private const float PitDepth = 0.75f;
        private const float FadedAlpha = 0.4f;
        private const float PlateDrop = 0.05f;

        private Material ringMaterial;
        private Material wallMaterial;
        private Transform plate;
        private Color wallColor;
        private float wallAlpha = 1f;
        private Coroutine fadeRoutine;

        public GridPos Cell { get; private set; }
        public TileType Type { get; private set; }
        public bool Occupied { get; private set; }
        public bool IsFrontWall { get; private set; }

        /// <summary>True: inner walls (fixed crates) never fade.</summary>
        public static bool FixedCratesStaySolid = true;

        /// <param name="wallHeight">Walls right in front of playable cells are half height (Ana Belge §2).</param>
        public void Build(TileType type, GridPos cell, bool darkChecker, WorldTheme theme, BoxType filledBy, float wallHeight, bool frontWall) =>
            Build(type, cell, darkChecker, theme, filledBy, wallHeight, frontWall, false, 0);

        /// <param name="outerWall">An outer boundary wall (Ana Belge §2.5): drawn as pavement, the yellow barrier runs along its edge.</param>
        /// <param name="seed">Level id: picks the obstacle kinds, the same every time the level opens.</param>
        public void Build(TileType type, GridPos cell, bool darkChecker, WorldTheme theme, BoxType filledBy, float wallHeight, bool frontWall,
            bool outerWall, int seed, ObstaclePiece piece = default)
        {
            Type = type;
            Cell = cell;
            IsFrontWall = frontWall && !outerWall;
            switch (type)
            {
                case TileType.Wall when outerWall:
                    MakePavement(theme);
                    break;
                case TileType.Wall when theme.ObstacleTints != null:
                    wallMaterial = ObstacleBuilder.Build(transform, theme, cell, seed, frontWall, piece, out wallColor);
                    break;
                case TileType.Wall:
                    BuildWall(theme, wallHeight, frontWall);
                    break;
                case TileType.Floor:
                    MakeSlab(theme, darkChecker);
                    break;
                case TileType.Target:
                    MakeSlab(theme, darkChecker);
                    MakePlate();
                    break;
                case TileType.Hole:
                    MakePit();
                    break;
                case TileType.FilledHole:
                    MakePit();
                    ShowSunkBox(filledBy, Cell.X * 7 + Cell.Y);
                    break;
                case TileType.Void:
                    break; // nothing is drawn
                default:
                    // Delivery area (Ana Belge §5.1): floor slab plus the painted bay.
                    MakeSlab(theme, darkChecker);
                    GoalTileBuilder.Build(transform, type, out areaLamp);
                    break;
            }
        }

        // ---------- delivery areas (Ana Belge §5.1) ----------

        private Material areaLamp;
        private static readonly Color AreaWait = new Color(1f, 0.82f, 0.25f), AreaDone = new Color(0.24f, 0.8f, 0.42f), AreaRefused = new Color(0.94f, 0.27f, 0.27f);

        /// <summary>The area's inner pad: amber while it waits, green once its crate is locked in.</summary>
        public void SetAreaDone(bool done)
        {
            if (areaLamp == null) return;
            var c = done ? AreaDone : AreaWait;
            MaterialLibrary.SetColor(areaLamp, c);
            MaterialLibrary.SetGlow(areaLamp, c * 0.6f);
        }

        /// <summary>A crate that may not be delivered here (wrong area, way stop missed): the pad flashes red twice.</summary>
        public IEnumerator FlashRefused()
        {
            if (areaLamp == null) yield break;
            var back = areaLamp.color;
            for (int i = 0; i < 2; i++)
            {
                MaterialLibrary.SetColor(areaLamp, AreaRefused);
                MaterialLibrary.SetGlow(areaLamp, AreaRefused * 0.8f);
                yield return new WaitForSeconds(0.14f);
                MaterialLibrary.SetColor(areaLamp, back);
                MaterialLibrary.SetGlow(areaLamp, back * 0.6f);
                yield return new WaitForSeconds(0.12f);
            }
        }

        // ---------- targets ----------

        public void SetOccupied(bool occupied)
        {
            Occupied = occupied;
            if (ringMaterial == null) return;
            var c = occupied ? Palette.TargetDone : Palette.Target;
            MaterialLibrary.SetColor(ringMaterial, c);
            MaterialLibrary.SetGlow(ringMaterial, c * 1.4f);
            if (plate != null) plate.localPosition = new Vector3(0, occupied ? -PlateDrop : 0f, 0);
        }

        /// <summary>Short bright flash of the target light.</summary>
        public IEnumerator Pulse(float duration = 0.3f)
        {
            if (ringMaterial == null) yield break;
            var baseColor = Occupied ? Palette.TargetDone : Palette.Target;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / duration * Mathf.PI);
                MaterialLibrary.SetGlow(ringMaterial, baseColor * (1.4f + 3f * k));
                yield return null;
            }
            MaterialLibrary.SetGlow(ringMaterial, baseColor * 1.4f);
        }

        /// <summary>A box lands: the plate clicks down and a golden light rises from it.</summary>
        public IEnumerator PlayLock()
        {
            SetOccupied(true);
            var beamMat = MaterialLibrary.Unlit(new Color(1f, 0.85f, 0.35f, 0.5f));
            var beam = Shapes.Make(PrimitiveType.Cylinder, transform, new Vector3(0, 0.5f, 0), new Vector3(0.7f, 0.5f, 0.7f), beamMat, "Beam");
            Shapes.NoShadows(beam);
            const float duration = 0.5f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                beam.transform.localScale = new Vector3(0.7f - 0.3f * k, 0.3f + 0.9f * k, 0.7f - 0.3f * k);
                beam.transform.localPosition = new Vector3(0, 0.3f + 0.9f * k, 0);
                MaterialLibrary.SetColor(beamMat, new Color(1f, 0.85f, 0.35f, 0.5f * (1f - k)));
                yield return null;
            }
            Destroy(beam);
        }

        // ---------- holes ----------

        /// <summary>The box is down in the hole now: settle wobble of the rim.</summary>
        public IEnumerator PlayFill()
        {
            Type = TileType.FilledHole;
            var start = transform.localPosition;
            const float duration = 0.22f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                transform.localPosition = start + Vector3.down * (Mathf.Sin(k * Mathf.PI * 3f) * (1f - k) * 0.04f);
                yield return null;
            }
            transform.localPosition = start;
        }

        // ---------- front wall fading ----------

        /// <summary>Front walls turn 40% see-through while something stands right behind them.</summary>
        public void SetOccluding(bool occluding)
        {
            // Owner 2026-10-31: the fixed crates (every inner wall now) stay solid even with the player right behind them.
            if (FixedCratesStaySolid) return;
            if (wallMaterial == null || !IsFrontWall) return;
            float target = occluding ? FadedAlpha : 1f;
            if (Mathf.Approximately(target, wallAlpha)) return;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeTo(target));
        }

        private IEnumerator FadeTo(float target)
        {
            float from = wallAlpha;
            for (float t = 0; t < 0.15f; t += Time.deltaTime)
            {
                SetWallAlpha(Mathf.Lerp(from, target, t / 0.15f));
                yield return null;
            }
            SetWallAlpha(target);
        }

        private void SetWallAlpha(float a)
        {
            wallAlpha = a;
            MaterialLibrary.SetColor(wallMaterial, new Color(wallColor.r, wallColor.g, wallColor.b, a));
        }

        // ---------- builders ----------

        private void BuildWall(WorldTheme theme, float height, bool front)
        {
            var custom = VisualCatalog.TryInstantiate(front ? $"WallHalf_{theme.Id}" : $"Wall_{theme.Id}", transform);
            if (custom != null) return;
            if (theme.WallTints != null && theme.WallTints.Length > 0)
            {
                BuildContainerWall(theme, height, front);
                return;
            }
            wallColor = theme.Wall;
            wallMaterial = front ? MaterialLibrary.CreateFadable(theme.Wall, 0.1f) : MaterialLibrary.Shared(theme.Wall, 0.1f);
            Shapes.Rounded(transform, new Vector3(0, (height - SlabHeight) / 2f, 0), new Vector3(1f, height + SlabHeight, 1f), wallMaterial, 0.07f, "Wall");
            if (!front)
                Shapes.Rounded(transform, new Vector3(0, height + 0.005f, 0), new Vector3(0.86f, 0.03f, 0.86f), MaterialLibrary.Shared(theme.WallTop, 0.1f), 0.012f, "WallTop");
        }

        // Container stack (Ana Belge §10): 0.82 tall (front row low), corrugated sides, mixed faded tints, dark top rail.
        private void BuildContainerWall(WorldTheme theme, float height, bool front)
        {
            int pick = Mathf.Abs(Cell.X * 7 + Cell.Y * 13 + (Cell.X * Cell.Y) % 5) % theme.WallTints.Length;
            wallColor = theme.WallTints[pick];
            float h = front ? height * 0.72f : 0.82f;
            wallMaterial = front ? MaterialLibrary.CreateFadable(wallColor, 0.2f) : MaterialLibrary.Shared(wallColor, 0.2f);
            MaterialLibrary.SetTexture(wallMaterial, CorrugatedTexture.Get());
            Shapes.Cube(transform, new Vector3(0, (h - SlabHeight) / 2f, 0), new Vector3(0.98f, h + SlabHeight, 0.98f), wallMaterial, "Container");
            var rail = MaterialLibrary.Shared(new Color(wallColor.r * 0.7f, wallColor.g * 0.7f, wallColor.b * 0.7f), 0.2f);
            if (!front)
            {
                Shapes.Cube(transform, new Vector3(0, h + 0.01f, 0.47f), new Vector3(0.98f, 0.04f, 0.04f), rail, "Rail");
                Shapes.Cube(transform, new Vector3(0, h + 0.01f, -0.47f), new Vector3(0.98f, 0.04f, 0.04f), rail, "Rail");
                Shapes.Cube(transform, new Vector3(0.47f, h + 0.01f, 0), new Vector3(0.04f, 0.04f, 0.98f), rail, "Rail");
                Shapes.Cube(transform, new Vector3(-0.47f, h + 0.01f, 0), new Vector3(0.04f, 0.04f, 0.98f), rail, "Rail");
            }
        }

        // Outer wall cells are pavement level with the district's ground, a little below the yard (a kerb).
        private void MakePavement(WorldTheme theme)
        {
            if (VisualCatalog.TryInstantiate($"Pavement_{theme.Id}", transform) != null) return;
            int v = Mathf.Abs(Cell.X * 31 + Cell.Y * 17) % 3;
            var c = theme.Sidewalk * (1f + (v - 1) * 0.02f);
            c.a = 1f;
            Shapes.Cube(transform, new Vector3(0, -0.03f - SlabHeight / 2f, 0), new Vector3(1f, SlabHeight, 1f), MaterialLibrary.Shared(c, 0.1f), "Pavement");
        }

        private void MakeSlab(WorldTheme theme, bool darkChecker)
        {
            if (VisualCatalog.TryInstantiate($"Floor_{theme.Id}", transform) != null) return;
            if (VisualCatalog.Floor == FloorStyle.Concrete)
            {
                // Depot concrete (Ana Belge §13): full-size flat cells meet without gaps; the texture paints the dashed cell lines.
                int variant = Mathf.Abs(Cell.X * 7 + Cell.Y * 13 + Cell.X * Cell.Y) % ConcreteTexture.Variants;
                Shapes.Cube(transform, new Vector3(0, -SlabHeight / 2f, 0), new Vector3(1f, SlabHeight, 1f), ConcreteFloor.Cell(theme, variant), "Floor");
                return;
            }
            // Small per-tile shade change (4 variations) so the floor does not look printed.
            int v = Mathf.Abs(Cell.X * 31 + Cell.Y * 17) % 4;
            float shade = 1f + (v - 1.5f) * 0.012f;
            var c = darkChecker ? theme.FloorDark : theme.FloorLight;
            c = new Color(c.r * shade, c.g * shade, c.b * shade);
            Shapes.Rounded(transform, new Vector3(0, -SlabHeight / 2f, 0), new Vector3(0.98f, SlabHeight, 0.98f), MaterialLibrary.Shared(c, 0.15f), 0.04f, "Floor");
        }

        private const float PlateTop = 0.03f;

        private void MakePlate()
        {
            plate = new GameObject("Plate").transform;
            plate.SetParent(transform, false);
            float ringY = 0.012f;
            var model = VisualCatalog.TryInstantiate("Target", plate);
            if (model != null && VisualCatalog.TryGetBounds("Target", out var b))
            {
                // The model is a thick block: sink it so only its top 3 cm stand out of the floor (doc: plate ≤ 0.06 high).
                model.transform.localPosition = new Vector3(0f, PlateTop - b.max.y, 0f);
                ringY = PlateTop + 0.004f;
            }
            else
            {
                Shapes.Rounded(plate, new Vector3(0, -0.02f, 0), new Vector3(0.8f, 0.06f, 0.8f), MaterialLibrary.Shared(Palette.TargetPlate, 0.5f, 0.3f), 0.03f, "Plate");
                Shapes.Cube(plate, new Vector3(0, 0.008f, 0), new Vector3(0.44f, 0.01f, 0.44f), MaterialLibrary.Shared(Palette.TargetHollow, 0.2f), "Hollow");
            }
            // The glowing ring keeps the empty / filled colour from code on top of either plate.
            ringMaterial = MaterialLibrary.Create(Palette.Target, 0.6f);
            const float size = 0.56f, bar = 0.06f, h = 0.02f;
            float off = size / 2f - bar / 2f;
            Shapes.Cube(plate, new Vector3(0, ringY, off), new Vector3(size, h, bar), ringMaterial);
            Shapes.Cube(plate, new Vector3(0, ringY, -off), new Vector3(size, h, bar), ringMaterial);
            Shapes.Cube(plate, new Vector3(off, ringY, 0), new Vector3(bar, h, size - 2 * bar), ringMaterial);
            Shapes.Cube(plate, new Vector3(-off, ringY, 0), new Vector3(bar, h, size - 2 * bar), ringMaterial);
            Shapes.Cube(plate, new Vector3(0, ringY, 0), new Vector3(0.14f, h, 0.14f), ringMaterial);
            SetOccupied(false);
        }

        // Deep well with broken stones around the edge, getting darker downward, a little mist inside.
        private void MakePit()
        {
            var pit = new GameObject("Pit").transform;
            pit.SetParent(transform, false);
            Shapes.Cube(pit, new Vector3(0, -PitDepth, 0), new Vector3(1f, 0.04f, 1f), MaterialLibrary.Shared(Palette.Pit, 0f), "Bottom");
            const float t = 0.05f;
            for (int band = 0; band < 3; band++)
            {
                float top = -band * PitDepth / 3f, h = PitDepth / 3f;
                float k = band / 2f;
                var c = Color.Lerp(Palette.PitWall, Palette.Pit, k);
                var mat = MaterialLibrary.Shared(c, 0.05f);
                float y = top - h / 2f;
                Shapes.Cube(pit, new Vector3(0, y, 0.5f - t / 2f), new Vector3(1f, h, t), mat);
                Shapes.Cube(pit, new Vector3(0, y, -0.5f + t / 2f), new Vector3(1f, h, t), mat);
                Shapes.Cube(pit, new Vector3(0.5f - t / 2f, y, 0), new Vector3(t, h, 1f), mat);
                Shapes.Cube(pit, new Vector3(-0.5f + t / 2f, y, 0), new Vector3(t, h, 1f), mat);
            }
            var stone = MaterialLibrary.Shared(Palette.RimStone, 0.1f);
            var rng = new System.Random(Cell.X * 73 + Cell.Y * 19);
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, 90 * side, 0);
                for (int i = 0; i < 3; i++)
                {
                    float x = -0.33f + i * 0.33f + ((float)rng.NextDouble() - 0.5f) * 0.08f;
                    var size = new Vector3(0.2f + (float)rng.NextDouble() * 0.1f, 0.07f + (float)rng.NextDouble() * 0.05f, 0.12f);
                    var s = Shapes.Rounded(pit, rot * new Vector3(x, -0.02f, 0.44f), size, stone, 0.03f, "RimStone");
                    s.transform.localRotation = rot * Quaternion.Euler(0, ((float)rng.NextDouble() - 0.5f) * 30f, 0);
                }
            }
            var mist = Shapes.Quad(pit, new Vector3(0, -0.18f, 0), new Vector3(0.9f, 0.9f, 1f), Quaternion.Euler(90, 0, 0),
                MaterialLibrary.Unlit(new Color(0.75f, 0.75f, 0.8f, 0.12f)), "Mist");
            Shapes.NoShadows(mist);
        }

        private void ShowSunkBox(BoxType type, int variant)
        {
            var size = BoxModelFactory.SizeOf(type);
            var body = new GameObject("SunkBox").transform;
            body.SetParent(transform, false);
            body.localScale = size;
            body.localPosition = new Vector3(0, size.y / 2f - size.y * 0.97f, 0);
            BoxModelFactory.Build(body, type, variant);
        }
    }
}
