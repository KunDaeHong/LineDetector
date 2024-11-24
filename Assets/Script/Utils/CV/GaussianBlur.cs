using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CV
{
    public class GaussianBlur
    {

        public static async Task<Texture2D> filtering(Texture2D target)
        {
            /// imgMat 구조
            ///[
            ///     0(width_pixel_idx):[
            ///             0(height_pixel_idx): [
            ///                                     r(red 0 ~ 255),
            ///                                     g(green 0 ~ 255),
            ///                                     b(blue 0 ~ 255),
            ///                                     a(alpha 0 ~ 255)
            ///                                  ]
            ///     ]
            ///]
            float[,,] imgMat = CVUtils.to3DArrayFromTexture2D(target);

            await Task.Delay(100); //샘플임.
            ///알아서 Texture2D에 색상값을 집어 넣고 apply 후 반환할 것.
            ///현재 반환값은 임시로 적어놓았음.
            return new Texture2D(0, 0);
        }

    }
}