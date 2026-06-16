using System;
using System.IO;
using GitBranchStats.Core.Services;
using GitBranchStats.Core.ViewModels;
using Microsoft.VisualStudio.Shell;
using GitBranchStats.UI.Views;

namespace GitBranchStats
{
    /// <summary>
    /// Tool window that displays Git branch statistics for the current solution's repository.
    /// </summary>
    public class GitBranchStatsToolWindow : ToolWindowPane
    {
        private readonly BranchStatsViewModel _viewModel;

        /// <summary>
        /// Parameterless constructor required by the VS shell, which instantiates tool
        /// windows via Activator.CreateInstance. Dependencies are created here because
        /// they have no dependency on Visual Studio services.
        /// </summary>
        public GitBranchStatsToolWindow() : base(null)
        {
            var gitService = new GitService();
            var statsService = new StatsService();
            _viewModel = new BranchStatsViewModel(gitService, statsService);

            Caption = "Git Branch Stats";

            Content = new BranchStatsView
            {
                DataContext = _viewModel
            };
        }

        /// <summary>
        /// Called when the tool window is created. Attempts to open the Git repo for the current solution.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();
            TryOpenSolutionRepository();
        }

        private async void TryOpenSolutionRepository()
        {
            try
            {
                var dte = ServiceProvider.GlobalProvider
                    .GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

                string solutionDir = null;
                if (dte?.Solution?.FullName != null)
                {
                    solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
                }

                if (!string.IsNullOrEmpty(solutionDir))
                {
                    await _viewModel.OpenRepositoryAsync(solutionDir);
                }
                else
                {
                    _viewModel.StatusMessage = "No solution is open.";
                }
            }
            catch (Exception ex)
            {
                _viewModel.StatusMessage = $"Error: {ex.Message}";
            }
        }
    }
}
