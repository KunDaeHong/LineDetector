using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarLane)), CanEditMultipleObjects]
public class CarLaneGUI : Editor
{

    CarLane carLane;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        carLane = (CarLane)target;
        drawUserPath();
        startLaneDetect();
    }

    private void drawUserPath()
    {
        GUILayout.Space(10f);
        string displayPath = "Not Selected";
        GUILayout.Label("Image Path Settings", Styles.subTitleBoldSettings(12));

        if (!string.IsNullOrEmpty(carLane.imgPath))
        {
            displayPath = carLane.imgPath;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(displayPath, Styles.subTitleSettings(12));
            GUILayout.FlexibleSpace();

            if (string.IsNullOrEmpty(carLane.imgPath))
            {
                if (GUILayout.Button("Find Path"))
                {
                    EditorApplication.delayCall += () =>
                    carLane.imgPath = EditorUtility.OpenFilePanel("Select Image file", "", ".jpg,.jpeg,.png");
                }
            }

            if (GUILayout.Button("Clear"))
            {
                carLane.imgPath = "";
            }
        }
    }

    private void startLaneDetect()
    {
        GUILayout.Space(10);
        bool activate = string.IsNullOrEmpty(carLane.imgPath);
        using (new EditorGUI.DisabledScope(activate))
        {
            if (GUILayout.Button("Find CarLane"))
            {
                carLane.detector();
            }
        }
    }
}
