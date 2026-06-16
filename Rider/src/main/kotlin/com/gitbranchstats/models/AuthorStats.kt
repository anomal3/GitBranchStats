package com.gitbranchstats.models

data class AuthorStats(
    val authorName: String,
    val authorEmail: String,
    val commitCount: Int,
    val filesChanged: Int,
    val additions: Int,
    val deletions: Int
)
