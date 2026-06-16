package com.gitbranchstats.actions

import com.intellij.notification.NotificationGroupManager
import com.intellij.notification.NotificationType
import com.intellij.openapi.actionSystem.ActionUpdateThread
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.project.DumbAware
import com.intellij.openapi.wm.ToolWindowManager

class ShowGitStatsAction : AnAction(), DumbAware {

    override fun getActionUpdateThread() = ActionUpdateThread.BGT

    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project ?: return
        val tw = ToolWindowManager.getInstance(project).getToolWindow("Git Branch Stats")
        if (tw != null) {
            if (!tw.isVisible) tw.show() else tw.activate(null)
        } else {
            NotificationGroupManager.getInstance()
                .getNotificationGroup("Git Branch Stats")
                ?.createNotification(
                    "Git Branch Stats",
                    "No Git repository found in this project.",
                    NotificationType.INFORMATION
                )
                ?.notify(project)
        }
    }

    override fun update(e: AnActionEvent) {
        // Always visible; only enabled when a project is open
        e.presentation.isVisible = true
        e.presentation.isEnabled = e.project != null
    }
}
