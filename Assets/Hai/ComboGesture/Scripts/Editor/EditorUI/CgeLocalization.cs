using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public class CgeLocalization
    {
        private const string Prefix = "cge.";
        private const string Suffix = ".json";
        private static readonly CgeLocalization Myself = new CgeLocalization();
        private Dictionary<string, Dictionary<string, string>> _localizations;
        private Dictionary<string, string> _activeLocaleNullable;
        private bool _isEnglish;
        private static int _lang;

        private CgeLocalization()
        {
            ReloadLocalizationsInternal();
        }

        public static void ReloadLocalizations()
        {
            Myself.ReloadLocalizationsInternal();
        }

        public static bool IsEnglishLocaleActive()
        {
            return Myself._isEnglish;
        }

        public static void CycleLocale()
        {
            _lang++;
            Myself.ReloadLocalizationsInternal();
        }

        private void ReloadLocalizationsInternal()
        {
            var localizationGuids = AssetDatabase.FindAssets("", new[] {"Assets/Hai/ComboGesture/Scripts/Editor/EditorUI/Locale"});
            _localizations = localizationGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                {
                    var fileName = Path.GetFileName(path);
                    return fileName.StartsWith(Prefix) && fileName.EndsWith(Suffix);
                })
                .ToDictionary(path =>
                {
                    var fileName = Path.GetFileName(path);
                    var languageCode = fileName.Substring(Prefix.Length, fileName.Length - Prefix.Length - Suffix.Length);
                    return languageCode;
                }, ExtractDictionaryFromPath);

            ReevaluateActiveLocale();
        }

        private void ReevaluateActiveLocale()
        {
            var nonEnglish = _localizations.ToList().Where(pair => pair.Key != "en").ToList();
            var langModulo = _lang % (nonEnglish.Count + 1);

            if (langModulo == 0)
            {
                _isEnglish = true;
                _activeLocaleNullable = _localizations.Keys.Contains("en") ? _localizations["en"] : null;
            }
            else
            {
                _isEnglish = false;
                _activeLocaleNullable = nonEnglish.ToList()[langModulo - 1].Value;
            }
        }

        public static string LocalizeOrElse(string key, string defaultCultureLocalization)
        {
            if (Myself._activeLocaleNullable == null) return defaultCultureLocalization;

            var anyLocalization = Myself._activeLocaleNullable.ContainsKey(key) ? Myself._activeLocaleNullable[key] : null;
            return anyLocalization ?? defaultCultureLocalization;
        }

        private Dictionary<string, string> ExtractDictionaryFromPath(string path)
        {
            try
            {
                var contents = ExtractTextFromPath(path);
                return ExtractDictionaryFromText(contents);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new Dictionary<string, string>();
            }
        }

        private static Dictionary<string, string> ExtractDictionaryFromText(string contents)
        {
            var localizations = new Dictionary<string, string>();
            var jsonObject = EeJSON.Parse(contents);
            foreach (var key in jsonObject.Keys)
            {
                localizations.Add(key, jsonObject[key]);
            }

            return localizations;
        }

        private static string ExtractTextFromPath(string path)
        {
            var streamReader = new StreamReader(path);
            var contents = streamReader.ReadToEnd();
            streamReader.Close();
            return contents;
        }
    }
}
