package com.gitbranchstats.ui

import com.intellij.openapi.project.DumbAware
import com.intellij.openapi.project.Project
import com.intellij.openapi.wm.ToolWindow
import com.intellij.openapi.wm.ToolWindowFactory
import com.intellij.ui.content.ContentFactory

class GitStatsToolWindowFactory : ToolWindowFactory, DumbAware {

    override fun createToolWindowContent(project: Project, toolWindow: ToolWindow) {
        val panel = GitStatsPanel(project)
        val content = ContentFactory.getInstance().createContent(panel, "", false)
        toolWindow.contentManager.addContent(content)
        // Keep the stripe icon but don't auto-expand the window on project open
        toolWindow.isShowStripeButton = true
    }

    // Always available — shouldBeAvailable with git-check fires before git4idea initializes
    // and returns false, making the stripe icon disappear permanently for that session.
    // GitStatsPanel handles the "no repo yet" state gracefully via StartupManager.runAfterOpened.
    override fun shouldBeAvailable(project: Project): Boolean = true
}
