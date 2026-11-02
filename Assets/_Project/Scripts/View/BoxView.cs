using System.Collections;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>One box. The root sits on the floor at the cell centre; the body is a child so it can squash.</summary>
    public class BoxView : MonoBehaviour
    {
        private Transform body;
        private Material shell;
        private Vector3 size;
        private Transform[] flecks;

        public int Id { get; private set; }
        public BoxType Type { get; private set; }

        public void Build(int id, BoxType type)
        {
            Id = id;
            Type = type;
            size = BoxModelFactory.SizeOf(type);

            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            SetBodyScale(size);
            shell = BoxModelFactory.Build(body, type, id % 3);
            if (type == BoxType.Normal) OutfitDresser.DressCrate(body);

            if (type == BoxType.Ice) AddIceExtras();
            else if (type == BoxType.Heavy) AddCracks();
        }

        /// <summary>
        /// Match crate (Ana Belge §5): a chunky toy block in its match colour with a lighter lid. Colour alone tells the kinds apart
        /// (owner 2026-10-31: no symbol decals).
        /// </summary>
        public void BuildSymbol(int id, int symbol)
        {
            Id = id;
            Type = BoxType.Normal;
            size = Vector3.one * 0.86f;
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            SetBodyScale(size);
            var color = SymbolArt.ColorOf(symbol);
            shell = MaterialLibrary.Create(color, 0.5f);
            // The owner's colour crates (2026-11-01): red and turquoise models as they are; the other colours are the white
            // model tinted (owner: "eksikleri boya"). Prefabs are sized already, so the body's scale is undone.
            var model = VisualCatalog.TryInstantiate($"BoxMatch_{symbol}", body);
            bool tinted = false;
            // Colour 3 is the white model as it is; only colours above 3 (old files) are tinted.
            if (model == null) { model = VisualCatalog.TryInstantiate("BoxMatch_White", body); tinted = model != null && symbol != 3; }
            if (model != null)
            {
                model.transform.localScale = new Vector3(1f / size.x, 1f / size.y, 1f / size.z);
                model.transform.localPosition = new Vector3(0, -0.5f, 0);
                if (tinted)
                {
                    var block = new MaterialPropertyBlock();
                    foreach (var r in model.GetComponentsInChildren<Renderer>())
                    {
                        r.GetPropertyBlock(block);
                        block.SetColor("_BaseColor", Color.Lerp(color, Color.white, 0.15f));
                        r.SetPropertyBlock(block);
                    }
                }
                return;
            }
            // A stackable plastic market crate (open top): floor, four ribbed walls with a grid of vent holes, a hand slot
            // near the top of each side, a thick rim. Body space is a unit cube centred on the origin.
            var dark = MaterialLibrary.Shared(color * 0.45f, 0.4f);
            var inside = MaterialLibrary.Shared(color * 0.7f, 0.4f);
            Shapes.Rounded(body, new Vector3(0, -0.44f, 0), new Vector3(1f, 0.12f, 1f), shell, 0.04f, "Floor");
            Shapes.Rounded(body, new Vector3(0, -0.36f, 0), new Vector3(0.84f, 0.02f, 0.84f), inside, 0.01f, "Inside");
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, 90 * side, 0);
                var wall = Shapes.Rounded(body, rot * new Vector3(0, 0, -0.46f), new Vector3(1f, 1f, 0.08f), shell, 0.03f, "Wall");
                wall.transform.localRotation = rot;
                for (int row = 0; row < 2; row++)
                for (int col = -1; col <= 1; col++)
                {
                    var hole = Shapes.Rounded(body, rot * new Vector3(col * 0.27f, -0.2f + row * 0.26f, -0.505f), new Vector3(0.17f, 0.16f, 0.02f), dark, 0.02f, "Vent");
                    hole.transform.localRotation = rot;
                    Shapes.NoShadows(hole);
                }
                var slot = Shapes.Rounded(body, rot * new Vector3(0, 0.3f, -0.505f), new Vector3(0.36f, 0.09f, 0.02f), dark, 0.04f, "HandSlot");
                slot.transform.localRotation = rot;
                Shapes.NoShadows(slot);
            }
            Shapes.Rounded(body, new Vector3(0, 0.47f, 0), new Vector3(1.04f, 0.07f, 1.04f), MaterialLibrary.Shared(Color.Lerp(color, Color.white, 0.2f), 0.5f), 0.03f, "Rim");
            Shapes.Rounded(body, new Vector3(0, 0.47f, 0), new Vector3(0.86f, 0.075f, 0.86f), inside, 0.02f, "RimInside");
        }

        /// <summary>Three in a row: a short glow and swell, then down through the open hatch (ends hidden).</summary>
        public IEnumerator SinkThroughHatch(float delay)
        {
            for (float t = 0; t < delay; t += Time.deltaTime) yield return null;
            const float swell = 0.16f;
            for (float t = 0; t < swell; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / swell * Mathf.PI);
                SetBodyScale(size * (1f + 0.12f * k));
                if (shell != null) MaterialLibrary.SetGlow(shell, Color.white * (0.6f * k));
                yield return null;
            }
            SetBodyScale(size);
            if (shell != null) MaterialLibrary.SetGlow(shell, Color.black);
            var start = transform.position;
            const float down = 0.28f;
            for (float t = 0; t < down; t += Time.deltaTime)
            {
                float k = t / down;
                transform.position = start + Vector3.down * (1.1f * k * k);
                // Ana Belge §16: the crate spins and shrinks on its way down.
                SetBodyScale(size * (1f - 0.55f * k));
                body.localRotation = Quaternion.Euler(0, 300f * k, 0);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Toy drop from the refill crane's hook: falls faster and faster (a little stretched), squashes flat on landing and
        /// bounces twice before it sits. A soft shadow grows on the floor while it falls.
        /// </summary>
        public IEnumerator ToyDrop(float height = 4f, float duration = 0.32f)
        {
            var rest = transform.position;
            var shadow = Shapes.Quad(transform.parent, rest + Vector3.up * 0.01f, Vector3.one * 0.2f, Quaternion.Euler(90, 0, 0),
                MaterialLibrary.Unlit(new Color(0f, 0f, 0f, 0.35f), UI.IconFactory.Circle.texture), "DropShadow");
            Shapes.NoShadows(shadow);
            shadow.transform.position = rest + Vector3.up * 0.01f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                transform.position = rest + Vector3.up * (height * (1f - k * k));
                SetBodyScale(new Vector3(size.x * 0.92f, size.y * 1.15f, size.z * 0.92f));
                shadow.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 0.95f, k);
                yield return null;
            }
            transform.position = rest;
            Destroy(shadow);
            // Squash, then two shrinking hops.
            yield return Squash(0.09f, 0.3f);
            float[] hops = { 0.22f, 0.08f };
            foreach (var hop in hops)
            {
                const float time = 0.16f;
                for (float t = 0; t < time; t += Time.deltaTime)
                {
                    float k = t / time;
                    transform.position = rest + Vector3.up * (hop * 4f * k * (1f - k));
                    yield return null;
                }
                transform.position = rest;
                yield return Squash(0.06f, 0.15f);
            }
        }

        /// <summary>Overshoots and settles back: 0 → 1 with a small bump past 1 (Ana Belge §16 ease-out-back).</summary>
        public static float EaseOutBack(float k)
        {
            const float c1 = 1.4f, c3 = c1 + 1f;
            float x = k - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }

        /// <summary>Shrinking hops after a landing (heights in cells, e.g. 0.25 then 0.08), each ending in a squash.</summary>
        public IEnumerator Hops(params float[] heights)
        {
            // Only the height: a slide running at the same time keeps the crate going where it goes (owner 2026-11-02: combo
            // crates that hopped while falling sprang back to the cell they started from).
            float floor = transform.position.y;
            foreach (var hop in heights)
            {
                const float time = 0.16f;
                for (float t = 0; t < time; t += Time.deltaTime)
                {
                    float k = t / time;
                    var p = transform.position;
                    transform.position = new Vector3(p.x, floor + hop * 4f * k * (1f - k), p.z);
                    yield return null;
                }
                var end = transform.position;
                transform.position = new Vector3(end.x, floor, end.z);
                yield return Squash(0.06f, 0.15f);
            }
        }

        /// <summary>A crate that settles on its slot: a short hop and a squash (Ana Belge §16, 0.15 s).</summary>
        public IEnumerator Hop() => Hops(0.18f);

        /// <summary>Ice stops with two small bounces back and forth along the slide.</summary>
        public IEnumerator Recoil(Vector3 dir)
        {
            var rest = transform.position;
            foreach (var amount in new[] { 0.09f, 0.035f })
            {
                const float time = 0.09f;
                for (float t = 0; t < time; t += Time.deltaTime)
                {
                    transform.position = rest - dir * (amount * Mathf.Sin(t / time * Mathf.PI));
                    yield return null;
                }
            }
            transform.position = rest;
        }

        /// <summary>A small wobble (a neighbour crate landed next to it).</summary>
        public IEnumerator Wobble(float duration = 0.25f)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = 1f - t / duration;
                body.localRotation = Quaternion.Euler(Mathf.Sin(t * 60f) * 6f * k, 0, Mathf.Cos(t * 50f) * 5f * k);
                yield return null;
            }
            body.localRotation = Quaternion.identity;
        }

        // Cold reflection under the ice and a couple of floating snow flecks.
        private void AddIceExtras()
        {
            var reflection = Shapes.Quad(transform, new Vector3(0, 0.006f, 0), new Vector3(1.05f, 1.05f, 1f), Quaternion.Euler(90, 0, 0),
                MaterialLibrary.Unlit(Palette.IceReflection), "ColdReflection");
            Shapes.NoShadows(reflection);
            var snow = MaterialLibrary.Shared(Color.white, 0.5f);
            flecks = new Transform[2];
            for (int i = 0; i < flecks.Length; i++)
            {
                var f = Shapes.Make(PrimitiveType.Sphere, transform, new Vector3(i == 0 ? 0.35f : -0.3f, 0.95f, i == 0 ? 0.2f : -0.25f), Vector3.one * 0.05f, snow, "Snow");
                Shapes.NoShadows(f);
                flecks[i] = f.transform;
            }
        }

        // Thin dark crack lines on the floor around the heavy box.
        private void AddCracks()
        {
            var crack = MaterialLibrary.Unlit(Palette.Crack);
            for (int i = 0; i < 4; i++)
            {
                var rot = Quaternion.Euler(0, 90 * i + 20, 0);
                var c = Shapes.Cube(transform, rot * new Vector3(0.12f, 0.004f, 0.5f), new Vector3(0.025f, 0.004f, 0.16f), crack, "Crack");
                c.transform.localRotation = rot * Quaternion.Euler(0, 25, 0);
                Shapes.NoShadows(c);
            }
        }

        private void Update()
        {
            if (flecks == null) return;
            for (int i = 0; i < flecks.Length; i++)
            {
                var p = flecks[i].localPosition;
                p.y = 0.95f + Mathf.Sin(Time.time * 1.6f + i * 2f) * 0.07f;
                flecks[i].localPosition = p;
            }
        }

        /// <summary>Slide to a cell; linear for ice, eased for pushes. Squashes on arrival unless it is about to fall.</summary>
        public IEnumerator MoveTo(Vector3 target, float duration, bool linear, bool squash)
        {
            var start = transform.position;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                if (!linear) k = EaseOutBack(k); // Ana Belge §16: a toy push runs a hair past the cell and springs back
                transform.position = Vector3.LerpUnclamped(start, target, k);
                yield return null;
            }
            transform.position = target;
            if (squash) yield return Squash();
        }

        public IEnumerator Squash(float duration = 0.1f) => Squash(duration, 0.18f);

        public IEnumerator Squash(float duration, float amount)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / duration * Mathf.PI);
                SetBodyScale(new Vector3(size.x * (1f + amount * 0.8f * k), size.y * (1f - amount * k), size.z * (1f + amount * 0.8f * k)));
                yield return null;
            }
            SetBodyScale(size);
        }

        /// <summary>Drops down into a hole (ends half sunk, top near the floor) or away into the void.</summary>
        public IEnumerator Fall(bool intoHole, float duration = 0.25f)
        {
            var start = transform.position;
            float depth = intoHole ? size.y * 0.97f : 3f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                transform.position = start + Vector3.down * (depth * k * k);
                if (!intoHole) SetBodyScale(size * (1f - 0.7f * k));
                yield return null;
            }
            transform.position = start + Vector3.down * depth;
            if (!intoHole) gameObject.SetActive(false);
        }

        /// <summary>Short glow when the box lands on a target.</summary>
        public IEnumerator Flash(float duration = 0.3f)
        {
            if (shell == null) yield break;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / duration * Mathf.PI);
                MaterialLibrary.SetGlow(shell, Palette.TargetDone * (0.5f * k));
                yield return null;
            }
            MaterialLibrary.SetGlow(shell, Color.black);
        }

        /// <summary>Removes floor decals (used once the box is down in a hole).</summary>
        public void SinkStatic()
        {
            foreach (Transform child in transform)
                if (child != body) child.gameObject.SetActive(false);
            flecks = null;
        }

        // Keep the bottom on the floor whatever the scale.
        private void SetBodyScale(Vector3 scale)
        {
            body.localScale = scale;
            body.localPosition = new Vector3(0, scale.y / 2f, 0);
        }
    }
}
