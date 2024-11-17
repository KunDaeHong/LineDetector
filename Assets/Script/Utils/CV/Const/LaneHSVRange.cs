using System;

using UnityEngine;

namespace CV
{
    public class LaneHSVRange
    {
        public static ColorHSV max_blueHSV = new ColorHSV(240, 100, 4);
        public static ColorHSV max_whiteHSV = new ColorHSV(0, 0, 22);
        public static ColorHSV max_yellowHSV = new ColorHSV(38, 100, 23);

        public static ColorHSV min_blueHSV = new ColorHSV(240, 6, 95);
        public static ColorHSV min_whiteHSV = new ColorHSV(0, 0, 97);
        public static ColorHSV min_yellowHSV = new ColorHSV(60, 51, 98);
    }
}