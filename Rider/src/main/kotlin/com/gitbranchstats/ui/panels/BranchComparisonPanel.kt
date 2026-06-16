package com.gitbranchstats.ui.panels

import com.gitbranchstats.models.BranchComparisonResult
import com.gitbranchstats.models.CommitInfo
import com.gitbranchstats.services.GitStatsService
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.progress.ProgressIndicator
import com.intellij.openapi.progress.ProgressManager
import com.intellij.openapi.progress.Task
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.components.JBScrollPane
import com.intellij.ui.table.JBTable
import com.intellij.util.ui.JBUI
import java.awt.BorderLayout
import java.awt.Color
import java.awt.FlowLayout
import java.awt.GridBagConstraints
import java.awt.GridBagLayout
import javax.swing.*
import javax.swing.table.DefaultTableModel

class BranchComparisonPanel(
    private val project: Project,
    private val service: GitStatsService
) : JPanel(BorderLayout()) {

    private val branchABox = JComboBox<String>()
    private val branchBBox = JComboBox<String>()
    private val compareBtn = JButton("Compare")
    private val spinner = JProgressBar().apply { isIndeterminate = true; isVisible = false }

    // Summary labels
    private val aheadLabel = JLabel("—")
    private val behindLabel = JLabel("—")
    private val filesLabel = JLabel("—")
    private val addLabel = JLabel("—").apply { foreground = Color(0x4EC9B0) }
    private val delLabel = JLabel("—").apply { foreground = Color(0xE06C75) }

    private val commitsModel = object : DefaultTableModel(arrayOf("SHA", "Author", "Date", "Subject"), 0) {
        override fun isCellEditable(r: Int, c: Int) = false
    }
    private val commitsTable = JBTable(commitsModel).apply {
        setShowGrid(false)
        rowHeight = 24
        columnModel.getColumn(0).preferredWidth = 70
        columnModel.getColumn(1).preferredWidth = 130
        columnModel.getColumn(2).preferredWidth = 85
        columnModel.getColumn(3).preferredWidth = 350
    }

    private val statusLabel = JLabel("").apply { font = font.deriveFont(11f) }

    private var root: VirtualFile? = null

    init {
        border = JBUI.Borders.empty(8)

        val selPanel = JPanel(FlowLayout(FlowLayout.LEFT, 6, 4))
        selPanel.add(JLabel("Branch A:"))
        selPanel.add(branchABox.apply { preferredSize = java.awt.Dimension(180, 26) })
        selPanel.add(JLabel("vs  Branch B:"))
        selPanel.add(branchBBox.apply { preferredSize = java.awt.Dimension(180, 26) })
        selPanel.add(compareBtn)
        selPanel.add(spinner)

        val summaryPanel = buildSummaryPanel()

        val center = JPanel(BorderLayout())
        center.add(summaryPanel, BorderLayout.NORTH)
        center.add(JBScrollPane(commitsTable), BorderLayout.CENTER)

        add(selPanel, BorderLayout.NORTH)
        add(center, BorderLayout.CENTER)
        add(statusLabel, BorderLayout.SOUTH)

        compareBtn.addActionListener { startCompare() }
    }

    private fun buildSummaryPanel(): JPanel {
        val panel = JPanel(GridBagLayout())
        panel.border = BorderFactory.createTitledBorder("Summary")
        val gbc = GridBagConstraints().apply {
            insets = JBUI.insets(2, 6)
            anchor = GridBagConstraints.WEST
        }

        fun addRow(label: String, value: JLabel, row: Int) {
            gbc.gridx = 0; gbc.gridy = row; panel.add(JLabel(label), gbc)
            gbc.gridx = 1; gbc.gridy = row; panel.add(value, gbc)
        }

        addRow("Commits ahead:", aheadLabel, 0)
        addRow("Commits behind:", behindLabel, 1)
        addRow("Files changed:", filesLabel, 2)
        addRow("Insertions:", addLabel, 3)
        addRow("Deletions:", delLabel, 4)

        return panel
    }

    fun updateBranches(branches: List<String>, currentBranch: String) {
        branchABox.removeAllItems()
        branchBBox.removeAllItems()
        branches.forEach {
            branchABox.addItem(it)
            branchBBox.addItem(it)
        }
        branchABox.selectedItem = currentBranch
        val other = branches.firstOrNull { it != currentBranch }
        if (other != null) branchBBox.selectedItem = other
    }

    fun setRoot(root: VirtualFile) {
        this.root = root
    }

    private fun startCompare() {
        val r = root ?: return
        val a = branchABox.selectedItem as? String ?: return
        val b = branchBBox.selectedItem as? String ?: return
        if (a == b) {
            statusLabel.text = "Select two different branches."
            return
        }

        spinner.isVisible = true
        compareBtn.isEnabled = false
        statusLabel.text = "Comparing $a vs $b…"

        ProgressManager.getInstance().run(object : Task.Backgroundable(project, "Comparing branches") {
            private var result: BranchComparisonResult? = null

            override fun run(indicator: ProgressIndicator) {
                indicator.text = "Running git diff…"
                result = service.getBranchComparison(project, r, a, b)
            }

            override fun onSuccess() {
                ApplicationManager.getApplication().invokeLater {
                    result?.let { showResult(it) }
                    spinner.isVisible = false
                    compareBtn.isEnabled = true
                }
            }

            override fun onThrowable(error: Throwable) {
                ApplicationManager.getApplication().invokeLater {
                    statusLabel.text = "Error: ${error.message}"
                    spinner.isVisible = false
                    compareBtn.isEnabled = true
                }
            }
        })
    }

    private fun showResult(r: BranchComparisonResult) {
        aheadLabel.text = "${r.commitsAhead}"
        behindLabel.text = "${r.commitsBehind}"
        filesLabel.text = "${r.filesChanged}"
        addLabel.text = "+${r.insertions}"
        delLabel.text = "−${r.deletions}"

        commitsModel.rowCount = 0
        r.uniqueCommits.forEach { c: CommitInfo ->
            commitsModel.addRow(arrayOf(c.shortSha, c.authorName, c.date.toString(), c.subject))
        }

        statusLabel.text = "Done — ${r.branchA} is ${r.commitsAhead} ahead, ${r.commitsBehind} behind ${r.branchB}"
    }
}
