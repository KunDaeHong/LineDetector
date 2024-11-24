using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GaussianBlur)), CanEditMultipleObjects]
public class GaussianBlurEditorGUI : Editor
{

    GaussianBlur gaussian;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        gaussian = (GaussianBlur)target;
        drawUserPath();
        startSaveGaussianBlur();
    }

    private void drawUserPath()
    {
        GUILayout.Space(10f);
        string displayPath = "Not Selected";
        GUILayout.Label("Image Path Settings", Styles.subTitleBoldSettings(12));

        if (!string.IsNullOrEmpty(gaussian.imgPath))
        {
            displayPath = gaussian.imgPath;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(displayPath, Styles.subTitleSettings(12));
            GUILayout.FlexibleSpace();

            if (string.IsNullOrEmpty(gaussian.imgPath))
            {
                if (GUILayout.Button("Find Path"))
                {
                    EditorApplication.delayCall += () =>
                    gaussian.imgPath = EditorUtility.OpenFilePanel("Select Image file", "", ".jpg,.jpeg,.png");
                }
            }

            if (GUILayout.Button("Clear"))
            {
                gaussian.imgPath = "";
            }
        }
    }

    private async void startSaveGaussianBlur()
    {
        GUILayout.Space(10);
        bool activate = string.IsNullOrEmpty(gaussian.imgPath);
        using (new EditorGUI.DisabledScope(activate))
        {
            if (GUILayout.Button("Convert to CannyEdge"))
            {
                await gaussian.filtering();
            }
        }
    }
}
