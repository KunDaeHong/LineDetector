using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using CV;
using UnityEngine;

[ExecuteInEditMode]
public class ContoursDetect : MonoBehaviour
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

    public async Task ContoursDetector()
    {
        if (!File.Exists(imgPath))
        {
            throw new Exception("Image file doesn't exists. Check that the file exists in the path.");
        }

        Texture2D inputTexture = new Texture2D(2, 2);
        byte[] imgData = await File.ReadAllBytesAsync(imgPath);
        inputTexture.LoadImage(imgData);
        inputTexture.Apply();

        //이미지 스케일 100% > 75% 스케일로 변환
        int inputWidth = (int)(inputTexture.width * (75 / 100f));
        int inputHeight = (int)(inputTexture.height * (75 / 100f));
        Texture2D smallSize = CVUtils.resizeTexture2D(inputTexture, inputWidth, inputHeight);
        Texture2D grayScale = CVUtils.toGrayScale(smallSize);

        //감마 보정
        Texture2D adjustGamma = CVUtils.clippingGammaFromTexture2D(grayScale, -80);
        Texture2D adjustContrast = CVUtils.clippingContrastFromTexture2D(adjustGamma, 80);

        //hsv컬러 필터를 통해 원하는 색상 범위만 통과되도록 변환
        // List<List<ColorHSV>> hsvList = new List<List<ColorHSV>>()
        // {
        //     // new List<ColorHSV>() {LaneHSVRange.min_blueHSV, LaneHSVRange.max_blueHSV },
        //     // new List<ColorHSV>() {LaneHSVRange.min_yellowHSV, LaneHSVRange.max_yellowHSV }
        //     new List<ColorHSV>() {LaneHSVRange.min_whiteHSV, LaneHSVRange.max_whiteHSV}
        // };

        // Texture2D hsvFilter_output = await CVUtils.hsvColorFilter(smallSize, hsvList);
        // Debug.Log("hsv필터링 완료");


        //Texture2D output = await Contours.contoursDetectorByCanny(hsvFilter_output);
        byte[] outputImage = adjustContrast.EncodeToPNG();
        string filePath = imgPath.Split(".")[0] + "-Contours_Detect_Converted" + ".png";
        await File.WriteAllBytesAsync(filePath, outputImage);
        Debug.Log("이미지 저장 완료: " + filePath);

    }
}
