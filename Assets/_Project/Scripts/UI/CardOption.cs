using System;
using UnityEngine;

namespace CrateBoss.UI
{
    /// <summary>One button on a MessageCard: its text, what it does, whether it can be pressed, its colour.</summary>
    public readonly struct CardOption
    {
        public readonly string Label;
        public readonly Action OnClick;
        public readonly bool Enabled;
        public readonly Color Color;
        public readonly IconKind? Icon;

        public CardOption(string label, Action onClick, Color color, bool enabled = true, IconKind? icon = null)
        {
            Label = label;
            OnClick = onClick;
            Color = color;
            Enabled = enabled;
            Icon = icon;
        }
    }
}
