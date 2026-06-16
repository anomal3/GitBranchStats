namespace GitBranchStats.Core.Models
{
    /// <summary>
    /// Aggregated commit statistics for a single author.
    /// </summary>
    public class AuthorStats
    {
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public int CommitCount { get; set; }
        public int FilesChanged { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }

        public override string ToString()
        {
            return $"{AuthorName}: {CommitCount} commits, {FilesChanged} files, +{Additions} -{Deletions}";
        }
    }
}
