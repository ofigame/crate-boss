using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Colour materials cloned from the base materials in Resources/Materials
    /// (kept as assets so their shader variants ship in builds).
    /// </summary>
    public static class MaterialLibrary
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        private static Material baseLit, baseTransparent, baseUnlit, baseUnlitTransparent, baseParticle;
        private static readonly Dictionary<(Color, float, float), Material> shared = new Dictionary<(Color, float, float), Material>();

        private static Material BaseLit => baseLit ??= Load("Lit", "Universal Render Pipeline/Lit");
        private static Material BaseTransparent => baseTransparent ??= Load("LitTransparent", "Universal Render Pipeline/Lit");
        private static Material BaseUnlit => baseUnlit ??= Load("Unlit", "Universal Render Pipeline/Unlit");
        private static Material BaseUnlitTransparent => baseUnlitTransparent ??= Load("UnlitTransparent", "Universal Render Pipeline/Unlit");
        public static Material Particle => baseParticle ??= Load("Particle", "Universal Render Pipeline/Particles/Unlit");

        /// <summary>A shared material per colour/finish (never modify it).</summary>
        public static Material Shared(Color color, float smoothness = 0.25f, float metallic = 0f)
        {
            var key = (color, smoothness, metallic);
            if (shared.TryGetValue(key, out var m) && m != null) return m;
            m = Create(color, smoothness, metallic);
            shared[key] = m;
            return m;
        }

        // ---------- toy finish (Ana Belge §2): moulded plastic, never metal, a soft sheen ----------

        public const float ToySmoothMin = 0.45f, ToySmoothMax = 0.55f, ToyNormalScale = 0.4f;
        private static readonly int BumpScaleId = Shader.PropertyToID("_BumpScale");

        /// <summary>Metallic 0, smoothness pulled into 0.45–0.55 (glass and water may shine a little more).</summary>
        public static float ToySmoothness(float smoothness) => smoothness >= 0.7f ? 0.7f : Mathf.Clamp(smoothness, ToySmoothMin, ToySmoothMax);

        /// <summary>Softens an imported model to the toy finish: no metal, plastic sheen, gentler normal maps (Tripo's relief).</summary>
        /// One toy copy per model material, shared by every instance (a copy per crate made hundreds of materials and frame spikes).
        /// Code that changes one instance's look takes renderer.material, which still makes its own copy.
        public static void MakeToy(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = ToyOf(mats[i]);
                r.sharedMaterials = mats;
            }
        }

        private static readonly Dictionary<Material, Material> toys = new Dictionary<Material, Material>();

        private static Material ToyOf(Material source)
        {
            if (source == null) return null;
            if (toys.TryGetValue(source, out var m) && m != null) return m;
            m = new Material(source) { name = source.name + " (toy)" };
            if (m.HasProperty(MetallicId)) m.SetFloat(MetallicId, 0f);
            if (m.HasProperty(SmoothnessId)) m.SetFloat(SmoothnessId, ToySmoothness(m.GetFloat(SmoothnessId)));
            if (m.HasProperty(BumpScaleId)) m.SetFloat(BumpScaleId, ToyNormalScale);
            toys[source] = m;
            return m;
        }

        /// <summary>A material of its own, for colour or glow changes. Transparent when alpha &lt; 1.</summary>
        public static Material Create(Color color, float smoothness = 0.25f, float metallic = 0f)
        {
            var m = new Material(color.a < 0.999f ? BaseTransparent : BaseLit);
            m.SetColor(BaseColorId, color);
            m.SetFloat(SmoothnessId, ToySmoothness(smoothness));
            m.SetFloat(MetallicId, 0f); // Ana Belge §2: no metal look anywhere
            SetGlow(m, Color.black);
            return m;
        }

        /// <summary>A lit material that can fade (front walls): always in the transparent queue.</summary>
        public static Material CreateFadable(Color color, float smoothness = 0.25f)
        {
            var m = new Material(BaseTransparent);
            m.SetColor(BaseColorId, color);
            m.SetFloat(SmoothnessId, ToySmoothness(smoothness));
            SetGlow(m, Color.black);
            return m;
        }

        /// <summary>Unlit (not affected by light): background, glows, screens.</summary>
        public static Material Unlit(Color color, Texture texture = null)
        {
            var m = new Material(color.a < 0.999f || texture != null ? BaseUnlitTransparent : BaseUnlit);
            m.SetColor(BaseColorId, color);
            if (texture != null) m.SetTexture(BaseMapId, texture);
            return m;
        }

        public static void SetColor(Material m, Color color) => m.SetColor(BaseColorId, color);
        public static Color GetColor(Material m) => m.GetColor(BaseColorId);
        public static void SetTexture(Material m, Texture texture) => m.SetTexture(BaseMapId, texture);

        public static void SetGlow(Material m, Color glow)
        {
            m.SetColor(EmissionId, glow);
            m.EnableKeyword("_EMISSION");
        }

        private static Material Load(string name, string fallbackShader)
        {
            var m = Resources.Load<Material>($"Materials/{name}");
            return m != null ? m : new Material(Shader.Find(fallbackShader));
        }
    }
}
