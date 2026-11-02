using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Everything around the board (Ana Belge §2): sun and ambient light, a three-layer background
    /// (sky gradient, soft world silhouettes, slow particles) and the world seen past the void edge.
    /// </summary>
    public class WorldEnvironment : MonoBehaviour
    {
        private Transform root;
        private Transform sky;
        private Light sun;
        private ParticleSystem ambient;
        private Camera cam;

        public Light Sun => sun;

        /// <param name="seed">Level id: the district layout (house colours, stacks) stays the same for a level.</param>
        /// <param name="district">False for the home screen: sky, light and ground only, the home view draws its own city.</param>
        /// <summary>Home screen: the district and the ground below it go away once their soft picture stands behind (BackdropBlur).</summary>
        public void HideNear()
        {
            if (root == null) return;
            foreach (var name in new[] { "District", "Below", "DeepSea" })
            {
                var t = root.Find(name);
                if (t != null) t.gameObject.SetActive(false);
            }
        }

        public void Apply(WorldTheme theme, int width, int height, Camera camera, float particleScale, int seed = 0, bool district = true)
        {
            cam = camera;
            if (root != null) Destroy(root.gameObject);
            root = new GameObject("Environment").transform;
            root.SetParent(transform, false);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = theme.AmbientSky;
            RenderSettings.ambientEquatorColor = theme.AmbientEquator;
            RenderSettings.ambientGroundColor = theme.AmbientGround;
            RenderSettings.fog = false;

            SetupSun(theme);
            BuildSky(theme);
            BuildBelow(theme, width, height);
            BuildSilhouettes(theme, width, height);
            if (district) DistrictBuilder.Build(root, theme, width, height, seed);
            BuildParticles(theme, width, height, particleScale);
            if (theme.Lanterns) BuildLanterns(width, height);
        }

        private void LateUpdate()
        {
            if (sky == null || cam == null) return;
            // Keep the sky card filling the view behind everything.
            const float distance = 150f;
            float h = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.3f;
            sky.position = cam.transform.position + cam.transform.forward * distance;
            sky.rotation = cam.transform.rotation;
            sky.localScale = new Vector3(h * cam.aspect * 1.3f, h, 1f);
        }

        private void SetupSun(WorldTheme theme)
        {
            if (sun == null)
            {
                sun = FindAnyObjectByType<Light>();
                if (sun == null || sun.type != LightType.Directional) sun = new GameObject("Sun").AddComponent<Light>();
            }
            sun.type = LightType.Directional;
            sun.useColorTemperature = true;
            sun.colorTemperature = theme.SunTemperature;
            sun.color = Color.white;
            sun.intensity = theme.SunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.65f; // Ana Belge §2: soft, light shadows
            sun.shadowBias = 0.03f;
            sun.shadowNormalBias = 0.3f;
            // From the upper left at ~40°: shadows fall down-right, so it is clear which cell a box is on.
            sun.transform.rotation = Quaternion.Euler(40f, 135f, 0f);
        }

        private void BuildSky(WorldTheme theme)
        {
            var tex = new Texture2D(2, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "SkyGradient" };
            for (int y = 0; y < 64; y++)
            {
                float k = Mathf.SmoothStep(0f, 1f, y / 63f);
                var c = Color.Lerp(theme.SkyBottom, theme.SkyTop, k);
                tex.SetPixel(0, y, c);
                tex.SetPixel(1, y, c);
            }
            tex.Apply();
            var mat = MaterialLibrary.Unlit(Color.white, tex);
            mat.renderQueue = 1000; // drawn first, behind everything
            var quad = Shapes.Quad(root, Vector3.zero, Vector3.one, Quaternion.identity, mat, "Sky");
            Shapes.NoShadows(quad);
            sky = quad.transform;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = theme.SkyBottom;
                cam.farClipPlane = 300f;
            }
        }

        // The world under the board: yard concrete with lane lines, the sea at the pier's feet, a frosty hall floor, site dirt.
        private void BuildBelow(WorldTheme theme, int w, int h)
        {
            var center = new Vector3((w - 1) / 2f, theme.BelowHeight, (h - 1) / 2f);
            var below = Shapes.Cube(root, center, new Vector3(140f, 0.2f, 140f), MaterialLibrary.Shared(theme.Below, theme.Sea ? 0.9f : 0.15f), "Below");
            below.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            float top = theme.BelowHeight + 0.11f;
            var rng = new System.Random(theme.Id.GetHashCode());
            if (theme.Sea)
            {
                // Deeper water further out, and short white wave lines.
                Shapes.NoShadows(Shapes.Cube(root, new Vector3(center.x, top - 0.1f, h + 40f), new Vector3(140f, 0.2f, 60f), MaterialLibrary.Shared(theme.SeaDeep, 0.9f), "DeepSea"));
                var foam = MaterialLibrary.Unlit(new Color(1f, 1f, 1f, 0.45f));
                for (int i = 0; i < 40; i++)
                {
                    var p = new Vector3(center.x + ((float)rng.NextDouble() - 0.5f) * 36f, top + 0.01f, center.z + ((float)rng.NextDouble() - 0.45f) * 34f);
                    if (p.z > -1.5f) continue; // only the open water in front of the quay
                    var wave = Shapes.Cube(root, p, new Vector3(0.6f + (float)rng.NextDouble() * 1.2f, 0.01f, 0.05f), foam, "Wave");
                    Shapes.NoShadows(wave);
                }
                return;
            }
            switch (theme.Silhouettes)
            {
                case SilhouetteKind.ContainerStacks:
                    // Yellow lane lines on the yard far below.
                    var lane = MaterialLibrary.Shared(new Color(0.95f, 0.76f, 0.19f), 0.2f);
                    for (int i = -5; i <= 5; i++)
                        Shapes.NoShadows(Shapes.Cube(root, new Vector3(center.x + i * 4f, top, center.z), new Vector3(0.12f, 0.02f, 140f), lane, "Lane"));
                    break;
                case SilhouetteKind.ColdRacks:
                    var seam = MaterialLibrary.Shared(new Color(0.78f, 0.86f, 0.95f), 0.6f);
                    for (int i = -8; i <= 8; i++)
                    {
                        Shapes.NoShadows(Shapes.Cube(root, new Vector3(center.x + i * 3f, top, center.z), new Vector3(0.05f, 0.02f, 140f), seam, "Seam"));
                        Shapes.NoShadows(Shapes.Cube(root, new Vector3(center.x, top, center.z + i * 3f), new Vector3(140f, 0.02f, 0.05f), seam, "Seam"));
                    }
                    break;
                case SilhouetteKind.Scaffolding:
                    // Sand piles and scattered blocks.
                    var sand = MaterialLibrary.Shared(new Color(0.85f, 0.70f, 0.50f), 0.05f);
                    var block = MaterialLibrary.Shared(new Color(0.62f, 0.60f, 0.58f), 0.1f);
                    for (int i = 0; i < 10; i++)
                    {
                        var p = new Vector3(center.x + ((float)rng.NextDouble() - 0.5f) * 34f, top, center.z + ((float)rng.NextDouble() - 0.5f) * 30f);
                        if (p.x > -3f && p.x < w + 2f && p.z > -3f && p.z < h + 2f) continue;
                        if (i % 2 == 0) Shapes.Make(PrimitiveType.Sphere, root, p, new Vector3(3f, 1.2f, 2.4f), sand, "SandPile");
                        else Shapes.Cube(root, p + Vector3.up * 0.3f, new Vector3(1.2f, 0.6f, 0.6f), block, "Block");
                    }
                    break;
            }
        }

        // Middle layer: big, low-contrast shapes far behind the board (soft because they are far and pale).
        private void BuildSilhouettes(WorldTheme theme, int w, int h)
        {
            var mat = MaterialLibrary.Unlit(theme.Silhouette);
            var dark = MaterialLibrary.Unlit(Color.Lerp(theme.Silhouette, theme.SkyTop * 0.6f, 0.35f));
            var rng = new System.Random(theme.Id.GetHashCode() + 3);
            float ground = theme.BelowHeight;
            float baseZ = h + 14f;
            float cx = (w - 1) / 2f;
            if (theme.Silhouettes == SilhouetteKind.None) return; // the district itself fills the view
            switch (theme.Silhouettes)
            {
                case SilhouetteKind.ContainerStacks:
                    for (int i = 0; i < 10; i++)
                    {
                        float x = cx + (i - 4.5f) * 4.2f + ((float)rng.NextDouble() - 0.5f) * 1.5f;
                        float z = baseZ + (float)rng.NextDouble() * 10f;
                        int levels = 1 + rng.Next(4);
                        for (int l = 0; l < levels; l++)
                            Shapes.Cube(root, new Vector3(x + ((float)rng.NextDouble() - 0.5f) * 0.4f, ground + 0.75f + l * 1.5f, z), new Vector3(3.8f, 1.45f, 1.5f), l % 2 == 0 ? mat : dark, "Stack");
                    }
                    GantryCrane(cx + 9f, baseZ + 8f, ground, 1.2f, mat);
                    break;
                case SilhouetteKind.PortCranes:
                    // A container ship along the horizon and three ship-to-shore cranes.
                    float shipZ = baseZ + 12f;
                    Shapes.Cube(root, new Vector3(cx, ground + 1.2f, shipZ), new Vector3(34f, 2.4f, 4f), dark, "Hull");
                    for (int i = 0; i < 12; i++)
                        Shapes.Cube(root, new Vector3(cx - 15f + i * 2.6f, ground + 2.9f + rng.Next(3) * 0.5f, shipZ), new Vector3(2.4f, 1f + rng.Next(3) * 0.5f, 3.4f), mat, "Deck");
                    Shapes.Cube(root, new Vector3(cx + 14f, ground + 4.5f, shipZ), new Vector3(2.5f, 4f, 3f), mat, "Bridge");
                    for (int i = 0; i < 3; i++) GantryCrane(cx - 10f + i * 10f, baseZ + 4f + i * 2f, ground, 1.6f, i == 1 ? dark : mat);
                    break;
                case SilhouetteKind.ColdRacks:
                    for (int i = 0; i < 8; i++)
                    {
                        float x = cx + (i - 3.5f) * 6f;
                        float z = baseZ + (float)rng.NextDouble() * 6f;
                        float height = 9f + (float)rng.NextDouble() * 4f;
                        for (int s = -1; s <= 1; s += 2)
                            Shapes.Cube(root, new Vector3(x + s * 2f, ground + height / 2f, z), new Vector3(0.3f, height, 0.3f), mat, "RackPost");
                        for (float y = 1.5f; y < height; y += 2.2f)
                        {
                            Shapes.Cube(root, new Vector3(x, ground + y, z), new Vector3(4.3f, 0.25f, 1.2f), mat, "Shelf");
                            Shapes.Cube(root, new Vector3(x + ((float)rng.NextDouble() - 0.5f) * 2f, ground + y + 0.6f, z), new Vector3(1.4f, 1f, 1f), dark, "Pallet");
                        }
                    }
                    break;
                default:
                    // Scaffolding towers, a half-built frame and a tower crane.
                    for (int i = 0; i < 5; i++)
                    {
                        float x = cx + (i - 2f) * 8f + ((float)rng.NextDouble() - 0.5f) * 2f;
                        float z = baseZ + (float)rng.NextDouble() * 8f;
                        float height = 7f + (float)rng.NextDouble() * 6f;
                        for (int c = 0; c < 4; c++)
                            Shapes.Cube(root, new Vector3(x + (c % 2) * 3f, ground + height / 2f, z + (c / 2) * 1.5f), new Vector3(0.18f, height, 0.18f), mat, "Pole");
                        for (float y = 1.8f; y < height; y += 1.8f)
                            Shapes.Cube(root, new Vector3(x + 1.5f, ground + y, z + 0.75f), new Vector3(3.4f, 0.2f, 1.7f), dark, "Plank");
                    }
                    float tx = cx + 13f, tz = baseZ + 10f;
                    Shapes.Cube(root, new Vector3(tx, ground + 8f, tz), new Vector3(0.6f, 16f, 0.6f), mat, "TowerMast");
                    Shapes.Cube(root, new Vector3(tx - 5f, ground + 16f, tz), new Vector3(16f, 0.5f, 0.5f), mat, "TowerJib");
                    break;
            }
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                if (r.sharedMaterial == mat || r.sharedMaterial == dark) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // Ship-to-shore crane silhouette: two leg frames, a long boom and an A-frame on top.
        private void GantryCrane(float x, float z, float ground, float scale, Material mat)
        {
            float legH = 7f * scale, span = 3f * scale;
            for (int s = -1; s <= 1; s += 2)
            {
                Shapes.Cube(root, new Vector3(x + s * span / 2f, ground + legH / 2f, z - 1f), new Vector3(0.35f, legH, 0.35f), mat, "Leg");
                Shapes.Cube(root, new Vector3(x + s * span / 2f, ground + legH / 2f, z + 1f), new Vector3(0.35f, legH, 0.35f), mat, "Leg");
            }
            Shapes.Cube(root, new Vector3(x, ground + legH, z - 3f * scale), new Vector3(span + 0.6f, 0.5f, 12f * scale), mat, "Boom");
            var apex = Shapes.Cube(root, new Vector3(x, ground + legH + 1.6f * scale, z), new Vector3(0.3f, 3.4f * scale, 0.3f), mat, "Apex");
            apex.transform.localRotation = Quaternion.Euler(-20f, 0f, 0f);
        }

        // Near layer: slow dust motes, falling snow or rising sparks around the board.
        private void BuildParticles(WorldTheme theme, int w, int h, float scale)
        {
            var go = new GameObject("AmbientParticles");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3((w - 1) / 2f, 1.5f, (h - 1) / 2f);
            ambient = go.AddComponent<ParticleSystem>();
            ambient.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ambient.main;
            main.loop = true;
            main.startLifetime = 6f;
            main.startSize = theme.Particles == AmbientParticleKind.Snow ? new ParticleSystem.MinMaxCurve(0.05f, 0.1f) : new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
            main.startColor = theme.ParticleColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;
            main.startSpeed = 0f;
            main.prewarm = true;
            var emission = ambient.emission;
            emission.rateOverTime = 12f * scale;
            var shape = ambient.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(w + 4f, 3f, h + 4f);
            var velocity = ambient.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            float vy = theme.Particles == AmbientParticleKind.Snow ? -0.35f : theme.Particles == AmbientParticleKind.Sparks ? 0.3f : 0.03f;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.12f);
            velocity.y = new ParticleSystem.MinMaxCurve(vy * 0.7f, vy * 1.3f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            var color = ambient.colorOverLifetime;
            color.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.2f), new GradientAlphaKey(1, 0.8f), new GradientAlphaKey(0, 1) });
            color.color = fade;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = MaterialLibrary.Particle;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ambient.Play();
        }

        // Mine lanterns: two warm point lights on posts beside the board.
        private void BuildLanterns(int w, int h)
        {
            var post = MaterialLibrary.Shared(new Color(0.3f, 0.22f, 0.16f), 0.2f);
            var glass = MaterialLibrary.Create(new Color(1f, 0.75f, 0.35f), 0.8f);
            MaterialLibrary.SetGlow(glass, new Color(1f, 0.6f, 0.2f) * 2.5f);
            Vector3[] spots = { new Vector3(-1.3f, 0, h - 1.5f), new Vector3(w + 0.3f, 0, 1.5f) };
            foreach (var p in spots)
            {
                Shapes.Cube(root, p + new Vector3(0, -0.5f, 0), new Vector3(0.12f, 2.6f, 0.12f), post, "LanternPost");
                Shapes.Rounded(root, p + new Vector3(0, 0.95f, 0), new Vector3(0.28f, 0.34f, 0.28f), glass, 0.06f, "Lantern");
                var light = new GameObject("LanternLight").AddComponent<Light>();
                light.transform.SetParent(root, false);
                light.transform.localPosition = p + new Vector3(0, 0.95f, 0);
                light.type = LightType.Point;
                light.color = new Color(1f, 0.65f, 0.3f);
                light.range = 6f;
                light.intensity = 2.2f;
                light.shadows = LightShadows.None;
            }
        }
    }
}
