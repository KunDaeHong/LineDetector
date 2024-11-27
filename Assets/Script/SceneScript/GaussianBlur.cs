using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CV;
using UnityEngine;

[ExecuteInEditMode]
public class GaussianBlur : MonoBehaviour
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

    public async Task filtering()
    {
        if (!File.Exists(imgPath))
        {
            throw new Exception("Image file doesn't exists. Check that the file exists in the path.");
        }

        Texture2D inputTexture = new Texture2D(2, 2);
        byte[] imgData = await File.ReadAllBytesAsync(imgPath);
        inputTexture.LoadImage(imgData);
        inputTexture.Apply();

        Texture2D output = await CV.GaussianBlur.filtering(inputTexture, 3, 255);
        byte[] outputImage = output.EncodeToPNG();
        string filePath = imgPath.Split(".")[0] + "-GaussianBlur_Filter_Converted" + ".png";
        await File.WriteAllBytesAsync(filePath, outputImage);
        Debug.Log("이미지 저장 완료: " + filePath);

    }
}
