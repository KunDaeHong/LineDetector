using System;

using UnityEngine;

namespace CV
{
    public class ColorHSV
    {
        public float hue { get; set; }
        public float saturation { get; set; }
        public float value { get; set; }

        public ColorHSV(float hue, float saturation, float value)
        {
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }

        public static bool operator <(ColorHSV left, ColorHSV right)
        {
            if (left is null) return right is not null;
            if (right is null) return false;

            if (left.hue < right.hue &&
                left.saturation < right.saturation &&
                left.value < right.value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator >(ColorHSV left, ColorHSV right)
        {
            if (left is null) return false;
            if (right is null) return true;

            if (left.hue > right.hue &&
                left.saturation > right.saturation &&
                left.value > right.value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ColorHSV rgb2hsv(Color target)
        {
            float r = target.r / 255;
            float g = target.g / 255;
            float b = target.b / 255;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0;
            float s = max != 0 ? delta / max : 0;
            float v = max;

            if (delta != 0)
            {
                if (r == max) h = 60 * ((g - b) / delta % 6);
                if (g == max) h = 60 * (2 + ((b - r) / delta));
                if (b == max) h = 60 * (4 + ((r - g) / delta));
            }

            if (h < 0) h += 360;

            s *= 100;
            v *= 100;

            return new ColorHSV(h, s, v);
        }
    }
}