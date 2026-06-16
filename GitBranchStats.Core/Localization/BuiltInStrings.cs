using System.Collections.Generic;

namespace GitBranchStats.Core.Localization
{
    /// <summary>
    /// The master list of localizable strings. English is the source of truth and the
    /// fallback for any key a custom translation does not provide.
    /// </summary>
    internal static class BuiltInStrings
    {
        /// <summary>
        /// English strings. The keys here define the full set of translatable strings;
        /// the translation editor is built from this dictionary.
        /// </summary>
        public static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            // Tabs
            ["Tab_Stats"] = "Statistics",
            ["Tab_Settings"] = "Settings",

            // Main tool window
            ["App_Title"] = "🔀 Git Branch Stats",
            ["Main_Refresh"] = "🔄 Refresh",
            ["Main_CurrentBranch"] = "Current Branch:",
            ["Main_AuthorStatistics"] = "👤 Author Statistics",
            ["Main_Col_Author"] = "Author",
            ["Main_Col_Commits"] = "Commits",
            ["Main_Col_Files"] = "Files",
            ["Main_Col_Additions"] = "Additions",
            ["Main_Col_Deletions"] = "Deletions",
            ["Main_SwitchBranch"] = "🔀 Switch Branch",
            ["Main_Switch"] = "Switch",

            // Branch comparison
            ["Cmp_CompareBranches"] = "📊 Compare Branches",
            ["Cmp_Compare"] = "Compare",
            ["Cmp_CommitsAhead"] = "Commits ahead: ",
            ["Cmp_CommitsBehind"] = "Commits behind: ",
            ["Cmp_FilesChanged"] = "Files changed: ",
            ["Cmp_Insertions"] = "Insertions: ",
            ["Cmp_Deletions"] = "Deletions: ",
            ["Cmp_UniqueCommits"] = "Unique Commits",
            ["Cmp_Col_SHA"] = "SHA",
            ["Cmp_Col_Author"] = "Author",
            ["Cmp_Col_Date"] = "Date",
            ["Cmp_Col_Message"] = "Message",

            // Branch display
            ["Branch_CurrentSuffix"] = "(current)",

            // Settings
            ["Settings_Title"] = "⚙ Git Branch Stats — Settings",
            ["Settings_Language"] = "Language:",
            ["Settings_Hint"] = "Choose the interface language. Untranslated text stays in English.",
            ["Settings_CreateTranslation"] = "Create your own translation…",
            ["Settings_EditTranslation"] = "Edit selected translation…",

            // Translation editor
            ["Editor_Title"] = "Translation editor",
            ["Editor_LanguageName"] = "Language name:",
            ["Editor_Hint"] = "Enter a translation for each English string. Empty fields stay in English.",
            ["Editor_Col_English"] = "English",
            ["Editor_Col_Translation"] = "Translation",
            ["Editor_Save"] = "Save",
            ["Editor_Cancel"] = "Cancel",
            ["Editor_NameRequired"] = "Please enter a language name.",

            // Error dialog
            ["Dialog_Ok"] = "OK",
            ["Dialog_SwitchErrorTitle"] = "Cannot switch branch",
            ["Dialog_FilesLocked"] = "Could not switch to the branch because some files are in use by another process — a running build, the running application, or files open in the editor.\n\nStop the build, close the application and the files, then try again.",
            ["Dialog_SwitchErrorGeneric"] = "Could not switch branch.\n\n{0}",

            // Status messages ({0}/{1} are filled at runtime)
            ["Status_OpeningRepo"] = "Opening repository...",
            ["Status_NoRepo"] = "No Git repository found at this path.",
            ["Status_NoSolution"] = "No solution is open.",
            ["Status_ErrorOpening"] = "Error opening repository: {0}",
            ["Status_ErrorLoading"] = "Error loading data: {0}",
            ["Status_Switching"] = "Switching to {0}...",
            ["Status_SwitchWarning"] = "Warning: uncommitted changes exist. Switching to {0}...",
            ["Status_Switched"] = "Switched to {0}",
            ["Status_ErrorSwitch"] = "Error switching branch: {0}",
            ["Status_Comparing"] = "Comparing {0} ↔ {1}...",
            ["Status_Compared"] = "Compared {0} ↔ {1}",
            ["Status_ErrorComparing"] = "Error comparing branches: {0}",
            ["Status_Error"] = "Error: {0}",
        };

