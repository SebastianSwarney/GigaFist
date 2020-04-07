using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LocalizedString))]
public class LocalizedStringDrawer : PropertyDrawer
{
    bool dropdown;
    float height;
    string[] languages;

    //Sets the height of the property
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (dropdown)
        {
            if (languages == null)
            {
                GetLanguages();
            }

            //Dynamically set height based on number of languages supported
            if (languages != null)
            {
                return height + (25 * languages.Length);
            }
            return height + 25;
        }
        return 20;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        //Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        position.width -= 57;
        position.height = 18;

        //Adjust Rect
        Rect valueRect = new Rect(position);
        valueRect.x += 15;
        valueRect.width -= 15;

        //Create rect for foldout
        position.x -= 35;
        Rect foldButtonRect = new Rect(position);
        foldButtonRect.width = 15;

        dropdown = EditorGUI.Foldout(foldButtonRect, dropdown, "");

        //Key Label
        position.x += 5;
        EditorGUI.LabelField(position, "Key", EditorStyles.boldLabel);
        position.x -= 5;

        position.x += 35;
        position.width -= 15;
        //Text field for the key
        SerializedProperty key = property.FindPropertyRelative("key");
        key.stringValue = EditorGUI.TextField(position, key.stringValue);

        //Draw Search Button
        position.x += position.width + 2;
        position.width = 25;
        position.height = 25;

        GUIContent searchContent = EditorGUIUtility.IconContent("ViewToolZoom On");
        if (GUI.Button(position, searchContent))
        {
            TextLocalizerSearchWindow.Open();
        }

        //Draw Add/Edit Button
        position.x += position.width + 2;

        GUIContent storeContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
        if (GUI.Button(position, storeContent))
        {
            TextLocalizerEditWindow.Open(key.stringValue);
        }

        //If the dropdown is enabled, draw the translations
        if (dropdown)
        {
            if (languages == null)
            {
                GetLanguages();
            }

            if (languages != null)
            {
                float startX = valueRect.x;
                for (int i = 0; i < languages.Length; i++)
                {
                    //Draw label with the specific language and its translation
                    valueRect.x = startX;
                    var value = LocalizationSystem.GetLocalizedValue(key.stringValue, (LocalizationSystem.Language)i);
                    GUIStyle style = GUI.skin.box;
                    height = style.CalcHeight(new GUIContent(value), valueRect.width);

                    //Language Label
                    valueRect.height = height;
                    valueRect.y += 21;
                    EditorGUI.LabelField(valueRect, languages[i] + ":");

                    //Translation Value Label
                    valueRect.x += position.width + 25;
                    EditorGUI.LabelField(valueRect, value, EditorStyles.wordWrappedLabel);
                }
            }
        }

        EditorGUI.EndProperty();
    }

    //Get the names of the languages supported
    private void GetLanguages()
    {
        languages = System.Enum.GetNames(typeof(LocalizationSystem.Language));
    }
}
