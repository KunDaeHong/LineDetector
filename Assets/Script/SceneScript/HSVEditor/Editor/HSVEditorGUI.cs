using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HSVDivide)), CanEditMultipleObjects]
public class HSVDivideGUI : Editor
{

    HSVDivide hsvDivide;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        hsvDivide = (HSVDivide)target;
        drawUserPath();
        startSaveHSVDivide();
    }

    private void drawUserPath()
    {
        GUILayout.Space(10f);
        string displayPath = "Not Selected";
        GUILayout.Label("Image Path Settings", Styles.subTitleBoldSettings(12));

        if (!string.IsNullOrEmpty(hsvDivide.imgPath))
        {
            displayPath = hsvDivide.imgPath;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(displayPath, Styles.subTitleSettings(12));
            GUILayout.FlexibleSpace();

            if (string.IsNullOrEmpty(hsvDivide.imgPath))
            {
                if (GUILayout.Button("Find Path"))
                {
                    EditorApplication.delayCall += () =>
                    hsvDivide.imgPath = EditorUtility.OpenFilePanel("Select Image file", "", ".jpg,.jpeg,.png");
                }
            }

            if (GUILayout.Button("Clear"))
            {
                hsvDivide.imgPath = "";
            }
        }
    }

    private async void startSaveHSVDivide()
    {
        GUILayout.Space(10);
        bool activate = string.IsNullOrEmpty(hsvDivide.imgPath);
        using (new EditorGUI.DisabledScope(activate))
        {
            if (GUILayout.Button("Convert to HSV Filtering"))
            {
                await hsvDivide.HSVDivider();
            }
        }
    }
}
