using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Two small particle systems for every one-shot effect: soft puffs (dust, frost, mist) that drift,
    /// and bits (sparkles, confetti) that fall. Counts scale with the quality level.
    /// </summary>
    public class EffectsView : MonoBehaviour
    {
        private ParticleSystem puffs;
        private ParticleSystem bits;

        public float CountScale { get; set; } = 1f;

        private ParticleSystem clouds;
        private ParticleSystem stars;

        public void Build()
        {
            puffs = MakeSystem("Puffs", -0.05f, new Vector2(0.3f, 0.55f));
            bits = MakeSystem("Bits", 1.2f, new Vector2(0.6f, 1.1f));
            // Ana Belge §16: cartoon clouds (round, whitish, grow then vanish) and little stars.
            clouds = MakeSystem("Clouds", -0.15f, new Vector2(0.35f, 0.5f));
            var grow = clouds.sizeOverLifetime;
            grow.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(0.35f, 1.15f), new Keyframe(1, 0.2f)));
            stars = MakeSystem("Stars", 0.9f, new Vector2(0.5f, 0.8f));
            var starMat = new Material(MaterialLibrary.Particle);
            MaterialLibrary.SetTexture(starMat, UI.IconFactory.Get(UI.IconKind.Star).texture);
            stars.GetComponent<ParticleSystemRenderer>().sharedMaterial = starMat;
        }

        // Ana Belge §2 candy tones for confetti and stars.
        public static readonly Color[] Candy =
        {
            new Color(0.95f, 0.33f, 0.54f), new Color(0.61f, 0.36f, 0.9f), new Color(0.24f, 0.76f, 0.83f), new Color(1f, 0.82f, 0.25f),
            new Color(1f, 0.55f, 0.26f), new Color(0.24f, 0.8f, 0.42f), new Color(0.35f, 0.66f, 0.94f),
        };

        /// <summary>A few round cartoon clouds (3–6 balls that swell and vanish), e.g. behind a pushed crate or under a landing.</summary>
        public void ToyClouds(Vector3 pos, int count = 4, float size = 0.26f, Vector3 drift = default)
        {
            for (int i = 0; i < Count(count); i++)
            {
                var r = Random.insideUnitCircle * 0.25f;
                Emit(clouds, pos + new Vector3(r.x, 0.08f, r.y), drift + new Vector3(r.x, Random.Range(0.15f, 0.45f), r.y),
                    size * Random.Range(0.8f, 1.2f), new Color(1f, 1f, 1f, 0.95f));
            }
        }

        /// <summary>A ring of cartoon clouds rolling outwards (a big landing).</summary>
        public void ToyCloudRing(Vector3 pos, int count = 8)
        {
            int n = Count(count);
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n;
                var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Emit(clouds, pos + dir * 0.45f + Vector3.up * 0.08f, dir * 0.9f + Vector3.up * 0.2f, Random.Range(0.24f, 0.34f), new Color(1f, 1f, 1f, 0.95f));
            }
        }

        /// <summary>Little candy stars popping up (Ana Belge §16 "parıltı").</summary>
        public void Stars(Vector3 pos, int count = 5)
        {
            for (int i = 0; i < Count(count); i++)
                Emit(stars, pos + Random.insideUnitSphere * 0.25f, Random.insideUnitSphere * 1.1f + Vector3.up * 1.6f,
                    Random.Range(0.12f, 0.2f), Candy[Random.Range(0, Candy.Length)]);
        }

        /// <summary>A flat ring of light spreading on the floor (a crate locks onto its slot).</summary>
        public void LightWave(Vector3 pos, Color color)
        {
            var ring = Shapes.Quad(transform, pos + Vector3.up * 0.06f, Vector3.one * 0.4f, Quaternion.Euler(90, 0, 0),
                MaterialLibrary.Unlit(color, RingTexture), "LightWave");
            Shapes.NoShadows(ring);
            ring.transform.position = pos + Vector3.up * 0.06f;
            StartCoroutine(Spread(ring, color));
        }

        private System.Collections.IEnumerator Spread(GameObject ring, Color color)
        {
            var mat = ring.GetComponent<Renderer>().material;
            const float time = 0.45f;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float k = t / time;
                ring.transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.9f, 1f - (1f - k) * (1f - k));
                MaterialLibrary.SetColor(mat, new Color(color.r, color.g, color.b, color.a * (1f - k)));
                yield return null;
            }
            Destroy(ring);
        }

        private static Texture2D ringTexture;
        private static Texture2D RingTexture
        {
            get
            {
                if (ringTexture != null) return ringTexture;
                var c = new UI.SdfCanvas(128);
                c.Fill((x, y) => Mathf.Abs(UI.SdfCanvas.Circle(x, y, 0.5f, 0.5f, 0.42f)) - 0.05f, Color.white);
                ringTexture = c.ToSprite("Ring").texture;
                return ringTexture;
            }
        }

        /// <summary>Dust behind a pushed crate.</summary>
        public void Dust(Vector3 pos, Vector3 pushDir, Color color, int count = 8)
        {
            for (int i = 0; i < Count(count); i++)
            {
                var side = Vector3.Cross(pushDir, Vector3.up) * Random.Range(-0.8f, 0.8f);
                Emit(puffs, pos + side * 0.3f + Vector3.up * 0.05f,
                    -pushDir * Random.Range(0.3f, 0.8f) + side + Vector3.up * Random.Range(0.2f, 0.6f),
                    Random.Range(0.1f, 0.22f), color);
            }
        }

        /// <summary>A flat ring of dust (heavy box landing).</summary>
        public void Ring(Vector3 pos, Color color, int count = 16)
        {
            int n = Count(count);
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n;
                var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Emit(puffs, pos + dir * 0.45f + Vector3.up * 0.05f, dir * 1.1f + Vector3.up * 0.15f, Random.Range(0.14f, 0.24f), color);
            }
        }

        /// <summary>A cloud rising out of a hole when a box drops in.</summary>
        public void Cloud(Vector3 pos, Color color, int count = 14)
        {
            for (int i = 0; i < Count(count); i++)
            {
                var r = Random.insideUnitCircle * 0.4f;
                Emit(puffs, pos + new Vector3(r.x, 0.05f, r.y), new Vector3(r.x, Random.Range(0.6f, 1.3f), r.y), Random.Range(0.18f, 0.32f), color);
            }
        }

        /// <summary>Frost left behind a sliding ice box.</summary>
        public void Frost(Vector3 pos)
        {
            for (int i = 0; i < Count(2); i++)
                Emit(puffs, pos + new Vector3(Random.Range(-0.3f, 0.3f), 0.03f, Random.Range(-0.3f, 0.3f)),
                    Vector3.up * Random.Range(0.05f, 0.2f), Random.Range(0.08f, 0.16f), new Color(0.9f, 0.98f, 1f, 0.8f));
        }

        public void Sparkle(Vector3 pos, Color color, int count = 6)
        {
            for (int i = 0; i < Count(count); i++)
                Emit(bits, pos + Random.insideUnitSphere * 0.3f, Random.insideUnitSphere * 1.2f + Vector3.up, Random.Range(0.04f, 0.08f), color);
        }

        public void Confetti(Vector3 pos, int count = 40)
        {
            var colors = Candy;
            for (int i = 0; i < Count(count); i++)
            {
                var v = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(2.5f, 4.5f), Random.Range(-1.5f, 1.5f));
                Emit(bits, pos + Vector3.up * 0.8f, v, Random.Range(0.06f, 0.11f), colors[i % colors.Length]);
            }
        }

        private int Count(int n) => Mathf.Max(1, Mathf.RoundToInt(n * CountScale));

        private static void Emit(ParticleSystem system, Vector3 pos, Vector3 velocity, float size, Color color)
        {
            system.Emit(new ParticleSystem.EmitParams
            {
                position = pos,
                velocity = velocity,
                startSize = size,
                startColor = color,
                applyShapeToPosition = false,
            }, 1);
        }

        private ParticleSystem MakeSystem(string name, float gravity, Vector2 life)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 600;
            var emission = system.emission;
            emission.enabled = false;
            var shape = system.shape;
            shape.enabled = false;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.2f));
            var color = system.colorOverLifetime;
            color.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1) });
            color.color = fade;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = MaterialLibrary.Particle;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            system.Play();
            return system;
        }
    }
}
