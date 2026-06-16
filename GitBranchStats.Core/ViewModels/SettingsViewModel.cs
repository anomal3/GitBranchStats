using System.Collections.Generic;
using System.Linq;
using GitBranchStats.Core.Localization;

namespace GitBranchStats.Core.ViewModels
{
    /// <summary>
    /// ViewModel for the settings tool window: language selection.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly LocalizationService _loc;

        public SettingsViewModel() : this(LocalizationService.Instance) { }

        public SettingsViewModel(LocalizationService loc)
        {
            _loc = loc;
            _loc.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LocalizationService.AvailableLanguages))
                    OnPropertyChanged(nameof(Languages));
                if (e.PropertyName == nameof(LocalizationService.CurrentLanguage))
                    OnPropertyChanged(nameof(SelectedLanguage));
            };
        }

        public IReadOnlyList<LanguageInfo> Languages => _loc.AvailableLanguages.ToList();

        public LanguageInfo SelectedLanguage
        {
            get => _loc.AvailableLanguages.FirstOrDefault(l => l.Code == _loc.CurrentLanguage);
            set
            {
                if (value != null)
                    _loc.SetLanguage(value.Code);
                OnPropertyChanged();
            }
        }
    }
}
