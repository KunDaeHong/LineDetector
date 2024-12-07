using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CV;
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

    public void detector()
    {
        CV.CarLane.detectCarLane();
    }
}
