using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GitBranchStats.Core.Localization;
using GitBranchStats.Core.Models;
using GitBranchStats.Core.Services;

namespace GitBranchStats.Core.ViewModels
{
    /// <summary>
    /// Main ViewModel for the Branch Stats Tool Window.
    /// </summary>
    public class BranchStatsViewModel : ViewModelBase
    {
        private readonly IGitService _gitService;
        private readonly IStatsService _statsService;

        private string _currentBranchName;
        private BranchInfo _currentBranch;
        private BranchInfo _selectedBranchForSwitch;
        private string _repositoryPath;
        private string _statusMessage;
        private bool _isLoading;

        public BranchStatsViewModel(IGitService gitService, IStatsService statsService)
        {
            _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
            _statsService = statsService ?? throw new ArgumentNullException(nameof(statsService));

            Branches = new ObservableCollection<BranchInfo>();
            AuthorStats = new ObservableCollection<AuthorStats>();
            Comparison = new BranchComparisonViewModel(gitService, statsService);

            RefreshCommand = new RelayCommand(async _ => await RefreshAsync(), _ => !IsLoading);
            SwitchBranchCommand = new RelayCommand(async _ => await SwitchBranchAsync(), _ => CanSwitchBranch());
        }

        // --- Collections ---

        public ObservableCollection<BranchInfo> Branches { get; }
        public ObservableCollection<AuthorStats> AuthorStats { get; }

        // --- Properties ---

        public string RepositoryPath
        {
            get => _repositoryPath;
            private set => SetProperty(ref _repositoryPath, value);
        }

        public BranchInfo CurrentBranch
        {
            get => _currentBranch;
            private set
            {
                if (SetProperty(ref _currentBranch, value))
                {
                    _currentBranchName = value?.Name;
                    OnPropertyChanged(nameof(CurrentBranchName));
                }
            }
        }

        public string CurrentBranchName
        {
            get => _currentBranchName;
            private set => SetProperty(ref _currentBranchName, value);
        }

        public BranchInfo SelectedBranchForSwitch
        {
            get => _selectedBranchForSwitch;
            set
            {
                SetProperty(ref _selectedBranchForSwitch, value);
                ((RelayCommand)SwitchBranchCommand).RaiseCanExecuteChanged();
            }
        }

        public BranchComparisonViewModel Comparison { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                SetProperty(ref _isLoading, value);
                ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SwitchBranchCommand).RaiseCanExecuteChanged();
            }
        }

        // --- Commands ---

        public ICommand RefreshCommand { get; }
        public ICommand SwitchBranchCommand { get; }

        // --- Events ---

        /// <summary>
        /// Raised when an operation fails and the UI should show a dialog.
        /// Arguments: (title, message).
        /// </summary>
        public event Action<string, string> ErrorReported;

        // --- Methods ---

        private bool CanSwitchBranch()
        {
            return !IsLoading
                   && SelectedBranchForSwitch != null
                   && SelectedBranchForSwitch.Name != CurrentBranchName;
        }

        private async Task SwitchBranchAsync()
        {
            if (SelectedBranchForSwitch == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = LocalizationService.Instance.Format("Status_Switching", SelectedBranchForSwitch.Name);

                if (_gitService.HasUncommittedChanges())
                {
                    StatusMessage = LocalizationService.Instance.Format("Status_SwitchWarning", SelectedBranchForSwitch.Name);
                }

                await Task.Run(() => _gitService.CheckoutBranch(SelectedBranchForSwitch.Name));

                await RefreshAsync();
                StatusMessage = LocalizationService.Instance.Format("Status_Switched", SelectedBranchForSwitch.Name);
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationService.Instance.Format("Status_ErrorSwitch", ex.Message);
                RaiseSwitchError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Builds a friendly, localized message for a failed checkout and raises
        /// <see cref="ErrorReported"/> so the UI can show a dialog.
        /// </summary>
        private void RaiseSwitchError(Exception ex)
        {
            string title = LocalizationService.Instance.Get("Dialog_SwitchErrorTitle");

            bool filesLocked = ex is LibGit2Sharp.LibGit2SharpException && (
                ex.Message.IndexOf("rmdir", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ex.Message.IndexOf("in use", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ex.Message.IndexOf("locked", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ex.Message.IndexOf("занят", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ex.Message.IndexOf("доступ", StringComparison.OrdinalIgnoreCase) >= 0);

            string message = filesLocked
                ? LocalizationService.Instance.Get("Dialog_FilesLocked")
                : LocalizationService.Instance.Format("Dialog_SwitchErrorGeneric", ex.Message);

            ErrorReported?.Invoke(title, message);
        }

        /// <summary>
        /// Try to open a Git repository at the given path and load data.
        /// </summary>
        public async Task OpenRepositoryAsync(string path)
        {
            try
            {
                StatusMessage = LocalizationService.Instance.Get("Status_OpeningRepo");
                var opened = await Task.Run(() => _gitService.OpenRepository(path));
                if (opened)
                {
                    await RefreshAsync();
                }
                else
                {
                    StatusMessage = LocalizationService.Instance.Get("Status_NoRepo");
                    Branches.Clear();
                    AuthorStats.Clear();
                    CurrentBranchName = null;
                    RepositoryPath = path;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationService.Instance.Format("Status_ErrorOpening", ex.Message);
            }
        }

        public async Task RefreshAsync()
        {
            if (!_gitService.IsRepositoryOpen) return;

            try
            {
                IsLoading = true;

                var branchName = await Task.Run(() => _gitService.GetCurrentBranch());
                var branches = await Task.Run(() => _gitService.GetBranches());

                Branches.Clear();
                foreach (var b in branches)
                    Branches.Add(b);

                CurrentBranch = Branches.FirstOrDefault(b => b.IsCurrent);
                RepositoryPath = _gitService.RepositoryPath;

                Comparison.Branches.Clear();
                foreach (var b in branches)
                    Comparison.Branches.Add(b);
                Comparison.BranchA = branches.FirstOrDefault(b => b.IsCurrent);
                Comparison.BranchB = branches.FirstOrDefault(b => !b.IsCurrent);

                await LoadAuthorStatsAsync(branchName);
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationService.Instance.Format("Status_ErrorLoading", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAuthorStatsAsync(string branchName)
        {
            var commits = await Task.Run(() => _gitService.GetBranchCommits(branchName));
            var stats = await Task.Run(() =>
            {
                // Use the repository-aware overload if the service supports it
                var gitSvc = _gitService as GitService;
                if (gitSvc != null && _statsService is StatsService ss)
                {
                    return ss.GetAuthorStats(commits, gitSvc.Repository);
                }
                return _statsService.GetAuthorStats(commits);
            });

            AuthorStats.Clear();
            foreach (var s in stats)
                AuthorStats.Add(s);
        }
    }
}
