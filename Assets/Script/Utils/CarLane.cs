using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

namespace CV
{
    public class CarLane
    {

        //다른 색상 차선 전용
        private static List<List<ColorHSV>> hsvList = new List<List<ColorHSV>>()
        {
            new List<ColorHSV>() {LaneHSVRange.min_blueHSV, LaneHSVRange.max_blueHSV },
            new List<ColorHSV>() {LaneHSVRange.min_yellowHSV, LaneHSVRange.max_yellowHSV }
        };

        //흰색 차선 전용
        private static List<Color> colorList = new List<Color>()
        {
           new Color(160, 160, 160)
        };

        public static void detectCarLane(Texture2D img)
        {


        }
    }

}