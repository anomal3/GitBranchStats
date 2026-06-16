using System.Windows;
using System.Windows.Controls;
using GitBranchStats.Core.ViewModels;

namespace GitBranchStats.UI.Views
{
    /// <summary>
    /// Main view for the Branch Stats Tool Window.
    /// </summary>
    public partial class BranchStatsView : UserControl
    {
        private BranchStatsViewModel _viewModel;

        public BranchStatsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.ErrorReported -= OnErrorReported;

            _viewModel = DataContext as BranchStatsViewModel;

            if (_viewModel != null)
                _viewModel.ErrorReported += OnErrorReported;
        }

        private void OnErrorReported(string title, string message)
        {
            var dialog = new MessageDialog(title, message);
            dialog.ShowModal();
        }
    }
}
