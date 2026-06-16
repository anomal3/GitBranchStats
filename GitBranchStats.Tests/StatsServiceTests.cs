using System.Collections.Generic;
using System.Linq;
using GitBranchStats.Core.Models;
using GitBranchStats.Core.Services;
using Xunit;

namespace GitBranchStats.Tests
{
    /// <summary>
    /// Unit tests for StatsService.
    /// </summary>
    public class StatsServiceTests
    {
        private readonly StatsService _service = new StatsService();

        [Fact]
        public void GetAuthorStats_EmptyInput_ReturnsEmptyList()
        {
            var result = _service.GetAuthorStats(new List<LibGit2Sharp.Commit>());
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CalculateComparison_NullPatch_ReturnsZeros()
        {
            var result = _service.CalculateComparison(
                patch: null,
                uniqueCommitsA: Enumerable.Empty<LibGit2Sharp.Commit>(),
                uniqueCommitsB: Enumerable.Empty<LibGit2Sharp.Commit>(),
                branchA: "main",
                branchB: "feature");

            Assert.Equal("main", result.BranchA);
            Assert.Equal("feature", result.BranchB);
            Assert.Equal(0, result.CommitsAhead);
            Assert.Equal(0, result.CommitsBehind);
            Assert.Equal(0, result.FilesChanged);
            Assert.Equal(0, result.Insertions);
            Assert.Equal(0, result.Deletions);
        }

        [Fact]
        public void CalculateComparison_PreservesBranchNames()
        {
            var result = _service.CalculateComparison(
                patch: null,
                uniqueCommitsA: Enumerable.Empty<LibGit2Sharp.Commit>(),
                uniqueCommitsB: Enumerable.Empty<LibGit2Sharp.Commit>(),
                branchA: "develop",
                branchB: "release-1.0");

            Assert.Equal("develop", result.BranchA);
            Assert.Equal("release-1.0", result.BranchB);
        }
    }
}
