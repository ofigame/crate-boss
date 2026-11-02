using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CrateBoss.View
{
    /// <summary>
    /// Home screen backdrop (owner 2026-11-01 "arkada şehir, otoparkta yoğun arabalar, bulanık"): the city is photographed
    /// once without the crate and the worker, shrunk and grown back (soft), and shown on a screen-filling card just behind
    /// them; the real city is then put away. Cheap on every phone and on every quality tier (no depth of field needed).
    /// </summary>
    public static class BackdropBlur
    {
        private static GameObject card;
        private static RenderTexture picture;

        /// <param name="hide">Left out of the picture (the crate and the worker); shown again afterwards.</param>
        /// <param name="distance">From the camera to the card: just behind what stays sharp.</param>
        public static void Show(Camera cam, GameObject hide, float distance)
        {
            Clear();
            int w = Mathf.Max(64, Screen.width / 4), h = Mathf.Max(64, Screen.height / 4);
            var full = RenderTexture.GetTemporary(Mathf.Max(128, Screen.width / 2), Mathf.Max(128, Screen.height / 2), 24);
            var data = cam.GetUniversalAdditionalCameraData();
            bool post = data.renderPostProcessing;
            bool shown = hide.activeSelf;
            hide.SetActive(false);
            data.renderPostProcessing = false; // the card is drawn through the post effects again
            var target = cam.targetTexture;
            cam.targetTexture = full;
            cam.Render();
            cam.targetTexture = target;
            data.renderPostProcessing = post;
            hide.SetActive(shown);

            // Down to a sixteenth and back up to a half, bilinear each step: a smooth blur.
            var half = RenderTexture.GetTemporary(w / 2, h / 2, 0);
            var small = RenderTexture.GetTemporary(w / 4, h / 4, 0);
            full.filterMode = half.filterMode = small.filterMode = FilterMode.Bilinear;
            Graphics.Blit(full, half);
            Graphics.Blit(half, small);
            // Back up in halving steps (a single big step leaves blocky edges).
            var up = RenderTexture.GetTemporary(w / 2, h / 2, 0);
            up.filterMode = FilterMode.Bilinear;
            Graphics.Blit(small, up);
            var mid = RenderTexture.GetTemporary(w, h, 0);
            mid.filterMode = FilterMode.Bilinear;
            Graphics.Blit(up, mid);
            picture = new RenderTexture(w * 2, h * 2, 0) { filterMode = FilterMode.Bilinear, name = "HomeBackdrop" };
            Graphics.Blit(mid, picture);
            RenderTexture.ReleaseTemporary(up);
            RenderTexture.ReleaseTemporary(mid);
            RenderTexture.ReleaseTemporary(full);
            RenderTexture.ReleaseTemporary(half);
            RenderTexture.ReleaseTemporary(small);

            float height = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            card = Shapes.Quad(cam.transform, new Vector3(0f, 0f, distance), new Vector3(height * cam.aspect, height, 1f), Quaternion.identity,
                MaterialLibrary.Unlit(Color.white, picture), "Backdrop");
            Shapes.NoShadows(card);
        }

        public static void Clear()
        {
            if (card != null) Object.Destroy(card);
            card = null;
            if (picture != null) { picture.Release(); Object.Destroy(picture); }
            picture = null;
        }
    }
}
