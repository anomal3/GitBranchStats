using System;
using GitBranchStats.Core.Models;
using Xunit;

namespace GitBranchStats.Tests
{
    /// <summary>
    /// Unit tests for model classes.
    /// </summary>
    public class ModelsTests
    {
        [Fact]
        public void AuthorStats_Defaults_AreZeroOrNull()
        {
            var stats = new AuthorStats();
            Assert.Null(stats.AuthorName);
            Assert.Equal(0, stats.CommitCount);
            Assert.Equal(0, stats.FilesChanged);
            Assert.Equal(0, stats.Additions);
            Assert.Equal(0, stats.Deletions);
        }

        [Fact]
        public void BranchInfo_DisplayName_ShowsCurrentIndicator()
        {
            var branch = new BranchInfo { Name = "main", IsCurrent = true };
            Assert.Equal("main (current)", branch.DisplayName);
        }

        [Fact]
        public void BranchInfo_DisplayName_NoIndicatorWhenNotCurrent()
        {
            var branch = new BranchInfo { Name = "develop", IsCurrent = false };
            Assert.Equal("develop", branch.DisplayName);
        }

        [Fact]
        public void CommitInfo_ShortSha_TruncatesTo7Chars()
        {
            var commit = new CommitInfo { Sha = "abcdef1234567890" };
            Assert.Equal("abcdef1", commit.ShortSha);
        }

        [Fact]
        public void CommitInfo_ShortSha_NullSha_ReturnsNull()
        {
            var commit = new CommitInfo { Sha = null };
            Assert.Null(commit.ShortSha);
        }

        [Fact]
        public void BranchComparisonResult_Defaults_UniqueCommitsNotEmpty()
        {
            var result = new BranchComparisonResult();
            Assert.NotNull(result.UniqueCommits);
            Assert.Empty(result.UniqueCommits);
        }
    }
}
