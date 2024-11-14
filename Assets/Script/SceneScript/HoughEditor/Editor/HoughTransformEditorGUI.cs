using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Hough)), CanEditMultipleObjects]
public class HoughTransformEditorGUI : Editor
{

    Hough hough;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        hough = (Hough)target;
        drawUserPath();
        startSaveCannyEdge();
    }

    private void drawUserPath()
    {
        GUILayout.Space(10f);
        string displayPath = "Not Selected";
        GUILayout.Label("Image Path Settings", Styles.subTitleBoldSettings(12));

        if (!string.IsNullOrEmpty(hough.imgPath))
        {
            displayPath = hough.imgPath;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(displayPath, Styles.subTitleSettings(12));
            GUILayout.FlexibleSpace();

            if (string.IsNullOrEmpty(hough.imgPath))
            {
                if (GUILayout.Button("Find Path"))
                {
                    EditorApplication.delayCall += () =>
                    hough.imgPath = EditorUtility.OpenFilePanel("Select Image file", "", ".jpg,.jpeg,.png");
                }
            }

            if (GUILayout.Button("Clear"))
            {
                hough.imgPath = "";
            }
        }
    }

    private async void startSaveCannyEdge()
    {
        GUILayout.Space(10);
        bool activate = string.IsNullOrEmpty(hough.imgPath);
        using (new EditorGUI.DisabledScope(activate))
        {
            if (GUILayout.Button("Convert to HoughTransform"))
            {
                await hough.HoughTransformDetector();
            }
        }
    }
}
