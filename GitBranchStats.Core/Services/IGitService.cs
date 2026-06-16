using System.Collections.Generic;
using GitBranchStats.Core.Models;
using LibGit2Sharp;

namespace GitBranchStats.Core.Services
{
    /// <summary>
    /// Abstraction over LibGit2Sharp for Git operations.
    /// </summary>
    public interface IGitService
    {
        /// <summary>Whether a git repository is currently open.</summary>
        bool IsRepositoryOpen { get; }

        /// <summary>Full path to the repository root.</summary>
        string RepositoryPath { get; }

        /// <summary>Get the current branch name.</summary>
        string GetCurrentBranch();

        /// <summary>Get all local branches with their info.</summary>
        List<BranchInfo> GetBranches();

        /// <summary>Switch to the specified branch.</summary>
        void CheckoutBranch(string branchName);

        /// <summary>Get all commits on the specified branch (from tip to base).</summary>
        IEnumerable<Commit> GetBranchCommits(string branchName);

        /// <summary>Get the diff patch between two branches (for comparison).</summary>
        Patch GetBranchPatch(string branchA, string branchB);

        /// <summary>Get unique commits reachable from branchA but not from branchB.</summary>
        IEnumerable<Commit> GetUniqueCommits(string branchA, string branchB);

        /// <summary>Open a git repository at the given path.</summary>
        bool OpenRepository(string path);

        /// <summary>Check if there are uncommitted changes.</summary>
        bool HasUncommittedChanges();

        /// <summary>Close the current repository.</summary>
        void CloseRepository();
    }
}
