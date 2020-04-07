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

    public static void Init() //Create CSVLoader and UpdateDictionaries
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

    public static void UpdateAllTextInScene() //Update all existing localized text in scene
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

    //! Must update if adding new languages
    public static void UpdateDictionaries() //Update localized dictionaries
    {
        localizedEN = csvLoader.GetDictionaryValues("en");
        localizedFR = csvLoader.GetDictionaryValues("fr");
    }

    public static string GetLocalizedValue(string key) //Get the localized value given a key for the set language
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

    public static string GetLocalizedValue(string key, Language _language) //Get localized value given a key and a specific language
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


    public static void Add(string key, string value) //Process the new value and add on necessary regex to identify it in the CSV file, and then add it to the CSV
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

    public static void Replace(string key, string value)//Process the new value and add on necessary regex to identify it in the CSV file, and then edit the specific key
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

    public static void Remove(string key)//Remove a specified key from the CSV file
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

    public static Dictionary<string, string> GetDictionaryForEditor() //Return English Dictionary for Search Window use
    {
        if (isInit == false) { Init(); }
        return localizedEN;
    }

    public static List<Dictionary<string, string>> GetAllDictionariesForEditor() //Return all dictionaries in a list for custom editor use
    {
        if (isInit == false) { Init(); }

        List<Dictionary<string, string>> allDictionaries = new List<Dictionary<string, string>>();
        allDictionaries.Add(localizedEN);
        allDictionaries.Add(localizedFR);

        return allDictionaries;
    }
}
