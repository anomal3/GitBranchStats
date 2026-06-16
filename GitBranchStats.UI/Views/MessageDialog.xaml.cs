using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace GitBranchStats.UI.Views
{
    /// <summary>
    /// Simple themed modal message dialog used to report errors with a friendly text.
    /// </summary>
    public partial class MessageDialog : DialogWindow
    {
        public MessageDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
