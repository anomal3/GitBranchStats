package com.gitbranchstats.ui.panels

import com.gitbranchstats.services.LocalizationService
import com.intellij.ui.components.JBScrollPane
import com.intellij.ui.table.JBTable
import com.intellij.util.ui.JBUI
import java.awt.BorderLayout
import java.awt.Dimension
import java.awt.FlowLayout
import java.awt.GridBagConstraints
import java.awt.GridBagLayout
import javax.swing.*
import javax.swing.table.DefaultTableModel

class SettingsPanel : JPanel(BorderLayout()) {

    private val langBox = JComboBox<String>()
    private val applyBtn = JButton("Apply")

    // Translation table: Key | English | Your Translation (editable)
    private val tableModel = object : DefaultTableModel(arrayOf("Key", "English", "Your Translation"), 0) {
        override fun isCellEditable(row: Int, col: Int) = col == 2
    }
    private val translationTable = JBTable(tableModel).apply {
        setShowGrid(true)
        rowHeight = 24
        columnModel.getColumn(0).preferredWidth = 190
        columnModel.getColumn(1).preferredWidth = 200
        columnModel.getColumn(2).preferredWidth = 200
    }

    private val newLangNameField = JTextField(18)
    private val saveLangBtn = JButton("Save as new language")
    private val statusLabel = JLabel(" ").apply { font = font.deriveFont(11f) }

    init {
        border = JBUI.Borders.empty(12)
        buildUI()
        populateLangBox()
        loadTableForCurrentLang()

        applyBtn.addActionListener { applyLanguage() }
        saveLangBtn.addActionListener { saveAsNewLanguage() }
        langBox.addActionListener { onLangBoxChanged() }
    }

    private fun buildUI() {
        val top = JPanel(GridBagLayout())
        top.border = BorderFactory.createTitledBorder("Language")
        val gbc = GridBagConstraints().apply {
            insets = JBUI.insets(4, 6)
            anchor = GridBagConstraints.WEST
        }

        gbc.gridx = 0; gbc.gridy = 0
        top.add(JLabel("Interface language:"), gbc)
        gbc.gridx = 1
        top.add(langBox.apply { preferredSize = Dimension(180, 26) }, gbc)
        gbc.gridx = 2
        top.add(applyBtn, gbc)

        val tableSection = JPanel(BorderLayout())
        tableSection.border = BorderFactory.createTitledBorder("Edit / create translation")

        val hint = JLabel("Select a language above to load its translations, then edit column 'Your Translation'.")
        hint.border = JBUI.Borders.emptyBottom(6)
        hint.font = hint.font.deriveFont(11f)

        tableSection.add(hint, BorderLayout.NORTH)
        tableSection.add(JBScrollPane(translationTable), BorderLayout.CENTER)

        val saveBar = JPanel(FlowLayout(FlowLayout.LEFT, 6, 4))
        saveBar.add(JLabel("New language name:"))
        saveBar.add(newLangNameField)
        saveBar.add(saveLangBtn)
        tableSection.add(saveBar, BorderLayout.SOUTH)

        add(top, BorderLayout.NORTH)
        add(tableSection, BorderLayout.CENTER)
        add(statusLabel, BorderLayout.SOUTH)
    }

    private fun populateLangBox() {
        langBox.removeAllItems()
        LocalizationService.allLanguages.forEach { langBox.addItem(it) }
        langBox.selectedItem = LocalizationService.currentLanguage
    }

    private fun loadTableForCurrentLang() {
        val selected = langBox.selectedItem as? String ?: return
        val translations = LocalizationService.getTranslationsFor(selected)
        val english = LocalizationService.getTranslationsFor("English")
        tableModel.rowCount = 0
        LocalizationService.KEYS.forEach { key ->
            tableModel.addRow(arrayOf(key, english[key] ?: key, translations[key] ?: ""))
        }
    }

    private fun onLangBoxChanged() {
        // stop any pending cell edit first
        if (translationTable.isEditing) translationTable.cellEditor?.stopCellEditing()
        loadTableForCurrentLang()
    }

    private fun applyLanguage() {
        val selected = langBox.selectedItem as? String ?: return
        LocalizationService.setLanguage(selected)
        statusLabel.text = "Language applied: $selected"
    }

    private fun saveAsNewLanguage() {
        if (translationTable.isEditing) translationTable.cellEditor?.stopCellEditing()

        val name = newLangNameField.text.trim()
        if (name.isEmpty()) {
            statusLabel.text = "Enter a language name first."
            return
        }
        if (name == "English" || name == "Russian") {
            statusLabel.text = "Cannot overwrite built-in languages."
            return
        }

        val map = mutableMapOf<String, String>()
        for (row in 0 until tableModel.rowCount) {
            val key = tableModel.getValueAt(row, 0) as? String ?: continue
            val value = tableModel.getValueAt(row, 2) as? String ?: ""
            map[key] = value
        }

        LocalizationService.saveCustomLanguage(name, map)

        // Refresh the language box adding the new entry if it's new
        val current = langBox.selectedItem
        populateLangBox()
        langBox.selectedItem = name
        loadTableForCurrentLang()

        statusLabel.text = "Saved: $name. Press Apply to activate."
    }

    fun onShown() {
        populateLangBox()
        loadTableForCurrentLang()
    }
}
