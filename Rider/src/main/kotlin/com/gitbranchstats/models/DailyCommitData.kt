package com.gitbranchstats.models

import java.time.LocalDate

data class DailyCommitData(
    val date: LocalDate,
    val commitsByAuthor: Map<String, Int>
)
