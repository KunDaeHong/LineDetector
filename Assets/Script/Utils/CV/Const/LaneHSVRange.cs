using System;

using UnityEngine;

namespace CV
{
    public class LaneHSVRange
    {
        public static ColorHSV max_blueHSV = new ColorHSV(240, 49, 100);
        public static ColorHSV max_yellowHSV = new ColorHSV(38, 100, 23);

        public static ColorHSV min_blueHSV = new ColorHSV(184, 28, 70);
        public static ColorHSV min_yellowHSV = new ColorHSV(60, 51, 98);

        public static ColorHSV min_whiteHSV = new ColorHSV(140, (float)3.0, (float)54.5);
        public static ColorHSV max_whiteHSV = new ColorHSV(192, (float)7.0, (float)76.1);
    }
}