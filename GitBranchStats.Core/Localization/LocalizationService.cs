using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GitBranchStats.Core.Localization
{
    /// <summary>
    /// Central localization service. Exposes a string indexer used by the UI and keeps
    /// track of the currently selected language. Any key missing from the current
    /// language falls back to English. Custom translations are persisted to disk so they
    /// survive restarts and can be selected like the built-in languages.
    /// </summary>
    public sealed class LocalizationService : INotifyPropertyChanged
    {
        public const string EnglishCode = "en";
        public const string RussianCode = "ru";

        private static readonly Lazy<LocalizationService> _instance =
            new Lazy<LocalizationService>(() => new LocalizationService());

        /// <summary>Process-wide singleton bound by the UI.</summary>
        public static LocalizationService Instance => _instance.Value;

        // code -> (key -> value)
        private readonly Dictionary<string, Dictionary<string, string>> _languages =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private readonly List<LanguageInfo> _languageInfos = new List<LanguageInfo>();

        private string _currentCode = EnglishCode;

        private LocalizationService()
        {
            RegisterLanguage(new LanguageInfo(EnglishCode, "English", true), BuiltInStrings.English);
            RegisterLanguage(new LanguageInfo(RussianCode, "Русский", true), BuiltInStrings.Russian);

            LoadCustomLanguages();

            var saved = LoadSavedLanguageCode();
            _currentCode = (saved != null && _languages.ContainsKey(saved)) ? saved : EnglishCode;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Localized string for <paramref name="key"/>, falling back to English then the key itself.</summary>
        public string this[string key] => Get(key);

        public string Get(string key)
        {
            if (key == null) return string.Empty;
            if (_languages.TryGetValue(_currentCode, out var map)
                && map.TryGetValue(key, out var value)
                && !string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (_languages.TryGetValue(EnglishCode, out var en) && en.TryGetValue(key, out var enValue))
                return enValue;
            return key;
        }

        /// <summary>Localized string with <see cref="string.Format(string, object[])"/> arguments applied.</summary>
        public string Format(string key, params object[] args)
        {
            try { return string.Format(Get(key), args); }
            catch (FormatException) { return Get(key); }
        }

        /// <summary>Currently selected language code. Setting it switches the UI language.</summary>
        public string CurrentLanguage
        {
            get => _currentCode;
            set => SetLanguage(value);
        }

        public IReadOnlyList<LanguageInfo> AvailableLanguages => _languageInfos;

        /// <summary>The full set of English strings, used to seed the translation editor.</summary>
        public IReadOnlyDictionary<string, string> EnglishStrings => _languages[EnglishCode];

        /// <summary>Returns the stored strings for a language, or an empty map if unknown.</summary>
        public IReadOnlyDictionary<string, string> GetStrings(string code)
        {
            return _languages.TryGetValue(code, out var map)
                ? map
                : new Dictionary<string, string>();
        }

        public void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code) || !_languages.ContainsKey(code) || code == _currentCode)
                return;

            _currentCode = code;
            SaveSelectedLanguageCode(code);

            OnPropertyChanged(nameof(CurrentLanguage));
            // Refresh every {Binding [key]} in the UI.
            OnPropertyChanged("Item[]");
        }

        /// <summary>
        /// Creates or updates a custom translation, persists it, makes it the active
        /// language and returns its <see cref="LanguageInfo"/>.
        /// </summary>
        public LanguageInfo SaveCustomTranslation(string displayName, IReadOnlyDictionary<string, string> values)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name is required.", nameof(displayName));

            string code = MakeCode(displayName);
            var map = values
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            PersistLanguage(code, displayName, map);

            var info = new LanguageInfo(code, displayName, false);
            RegisterLanguage(info, map);
            OnPropertyChanged(nameof(AvailableLanguages));

            SetLanguage(code);
            return info;
        }

        // --- internals -------------------------------------------------------

        private void RegisterLanguage(LanguageInfo info, Dictionary<string, string> strings)
        {
            _languages[info.Code] = new Dictionary<string, string>(strings, StringComparer.Ordinal);
            _languageInfos.RemoveAll(l => string.Equals(l.Code, info.Code, StringComparison.OrdinalIgnoreCase));
            _languageInfos.Add(info);
        }

        private static string MakeCode(string displayName)
        {
            var sb = new StringBuilder("custom_");
            foreach (char c in displayName.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c) && c < 128) sb.Append(c);
                else if (c == ' ' || c == '-' || c == '_') sb.Append('_');
            }
            string slug = sb.ToString().Trim('_');
            return slug.Length > "custom_".Length ? slug : "custom_" + Math.Abs(displayName.GetHashCode());
        }

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // --- persistence -----------------------------------------------------

        private static string StorageDir
        {
            get
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "GitBranchStats");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private static string LangDir
        {
            get
            {
                string dir = Path.Combine(StorageDir, "lang");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private void LoadCustomLanguages()
        {
            try
            {
                foreach (var file in Directory.GetFiles(LangDir, "*.json"))
                {
                    try
                    {
                        var dto = ReadJson<LanguagePackDto>(file);
                        if (dto == null || string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.DisplayName))
                            continue;
                        var map = (dto.Strings ?? new List<StringEntryDto>())
                            .Where(e => e != null && !string.IsNullOrEmpty(e.Key))
                            .GroupBy(e => e.Key)
                            .ToDictionary(g => g.Key, g => g.Last().Value);
                        RegisterLanguage(new LanguageInfo(dto.Code, dto.DisplayName, false), map);
                    }
                    catch { /* skip a corrupt language file */ }
                }
            }
            catch { /* directory issues – fall back to built-in only */ }
        }

        private void PersistLanguage(string code, string displayName, Dictionary<string, string> map)
        {
            var dto = new LanguagePackDto
            {
                Code = code,
                DisplayName = displayName,
                Strings = map.Select(kv => new StringEntryDto { Key = kv.Key, Value = kv.Value }).ToList()
            };
            WriteJson(Path.Combine(LangDir, code + ".json"), dto);
        }

        private static string SettingsPath => Path.Combine(StorageDir, "settings.json");

        private string LoadSavedLanguageCode()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return null;
                return ReadJson<SettingsDto>(SettingsPath)?.Language;
            }
            catch { return null; }
        }

        private void SaveSelectedLanguageCode(string code)
        {
            try { WriteJson(SettingsPath, new SettingsDto { Language = code }); }
            catch { /* non-fatal */ }
        }

        private static T ReadJson<T>(string path) where T : class
        {
            using (var fs = File.OpenRead(path))
            {
                var ser = new DataContractJsonSerializer(typeof(T));
                return ser.ReadObject(fs) as T;
            }
        }

        private static void WriteJson<T>(string path, T value)
        {
            using (var fs = File.Create(path))
            {
                var ser = new DataContractJsonSerializer(typeof(T));
                ser.WriteObject(fs, value);
            }
        }

        [DataContract]
        private class SettingsDto
        {
            [DataMember(Name = "language")] public string Language { get; set; }
        }

        [DataContract]
        private class LanguagePackDto
        {
            [DataMember(Name = "code")] public string Code { get; set; }
            [DataMember(Name = "displayName")] public string DisplayName { get; set; }
            [DataMember(Name = "strings")] public List<StringEntryDto> Strings { get; set; }
        }

        [DataContract]
        private class StringEntryDto
        {
            [DataMember(Name = "key")] public string Key { get; set; }
            [DataMember(Name = "value")] public string Value { get; set; }
        }
    }
}
