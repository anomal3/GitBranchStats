package com.gitbranchstats.services

import com.intellij.ide.util.PropertiesComponent

object LocalizationService {

    val KEYS = listOf(
        "tab.statistics", "tab.commit_chart", "tab.branch_comparison", "tab.settings",
        "label.branch", "label.switch_to", "btn.refresh", "btn.switch", "btn.compare",
        "label.branch_a", "label.branch_b", "label.summary",
        "label.commits_ahead", "label.commits_behind", "label.files_changed",
        "label.insertions", "label.deletions",
        "col.author", "col.email", "col.commits", "col.files", "col.added", "col.deleted",
        "col.sha", "col.date", "col.subject",
        "label.period", "label.authors_chart"
    )

    private val english = mapOf(
        "tab.statistics" to "Statistics",
        "tab.commit_chart" to "Commit Chart",
        "tab.branch_comparison" to "Branch Comparison",
        "tab.settings" to "Settings",
        "label.branch" to "Branch:",
        "label.switch_to" to "Switch to:",
        "btn.refresh" to "Refresh",
        "btn.switch" to "Switch",
        "btn.compare" to "Compare",
        "label.branch_a" to "Branch A:",
        "label.branch_b" to "vs  Branch B:",
        "label.summary" to "Summary",
        "label.commits_ahead" to "Commits ahead:",
        "label.commits_behind" to "Commits behind:",
        "label.files_changed" to "Files changed:",
        "label.insertions" to "Insertions:",
        "label.deletions" to "Deletions:",
        "col.author" to "Author",
        "col.email" to "Email",
        "col.commits" to "Commits",
        "col.files" to "Files",
        "col.added" to "Added",
        "col.deleted" to "Deleted",
        "col.sha" to "SHA",
        "col.date" to "Date",
        "col.subject" to "Subject",
        "label.period" to "Period:",
        "label.authors_chart" to "Authors"
    )

    private val russian = mapOf(
        "tab.statistics" to "Статистика",
        "tab.commit_chart" to "График коммитов",
        "tab.branch_comparison" to "Сравнение веток",
        "tab.settings" to "Настройки",
        "label.branch" to "Ветка:",
        "label.switch_to" to "Переключить на:",
        "btn.refresh" to "Обновить",
        "btn.switch" to "Переключить",
        "btn.compare" to "Сравнить",
        "label.branch_a" to "Ветка A:",
        "label.branch_b" to "vs  Ветка B:",
        "label.summary" to "Сводка",
        "label.commits_ahead" to "Коммитов впереди:",
        "label.commits_behind" to "Коммитов позади:",
        "label.files_changed" to "Изменено файлов:",
        "label.insertions" to "Добавлено строк:",
        "label.deletions" to "Удалено строк:",
        "col.author" to "Автор",
        "col.email" to "Email",
        "col.commits" to "Коммиты",
        "col.files" to "Файлы",
        "col.added" to "Добавлено",
        "col.deleted" to "Удалено",
        "col.sha" to "SHA",
        "col.date" to "Дата",
        "col.subject" to "Сообщение",
        "label.period" to "Период:",
        "label.authors_chart" to "Авторы"
    )

    private val builtinLanguages: Map<String, Map<String, String>> =
        linkedMapOf("English" to english, "Russian" to russian)

    private var _currentLanguage: String = "English"
    private val _customLanguages: MutableMap<String, MutableMap<String, String>> = linkedMapOf()
    private val listeners: MutableList<() -> Unit> = mutableListOf()

    val currentLanguage get() = _currentLanguage
    val allLanguages get() = builtinLanguages.keys.toList() + _customLanguages.keys.toList()

    init {
        load()
    }

    fun t(key: String): String {
        val map = builtinLanguages[_currentLanguage] ?: _customLanguages[_currentLanguage]
        return map?.get(key) ?: english[key] ?: key
    }

    fun setLanguage(lang: String) {
        if (lang != _currentLanguage && (builtinLanguages.containsKey(lang) || _customLanguages.containsKey(lang))) {
            _currentLanguage = lang
            save()
            notifyListeners()
        }
    }

    fun getTranslationsFor(lang: String): Map<String, String> =
        (builtinLanguages[lang] ?: _customLanguages[lang])?.toMap() ?: english.toMap()

    fun saveCustomLanguage(name: String, translations: Map<String, String>) {
        _customLanguages[name] = translations.toMutableMap()
        save()
    }

    fun customLanguageNames(): List<String> = _customLanguages.keys.toList()

    fun addChangeListener(listener: () -> Unit) {
        listeners.add(listener)
    }

    fun removeChangeListener(listener: () -> Unit) {
        listeners.remove(listener)
    }

    private fun notifyListeners() = listeners.toList().forEach { it() }

    // Storage: each custom language key stored as gbs.cl.<langIndex>.name / gbs.cl.<langIndex>.<translationKey>
    // Simple flat approach using PropertiesComponent
    private fun save() {
        val props = PropertiesComponent.getInstance()
        props.setValue("gbs.language", _currentLanguage)
        val names = _customLanguages.keys.toList()
        props.setValue("gbs.cl.count", names.size.toString())
        names.forEachIndexed { i, name ->
            props.setValue("gbs.cl.$i.name", name)
            val map = _customLanguages[name] ?: return@forEachIndexed
            KEYS.forEach { key ->
                val v = map[key] ?: ""
                props.setValue("gbs.cl.$i.$key", v)
            }
        }
    }

    private fun load() {
        val props = PropertiesComponent.getInstance()
        _currentLanguage = props.getValue("gbs.language", "English") ?: "English"
        val count = props.getValue("gbs.cl.count", "0")?.toIntOrNull() ?: 0
        for (i in 0 until count) {
            val name = props.getValue("gbs.cl.$i.name") ?: continue
            val map: MutableMap<String, String> = mutableMapOf()
            KEYS.forEach { key ->
                val v = props.getValue("gbs.cl.$i.$key", "") ?: ""
                map[key] = v
            }
            _customLanguages[name] = map
        }
        if (!builtinLanguages.containsKey(_currentLanguage) && !_customLanguages.containsKey(_currentLanguage)) {
            _currentLanguage = "English"
        }
    }
}
