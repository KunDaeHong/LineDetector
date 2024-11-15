using System;

using UnityEngine;

namespace CV
{
    public class LaneHSVRange
    {
        ColorHSV max_blueHSV = new ColorHSV(240, 100, 4);
        ColorHSV max_whiteHSV = new ColorHSV(0, 0, 22);
        ColorHSV max_yellowHSV = new ColorHSV(38, 100, 23);

        ColorHSV min_blueHSV = new ColorHSV(240, 6, 95);
        ColorHSV min_whiteHSV = new ColorHSV(0, 0, 97);
        ColorHSV min_yellowHSV = new ColorHSV(60, 51, 98);
    }
}