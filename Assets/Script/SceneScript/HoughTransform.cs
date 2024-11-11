using System;
using System.IO;
using System.Threading.Tasks;
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

        float[,] output = await HoughTransform.houghTransformDetector(inputTexture, 50, 150, 180, 180, 1);

        // for(int x = 0; x < output.GetLength(0); x++)
        // {
        //     for(int y = 0; y < output.GetLength(1); y++)
        //     {
        //         Console.WriteLine($"hough Result x: ${}");
        //     }
        // }

        // output.Apply();
        // byte[] outputImage = output.EncodeToPNG();
        // string filePath = imgPath.Split(".")[0] + "-Converted" + ".png";
        // await File.WriteAllBytesAsync(filePath, outputImage);
        // Debug.Log("이미지 저장 완료: " + filePath);

    }
}
