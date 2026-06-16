using System.Windows;
using GitBranchStats.Core.Localization;
using GitBranchStats.Core.ViewModels;
using Microsoft.VisualStudio.PlatformUI;

namespace GitBranchStats.UI.Views
{
    /// <summary>
    /// Modal editor that lets the user provide their own translation of every English
    /// string and save it as a new, selectable language. Derives from the VS-themed
    /// <see cref="DialogWindow"/> so it follows the active Visual Studio theme.
    /// </summary>
    public partial class TranslationEditorWindow : DialogWindow
    {
        private readonly TranslationEditorViewModel _viewModel;

        public TranslationEditorWindow() : this(null) { }

        public TranslationEditorWindow(LanguageInfo existing)
        {
            InitializeComponent();
            _viewModel = new TranslationEditorViewModel(LocalizationService.Instance, existing);
            DataContext = _viewModel;
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            // Commit a cell that may still be in edit mode.
            Grid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            if (string.IsNullOrWhiteSpace(_viewModel.LanguageName))
            {
                MessageBox.Show(this,
                    LocalizationService.Instance.Get("Editor_NameRequired"),
                    LocalizationService.Instance.Get("Editor_Title"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _viewModel.Save();
            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
