using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private CgeLocalization()
        {
            ReloadLocalizationsInternal();
        }

        public static void ReloadLocalizations()
        {
            Myself.ReloadLocalizationsInternal();
        }

        private void ReloadLocalizationsInternal()
        {
            var localizationGuids = AssetDatabase.FindAssets("", new[] {"Assets/Hai/ComboGesture/Scripts/Editor/EditorUI/Locale"});
            _localizations = localizationGuids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
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
        }

        public static string LocalizeOrElse(string key, string defaultCultureLocalization)
        {
            var anyLocalization = Myself._localizations
                .Select(pair => pair.Value.ContainsKey(key) ? pair.Value[key] : null)
                .LastOrDefault(result => result != null);
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
            var jsonObject = CgeJSON.Parse(contents);
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
