package com.gitbranchstats.ui.panels

import com.gitbranchstats.models.AuthorStats
import com.intellij.ui.components.JBScrollPane
import com.intellij.ui.table.JBTable
import com.intellij.util.ui.JBUI
import java.awt.BorderLayout
import java.awt.Color
import java.awt.Component
import javax.swing.*
import javax.swing.table.DefaultTableCellRenderer
import javax.swing.table.DefaultTableModel

class AuthorStatsPanel : JPanel(BorderLayout()) {

    private val columnNames = arrayOf("Author", "Email", "Commits", "Files", "+Lines", "−Lines")
    private val model = object : DefaultTableModel(columnNames, 0) {
        override fun isCellEditable(row: Int, column: Int) = false
        override fun getColumnClass(col: Int) = if (col >= 2) Integer::class.java else String::class.java
    }

    private val table = JBTable(model).apply {
        setShowGrid(false)
        intercellSpacing = java.awt.Dimension(0, 0)
        rowHeight = 26
        tableHeader.reorderingAllowed = false
        autoCreateRowSorter = true
        rowSorter.toggleSortOrder(2)  // sort by commits desc by default

        columnModel.getColumn(0).preferredWidth = 160
        columnModel.getColumn(1).preferredWidth = 180
        columnModel.getColumn(2).preferredWidth = 70
        columnModel.getColumn(3).preferredWidth = 65
        columnModel.getColumn(4).preferredWidth = 75
        columnModel.getColumn(5).preferredWidth = 75

        columnModel.getColumn(4).cellRenderer = ColorCellRenderer(Color(0x4EC9B0), alignRight = true)
        columnModel.getColumn(5).cellRenderer = ColorCellRenderer(Color(0xE06C75), alignRight = true)
        columnModel.getColumn(2).cellRenderer = AlignRightRenderer()
        columnModel.getColumn(3).cellRenderer = AlignRightRenderer()
    }

    private val titleLabel = JLabel("Author Statistics").apply {
        font = font.deriveFont(java.awt.Font.BOLD, 13f)
        border = JBUI.Borders.emptyBottom(6)
    }

    init {
        border = JBUI.Borders.empty(8)
        add(titleLabel, BorderLayout.NORTH)
        add(JBScrollPane(table), BorderLayout.CENTER)
    }

    fun updateStats(stats: List<AuthorStats>) {
        model.rowCount = 0
        stats.forEach { s ->
            model.addRow(arrayOf(s.authorName, s.authorEmail, s.commitCount, s.filesChanged, s.additions, s.deletions))
        }
        // re-apply sort
        (table.rowSorter as? javax.swing.table.TableRowSorter<*>)?.let {
            it.toggleSortOrder(2)
        }
    }

    private class ColorCellRenderer(private val color: Color, private val alignRight: Boolean) :
        DefaultTableCellRenderer() {
        init {
            if (alignRight) horizontalAlignment = RIGHT
        }

        override fun getTableCellRendererComponent(
            table: JTable, value: Any?, selected: Boolean, focused: Boolean, row: Int, col: Int
        ): Component {
            super.getTableCellRendererComponent(table, value, selected, focused, row, col)
            if (!selected) foreground = color
            val v = value as? Int ?: 0
            text = if (col == 4) "+$v" else "−$v"
            return this
        }
    }

    private class AlignRightRenderer : DefaultTableCellRenderer() {
        init { horizontalAlignment = RIGHT }
    }
}
