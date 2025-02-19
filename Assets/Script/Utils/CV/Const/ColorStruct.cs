using System;

using UnityEngine;

namespace CV
{
    public class ColorRGB
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public ColorRGB(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

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

            if (left.hue > right.hue)
            {
                return false;
            }

            float left_dist = (float)(Math.Pow(left.saturation, 2) + Math.Pow(left.value, 2));
            float right_dist = (float)(Math.Pow(right.saturation, 2) + Math.Pow(right.value, 2));

            return left_dist < right_dist;
        }

        public static bool operator >(ColorHSV left, ColorHSV right)
        {
            if (left is null) return false;
            if (right is null) return true;

            if (left.hue < right.hue)
            {
                return false;
            }

            float left_dist = (float)(Math.Pow(left.saturation, 2) + Math.Pow(left.value, 2));
            float right_dist = (float)(Math.Pow(right.saturation, 2) + Math.Pow(right.value, 2));

            return left_dist > right_dist;
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

