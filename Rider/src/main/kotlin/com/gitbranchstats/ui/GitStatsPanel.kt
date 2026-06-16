package com.gitbranchstats.ui

import com.gitbranchstats.services.GitStatsService
import com.gitbranchstats.services.LocalizationService
import com.gitbranchstats.ui.panels.AuthorStatsPanel
import com.gitbranchstats.ui.panels.BranchComparisonPanel
import com.gitbranchstats.ui.panels.CommitChartPanel
import com.gitbranchstats.ui.panels.SettingsPanel
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.progress.ProgressIndicator
import com.intellij.openapi.progress.ProgressManager
import com.intellij.openapi.progress.Task
import com.intellij.openapi.project.Project
import com.intellij.openapi.startup.StartupManager
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.components.JBScrollPane
import com.intellij.util.ui.JBUI
import git4idea.repo.GitRepositoryManager
import java.awt.BorderLayout
import java.awt.FlowLayout
import java.time.LocalDate
import javax.swing.*

class GitStatsPanel(private val project: Project) : JPanel(BorderLayout()) {

    private val service = GitStatsService()

    private val authorStatsPanel = AuthorStatsPanel()
    private val chartPanel = CommitChartPanel(project, service)
    private val comparisonPanel = BranchComparisonPanel(project, service)
    private val settingsPanel = SettingsPanel()

    private val branchLabel = JLabel("—").apply { font = font.deriveFont(java.awt.Font.BOLD, 13f) }
    private val repoLabel = JLabel("").apply {
        font = font.deriveFont(11f)
        foreground = UIManager.getColor("Label.disabledForeground")
    }
    private val refreshBtn = JButton("⟳ ${LocalizationService.t("btn.refresh")}")
    private val branchBox = JComboBox<String>()
    private val switchBtn = JButton(LocalizationService.t("btn.switch"))
    private val branchHeaderLabel = JLabel(LocalizationService.t("label.branch"))
    private val switchToLabel = JLabel(LocalizationService.t("label.switch_to"))
    private val spinner = JProgressBar().apply { isIndeterminate = true; isVisible = false; preferredSize = java.awt.Dimension(60, 16) }

    private val tabs = JTabbedPane()

    private var root: VirtualFile? = null
    private var currentBranch: String = ""

    init {
        border = JBUI.Borders.empty(4)
        buildHeader()

        tabs.addTab(LocalizationService.t("tab.statistics"), authorStatsPanel)
        tabs.addTab(LocalizationService.t("tab.commit_chart"), chartPanel)
        tabs.addTab(LocalizationService.t("tab.branch_comparison"), comparisonPanel)
        tabs.addTab(LocalizationService.t("tab.settings"), settingsPanel)
        add(tabs, BorderLayout.CENTER)

        LocalizationService.addChangeListener(::applyLocalization)

        val sm = StartupManager.getInstance(project)
        if (sm.postStartupActivityPassed()) {
            // Project already fully opened (tool window created lazily) — load immediately
            ApplicationManager.getApplication().executeOnPooledThread { findRepository() }
        } else {
            // Project still opening — wait until startup activities (incl. git4idea) finish
            sm.runAfterOpened {
                ApplicationManager.getApplication().executeOnPooledThread { findRepository() }
            }
        }
    }

    private fun buildHeader() {
        val top = JPanel(BorderLayout())
        top.border = JBUI.Borders.emptyBottom(6)

        val info = JPanel(java.awt.GridLayout(2, 1, 0, 2))
        info.add(JPanel(FlowLayout(FlowLayout.LEFT, 4, 0)).apply {
            add(branchHeaderLabel)
            add(branchLabel)
        })
        info.add(repoLabel)

        val controls = JPanel(FlowLayout(FlowLayout.RIGHT, 6, 0))
        controls.add(spinner)
        controls.add(refreshBtn)

        top.add(info, BorderLayout.CENTER)
        top.add(controls, BorderLayout.EAST)

        val switchBar = JPanel(FlowLayout(FlowLayout.LEFT, 6, 2))
        switchBar.add(switchToLabel)
        switchBar.add(branchBox.apply { preferredSize = java.awt.Dimension(200, 26) })
        switchBar.add(switchBtn)

        val header = JPanel(BorderLayout())
        header.border = BorderFactory.createCompoundBorder(
            JBUI.Borders.customLine(UIManager.getColor("Separator.foreground"), 0, 0, 1, 0),
            JBUI.Borders.empty(4, 6, 6, 6)
        )
        header.add(top, BorderLayout.NORTH)
        header.add(switchBar, BorderLayout.SOUTH)

        add(header, BorderLayout.NORTH)

        refreshBtn.addActionListener { loadAll() }
        switchBtn.addActionListener { doSwitch() }
    }

