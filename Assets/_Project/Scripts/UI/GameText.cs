using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// Plain Text that handles Arabic (Ana Belge §18): letters joined, lines in visual order, left-aligned text
    /// right-aligned. Wrapping labels are broken into lines here (on the logical text, then each line shaped), because Unity
    /// would wrap the already reversed string from the wrong end. Other languages pass straight through.
    /// </summary>
    public sealed class GameText : Text
    {
        private string logical;
        private float laidOutWidth = -1;

        public override string text
        {
            get => base.text;
            set
            {
                logical = value;
                Apply();
            }
        }

        private void Apply()
        {
            if (!Loc.IsRtl || !ArabicShaper.HasArabic(logical))
            {
                base.text = logical;
                return;
            }
            alignment = Mirror(alignment);
            float width = rectTransform.rect.width;
            laidOutWidth = width;
            base.text = horizontalOverflow == HorizontalWrapMode.Wrap && width > 1 ? WrapRtl(logical, width) : ArabicShaper.ShapeLines(logical);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (logical != null && Loc.IsRtl && horizontalOverflow == HorizontalWrapMode.Wrap && Mathf.Abs(rectTransform.rect.width - laidOutWidth) > 0.5f)
                Apply();
        }

        private string WrapRtl(string source, float width)
        {
            var settings = GetGenerationSettings(new Vector2(float.MaxValue, float.MaxValue));
            var generator = cachedTextGeneratorForLayout;
            float Width(string line) => generator.GetPreferredWidth(ArabicShaper.ShapeLine(line), settings) / pixelsPerUnit;

            var result = new System.Text.StringBuilder();
            foreach (var paragraph in source.Split('\n'))
            {
                string line = "";
                foreach (var word in paragraph.Split(' '))
                {
                    string tryLine = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && Width(tryLine) > width)
                    {
                        result.Append(ArabicShaper.ShapeLine(line)).Append('\n');
                        line = word;
                    }
                    else line = tryLine;
                }
                result.Append(ArabicShaper.ShapeLine(line)).Append('\n');
            }
            return result.ToString().TrimEnd('\n');
        }

        private static TextAnchor Mirror(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAnchor.UpperRight;
                case TextAnchor.MiddleLeft: return TextAnchor.MiddleRight;
                case TextAnchor.LowerLeft: return TextAnchor.LowerRight;
                default: return a;
            }
        }
    }
}
