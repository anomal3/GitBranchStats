using System.Collections.Generic;
using GitBranchStats.Core.Models;
using LibGit2Sharp;

namespace GitBranchStats.Core.Services
{
    /// <summary>
    /// Service for computing author and branch comparison statistics.
    /// </summary>
    public interface IStatsService
    {
        /// <summary>
        /// Aggregate commit statistics by author for the given commits.
        /// </summary>
        List<AuthorStats> GetAuthorStats(IEnumerable<Commit> commits);

        /// <summary>
        /// Calculate comparison result between two branches.
        /// </summary>
        BranchComparisonResult CalculateComparison(
            Patch patch,
            IEnumerable<Commit> uniqueCommitsA,
            IEnumerable<Commit> uniqueCommitsB,
            string branchA,
            string branchB);
    }
}
