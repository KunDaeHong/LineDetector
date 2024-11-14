using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using CV;

using UnityEngine;

[ExecuteInEditMode]
public class Hough : MonoBehaviour
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

    public async Task HoughTransformDetector()
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
        Texture2D finish = new Texture2D(inputWidth, inputHeight);

        List<List<int>> output = await HoughTransform.houghTransformDetector(inputTexture, 50, 150, 1, 1, 97);

        //블랙 셋팅
        for (int x = 0; x < inputWidth; x++)
        {
            for (int y = 0; y < inputHeight; y++)
            {
                finish.SetPixel(x, y, new Color(0, 0, 0));
            }
        }

        foreach (var rho in output)
        {
            //CVUtils.DrawLine(finish, rho[0], rho[1], rho[2], rho[3], new Color(1, 1, 1));
            List<Vector2> points = CVUtils.getLineCoordinates(new Vector2(rho[0], rho[1]), new Vector2(rho[2], rho[3]));

            foreach (var point in points)
            {
                int x = (int)point.x;
                int y = (int)point.y;

                if (x >= 0 && x < inputWidth && y >= 0 && y < inputHeight)
                {
                    finish.SetPixel(x, y, new Color(1, 1, 1));
                }
            }
        }

        finish.Apply();
        finish = CVUtils.resizeTexture2D(finish, inputWidth * 2, inputHeight * 2);
        byte[] outputImage = finish.EncodeToPNG();
        string filePath = imgPath.Split(".")[0] + "-Converted" + ".png";
        await File.WriteAllBytesAsync(filePath, outputImage);
        Debug.Log("이미지 저장 완료: " + filePath);

    }
}
