using System.Collections.Generic;

namespace GitBranchStats.Core.Models
{
    /// <summary>
    /// Result of comparing two branches: commits ahead/behind and diff stats.
    /// </summary>
    public class BranchComparisonResult
    {
        public string BranchA { get; set; }
        public string BranchB { get; set; }
        public int CommitsAhead { get; set; }
        public int CommitsBehind { get; set; }
        public int FilesChanged { get; set; }
        public int Insertions { get; set; }
        public int Deletions { get; set; }
        public List<CommitInfo> UniqueCommits { get; set; } = new List<CommitInfo>();

        public override string ToString()
        {
            return $"{BranchA} vs {BranchB}: ahead {CommitsAhead}, behind {CommitsBehind}, " +
                   $"{FilesChanged} files, +{Insertions} -{Deletions}";
        }
    }

    /// <summary>
    /// Lightweight commit information for display.
    /// </summary>
    public class CommitInfo
    {
        public string Sha { get; set; }
        public string ShortSha => Sha?.Length > 7 ? Sha.Substring(0, 7) : Sha;
        public string AuthorName { get; set; }
        public string Message { get; set; }
        public System.DateTimeOffset Date { get; set; }
    }
}
