using UnityEditor;
using UnityEngine;

public class Styles
{

    public static GUIStyle mainTitleSettings(int fontSize = 20)
    {
        GUIStyle mainTitleSettings = new GUIStyle(EditorStyles.label);
        mainTitleSettings.normal.textColor = Color.white;
        mainTitleSettings.hover.textColor = Color.white;
        mainTitleSettings.fontSize = fontSize;

        return mainTitleSettings;
    }

    public static GUIStyle subTitleBoldSettings(int fontSize = 14)
    {
        GUIStyle subTitleSettings = new GUIStyle(EditorStyles.label);
        subTitleSettings.normal.textColor = new Color32(189, 189, 189, 255);
        subTitleSettings.hover.textColor = new Color32(189, 189, 189, 255);
        subTitleSettings.fontStyle = FontStyle.Bold;
        subTitleSettings.fontSize = fontSize;
        subTitleSettings.wordWrap = true;

        return subTitleSettings;
    }

    public static GUIStyle subTitleSettings(int fontSize = 14)
    {
        GUIStyle subTitleSettings = new GUIStyle(EditorStyles.label);
        subTitleSettings.normal.textColor = new Color32(189, 189, 189, 255);
        subTitleSettings.hover.textColor = new Color32(189, 189, 189, 255);
        subTitleSettings.fontSize = fontSize;
        subTitleSettings.wordWrap = true;

        return subTitleSettings;
    }

}