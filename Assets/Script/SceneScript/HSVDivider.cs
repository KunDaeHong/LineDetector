using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CV;
using UnityEngine;

[ExecuteInEditMode]
public class HSVDivide : MonoBehaviour
{
    [Space(7f)]
    [HideInInspector]
    public string imgPath = "";

    void Start()
    {
        useGUILayout = false;
    }

    void Update()
    {

    }

    public async Task HSVDivider()
    {
        if (!File.Exists(imgPath))
        {
            throw new Exception("Image file doesn't exists. Check that the file exists in the path.");
        }

        Texture2D inputTexture = new Texture2D(2, 2);
        byte[] imgData = await File.ReadAllBytesAsync(imgPath);
        inputTexture.LoadImage(imgData);
        inputTexture.Apply();

        int inputWidth = (int)(inputTexture.width * (50 / 100f));
        int inputHeight = (int)(inputTexture.height * (50 / 100f));
        Texture2D smallSize = CVUtils.resizeTexture2D(inputTexture, inputWidth, inputHeight);

        List<List<ColorHSV>> hsvList = new List<List<ColorHSV>>()
        {
            new List<ColorHSV>() {LaneHSVRange.min_blueHSV, LaneHSVRange.max_blueHSV },
            new List<ColorHSV>() {LaneHSVRange.min_whiteHSV, LaneHSVRange.max_whiteHSV },
            new List<ColorHSV>() {LaneHSVRange.min_yellowHSV, LaneHSVRange.max_yellowHSV }
        };

        Texture2D output = await CVUtils.hsvColorFilter(smallSize, hsvList);
        byte[] outputImage = output.EncodeToPNG();
        string filePath = imgPath.Split(".")[0] + "-HSV_Filter_Converted" + ".png";
        await File.WriteAllBytesAsync(filePath, outputImage);
        Debug.Log("이미지 저장 완료: " + filePath);

    }
}
