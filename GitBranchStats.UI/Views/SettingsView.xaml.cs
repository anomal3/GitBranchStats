using System.Windows;
using System.Windows.Controls;
using GitBranchStats.Core.Localization;
using GitBranchStats.Core.ViewModels;

namespace GitBranchStats.UI.Views
{
    /// <summary>
    /// Settings panel: language selection and access to the translation editor.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView()
        {
            InitializeComponent();
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;
        }

        private void OnCreateTranslation(object sender, RoutedEventArgs e)
        {
            ShowEditor(existing: null);
        }

        private void OnEditTranslation(object sender, RoutedEventArgs e)
        {
            // Pre-fill from the currently selected language when it is a custom one.
            var selected = _viewModel.SelectedLanguage;
            ShowEditor(existing: (selected != null && !selected.IsBuiltIn) ? selected : null);
        }

        private void ShowEditor(LanguageInfo existing)
        {
            // DialogWindow.ShowModal() shows the editor modally with correct VS ownership
            // and theming.
            var editor = new TranslationEditorWindow(existing);
            editor.ShowModal();
        }
    }
}
