namespace GitBranchStats.Core.Localization
{
    /// <summary>
    /// Describes a language available for the extension UI.
    /// </summary>
    public class LanguageInfo
    {
        public LanguageInfo(string code, string displayName, bool isBuiltIn)
        {
            Code = code;
            DisplayName = displayName;
            IsBuiltIn = isBuiltIn;
        }

        /// <summary>Stable identifier, e.g. "en", "ru" or a custom slug.</summary>
        public string Code { get; }

        /// <summary>Human-readable name shown in the language picker.</summary>
        public string DisplayName { get; }

        /// <summary>True for the shipped en/ru languages that cannot be deleted.</summary>
        public bool IsBuiltIn { get; }

        public override string ToString() => DisplayName;
    }
}
