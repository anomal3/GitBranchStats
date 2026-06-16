package com.gitbranchstats.services

import com.gitbranchstats.models.AuthorStats
import com.gitbranchstats.models.BranchComparisonResult
import com.gitbranchstats.models.CommitInfo
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import git4idea.branch.GitBrancher
import git4idea.commands.Git
import git4idea.commands.GitCommand
import git4idea.commands.GitLineHandler
import git4idea.repo.GitRepositoryManager
import java.time.LocalDate
import java.time.format.DateTimeParseException

class GitStatsService {

    fun getRepositoryRoot(project: Project): VirtualFile? =
        GitRepositoryManager.getInstance(project).repositories.firstOrNull()?.root

    fun getCurrentBranch(project: Project, root: VirtualFile): String =
        GitRepositoryManager.getInstance(project).getRepositoryForRoot(root)
            ?.currentBranchName ?: ""

    fun getBranches(project: Project, root: VirtualFile): List<String> =
        GitRepositoryManager.getInstance(project).getRepositoryForRoot(root)
            ?.branches?.localBranches
            ?.map { it.name }
            ?.sorted()
            ?: emptyList()

    fun checkoutBranch(project: Project, root: VirtualFile, branchName: String) {
        val repo = GitRepositoryManager.getInstance(project).getRepositoryForRoot(root) ?: return
        GitBrancher.getInstance(project).checkout(branchName, false, listOf(repo), null)
    }

    fun getAuthorStats(project: Project, root: VirtualFile, branchName: String): List<AuthorStats> {
        val handler = GitLineHandler(project, root, GitCommand.LOG)
        handler.addParameters(
            "--pretty=tformat:COMMIT||%ae||%an",
            "--shortstat",
            branchName
        )

        val result = Git.getInstance().runCommand(handler)
        if (!result.success()) return emptyList()

        return parseAuthorStats(result.output)
    }

    fun getDailyCommitData(
        project: Project,
        root: VirtualFile,
        branchName: String,
        daysBack: Int
    ): Map<String, Map<LocalDate, Int>> {
        val handler = GitLineHandler(project, root, GitCommand.LOG)
        handler.addParameters(
            "--pretty=tformat:%ae||%an||%cd",
            "--date=short"
        )
        if (daysBack < Int.MAX_VALUE) {
            handler.addParameters("--after=${LocalDate.now().minusDays(daysBack.toLong())}")
        }
        handler.addParameters(branchName)

        val result = Git.getInstance().runCommand(handler)
        if (!result.success()) return emptyMap()

        val data = mutableMapOf<String, MutableMap<LocalDate, Int>>()

        for (line in result.output) {
            if (line.isBlank()) continue
            val parts = line.split("||")
            if (parts.size < 3) continue
            val name = parts[1].trim().ifEmpty { parts[0].trim() }
            val dateStr = parts[2].trim()
            val date = try {
                LocalDate.parse(dateStr)
            } catch (_: DateTimeParseException) {
                continue
            }
            data.getOrPut(name) { mutableMapOf() }.merge(date, 1, Int::plus)
        }

        return data
    }

    fun getBranchComparison(
        project: Project,
        root: VirtualFile,
        branchA: String,
        branchB: String
    ): BranchComparisonResult {
        val git = Git.getInstance()

        val aheadHandler = GitLineHandler(project, root, GitCommand.LOG)
        aheadHandler.addParameters(
            "--pretty=tformat:%H||%ae||%an||%s||%cd",
            "--date=short",
            "$branchB..$branchA"
        )
        val aheadResult = git.runCommand(aheadHandler)

        val behindHandler = GitLineHandler(project, root, GitCommand.LOG)
        behindHandler.addParameters("--oneline", "$branchA..$branchB")
        val behindResult = git.runCommand(behindHandler)

        val diffHandler = GitLineHandler(project, root, GitCommand.DIFF)
        diffHandler.addParameters("--shortstat", "$branchB...$branchA")
        val diffResult = git.runCommand(diffHandler)

        val uniqueCommits = parseCommitInfos(aheadResult.output)
        val commitsBehind = behindResult.output.count { it.isNotBlank() }

        var filesChanged = 0
        var insertions = 0
        var deletions = 0
        diffResult.output.firstOrNull { it.contains("changed") }?.let { line ->
            filesChanged = parseFiles(line)
            insertions = parseInsertions(line)
            deletions = parseDeletions(line)
        }

        return BranchComparisonResult(
            branchA = branchA,
            branchB = branchB,
            commitsAhead = uniqueCommits.size,
            commitsBehind = commitsBehind,
            filesChanged = filesChanged,
            insertions = insertions,
            deletions = deletions,
            uniqueCommits = uniqueCommits
        )
    }

    // --- parsers ---

    private data class MutableAuthorData(
        val name: String,
        val email: String,
        var commitCount: Int = 0,
        var filesChanged: Int = 0,
        var additions: Int = 0,
        var deletions: Int = 0
    )

    private fun parseAuthorStats(lines: List<String>): List<AuthorStats> {
        val map = mutableMapOf<String, MutableAuthorData>()
        var currentEmail = ""

        for (line in lines) {
            when {
                line.startsWith("COMMIT||") -> {
                    val parts = line.split("||")
                    currentEmail = parts.getOrNull(1)?.trim() ?: ""
                    val name = parts.getOrNull(2)?.trim() ?: ""
                    if (currentEmail.isNotEmpty()) {
                        map.getOrPut(currentEmail) { MutableAuthorData(name, currentEmail) }
                            .commitCount++
                    }
                }

                line.contains("changed") && currentEmail.isNotEmpty() -> {
                    val data = map[currentEmail] ?: continue
                    data.filesChanged += parseFiles(line)
                    data.additions += parseInsertions(line)
                    data.deletions += parseDeletions(line)
                }
            }
        }

        return map.values
            .map { d ->
                AuthorStats(d.name, d.email, d.commitCount, d.filesChanged, d.additions, d.deletions)
            }
            .sortedByDescending { it.commitCount }
    }

    private fun parseCommitInfos(lines: List<String>): List<CommitInfo> =
        lines.filter { it.isNotBlank() }.mapNotNull { line ->
            val parts = line.split("||")
            if (parts.size < 5) return@mapNotNull null
            val date = try { LocalDate.parse(parts[4].trim()) } catch (_: Exception) { LocalDate.now() }
            CommitInfo(
                sha = parts[0].trim(),
                authorName = parts[2].trim(),
                subject = parts[3].trim(),
                date = date
            )
        }

    private fun parseFiles(line: String): Int =
        Regex("""(\d+) files? changed""").find(line)?.groupValues?.get(1)?.toIntOrNull() ?: 0

    private fun parseInsertions(line: String): Int =
        Regex("""(\d+) insertions?\(\+\)""").find(line)?.groupValues?.get(1)?.toIntOrNull() ?: 0

    private fun parseDeletions(line: String): Int =
        Regex("""(\d+) deletions?\(-\)""").find(line)?.groupValues?.get(1)?.toIntOrNull() ?: 0
}
