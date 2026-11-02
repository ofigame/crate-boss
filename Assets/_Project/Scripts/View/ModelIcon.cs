using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CrateBoss.View
{
    /// <summary>
    /// A UI sprite photographed from one of the game's models (owner 2026-11-01: the hammer on the HUD is the hammer model).
    /// The model is put far below the world on its own layer, lit by the scene's sun, shot once on a clear background and
    /// kept. Null when the model is missing.
    /// </summary>
    public static class ModelIcon
    {
        private const int Layer = 31;
        private static readonly Dictionary<string, Sprite> made = new Dictionary<string, Sprite>();

        /// <param name="rotation">How the model is turned toward the camera (which looks along +z).</param>
        public static Sprite Get(string key, Quaternion rotation, int size = 256)
        {
            if (made.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            var stage = new GameObject("ModelIcon " + key);
            stage.transform.position = new Vector3(0f, -500f, 0f);
            var model = VisualCatalog.TryInstantiate(key, stage.transform);
            if (model == null) { Object.Destroy(stage); return null; }
            model.transform.localRotation = rotation;
            foreach (var t in stage.GetComponentsInChildren<Transform>()) t.gameObject.layer = Layer;

            // Frame the model's bounds from the front.
            var renderers = model.GetComponentsInChildren<Renderer>();
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            var cam = new GameObject("IconCamera").AddComponent<Camera>();
            cam.transform.SetParent(stage.transform, false);
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(b.extents.x, b.extents.y) * 1.08f;
            cam.transform.position = b.center - Vector3.forward * (b.extents.z + 5f);
            cam.transform.rotation = Quaternion.identity;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = b.size.z + 10f;
            cam.cullingMask = 1 << Layer;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = false;
            data.antialiasing = AntialiasingMode.None;
            var rt = RenderTexture.GetTemporary(size, size, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            cam.targetTexture = rt;
            cam.Render();

            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Icon " + key, wrapMode = TextureWrapMode.Clamp };
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            cam.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);
            Object.Destroy(stage);

            sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            made[key] = sprite;
            return sprite;
        }

        /// <summary>The hammer model, head up and to the right, handle down to the left, a little turned to show its depth.</summary>
        public static Sprite Hammer => Get("Hammer", Quaternion.Euler(0f, 0f, 315f) * Quaternion.Euler(20f, 0f, 0f)) ?? UI.IconFactory.ToyHammer;
    }
}
