using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Canny)), CanEditMultipleObjects]
public class CannyEditorGUI : Editor
{

    Canny canny;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        canny = (Canny)target;
        drawUserPath();
        startSaveCannyEdge();
    }

    private void drawUserPath()
    {
        GUILayout.Space(10f);
        string displayPath = "Not Selected";
        GUILayout.Label("Image Path Settings", Styles.subTitleBoldSettings(12));

        if (!string.IsNullOrEmpty(canny.imgPath))
        {
            displayPath = canny.imgPath;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(displayPath, Styles.subTitleSettings(12));
            GUILayout.FlexibleSpace();

            if (string.IsNullOrEmpty(canny.imgPath))
            {
                if (GUILayout.Button("Find Path"))
                {
                    EditorApplication.delayCall += () =>
                    canny.imgPath = EditorUtility.OpenFilePanel("Select Image file", "", ".jpg,.jpeg,.png");
                }
            }

            if (GUILayout.Button("Clear"))
            {
                canny.imgPath = "";
            }
        }
    }

    private async void startSaveCannyEdge()
    {
        GUILayout.Space(10);
        bool activate = string.IsNullOrEmpty(canny.imgPath);
        using (new EditorGUI.DisabledScope(activate))
        {
            if (GUILayout.Button("Convert to CannyEdge"))
            {
                await canny.cannyEdgeDetector();
            }
        }
    }
}