    private fun findRepository() {
        if (root != null) return
        val repo = GitRepositoryManager.getInstance(project).repositories.firstOrNull() ?: return
        root = repo.root
        currentBranch = repo.currentBranchName ?: return

        ApplicationManager.getApplication().invokeLater {
            branchLabel.text = currentBranch
            repoLabel.text = repo.root.path
            loadAll()
        }
    }

    private fun loadAll() {
        val r = root ?: return

        spinner.isVisible = true
        refreshBtn.isEnabled = false

        ProgressManager.getInstance().run(object : Task.Backgroundable(project, "Loading Git stats") {
            private var authorStats = emptyList<com.gitbranchstats.models.AuthorStats>()
            private var chartData = emptyMap<String, Map<LocalDate, Int>>()
            private var branches = emptyList<String>()

            override fun run(indicator: ProgressIndicator) {
                indicator.text = "Reading author statistics…"
                authorStats = service.getAuthorStats(project, r, currentBranch)

                indicator.text = "Reading commit history for chart…"
                chartData = service.getDailyCommitData(project, r, currentBranch, 90)

                indicator.text = "Reading branches…"
                branches = service.getBranches(project, r)
            }

            override fun onSuccess() {
                ApplicationManager.getApplication().invokeLater {
                    authorStatsPanel.updateStats(authorStats)

                    chartPanel.setRepository(r, currentBranch)
                    chartPanel.updateData(chartData, 90)

                    comparisonPanel.setRoot(r)
                    comparisonPanel.updateBranches(branches, currentBranch)

                    branchBox.removeAllItems()
                    branches.forEach { branchBox.addItem(it) }
                    branchBox.selectedItem = currentBranch

                    spinner.isVisible = false
                    refreshBtn.isEnabled = true
                }
            }

            override fun onThrowable(error: Throwable) {
                ApplicationManager.getApplication().invokeLater {
                    spinner.isVisible = false
                    refreshBtn.isEnabled = true
                    JOptionPane.showMessageDialog(
                        this@GitStatsPanel,
                        "Failed to load git data: ${error.message}",
                        "Git Branch Stats",
                        JOptionPane.ERROR_MESSAGE
                    )
                }
            }
        })
    }

    private fun applyLocalization() {
        ApplicationManager.getApplication().invokeLater {
            tabs.setTitleAt(0, LocalizationService.t("tab.statistics"))
            tabs.setTitleAt(1, LocalizationService.t("tab.commit_chart"))
            tabs.setTitleAt(2, LocalizationService.t("tab.branch_comparison"))
            tabs.setTitleAt(3, LocalizationService.t("tab.settings"))
            branchHeaderLabel.text = LocalizationService.t("label.branch")
            switchToLabel.text = LocalizationService.t("label.switch_to")
            refreshBtn.text = "⟳ ${LocalizationService.t("btn.refresh")}"
            switchBtn.text = LocalizationService.t("btn.switch")
            settingsPanel.onShown()
        }
    }

    private fun doSwitch() {
        val selected = branchBox.selectedItem as? String ?: return
        if (selected == currentBranch) return
        val r = root ?: return

        val result = JOptionPane.showConfirmDialog(
            this,
            "Switch to branch '$selected'?",
            "Git Branch Stats",
            JOptionPane.OK_CANCEL_OPTION
        )
        if (result != JOptionPane.OK_OPTION) return

        service.checkoutBranch(project, r, selected)
        // After checkout, refresh with new branch
        ApplicationManager.getApplication().executeOnPooledThread {
            Thread.sleep(500)  // let git4idea update the repo state
            val newBranch = service.getCurrentBranch(project, r)
            ApplicationManager.getApplication().invokeLater {
                if (newBranch.isNotEmpty()) {
                    currentBranch = newBranch
                    branchLabel.text = currentBranch
                    loadAll()
                }
            }
        }
    }
}
