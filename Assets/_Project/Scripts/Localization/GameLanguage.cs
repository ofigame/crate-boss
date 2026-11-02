namespace CrateBoss.Localization
{
    /// <summary>One of the nine languages (Ana Belge §18): code, its own name for the settings list, writing direction.</summary>
    public sealed class GameLanguage
    {
        public string Code;
        /// <summary>Written in the language itself (Türkçe, English, العربية ...).</summary>
        public string NativeName;
        /// <summary>.NET culture for upper case and the like (Turkish İ/I rule).</summary>
        public string Culture;
        public bool RightToLeft;
    }
}
