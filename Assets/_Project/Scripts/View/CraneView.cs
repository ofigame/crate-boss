using System;
using System.Collections;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The tower crane (Ana Belge §5.3): a lattice mast at the back-left corner outside the board,
    /// a slewing jib with a trolley, the next box hanging on the hook, and a telegraph frame on the floor
    /// where it will land. It only shows the crane part of the GameState; the rules live in CraneSystem.
    /// </summary>
    public class CraneView : MonoBehaviour
    {
        private const float JibHeight = 4.6f;
        private const float HangBottom = 2.2f;
        private const float DropTime = 0.35f;
        private const float SlewTime = 0.55f;
        private const float ParkReach = 1.2f;

        private Func<GridPos, Vector3> toWorld;
        private GameFeedback feedback;
        private int slot;
        private float parkYaw;
        private Vector3 mast;
        private Transform pivot, trolley, rope, hook, hanging, marker, dropLine;
        private Material frameMat, glowMat, lampMat, lineMat, shadowMat;
        private float yaw, reach;
        private float hangHeight; // height of the box now on the hook
        private Coroutine motion;
        private bool dropping;
        private float hookDrop = 0.12f; // from the rope end to the top of the hanging box

        /// <summary>Shown values (updated after animations, so the badge never runs ahead of the picture).</summary>
        public bool Active { get; private set; }
        public bool Waiting { get; private set; }
        public int Counter { get; private set; }
        /// <summary>Counter ≤ 2: the worker looks worried, the frame pulses fast.</summary>
        public bool IsTense => Active && Counter <= 2;
        public Vector3 BadgeAnchor => marker != null ? marker.position + new Vector3(0, 0.05f, -0.62f) : transform.position;
        /// <summary>The crane's letter ("A", "B", "C"), shown on the cabin, the badge and the HUD chip.</summary>
        public string Id { get; private set; } = "A";

        public void Build(GameState state, Func<GridPos, Vector3> cellToWorld, GameFeedback sounds) => Build(state, 0, cellToWorld, sounds);

        /// <param name="craneSlot">Index into <see cref="GameState.Cranes"/>.</param>
        public void Build(GameState state, int craneSlot, Func<GridPos, Vector3> cellToWorld, GameFeedback sounds)
        {
            toWorld = cellToWorld;
            feedback = sounds;
            slot = craneSlot;
            var def = state.Cranes[slot];
            Id = def.Id;
            mast = MastPosition(state, def.Side);
            // Parked, the jib points away from the board.
            var center = (toWorld(new GridPos(0, 0)) + toWorld(new GridPos(state.Width - 1, state.Height - 1))) / 2f;
            var outward = mast - center;
            parkYaw = Mathf.Atan2(outward.x, outward.z) * Mathf.Rad2Deg;
            BuildTower(MaxReach(state) + 0.9f);
            Ghost();
            BuildMarker();
            Show(state);
        }

        // Towers stand outside the board: left = back-left corner, right = back-right, top = behind the middle,
        // bottom = front-left corner (kept low in the frame so it hides as little as possible).
        private Vector3 MastPosition(GameState state, CraneSide side) => MastAt(state, side);

        /// <summary>Where a crane of this side stands (board space, as BoardView.ToWorld); the district keeps roads away from it.</summary>
        public static Vector3 MastAt(GameState state, CraneSide side)
        {
            var backLeft = new Vector3(0, 0, state.Height - 1);
            var frontRight = new Vector3(state.Width - 1, 0, 0);
            switch (side)
            {
                case CraneSide.Right: return new Vector3(frontRight.x + 1.6f, 0f, backLeft.z + 1.4f);
                case CraneSide.Top: return new Vector3((backLeft.x + frontRight.x) / 2f, 0f, backLeft.z + 2.2f);
                case CraneSide.Bottom: return new Vector3(backLeft.x - 1.6f, 0f, frontRight.z - 1.4f);
                default: return new Vector3(backLeft.x - 1.6f, 0f, backLeft.z + 1.4f);
            }
        }

        /// <summary>Snaps to the state (level start, undo, restart).</summary>
        public void Show(GameState state)
        {
            if (motion != null) StopCoroutine(motion);
            motion = null;
            dropping = false;
            Active = state.IsCraneActive(slot);
            Counter = state.CraneStates[slot].Counter;
            Waiting = CraneSystem.IsWaiting(state, slot);
            if (Active)
            {
                var drop = state.NextDropOf(slot);
                Aim(drop.Cell, out yaw, out reach);
                Hang(drop.Type);
                PlaceMarker(drop.Cell);
            }
            else
            {
                yaw = parkYaw;
                reach = ParkReach;
                Hang(null);
                marker.gameObject.SetActive(false);
            }
            ApplyPose();
        }

        /// <summary>After a move without a drop: count down, beep at 2, double beep at 1.</summary>
        public void Refresh(GameState state)
        {
            if (!Active) return;
            int before = Counter;
            bool wasWaiting = Waiting;
            Counter = state.CraneStates[slot].Counter;
            Waiting = CraneSystem.IsWaiting(state, slot);
            if (Counter == before && Waiting == wasWaiting) return;
            if (Counter == 2) feedback?.CraneWarn();
            else if (Counter == 1) feedback?.CraneAlarm();
        }

        /// <summary>The hanging box drops onto its cell (0.35 s, speeding up). The caller then shows the landed box.</summary>
        public IEnumerator Drop()
        {
            if (hanging == null) yield break;
            if (motion != null) StopCoroutine(motion);
            motion = null;
            marker.gameObject.SetActive(false);
            dropping = true;
            float startY = hanging.position.y;
            float endY = 0.12f + hangHeight; // the box's bottom touches the floor
            float ropeStart = rope.localScale.y;
            for (float t = 0; t < DropTime; t += Time.deltaTime)
            {
                float k = t / DropTime;
                k *= k;
                var p = hanging.position;
                p.y = Mathf.Lerp(startY, endY, k);
                hanging.position = p;
                hanging.localRotation = Quaternion.identity;
                // The rope pays out a little, then the box comes free.
                SetRope(Mathf.Lerp(ropeStart, ropeStart + 0.4f, Mathf.Min(1f, k * 3f)));
                yield return null;
            }
            Hang(null);
            SetRope(0.4f);
            dropping = false;
        }

        /// <summary>After a drop: slew to the next cell and lower the next box onto the hook, or park when the list is done.</summary>
        public void Reload(GameState state)
        {
            Active = state.IsCraneActive(slot);
            Counter = state.CraneStates[slot].Counter;
            Waiting = CraneSystem.IsWaiting(state, slot);
            if (motion != null) StopCoroutine(motion);
            motion = StartCoroutine(Active ? SlewToNext(state.NextDropOf(slot)) : Park());
        }

        private IEnumerator SlewToNext(CraneDrop drop)
        {
            feedback?.CraneMove();
            Aim(drop.Cell, out float toYaw, out float toReach);
            yield return Slew(toYaw, toReach);
            PlaceMarker(drop.Cell);
            Hang(drop.Type);
            // The new box comes down from the trolley onto the hook height.
            float full = rope.localScale.y;
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                float k = 1f - Mathf.Pow(1f - t / 0.3f, 2f);
                SetRope(Mathf.Lerp(0.3f, full, k));
                yield return null;
            }
            SetRope(full);
            motion = null;
        }

        private IEnumerator Park()
        {
            feedback?.CraneOff();
            marker.gameObject.SetActive(false);
            yield return Slew(parkYaw, ParkReach);
            motion = null;
        }

        private IEnumerator Slew(float toYaw, float toReach)
        {
            float fromYaw = yaw, fromReach = reach;
            float delta = Mathf.DeltaAngle(fromYaw, toYaw);
            for (float t = 0; t < SlewTime; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / SlewTime);
                yaw = fromYaw + delta * k;
                reach = Mathf.Lerp(fromReach, toReach, k);
                ApplyPose();
                yield return null;
            }
            yaw = toYaw;
            reach = toReach;
            ApplyPose();
        }

        private void Update()
        {
            if (hanging != null && motion == null && !dropping)
            {
                // Gentle sway (2.5 s cycle); calmer when the drop is close.
                float amp = Counter > 2 ? 3f : 1.2f;
                hanging.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * Mathf.PI * 2f / 2.5f) * amp, 0f, Mathf.Sin(Time.time * Mathf.PI * 2f / 3.1f) * amp * 0.6f);
            }
            if (marker != null && marker.gameObject.activeSelf)
            {
                bool alarm = Counter <= 1;
                float speed = Counter > 2 ? 2.2f : 7f;
                float pulse = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
                var color = alarm ? Palette.CraneAlarm : Palette.CraneWarning;
                MaterialLibrary.SetColor(frameMat, Color.Lerp(color * 0.75f, color, pulse));
                MaterialLibrary.SetColor(glowMat, new Color(color.r, color.g, color.b, 0.12f + 0.18f * pulse));
                MaterialLibrary.SetColor(lineMat, new Color(color.r, color.g, color.b, 0.8f));
                float s = Counter == 2 ? 1f + 0.06f * pulse : 1f;
                marker.localScale = new Vector3(s, 1f, s);
            }
            if (lampMat != null)
            {
                bool blink = Active && Counter <= 1 && Mathf.Repeat(Time.time, 0.5f) < 0.25f;
                bool on = Active && (Counter > 1 || blink);
                MaterialLibrary.SetGlow(lampMat, on ? Palette.CraneAlarm * (blink ? 4f : 1.2f) : Color.black);
            }
        }

        // ---------- pose ----------

        private void Aim(GridPos cell, out float aimYaw, out float aimReach)
        {
            var d = toWorld(cell) - mast;
            d.y = 0f;
            aimYaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            aimReach = d.magnitude;
        }

        private float MaxReach(GameState state)
        {
            float max = 2f;

            foreach (var drop in state.Cranes[slot].Drops)
            {
                Aim(drop.Cell, out _, out float r);
                max = Mathf.Max(max, r);
            }
            return max;
        }

        private void ApplyPose()
        {
            pivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            trolley.localPosition = new Vector3(0f, -0.08f, reach);
        }

        private void Hang(BoxType? type)
        {
            if (hanging != null) Destroy(hanging.gameObject);
            hanging = null;
            if (!type.HasValue) return;
            var size = BoxModelFactory.SizeOf(type.Value);
            hangHeight = size.y;
            hanging = new GameObject("HangingBox").transform;
            hanging.SetParent(hook, false);
            var body = new GameObject("Body").transform;
            body.SetParent(hanging, false);
            body.localScale = size;
            body.localPosition = new Vector3(0, -hookDrop - size.y / 2f, 0);
            BoxModelFactory.Build(body, type.Value, 1);
            // Rope down to just above the box top.
            SetRope(JibHeight - 0.08f - (HangBottom + size.y + hookDrop));
        }

        private void SetRope(float length)
        {
            rope.localScale = new Vector3(1f, length, 1f);
            hook.localPosition = new Vector3(0f, -length, 0f);
        }

        private void PlaceMarker(GridPos cell)
        {
            marker.gameObject.SetActive(true);
            marker.position = toWorld(cell) + Vector3.up * 0.02f;
            // Dashed fall line from the floor up to the bottom of the hanging box.
            float top = HangBottom - 0.1f;
            dropLine.localScale = new Vector3(1f, top, 1f);
        }

        // ---------- building ----------

        private void BuildTower(float jibLength)
        {
            var yellow = MaterialLibrary.Shared(Palette.CraneYellow, 0.35f, 0.3f);
            var yellowDark = MaterialLibrary.Shared(Palette.CraneYellowDark, 0.3f, 0.3f);
            var concrete = MaterialLibrary.Shared(new Color(0.62f, 0.64f, 0.66f), 0.1f);
            var steel = MaterialLibrary.Shared(new Color(0.35f, 0.37f, 0.40f), 0.4f, 0.6f);

            // Concrete foundation on its own small pedestal block.
            Shapes.Cube(transform, mast + new Vector3(0, -0.75f, 0), new Vector3(1.4f, 1.5f, 1.4f), concrete, "Foundation");
            Shapes.Cube(transform, mast + new Vector3(0, 0.08f, 0), new Vector3(0.9f, 0.16f, 0.9f), steel, "Base");

            if (BuildModelCrane(jibLength, yellow, yellowDark))
            {
                BuildTrolley(steel);
                foreach (var r in GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                return;
            }

            // Lattice mast: four corner posts plus zig-zag braces on every face.
            const float half = 0.22f, segment = 0.55f;
            float mastTop = JibHeight - 0.35f;
            for (int i = 0; i < 4; i++)
            {
                var c = new Vector3((i & 1) == 0 ? -half : half, 0, (i & 2) == 0 ? -half : half);
                Shapes.Cube(transform, mast + c + Vector3.up * mastTop / 2f, new Vector3(0.06f, mastTop, 0.06f), yellow, "Post");
            }
            int segments = Mathf.FloorToInt(mastTop / segment);
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, 90 * side, 0);
                for (int s = 0; s < segments; s++)
                {
                    float y0 = s * segment + 0.16f, y1 = y0 + segment;
                    var a = rot * new Vector3((s & 1) == 0 ? -half : half, y0, half);
                    var b = rot * new Vector3((s & 1) == 0 ? half : -half, y1, half);
                    Strut(transform, mast + a, mast + b, 0.035f, yellowDark);
                }
            }

            BuildLetterPlate(mastTop - 0.75f, 0.27f);

            // Slewing part: cabin, jib, counter-jib, counterweight, peak with ties and a warning lamp.
            pivot = new GameObject("Slewing").transform;
            pivot.SetParent(transform, false);
            pivot.position = mast + Vector3.up * JibHeight;
            Shapes.Cube(pivot, new Vector3(0, -0.28f, 0), new Vector3(0.6f, 0.14f, 0.6f), steel, "Turntable");
            Shapes.Rounded(pivot, new Vector3(0.36f, -0.05f, 0.1f), new Vector3(0.36f, 0.4f, 0.42f), MaterialLibrary.Shared(new Color(0.96f, 0.93f, 0.86f), 0.4f), 0.05f, "Cabin");
            var glass = MaterialLibrary.Shared(new Color(0.35f, 0.62f, 0.85f), 0.9f, 0.2f);
            Shapes.Cube(pivot, new Vector3(0.36f, 0.0f, 0.32f), new Vector3(0.3f, 0.2f, 0.02f), glass, "CabinGlass");

            // Jib: triangular lattice along +z.
            const float w = 0.16f, hgt = 0.32f;
            Shapes.Cube(pivot, new Vector3(-w, 0, jibLength / 2f), new Vector3(0.05f, 0.05f, jibLength), yellow, "Chord");
            Shapes.Cube(pivot, new Vector3(w, 0, jibLength / 2f), new Vector3(0.05f, 0.05f, jibLength), yellow, "Chord");
            Shapes.Cube(pivot, new Vector3(0, hgt, jibLength / 2f), new Vector3(0.05f, 0.05f, jibLength), yellow, "Chord");
            int bays = Mathf.Max(2, Mathf.RoundToInt(jibLength / 0.5f));
            for (int i = 0; i < bays; i++)
            {
                float z0 = i * jibLength / bays, z1 = (i + 1) * jibLength / bays;
                Strut(pivot, new Vector3(-w, 0, z0), new Vector3(0, hgt, z1), 0.03f, yellowDark);
                Strut(pivot, new Vector3(w, 0, z0), new Vector3(0, hgt, z1), 0.03f, yellowDark);
            }
            // Counter-jib and its weight.
            Shapes.Cube(pivot, new Vector3(0, 0.05f, -0.9f), new Vector3(0.36f, 0.08f, 1.8f), yellow, "CounterJib");
            Shapes.Cube(pivot, new Vector3(0, -0.12f, -1.55f), new Vector3(0.5f, 0.42f, 0.5f), MaterialLibrary.Shared(new Color(0.55f, 0.57f, 0.6f), 0.15f), "Counterweight");
            // Peak and ties.
            var peakTop = new Vector3(0, 1.25f, 0);
            Strut(pivot, new Vector3(-0.2f, 0.05f, 0), peakTop, 0.05f, yellow);
            Strut(pivot, new Vector3(0.2f, 0.05f, 0), peakTop, 0.05f, yellow);
            Strut(pivot, peakTop, new Vector3(0, hgt, jibLength * 0.6f), 0.02f, steel);
            Strut(pivot, peakTop, new Vector3(0, 0.1f, -1.7f), 0.02f, steel);
            lampMat = MaterialLibrary.Create(new Color(0.8f, 0.15f, 0.12f), 0.7f);
            Shapes.Make(PrimitiveType.Sphere, pivot, peakTop + Vector3.up * 0.08f, Vector3.one * 0.14f, lampMat, "WarningLamp");

            BuildTrolley(steel);
            foreach (var r in GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        /// <summary>
        /// Real model (Ana Belge §19): the Tripo crane (tower, cabin, jib in one piece) turns on the slewing pivot; its tower
        /// stands on the foundation. A jib that does not reach the far cells gets procedural lattice bays in the same
        /// yellow (Ana Belge §19). Returns false when there is no model.
        /// </summary>
        private bool BuildModelCrane(float jibLength, Material yellow, Material yellowDark)
        {
            if (!VisualCatalog.TryGetBounds("CraneBody", out var b)) return false;
            pivot = new GameObject("Slewing").transform;
            pivot.SetParent(transform, false);
            pivot.position = mast + Vector3.up * JibHeight;
            VisualCatalog.TryInstantiate("CraneBody", pivot);
            float reach = b.max.z;
            if (jibLength > reach)
            {
                const float w = 0.24f, hgt = 0.7f;
                float z0 = reach - 0.15f, len = jibLength - z0;
                foreach (var x in new[] { -w, w })
                    Shapes.Cube(pivot, new Vector3(x, 0.03f, z0 + len / 2f), new Vector3(0.06f, 0.06f, len), yellow, "ExtensionChord");
                Shapes.Cube(pivot, new Vector3(0, hgt, z0 + len / 2f), new Vector3(0.06f, 0.06f, len), yellow, "ExtensionChord");
                int bays = Mathf.Max(1, Mathf.RoundToInt(len / 0.6f));
                for (int i = 0; i < bays; i++)
                {
                    float a = z0 + i * len / bays, c = z0 + (i + 1) * len / bays;
                    Strut(pivot, new Vector3(-w, 0.03f, a), new Vector3(0, hgt, c), 0.035f, yellowDark);
                    Strut(pivot, new Vector3(w, 0.03f, a), new Vector3(0, hgt, c), 0.035f, yellowDark);
                }
            }
            lampMat = MaterialLibrary.Create(new Color(0.8f, 0.15f, 0.12f), 0.7f);
            Shapes.Make(PrimitiveType.Sphere, pivot, new Vector3(0f, b.max.y + 0.08f, 0f), Vector3.one * 0.16f, lampMat, "WarningLamp");
            BuildLetterPlate(JibHeight * 0.55f, Mathf.Max(b.size.x, 0.6f) * 0.72f); // clear of the turning tower
            return true;
        }

        /// <summary>See-through share of the slewing part over the yard (owner 2026-10-31: "vinçler alanın görünümünü engellememeli").</summary>
        private const float GhostAlpha = 0.3f;
        private static readonly System.Collections.Generic.Dictionary<Material, Material> ghosts = new System.Collections.Generic.Dictionary<Material, Material>();

        /// <summary>
        /// The jib, cabin, counter-jib and trolley swing over the yard, so they are drawn see-through and cast no shadow; the
        /// rope, the hook and the box on it stay solid (the player must see what comes down and where).
        /// </summary>
        private void Ghost()
        {
            foreach (var r in pivot.GetComponentsInChildren<Renderer>())
            {
                if (r.transform.IsChildOf(hook) || r.transform.IsChildOf(rope) || r.sharedMaterial == lampMat) continue;
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = GhostOf(mats[i]);
                r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static Material GhostOf(Material m)
        {
            if (m == null) return null;
            if (ghosts.TryGetValue(m, out var g) && g != null) return g;
            var c = m.HasProperty("_BaseColor") ? MaterialLibrary.GetColor(m) : Color.white;
            c.a = GhostAlpha;
            g = MaterialLibrary.CreateFadable(c, m.HasProperty("_Smoothness") ? m.GetFloat("_Smoothness") : 0.5f);
            if (m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null) MaterialLibrary.SetTexture(g, m.GetTexture("_BaseMap"));
            ghosts[m] = g;
            return g;
        }

        // Trolley with a double rope and the hook block (real models when they exist).
        private void BuildTrolley(Material steel)
        {
            trolley = new GameObject("Trolley").transform;
            trolley.SetParent(pivot, false);
            if (VisualCatalog.TryInstantiate("CraneTrolley", trolley) == null)
                Shapes.Cube(trolley, Vector3.zero, new Vector3(0.42f, 0.12f, 0.3f), steel, "Carriage");
            var ropeRoot = new GameObject("Rope").transform;
            ropeRoot.SetParent(trolley, false);
            rope = ropeRoot;
            var ropeMat = MaterialLibrary.Shared(new Color(0.15f, 0.15f, 0.17f), 0.3f);
            Shapes.Cube(ropeRoot, new Vector3(-0.06f, -0.5f, 0), new Vector3(0.018f, 1f, 0.018f), ropeMat, "Line");
            Shapes.Cube(ropeRoot, new Vector3(0.06f, -0.5f, 0), new Vector3(0.018f, 1f, 0.018f), ropeMat, "Line");
            hook = new GameObject("Hook").transform;
            hook.SetParent(trolley, false);
            if (VisualCatalog.TryInstantiate("CraneHook", hook) != null && VisualCatalog.TryGetBounds("CraneHook", out var hb))
                hookDrop = hb.size.y * 0.9f; // the box hangs under the hook model
            else
                Shapes.Rounded(hook, new Vector3(0, -0.05f, 0), new Vector3(0.24f, 0.12f, 0.16f), MaterialLibrary.Shared(Palette.CraneWarning, 0.4f), 0.03f, "HookBlock");
        }

        // Telegraph: dashed orange frame, soft glow, the hanging box's shadow and a dashed fall line.
        private void BuildMarker()
        {
            marker = new GameObject("DropMarker").transform;
            marker.SetParent(transform, false);
            frameMat = MaterialLibrary.Unlit(Palette.CraneWarning);
            glowMat = MaterialLibrary.Unlit(new Color(1f, 0.42f, 0.24f, 0.25f));
            lineMat = MaterialLibrary.Unlit(new Color(1f, 0.42f, 0.24f, 0.8f));
            shadowMat = MaterialLibrary.Unlit(new Color(0f, 0f, 0f, 0.28f));
            var glow = Shapes.Quad(marker, new Vector3(0, 0.005f, 0), new Vector3(0.9f, 0.9f, 1f), Quaternion.Euler(90, 0, 0), glowMat, "Glow");
            Shapes.NoShadows(glow);
            var shadow = Shapes.Quad(marker, new Vector3(0, 0.01f, 0), new Vector3(0.62f, 0.62f, 1f), Quaternion.Euler(90, 0, 0), shadowMat, "BoxShadow");
            Shapes.NoShadows(shadow);
            const float edge = 0.92f, dash = 0.2f, thick = 0.06f;
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, 90 * side, 0);
                for (int i = 0; i < 3; i++)
                {
                    float x = -edge / 2f + dash / 2f + i * (edge - dash) / 2f;
                    var pos = rot * new Vector3(x, 0.015f, edge / 2f);
                    var d = Shapes.Cube(marker, pos, new Vector3(dash, 0.02f, thick), frameMat, "Dash");
                    d.transform.localRotation = rot;
                    Shapes.NoShadows(d);
                }
            }
            dropLine = new GameObject("FallLine").transform;
            dropLine.SetParent(marker, false);
            for (int i = 0; i < 8; i++)
            {
                var d = Shapes.Cube(dropLine, new Vector3(0, (i + 0.5f) / 8f, 0), new Vector3(0.03f, 0.06f, 0.03f), lineMat, "Dash");
                d.transform.localScale = new Vector3(0.03f, 0.5f / 8f, 0.03f);
                Shapes.NoShadows(d);
            }
        }

        // Big letter sign on the mast's camera side (Ana Belge §5.3), so the crane can be told apart.
        private void BuildLetterPlate(float height, float front)
        {
            var plate = Shapes.Rounded(transform, mast + new Vector3(0, height, -front), new Vector3(0.62f, 0.62f, 0.05f),
                MaterialLibrary.Shared(new Color(0.13f, 0.14f, 0.17f), 0.3f), 0.06f, "LetterPlate");
            Shapes.NoShadows(plate);
            var textGo = new GameObject("Letter");
            textGo.transform.SetParent(transform, false);
            textGo.transform.localPosition = mast + new Vector3(0, height, -front - 0.03f);
            var text = textGo.AddComponent<TextMesh>();
            text.text = Id;
            text.font = CrateBoss.UI.UiFonts.Title;
            text.fontSize = 96;
            text.characterSize = 0.055f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Palette.CraneYellow;
            textGo.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        }

        /// <summary>A thin bar from a to b (local space of parent).</summary>
        private static void Strut(Transform parent, Vector3 a, Vector3 b, float thickness, Material mat)
        {
            var go = Shapes.Cube(parent, (a + b) / 2f, new Vector3(thickness, thickness, Vector3.Distance(a, b)), mat, "Strut");
            go.transform.localRotation = Quaternion.LookRotation(b - a);
        }
    }
}
