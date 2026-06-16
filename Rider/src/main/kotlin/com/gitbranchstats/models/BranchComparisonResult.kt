package com.gitbranchstats.models

import java.time.LocalDate

data class CommitInfo(
    val sha: String,
    val authorName: String,
    val subject: String,
    val date: LocalDate
) {
    val shortSha: String get() = if (sha.length > 7) sha.substring(0, 7) else sha
}

data class BranchComparisonResult(
    val branchA: String,
    val branchB: String,
    val commitsAhead: Int,
    val commitsBehind: Int,
    val filesChanged: Int,
    val insertions: Int,
    val deletions: Int,
    val uniqueCommits: List<CommitInfo> = emptyList()
)
