using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Draws the bought items (<see cref="Outfit"/>) on the models as simple placeholder pieces until real ones exist:
    /// a hard hat or flat cap over the worker's own helmet, a vest, gloves; a tinted crate; a collar; a basket rim.
    /// Sizes follow the model's own height, so a new worker model keeps them in proportion.
    /// </summary>
    public static class OutfitDresser
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public static void DressWorker(Transform root, Animator animator)
        {
            if (root == null || animator == null || !animator.isHuman) return;
            if (Outfit.Hat == null && Outfit.Vest == null && Outfit.Gloves == null) return;
            var skin = root.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin == null) return;
            var bounds = skin.bounds;
            float h = bounds.size.y;
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var chest = animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? animator.GetBoneTransform(HumanBodyBones.Chest) ?? animator.GetBoneTransform(HumanBodyBones.Spine);

            if (Outfit.Hat is Color hat && head != null)
            {
                var mat = MaterialLibrary.Shared(hat, Outfit.Cap ? 0.15f : 0.55f);
                var top = new Vector3(head.position.x, bounds.max.y, head.position.z);
                if (Outfit.Cap)
                {
                    Piece(PrimitiveType.Sphere, head, top - Vector3.up * 0.05f * h, new Vector3(0.27f, 0.11f, 0.27f) * h, mat, "Cap");
                    Piece(PrimitiveType.Cylinder, head, top - Vector3.up * 0.075f * h + root.forward * 0.1f * h, new Vector3(0.2f, 0.008f, 0.14f) * h, mat, "CapPeak");
                }
                else
                {
                    Piece(PrimitiveType.Sphere, head, top - Vector3.up * 0.07f * h, new Vector3(0.29f, 0.17f, 0.29f) * h, mat, "HardHat");
                    Piece(PrimitiveType.Cylinder, head, top - Vector3.up * 0.1f * h + root.forward * 0.02f * h, new Vector3(0.34f, 0.01f, 0.34f) * h, mat, "HardHatBrim");
                }
            }
            if (Outfit.Vest is Color vest && chest != null)
                Piece(PrimitiveType.Cylinder, chest, chest.position, new Vector3(0.3f, 0.09f, 0.22f) * h, MaterialLibrary.Shared(vest, 0.2f), "Vest");
            if (Outfit.Gloves is Color gloves)
            {
                var mat = MaterialLibrary.Shared(gloves, 0.2f);
                foreach (var bone in new[] { HumanBodyBones.LeftHand, HumanBodyBones.RightHand })
                {
                    var hand = animator.GetBoneTransform(bone);
                    if (hand != null) Piece(PrimitiveType.Sphere, hand, hand.position, Vector3.one * 0.075f * h, mat, "Glove");
                }
            }
        }

        /// <summary>Crate skins (Ana Belge §13) colour the plain wooden crates only; ice, steel and gold keep their meaning.</summary>
        public static void DressCrate(Transform body)
        {
            if (Outfit.CrateSkin is Color skin) DressCrate(body, skin);
        }

        /// <summary>Tints a crate body with a skin colour and adds the ribbon (also the shop picture).</summary>
        public static void DressCrate(Transform body, Color skin)
        {
            if (body == null) return;
            var block = new MaterialPropertyBlock();
            foreach (var r in body.GetComponentsInChildren<Renderer>())
            {
                r.GetPropertyBlock(block);
                var tint = Color.Lerp(Color.white, skin, 0.7f);
                block.SetColor(BaseColor, tint);
                block.SetColor(ColorId, tint);
                r.SetPropertyBlock(block);
            }
            // A ribbon cross on the lid, so the skin reads as a different crate and not just a colour.
            var ribbon = MaterialLibrary.Shared(Color.Lerp(skin, Color.white, 0.75f), 0.4f);
            Shapes.Cube(body, new Vector3(0, 0.505f, 0), new Vector3(0.16f, 0.02f, 1.02f), ribbon, "Ribbon");
            Shapes.Cube(body, new Vector3(0, 0.505f, 0), new Vector3(1.02f, 0.02f, 0.16f), ribbon, "Ribbon");
        }

        // A primitive placed at a world position and world size, then hung on a bone so it follows the animation.
        private static void Piece(PrimitiveType type, Transform parent, Vector3 worldPos, Vector3 worldSize, Material material, string name)
        {
            var go = Shapes.Make(type, null, Vector3.zero, Vector3.one, material, name);
            go.transform.position = worldPos;
            go.transform.rotation = parent.root.rotation;
            go.transform.localScale = worldSize;
            go.transform.SetParent(parent, true);
        }
    }
}
