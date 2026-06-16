using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GitBranchStats.Core.Localization;

namespace GitBranchStats.Core.ViewModels
{
    /// <summary>
    /// A single editable translation entry: an English source string and the
    /// translation the user provides for it.
    /// </summary>
    public class TranslationRow : ViewModelBase
    {
        private string _translation;

        public string Key { get; set; }
        public string English { get; set; }

        public string Translation
        {
            get => _translation;
            set => SetProperty(ref _translation, value);
        }
    }

    /// <summary>
    /// ViewModel for the translation editor window. Seeds rows from the English
    /// strings (optionally pre-filling an existing custom translation) and saves the
    /// result as a selectable custom language.
    /// </summary>
    public class TranslationEditorViewModel : ViewModelBase
    {
        private readonly LocalizationService _loc;
        private string _languageName;

        public TranslationEditorViewModel() : this(LocalizationService.Instance, null) { }

        public TranslationEditorViewModel(LocalizationService loc, LanguageInfo existing)
        {
            _loc = loc;
            Rows = new ObservableCollection<TranslationRow>();

            IReadOnlyDictionary<string, string> preset =
                (existing != null && !existing.IsBuiltIn) ? _loc.GetStrings(existing.Code) : null;

            if (existing != null && !existing.IsBuiltIn)
                _languageName = existing.DisplayName;

            foreach (var kv in _loc.EnglishStrings)
            {
                string value = null;
                preset?.TryGetValue(kv.Key, out value);
                Rows.Add(new TranslationRow
                {
                    Key = kv.Key,
                    English = kv.Value,
                    Translation = value
                });
            }
        }

        public ObservableCollection<TranslationRow> Rows { get; }

        public string LanguageName
        {
            get => _languageName;
            set => SetProperty(ref _languageName, value);
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(LanguageName);

        /// <summary>Persists the translation and makes it the active language.</summary>
        public LanguageInfo Save()
        {
            var map = Rows.ToDictionary(r => r.Key, r => r.Translation);
            return _loc.SaveCustomTranslation(LanguageName, map);
        }
    }
}
