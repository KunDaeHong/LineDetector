using System;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

[ExecuteInEditMode]
public class CarLane : MonoBehaviour
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

    public async Task detector()
    {
        if (!File.Exists(imgPath))
        {
            throw new Exception("Image file doesn't exists. Check that the file exists in the path.");
        }

        Texture2D inputTexture = new Texture2D(2, 2);
        byte[] imgData = await File.ReadAllBytesAsync(imgPath);
        inputTexture.LoadImage(imgData);
        inputTexture.Apply();

        await CV.CarLane.detectCarLane(inputTexture);
    }
}
