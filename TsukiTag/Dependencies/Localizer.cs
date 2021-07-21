using Avalonia;
using Avalonia.Platform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Dependencies
{
    public interface ILocalizer
    {
        void SwitchLanguage(string languageName);

        string Format(string key, string languageName, params object[] values);

        string Format(string key, params object[] values);

        string Get(string key);

        string Get(string key, string languageName);
    }

    public class Localizer : ILocalizer
    {
        public const string EnglishLanguage = "en";

        private Dictionary<string, Dictionary<string, string>> languages;
        private List<string> ignoredLanguages;

        public string CurrentLanguageName { get; private set; }

        public Localizer()
        {
            languages = new Dictionary<string, Dictionary<string, string>>();
            ignoredLanguages = new List<string>();

            SwitchLanguage(EnglishLanguage);
        }

        public void SwitchLanguage(string languageName)
        {
            SetupLanguage(languageName);
            CurrentLanguageName = languageName;
        }

        public string Format(string key, string languageName, params object[] values)
        {
            return string.Format(Get(key, languageName), values.Select(v => Get(v.ToString(), languageName).ToArray()));
        }

        public string Format(string key, params object[] values)
        {
            return Format(key, CurrentLanguageName, values);
        }

        public string Get(string key)
        {
            return Get(key, CurrentLanguageName);
        }

        public string Get(string key, string languageName)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            else
            {
                SetupLanguage(languageName);

                if (languages.ContainsKey(languageName))
                {
                    var language = languages[languageName];
                    if (language.ContainsKey(key))
                    {
                        return language[key];
                    }
                    else
                    {
                        SetupLanguage(EnglishLanguage);
                        language = languages[EnglishLanguage];

                        if (language.ContainsKey(key))
                        {
                            return language[key];
                        }
                    }
                }
                else
                {
                    SetupLanguage(EnglishLanguage);
                    var language = languages[EnglishLanguage];

                    if (language.ContainsKey(key))
                    {
                        return language[key];
                    }
                }

                return key;
            }
        }

        private void SetupLanguage(string languageCode)
        {
            if (languages.ContainsKey(languageCode))
            {
                return;
            }
            else if (ignoredLanguages.Contains(languageCode))
            {
                return;
            }
            else
            {
                LoadLanguageDictionary(languageCode);
            }
        }

        private void LoadLanguageDictionary(string languageCode)
        {
            var mainDict = new Dictionary<string, string>();
            var content = GetLanguageContent(languageCode);

            if (content != null)
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                if (dict != null)
                {
                    foreach (var key in dict.Keys)
                    {
                        if (!mainDict.ContainsKey(key))
                        {
                            mainDict.Add(key, dict[key]);
                        }
                    }
                }

                if (mainDict.Count != 0)
                {
                    if (languages.ContainsKey(languageCode))
                    {
                        languages[languageCode] = mainDict;
                    }
                    else
                    {
                        languages.Add(languageCode, mainDict);
                    }
                }
                else
                {
                    ignoredLanguages.Add(languageCode);
                }
            }
        }

        private string GetLanguageContent(string languageName)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using (var reader = new StreamReader(assets.Open(new Uri($"avares://TsukiTag/Assets/Languages/{languageName}.json"))))
            {
                var content = reader.ReadToEnd();
                return content;
            }
        }
    }
}
