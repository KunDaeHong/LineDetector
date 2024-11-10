using System;

using UnityEngine;

namespace CV
{
    public class ColorMNM
    {
        ColorHSV maxblueHSV = ColorHSV(240, 100, 4);
        ColorHSV maxwhiteHSV = ColorHSV(0, 0, 22);
        ColorHSV maxyellowHSV = ColorHSV(38, 100, 23);

        ColorHSV minblueHSV = ColorHSV(240, 6, 95);
        ColorHSV minwhiteHSV = ColorHSV(0, 0, 97);
        ColorHSV minyellowHSV = ColorHSV(60, 51, 98);
    }
}