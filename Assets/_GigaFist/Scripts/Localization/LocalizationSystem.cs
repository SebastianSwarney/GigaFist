using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationSystem
{
    public enum Language { English, French }

    public static Language language = Language.English;

    private static Dictionary<string, string> localizedEN;
    private static Dictionary<string, string> localizedFR;

    public static bool isInit;

    public static CSVLoader csvLoader;

    public static void Init() //Load specific language dictionaries
    {
        csvLoader = new CSVLoader();
        csvLoader.LoadCSV();

        UpdateDictionaries();

        isInit = true;
    }

    public static void SetLanguage(Language _language)
    {
        language = _language;
    }

    public static void SetLanguage(Language _language, bool updateAllText)
    {
        SetLanguage(_language);
        if (updateAllText)
        {
            UpdateAllTextInScene();
        }
    }

    public static void UpdateAllTextInScene()
    {
        Object[] textInScene = Resources.FindObjectsOfTypeAll(typeof(TextLocalizerUI));
        if (textInScene != null)
        {
            foreach (TextLocalizerUI textLocalizerInstance in textInScene)
            {
                textLocalizerInstance.UpdateText();
            }
        }
    }

    public static void UpdateDictionaries()
    {
        localizedEN = csvLoader.GetDictionaryValues("en");
        localizedFR = csvLoader.GetDictionaryValues("fr");
    }

    public static string GetLocalizedValue(string key)
    {
        if (isInit == false) { Init(); }

        string value = key;

        switch (language)
        {
            case Language.English:
                localizedEN.TryGetValue(key, out value);
                break;
            case Language.French:
                localizedFR.TryGetValue(key, out value);
                break;
        }
        return value;
    }

    public static string GetLocalizedValue(string key, Language _language)
    {
        if (isInit == false) { Init(); }

        string value = key;

        switch (_language)
        {
            case Language.English:
                localizedEN.TryGetValue(key, out value);
                break;
            case Language.French:
                localizedFR.TryGetValue(key, out value);
                break;
        }
        return value;
    }

    public static void Add(string key, string value)
    {
        if (value.Contains("\""))
        {
            value.Replace('"', '\"');
        }

        if (csvLoader == null)
        {
            csvLoader = new CSVLoader();
        }

        csvLoader.LoadCSV();
        csvLoader.Add(key, value);
        csvLoader.LoadCSV();

        UpdateDictionaries();
    }

    public static void Replace(string key, string value)
    {
        if (value.Contains("\""))
        {
            value.Replace('"', '\"');
        }

        if (csvLoader == null)
        {
            csvLoader = new CSVLoader();
        }

        csvLoader.LoadCSV();
        csvLoader.Edit(key, value);
        csvLoader.LoadCSV();

        UpdateDictionaries();
    }

    public static void Remove(string key)
    {
        if (csvLoader == null)
        {
            csvLoader = new CSVLoader();
        }

        csvLoader.LoadCSV();
        csvLoader.Remove(key);
        csvLoader.LoadCSV();

        UpdateDictionaries();
    }

    public static Dictionary<string, string> GetDictionaryForEditor()
    {
        if (isInit == false) { Init(); }
        return localizedEN;
    }

    public static List<Dictionary<string, string>> GetAllDictionariesForEditor()
    {
        if (isInit == false) { Init(); }

        List<Dictionary<string, string>> allDictionaries = new List<Dictionary<string, string>>();
        allDictionaries.Add(localizedEN);
        allDictionaries.Add(localizedFR);

        return allDictionaries;
    }
}
