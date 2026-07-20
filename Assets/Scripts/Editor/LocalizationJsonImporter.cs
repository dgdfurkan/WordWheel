using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

public class LocalizationJsonImporter : EditorWindow
{
    private string jsonFilePath = "";
    private int selectedCollectionIndex = 0;
    private List<StringTableCollection> collections = new List<StringTableCollection>();
    private string[] collectionNames = new string[0];

    [MenuItem("Window/Localization/JSON Importer")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationJsonImporter>("JSON Importer");
    }

    private void OnEnable()
    {
        RefreshCollections();
    }

    private void RefreshCollections()
    {
        collections = LocalizationEditorSettings.GetStringTableCollections().ToList();
        collectionNames = collections.Select(c => c.TableCollectionName).ToArray();
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Translations from JSON", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string path = EditorUtility.OpenFilePanel("Select Localization JSON", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                jsonFilePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (collections.Count == 0)
        {
            EditorGUILayout.HelpBox("No String Table Collections found. Create one first in the Localization Tables window.", MessageType.Warning);
            if (GUILayout.Button("Refresh Collections"))
            {
                RefreshCollections();
            }
            return;
        }

        selectedCollectionIndex = EditorGUILayout.Popup("Target Collection", selectedCollectionIndex, collectionNames);

        EditorGUILayout.Space();

        if (GUILayout.Button("Import Localization", GUILayout.Height(35)))
        {
            Import();
        }
    }

    private void Import()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            EditorUtility.DisplayDialog("Error", "Selected file path is invalid or does not exist.", "OK");
            return;
        }

        if (selectedCollectionIndex < 0 || selectedCollectionIndex >= collections.Count)
        {
            EditorUtility.DisplayDialog("Error", "Selected collection is invalid.", "OK");
            return;
        }

        string jsonContent = File.ReadAllText(jsonFilePath);
        LocalizationData data;
        try
        {
            data = JsonUtility.FromJson<LocalizationData>(jsonContent);
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"JSON Parsing failed: {ex.Message}", "OK");
            return;
        }

        if (data == null || data.items == null || data.items.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "JSON data has no valid localization items.", "OK");
            return;
        }

        StringTableCollection collection = collections[selectedCollectionIndex];
        var sharedTableData = collection.SharedData;

        int addedCount = 0;
        int updatedCount = 0;

        foreach (var item in data.items)
        {
            if (string.IsNullOrEmpty(item.key)) continue;

            var sharedEntry = sharedTableData.GetEntry(item.key);
            if (sharedEntry == null)
            {
                sharedEntry = sharedTableData.AddKey(item.key);
                addedCount++;
            }
            else
            {
                updatedCount++;
            }

            foreach (var table in collection.StringTables)
            {
                string localeCode = table.LocaleIdentifier.Code;
                string value = GetTranslationValue(item, localeCode);

                if (!string.IsNullOrEmpty(value))
                {
                    string cleanedValue = CleanTranslation(value);
                    table.AddEntry(sharedEntry.Id, cleanedValue);
                    EditorUtility.SetDirty(table);
                }
            }
        }

        EditorUtility.SetDirty(sharedTableData);
        EditorUtility.SetDirty(collection);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Success", $"Import completed!\nAdded keys: {addedCount}\nUpdated keys: {updatedCount}", "OK");
    }

    private string CleanTranslation(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        int slashIndex = text.IndexOf('/');
        if (slashIndex >= 0)
        {
            return text.Substring(0, slashIndex).Trim();
        }
        return text;
    }

    private string GetTranslationValue(LocalizationItem item, string localeCode)
    {
        switch (localeCode.ToLower())
        {
            case "en": return item.en;
            case "tr": return item.tr;
            case "es": return item.es;
            case "de": return item.de;
            case "fr": return item.fr;
            default: return null;
        }
    }

    [Serializable]
    private class LocalizationItem
    {
        public string key;
        public string en;
        public string tr;
        public string es;
        public string de;
        public string fr;
    }

    [Serializable]
    private class LocalizationData
    {
        public List<LocalizationItem> items;
    }
}
