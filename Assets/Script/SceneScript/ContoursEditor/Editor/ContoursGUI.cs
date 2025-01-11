using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContoursDetect)), CanEditMultipleObjects]
public class ContoursGUI : Editor
{

    ContoursDetect contoursDetect;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        contoursDetect = (ContoursDetect)target;
        drawUserPath();
        startSaveContoursDetect();
    }

    private void drawUserPath()
    {
        GUILayout.Space(10f);
        string displayPath = "Not Selected";
        GUILayout.Label("Image Path Settings", Styles.subTitleBoldSettings(12));

        if (!string.IsNullOrEmpty(contoursDetect.imgPath))
        {
            displayPath = contoursDetect.imgPath;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(displayPath, Styles.subTitleSettings(12));
            GUILayout.FlexibleSpace();

            if (string.IsNullOrEmpty(contoursDetect.imgPath))
            {
                if (GUILayout.Button("Find Path"))
                {
                    EditorApplication.delayCall += () =>
                    contoursDetect.imgPath = EditorUtility.OpenFilePanel("Select Image file", "", ".jpg,.jpeg,.png");
                }
            }

            if (GUILayout.Button("Clear"))
            {
                contoursDetect.imgPath = "";
            }
        }
    }

    private async void startSaveContoursDetect()
    {
        GUILayout.Space(10);
        bool activate = string.IsNullOrEmpty(contoursDetect.imgPath);
        using (new EditorGUI.DisabledScope(activate))
        {
            if (GUILayout.Button("Detect Contours"))
            {
                await contoursDetect.ContoursDetector();
            }
        }
    }
}
