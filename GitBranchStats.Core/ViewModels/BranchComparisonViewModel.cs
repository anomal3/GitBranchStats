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
    /// ViewModel for the branch comparison section.
    /// </summary>
    public class BranchComparisonViewModel : ViewModelBase
    {
        private readonly IGitService _gitService;
        private readonly IStatsService _statsService;

        private BranchInfo _branchA;
        private BranchInfo _branchB;
        private BranchComparisonResult _result;
        private bool _showResult;
        private bool _isLoading;
        private string _statusMessage;

        public BranchComparisonViewModel(IGitService gitService, IStatsService statsService)
        {
            _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
            _statsService = statsService ?? throw new ArgumentNullException(nameof(statsService));

            Branches = new ObservableCollection<BranchInfo>();
            CompareCommand = new RelayCommand(async _ => await CompareAsync(), _ => CanCompare());
        }

        // --- Collections ---

        public ObservableCollection<BranchInfo> Branches { get; }

        // --- Properties ---

        public BranchInfo BranchA
        {
            get => _branchA;
            set
            {
                SetProperty(ref _branchA, value);
                ((RelayCommand)CompareCommand).RaiseCanExecuteChanged();
            }
        }

        public BranchInfo BranchB
        {
            get => _branchB;
            set
            {
                SetProperty(ref _branchB, value);
                ((RelayCommand)CompareCommand).RaiseCanExecuteChanged();
            }
        }

        public BranchComparisonResult Result
        {
            get => _result;
            private set => SetProperty(ref _result, value);
        }

        public bool ShowResult
        {
            get => _showResult;
            private set => SetProperty(ref _showResult, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                SetProperty(ref _isLoading, value);
                ((RelayCommand)CompareCommand).RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // --- Commands ---

        public ICommand CompareCommand { get; }

        // --- Methods ---

        private bool CanCompare()
        {
            return !IsLoading
                   && BranchA != null
                   && BranchB != null
                   && BranchA.Name != BranchB.Name;
        }

        private async Task CompareAsync()
        {
            if (BranchA == null || BranchB == null) return;

            try
            {
                IsLoading = true;
                ShowResult = false;
                StatusMessage = LocalizationService.Instance.Format("Status_Comparing", BranchA.Name, BranchB.Name);

                var result = await Task.Run(() =>
                {
                    var patch = _gitService.GetBranchPatch(BranchA.Name, BranchB.Name);
                    var commitsA = _gitService.GetUniqueCommits(BranchA.Name, BranchB.Name);
                    var commitsB = _gitService.GetUniqueCommits(BranchB.Name, BranchA.Name);
                    return _statsService.CalculateComparison(patch, commitsA, commitsB, BranchA.Name, BranchB.Name);
                });

                Result = result;
                ShowResult = true;
                StatusMessage = LocalizationService.Instance.Format("Status_Compared", BranchA.Name, BranchB.Name);
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationService.Instance.Format("Status_ErrorComparing", ex.Message);
                ShowResult = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
