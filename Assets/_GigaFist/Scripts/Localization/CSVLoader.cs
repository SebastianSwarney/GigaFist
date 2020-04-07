using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System;

public class CSVLoader
{
    public string localizationFileName = "localization";
    private TextAsset csvFile;
    private char lineSeperator = '\n';
    private char surround = '"';
    private string[] fieldSeperator = { "\",\"" };

    public void LoadCSV()
    {
        csvFile = Resources.Load<TextAsset>(localizationFileName);
    }

    public Dictionary<string, string> GetDictionaryValues(string attributeID)
    {
        //Create dictionary
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        //Split the CSV File into its separate lines
        string[] lines = csvFile.text.Split(lineSeperator);

        int attributeIndex = -1;

        string[] headers = lines[0].Split(fieldSeperator, StringSplitOptions.None);

        //Get Attribute Index from headers
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Contains(attributeID))
            {
                attributeIndex = i;
                break;
            }
        }


        //Parse the lines for specific fields, and construct dictionary from that
        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            string[] fields = CSVParser.Split(line);

            for (int f = 0; f < fields.Length; f++)
            {
                fields[f] = fields[f].TrimStart(' ', surround);
                fields[f] = fields[f].Replace(surround.ToString(), "");
            }

            if (fields.Length > attributeIndex)
            {
                var key = fields[0];

                if (dictionary.ContainsKey(key)) { continue; }

                var value = fields[attributeIndex];
                //Debug.Log(key + "   :    " + value);
                dictionary.Add(key, value);
            }
        }
        return dictionary;
    }
#if UNITY_EDITOR
    public void Add(string key, string value) //Add a new entry to the file, only adds first language at the moment
    {
        //Only adds first language, if you wanted to add more the line would look like this
        //* string.Format("\n\"{0}\",\"{1}\",\"{2}\",\"{3}\", key, value, value2, value3);
        string appended = string.Format("\n\"{0}\",\"{1}\",\"\"", key, value);

        //Add the new entry to the file
        File.AppendAllText("Assets/Resources/" + localizationFileName + ".csv", appended);

        //Refresh the database to ensure no old versions
        UnityEditor.AssetDatabase.Refresh();
    }

    public void Remove(string key) //Parse through the file and look for the key, and remove that line entirely. Replace all the text with the key's according line removed.
    {
        string[] lines = csvFile.text.Split(lineSeperator);

        string[] keys = new string[lines.Length];

        //Get the keys from each line
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            keys[i] = line.Split(fieldSeperator, StringSplitOptions.None)[0];
        }

        int index = -1;

        //If the keys contain the specific key, save that index
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i].Contains(key))
            {
                index = i;
                break;
            }
        }

        //If the index was changed (meaning a key was found), remove the specific line and then save the new CSV
        if (index > -1)
        {
            string[] newLines;
            newLines = lines.Where(w => w != lines[index]).ToArray();

            string replaced = string.Join(lineSeperator.ToString(), newLines);
            File.WriteAllText("Assets/Resources/" + localizationFileName + ".csv", replaced);
        }
    }

    public void Edit(string key, string value)
    {
        Remove(key);
        Add(key, value);
    }
#endif
}