        /// <summary>Russian translation of the built-in strings.</summary>
        public static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            ["Tab_Stats"] = "Статистика",
            ["Tab_Settings"] = "Настройки",

            ["App_Title"] = "🔀 Статистика веток Git",
            ["Main_Refresh"] = "🔄 Обновить",
            ["Main_CurrentBranch"] = "Текущая ветка:",
            ["Main_AuthorStatistics"] = "👤 Статистика по авторам",
            ["Main_Col_Author"] = "Автор",
            ["Main_Col_Commits"] = "Коммиты",
            ["Main_Col_Files"] = "Файлы",
            ["Main_Col_Additions"] = "Добавлено",
            ["Main_Col_Deletions"] = "Удалено",
            ["Main_SwitchBranch"] = "🔀 Переключить ветку",
            ["Main_Switch"] = "Переключить",

            ["Cmp_CompareBranches"] = "📊 Сравнить ветки",
            ["Cmp_Compare"] = "Сравнить",
            ["Cmp_CommitsAhead"] = "Коммитов впереди: ",
            ["Cmp_CommitsBehind"] = "Коммитов позади: ",
            ["Cmp_FilesChanged"] = "Изменено файлов: ",
            ["Cmp_Insertions"] = "Добавлено строк: ",
            ["Cmp_Deletions"] = "Удалено строк: ",
            ["Cmp_UniqueCommits"] = "Уникальные коммиты",
            ["Cmp_Col_SHA"] = "SHA",
            ["Cmp_Col_Author"] = "Автор",
            ["Cmp_Col_Date"] = "Дата",
            ["Cmp_Col_Message"] = "Сообщение",

            ["Branch_CurrentSuffix"] = "(текущая)",

            ["Settings_Title"] = "⚙ Статистика веток Git — Настройки",
            ["Settings_Language"] = "Язык:",
            ["Settings_Hint"] = "Выберите язык интерфейса. Непереведённый текст останется на английском.",
            ["Settings_CreateTranslation"] = "Создать свой перевод…",
            ["Settings_EditTranslation"] = "Изменить выбранный перевод…",

            ["Editor_Title"] = "Редактор перевода",
            ["Editor_LanguageName"] = "Название языка:",
            ["Editor_Hint"] = "Введите перевод для каждой английской строки. Пустые поля останутся на английском.",
            ["Editor_Col_English"] = "Английский",
            ["Editor_Col_Translation"] = "Перевод",
            ["Editor_Save"] = "Сохранить",
            ["Editor_Cancel"] = "Отмена",
            ["Editor_NameRequired"] = "Введите название языка.",

            ["Dialog_Ok"] = "ОК",
            ["Dialog_SwitchErrorTitle"] = "Не удалось переключить ветку",
            ["Dialog_FilesLocked"] = "Не удалось переключиться на ветку: часть файлов занята другим процессом — идёт сборка, запущено приложение или файлы открыты в редакторе.\n\nОстановите сборку, закройте приложение и файлы, затем повторите попытку.",
            ["Dialog_SwitchErrorGeneric"] = "Не удалось переключить ветку.\n\n{0}",

            ["Status_OpeningRepo"] = "Открытие репозитория...",
            ["Status_NoRepo"] = "Git-репозиторий по этому пути не найден.",
            ["Status_NoSolution"] = "Нет открытого решения.",
            ["Status_ErrorOpening"] = "Ошибка открытия репозитория: {0}",
            ["Status_ErrorLoading"] = "Ошибка загрузки данных: {0}",
            ["Status_Switching"] = "Переключение на {0}...",
            ["Status_SwitchWarning"] = "Внимание: есть незафиксированные изменения. Переключение на {0}...",
            ["Status_Switched"] = "Переключено на {0}",
            ["Status_ErrorSwitch"] = "Ошибка переключения ветки: {0}",
            ["Status_Comparing"] = "Сравнение {0} ↔ {1}...",
            ["Status_Compared"] = "Сравнено: {0} ↔ {1}",
            ["Status_ErrorComparing"] = "Ошибка сравнения веток: {0}",
            ["Status_Error"] = "Ошибка: {0}",
        };
    }
}
