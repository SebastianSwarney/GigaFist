using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextLocalizerEditWindow : EditorWindow
{
    public static void Open(string key) //Open the custom edit window for an entry
    {
        TextLocalizerEditWindow window = (TextLocalizerEditWindow)ScriptableObject.CreateInstance(typeof(TextLocalizerEditWindow));
        window.titleContent = new GUIContent("Localizer Window");
        window.ShowUtility();
        window.key = key;
    }

    public string key;
    public string value;

    public void OnGUI() //Draw the Add GUI
    {
        key = EditorGUILayout.TextField("Key : ", key);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(50));

        EditorStyles.textArea.wordWrap = true;
        value = EditorGUILayout.TextArea(value, EditorStyles.textArea, GUILayout.Height(100), GUILayout.Width(400));
        EditorGUILayout.EndHorizontal();

        //Add button
        if (GUILayout.Button("Add"))
        {
            //Call specific localization system methods
            if (LocalizationSystem.GetLocalizedValue(key) != string.Empty)
            {
                LocalizationSystem.Replace(key, value);
            }
            else
            {
                LocalizationSystem.Add(key, value);
            }
        }

        minSize = new Vector2(460, 250);
        maxSize = minSize;
    }
}

public class TextLocalizerSearchWindow : EditorWindow
{
    public static void Open() //Open the search window
    {
        TextLocalizerSearchWindow window = (TextLocalizerSearchWindow)ScriptableObject.CreateInstance(typeof(TextLocalizerSearchWindow));
        window.titleContent = new GUIContent("Localization Search");

        Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        Rect r = new Rect(mouse.x - 450, mouse.y + 10, 10, 10);
        window.ShowAsDropDown(r, new Vector2(500, 300));
    }

    public string value;
    public Vector2 scroll;
    public Dictionary<string, string> dictionary;

    private void OnEnable() //Get the dictionary for the editor (DEFAULT: English)
    {
        dictionary = LocalizationSystem.GetDictionaryForEditor();
    }

    private void OnGUI() //Draw the search box
    {
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField("Search: ", EditorStyles.boldLabel);
        value = EditorGUILayout.TextField(value);
        EditorGUILayout.EndHorizontal();

        GetSearchResults();
    }

    private void GetSearchResults() //Draw the search results
    {
        if (value == null) { value = ""; } //If the value is null, just set it to empty so that no errors occur

        EditorGUILayout.BeginVertical();
        scroll = EditorGUILayout.BeginScrollView(scroll);

        //For each key, draw its corresponding value in a list format
        foreach (KeyValuePair<string, string> element in dictionary)
        {
            if (element.Key.ToLower().Contains(value.ToLower()) || element.Value.ToLower().Contains(value.ToLower()))
            {
                //Delete Entry Button
                EditorGUILayout.BeginHorizontal("box");
                GUIContent deleteButton = EditorGUIUtility.IconContent("d_LookDevClose@2x"); //This is an X icon...

                if (GUILayout.Button(deleteButton, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
                {
                    if (EditorUtility.DisplayDialog("Remove Key '" + element.Key + "'?", "This will remove the element from localization, are you sure?", "Yes I'm Sure", "Cancel"))
                    {
                        LocalizationSystem.Remove(element.Key);
                        AssetDatabase.Refresh();
                        LocalizationSystem.Init();
                        dictionary = LocalizationSystem.GetDictionaryForEditor();
                    }
                }

                //Clipboard Copy Button
                GUIContent copyContent = EditorGUIUtility.IconContent("Clipboard"); //This is a clipboard icon...
                if (GUILayout.Button(copyContent, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
                {
                    //Copy to clipboard
                    TextEditor te = new TextEditor();
                    te.text = element.Key;
                    te.SelectAll();
                    te.Copy();
                }

                //Draw the values
                EditorGUILayout.TextField(element.Key);
                EditorGUILayout.LabelField(element.Value);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}
