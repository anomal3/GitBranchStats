using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitBranchStats.Core.Models;
using LibGit2Sharp;

namespace GitBranchStats.Core.Services
{
    /// <summary>
    /// LibGit2Sharp-based implementation of IGitService.
    /// </summary>
    public class GitService : IGitService, IDisposable
    {
        private IRepository _repository;

        public bool IsRepositoryOpen => _repository != null;

        public string RepositoryPath => _repository?.Info?.Path?.Replace(".git/", "") ?? string.Empty;

        /// <inheritdoc />
        public string GetCurrentBranch()
        {
            ThrowIfNotOpen();
            return _repository.Head.FriendlyName;
        }

        /// <inheritdoc />
        public List<BranchInfo> GetBranches()
        {
            ThrowIfNotOpen();
            var currentBranch = GetCurrentBranch();

            return _repository.Branches
                .Where(b => !b.IsRemote)
                .Select(b => new BranchInfo
                {
                    Name = b.FriendlyName,
                    IsCurrent = b.FriendlyName == currentBranch,
                    LastCommitMessage = b.Tip?.Message?.Trim() ?? "(no commits)",
                    LastCommitDate = b.Tip?.Author?.When ?? DateTimeOffset.MinValue
                })
                .OrderByDescending(b => b.IsCurrent)
                .ThenBy(b => b.Name)
                .ToList();
        }

        /// <inheritdoc />
        public void CheckoutBranch(string branchName)
        {
            ThrowIfNotOpen();
            var branch = _repository.Branches[branchName];
            if (branch == null)
                throw new ArgumentException($"Branch '{branchName}' not found.", nameof(branchName));

            var checkoutOptions = new CheckoutOptions
            {
                CheckoutModifiers = CheckoutModifiers.Force
            };

            Commands.Checkout(_repository, branch, checkoutOptions);
        }

        /// <inheritdoc />
        public IEnumerable<Commit> GetBranchCommits(string branchName)
        {
            ThrowIfNotOpen();
            var branch = _repository.Branches[branchName];
            if (branch == null)
                throw new ArgumentException($"Branch '{branchName}' not found.", nameof(branchName));

            return branch.Commits.ToList();
        }

        /// <inheritdoc />
        public Patch GetBranchPatch(string branchA, string branchB)
        {
            ThrowIfNotOpen();
            var branchAObj = _repository.Branches[branchA];
            var branchBObj = _repository.Branches[branchB];
            if (branchAObj == null || branchBObj == null)
                throw new ArgumentException("One or both branches not found.");
            if (branchAObj.Tip == null || branchBObj.Tip == null)
                throw new ArgumentException("One or both branches have no commits.");

            return _repository.Diff.Compare<Patch>(
                branchAObj.Tip.Tree,
                branchBObj.Tip.Tree);
        }

        /// <inheritdoc />
        public IEnumerable<Commit> GetUniqueCommits(string branchA, string branchB)
        {
            ThrowIfNotOpen();
            var branchAObj = _repository.Branches[branchA];
            var branchBObj = _repository.Branches[branchB];
            if (branchAObj == null || branchBObj == null)
                throw new ArgumentException("One or both branches not found.");

            var filter = new CommitFilter
            {
                IncludeReachableFrom = branchAObj.Tip,
                ExcludeReachableFrom = branchBObj.Tip
            };

            return _repository.Commits.QueryBy(filter).ToList();
        }

        /// <inheritdoc />
        public bool OpenRepository(string path)
        {
            try
            {
                CloseRepository();

                if (!Directory.Exists(path))
                    return false;

                string repoPath = LibGit2Sharp.Repository.Discover(path);
                if (string.IsNullOrEmpty(repoPath))
                    return false;
                _repository = new LibGit2Sharp.Repository(repoPath);
                return true;
            }
            catch (RepositoryNotFoundException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool HasUncommittedChanges()
        {
            ThrowIfNotOpen();
            var status = _repository.RetrieveStatus();
            return status.IsDirty;
        }

        /// <inheritdoc />
        public void CloseRepository()
        {
            _repository?.Dispose();
            _repository = null;
        }

        private void ThrowIfNotOpen()
        {
            if (_repository == null)
                throw new InvalidOperationException("No repository is open. Call OpenRepository first.");
        }

        public void Dispose()
        {
            CloseRepository();
        }

        /// <summary>
        /// Internal access to the repository for StatsService to compute per-commit diffs.
        /// </summary>
        internal IRepository Repository => _repository;
    }
}
