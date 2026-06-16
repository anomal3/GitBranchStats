using System;
using GitBranchStats.Core.Localization;

namespace GitBranchStats.Core.Models
{
    /// <summary>
    /// Information about a git branch.
    /// </summary>
    public class BranchInfo
    {
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
        public string LastCommitMessage { get; set; }
        public DateTimeOffset LastCommitDate { get; set; }

        /// <summary>
        /// Display-friendly representation: branch name with "(current)" indicator if applicable.
        /// </summary>
        public string DisplayName => IsCurrent
            ? $"{Name} {LocalizationService.Instance.Get("Branch_CurrentSuffix")}"
            : Name;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
