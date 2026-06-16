package com.gitbranchstats.ui.panels

import com.gitbranchstats.services.GitStatsService
import com.gitbranchstats.ui.chart.CommitTimelineChart
import com.intellij.openapi.progress.ProgressIndicator
import com.intellij.openapi.progress.ProgressManager
import com.intellij.openapi.progress.Task
import com.intellij.openapi.project.Project
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.components.JBScrollPane
import com.intellij.util.ui.JBUI
import java.awt.BorderLayout
import java.awt.FlowLayout
import java.time.LocalDate
import javax.swing.*

class CommitChartPanel(
    private val project: Project,
    private val service: GitStatsService
) : JPanel(BorderLayout()) {

    private val chart = CommitTimelineChart()
    private val rangeBox = JComboBox(arrayOf("Last 30 days", "Last 90 days", "Last year", "All time"))
    private val refreshBtn = JButton("Refresh")
    private val statusLabel = JLabel("").apply { font = font.deriveFont(11f) }
    private val spinner = JProgressBar().apply { isIndeterminate = true; isVisible = false }

    private var root: VirtualFile? = null
    private var branch: String = ""

    init {
        border = JBUI.Borders.empty(8)

        val controls = JPanel(FlowLayout(FlowLayout.LEFT, 6, 4))
        controls.add(JLabel("Period:"))
        controls.add(rangeBox)
        controls.add(refreshBtn)
        controls.add(spinner)

        val bottom = JPanel(FlowLayout(FlowLayout.LEFT, 6, 2))
        bottom.add(statusLabel)

        add(controls, BorderLayout.NORTH)
        add(JBScrollPane(chart).apply { border = JBUI.Borders.empty() }, BorderLayout.CENTER)
        add(bottom, BorderLayout.SOUTH)

        refreshBtn.addActionListener { loadData() }
        rangeBox.addActionListener { loadData() }
    }

    fun setRepository(root: VirtualFile, branch: String) {
        this.root = root
        this.branch = branch
    }

    fun updateData(data: Map<String, Map<LocalDate, Int>>, daysBack: Int) {
        val start = if (daysBack == Int.MAX_VALUE) {
            data.values.flatMap { it.keys }.minOrNull() ?: LocalDate.now().minusDays(30)
        } else {
            LocalDate.now().minusDays(daysBack.toLong())
        }
        chart.setData(data, start, LocalDate.now())
        val authorCount = data.size
        val totalCommits = data.values.sumOf { it.values.sum() }
        statusLabel.text = "$authorCount author(s), $totalCommits commit(s) in selected period"
    }

    fun loadData() {
        val r = root ?: return
        val days = when (rangeBox.selectedIndex) {
            0 -> 30
            1 -> 90
            2 -> 365
            else -> Int.MAX_VALUE
        }

        spinner.isVisible = true
        refreshBtn.isEnabled = false
        statusLabel.text = "Loading…"

        ProgressManager.getInstance().run(object : Task.Backgroundable(project, "Loading commit chart") {
            private var result: Map<String, Map<LocalDate, Int>> = emptyMap()

            override fun run(indicator: ProgressIndicator) {
                indicator.text = "Reading commit history…"
                result = service.getDailyCommitData(project, r, branch, days)
            }

            override fun onSuccess() {
                ApplicationManager.getApplication().invokeLater {
                    updateData(result, days)
                    spinner.isVisible = false
                    refreshBtn.isEnabled = true
                }
            }

            override fun onThrowable(error: Throwable) {
                ApplicationManager.getApplication().invokeLater {
                    statusLabel.text = "Error: ${error.message}"
                    spinner.isVisible = false
                    refreshBtn.isEnabled = true
                }
            }
        })
    }
}
