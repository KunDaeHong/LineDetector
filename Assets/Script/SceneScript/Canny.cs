using System;
using System.IO;
using System.Threading.Tasks;
using CV;
using UnityEngine;

[ExecuteInEditMode]
public class Canny : MonoBehaviour
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

    public async Task cannyEdgeDetector()
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

        // Texture2D targetGrayscale = CVUtils.toGrayScale(smallSize);
        // float[,] target2DArray = CVUtils.to2DArrayFromTexture2D(targetGrayscale);
        // float[,] filtering = await CannyEdge.bilateralFilterCoroutine(target2DArray);
        // Texture2D test = new Texture2D(filtering.GetLength(0), filtering.GetLength(1));

        float[,] outputTexture = await CannyEdge.cannyEdgeDetector(smallSize, 50, 150);
        Texture2D output = new Texture2D(outputTexture.GetLength(0), outputTexture.GetLength(1));

        for (int x = 0; x < outputTexture.GetLength(0); x++)
        {
            for (int y = 0; y < outputTexture.GetLength(1); y++)
            {
                float newColor = outputTexture[x, y] / 255;
                output.SetPixel(x, y, new Color(newColor, newColor, newColor));
            }
        }

        // for (int x = 0; x < filtering.GetLength(0); x++)
        // {
        //     for (int y = 0; y < filtering.GetLength(1); y++)
        //     {
        //         test.SetPixel(x, y, new Color(filtering[x, y], filtering[x, y], filtering[x, y]));
        //     }
        // }

        output.Apply();
        byte[] outputImage = output.EncodeToPNG();
        string filePath = imgPath.Split(".")[0] + "Canny-Converted" + ".png";
        await File.WriteAllBytesAsync(filePath, outputImage);
        Debug.Log("이미지 저장 완료: " + filePath);

    }
}
