using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CrateBoss.View
{
    /// <summary>
    /// One global post-processing volume (Ana Belge §2): ACES tone mapping, gentle bloom, light vignette,
    /// per-world colour grading, tilt-shift style depth of field on High, and a blur behind the win card.
    /// </summary>
    public class PostEffects : MonoBehaviour
    {
        private VolumeProfile profile;
        private ColorAdjustments grading;
        private DepthOfField dof;
        private Vignette vignette;
        private float focusDistance = 15f;
        private bool blurred;

        public void Build(Camera cam)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.None; // MSAA from the pipeline asset

            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.Neutral); // Ana Belge §2: clean candy colours (ACES darkened and dulled them)
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(0.15f); // Ana Belge §2: very light
            bloom.scatter.Override(0.6f);
            vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.06f);
            vignette.smoothness.Override(0.5f);
            grading = profile.Add<ColorAdjustments>(true);
            dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Off);
        }

        public void ApplyTheme(WorldTheme theme)
        {
            grading.saturation.Override(theme.Saturation);
            grading.contrast.Override(theme.Contrast);
            grading.postExposure.Override(theme.PostExposure);
            grading.colorFilter.Override(theme.ColorFilter);
        }

        /// <summary>Distance from camera to the board centre; the in-focus band sits there.</summary>
        public void SetFocus(float distance)
        {
            focusDistance = distance;
            if (!blurred) ApplyDof();
        }

        public void Refresh()
        {
            if (!blurred) ApplyDof();
        }

        /// <summary>Win card: blur the scene behind it (Medium/High) and darken the edges a little more.</summary>
        public IEnumerator BlurBackground(bool on, float duration = 0.35f)
        {
            blurred = on;
            bool canBlur = QualityManager.Current != QualityTier.Low;
            if (on && canBlur)
            {
                dof.mode.Override(DepthOfFieldMode.Gaussian);
                dof.gaussianStart.Override(0f);
                dof.gaussianEnd.Override(focusDistance * 0.35f);
                dof.highQualitySampling.Override(true);
            }
            float from = on ? 0f : 1f, to = on ? 1f : 0f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.Lerp(from, to, t / duration);
                if (on && canBlur) dof.gaussianMaxRadius.Override(Mathf.Lerp(0.01f, 1.5f, k));
                vignette.intensity.Override(Mathf.Lerp(0.06f, 0.3f, k));
                yield return null;
            }
            vignette.intensity.Override(on ? 0.3f : 0.06f);
            if (!on) ApplyDof();
        }

        // Tilt-shift: only a thin band around the board is sharp; the near and far edges of the view go soft.
        private void ApplyDof()
        {
            if (!QualityManager.DepthOfField)
            {
                dof.mode.Override(DepthOfFieldMode.Off);
                return;
            }
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(focusDistance);
            dof.focalLength.Override(55f);
            dof.aperture.Override(9f);
            dof.bladeCount.Override(6);
        }
    }
}
