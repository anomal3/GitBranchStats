using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitBranchStats
{
    /// <summary>
    /// VSPackage that registers the Git Branch Stats tool window and menu command.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(GitBranchStatsPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(GitBranchStatsToolWindow))]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class GitBranchStatsPackage : AsyncPackage
    {
        public const string PackageGuidString = "e8b4d4a0-5f2e-4d1a-9c8b-3a6f7e2d4c1b";

        // Command set and command IDs (must match the .vsct file)
        public const string UiCmdSetGuidString = "a1c2e3f4-5b6d-4c7e-9f0a-1b2c3d4e5f6a";
        public const int CmdIdShowBranchStatsWindow = 0x0100;

        /// <summary>
        /// Initialization: register the Show Tool Window menu command.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Register the menu commands that show the tool windows
            var cmdSet = new Guid(UiCmdSetGuidString);
            var menuCommandService = (OleMenuCommandService)GetService(typeof(IMenuCommandService));
            if (menuCommandService != null)
            {
                menuCommandService.AddCommand(new MenuCommand(
                    ShowToolWindow, new CommandID(cmdSet, CmdIdShowBranchStatsWindow)));
            }
        }

        /// <summary>
        /// Show the Git Branch Stats tool window.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // AsyncPackage tool windows must be shown through the async-aware
            // ShowToolWindowAsync. The synchronous FindToolWindow path fails inside
            // the VS shell (ToolWindowListener.SubscribeForEvents) on background-loaded
            // packages.
            JoinableTaskFactory.RunAsync(async () =>
            {
                ToolWindowPane window = await ShowToolWindowAsync(
                    typeof(GitBranchStatsToolWindow),
                    id: 0,
                    create: true,
                    cancellationToken: DisposalToken);

                if (window?.Frame == null)
                    throw new NotSupportedException("Cannot create the Git Branch Stats tool window.");
            }).FileAndForget("GitBranchStats/ShowToolWindow");
        }

    }
}
