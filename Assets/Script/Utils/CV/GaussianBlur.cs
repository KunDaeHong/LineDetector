using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CV
{
    public class GaussianBlur
    {

        public static async Task<Texture2D> filtering(Texture2D target, int size, float sigma )
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
            ///
            float[,,] imgMat = CVUtils.to3DArrayFromTexture2D(target);

            float[,] kernel = gaussianColorKernel(size, sigma);
            int kernelSize = size / 2;

            for (int x = 0; x < target.width; x++)
            {
                for (int y = 0; y < target.height; y++)
                {
                    float avgSum = 0;
                    float r = 0;
                    float g = 0;
                    float b = 0;

                    for (int kernelX = -kernelSize; kernelX < kernelSize; kernelX++)
                    {
                        for (int kernelY = -kernelSize; kernelY < kernelSize; kernelY++)
                        {
                            int kx2tx = Math.Clamp(x + kernelX, 0, target.width - 1);
                            int ky2ty = Math.Clamp(y + kernelY, 0, target.height - 1);

                            Color oldColor = target.GetPixel(kx2tx, ky2ty);
                            float kernelResult = kernel[kernelX + kernelSize, kernelY + kernelSize];

                            r = oldColor.r * kernelResult * 255;
                            g = oldColor.g * kernelResult * 255;
                            b = oldColor.b * kernelResult * 255;
                            
                        }
                    }

                    target.SetPixel(x, y, new Color(r, g, b, avgSum)/255);
                }
            }

            return target;
            await Task.Delay(100); //샘플임.
            ///알아서 Texture2D에 색상값을 집어 넣고 apply 후 반환할 것.
            ///현재 반환값은 임시로 적어놓았음.
            return new Texture2D(0, 0);
        }

        private static float[,] gaussianColorKernel(int size, float sigma)
        {
            float[,] kernel = new float[size, size];
            float sum = 0;
            
            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    int kernelX = x - size / 2;
                    int kernelY = y - size / 2;
                    kernel[x, y] = (float)Math.Exp(-(Math.Pow(kernelX, 2) + Math.Pow(kernelY, 2)) / (2 * Math.Pow(sigma, 2)));
                    sum += kernel[x, y];
                }
            }

            for(int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    kernel[x, y] /= sum;
                }
            }

            return kernel;
        }

    }
}