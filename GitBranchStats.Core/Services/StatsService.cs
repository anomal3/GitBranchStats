using System.Collections.Generic;
using System.Linq;
using GitBranchStats.Core.Models;
using LibGit2Sharp;

namespace GitBranchStats.Core.Services
{
    /// <summary>
    /// Computes statistics from git commits and patches.
    /// </summary>
    public class StatsService : IStatsService
    {
        /// <inheritdoc />
        public List<AuthorStats> GetAuthorStats(IEnumerable<Commit> commits)
        {
            return GetAuthorStats(commits, repository: null);
        }

        /// <summary>
        /// Overload that accepts a repository for computing per-commit diffs.
        /// </summary>
        public List<AuthorStats> GetAuthorStats(IEnumerable<Commit> commits, IRepository repository)
        {
            var result = commits
                .GroupBy(c => new { c.Author.Name, c.Author.Email })
                .Select(g =>
                {
                    var authorName = g.Key.Name;
                    var authorEmail = g.Key.Email;

                    int filesChanged = 0;
                    int additions = 0;
                    int deletions = 0;

                    foreach (var commit in g)
                    {
                        if (repository != null)
                        {
                            foreach (var parent in commit.Parents)
                            {
                                var patch = repository.Diff.Compare<Patch>(parent.Tree, commit.Tree);
                                filesChanged += patch.Count();
                                additions += SumAdditions(patch);
                                deletions += SumDeletions(patch);
                            }
                        }
                    }

                    return new AuthorStats
                    {
                        AuthorName = authorName,
                        AuthorEmail = authorEmail,
                        CommitCount = g.Count(),
                        FilesChanged = filesChanged,
                        Additions = additions,
                        Deletions = deletions
                    };
                })
                .OrderByDescending(s => s.CommitCount)
                .ToList();

            return result;
        }

        /// <inheritdoc />
        public BranchComparisonResult CalculateComparison(
            Patch patch,
            IEnumerable<Commit> uniqueCommitsA,
            IEnumerable<Commit> uniqueCommitsB,
            string branchA,
            string branchB)
        {
            var commitsA = uniqueCommitsA.ToList();
            var commitsB = uniqueCommitsB.ToList();

            var uniqueCommits = commitsA
                .Select(c => new CommitInfo
                {
                    Sha = c.Sha,
                    AuthorName = c.Author.Name,
                    Message = c.Message?.Trim() ?? "(no message)",
                    Date = c.Author.When
                })
                .ToList();

            return new BranchComparisonResult
            {
                BranchA = branchA,
                BranchB = branchB,
                CommitsAhead = commitsA.Count,
                CommitsBehind = commitsB.Count,
                FilesChanged = patch?.Count() ?? 0,
                Insertions = patch != null ? SumAdditions(patch) : 0,
                Deletions = patch != null ? SumDeletions(patch) : 0,
                UniqueCommits = uniqueCommits
            };
        }

        /// <summary>
        /// Sum added lines across all patch entries.
        /// </summary>
        private static int SumAdditions(Patch patch)
        {
            return patch.Sum(entry => entry.AddedLines.Count);
        }

        /// <summary>
        /// Sum deleted lines across all patch entries.
        /// </summary>
        private static int SumDeletions(Patch patch)
        {
            return patch.Sum(entry => entry.DeletedLines.Count);
        }
    }
}
