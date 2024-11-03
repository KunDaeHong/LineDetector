using System;
using System.Collections.Generic;
using UnityEngine;

namespace CV
{
    public class CVUtils
    {

        //MARK: FOR Texture2D Utils
        public static Texture2D resizeTexture2D(Texture2D target, int width, int height)
        {
            Texture2D resizedTexture = new Texture2D(width, height);
            Color[] pixels = resizedTexture.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / (float)width;
                    float v = (float)y / (float)height;
                    pixels[y * width + x] = target.GetPixelBilinear(u, v);
                }
            }

            resizedTexture.SetPixels(pixels);
            resizedTexture.Apply();

            return resizedTexture;
        }

        public static Texture2D toGrayScale(Texture2D target)
        {
            Texture2D newTexture = target;

            for (int x = 0; x < target.width; x++)
            {
                for (int y = 0; y < target.height; y++)
                {
                    Color oldColor = target.GetPixel(x, y);
                    float grayscale = (oldColor.r + oldColor.g + oldColor.b) / 3;
                    newTexture.SetPixel(x, y, new Color(grayscale, grayscale, grayscale, oldColor.a));
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        public static Texture2D extractSpecificColors(Texture2D target, List<Color> colors)
        {
            Texture2D output = new Texture2D(target.width, target.height);

            foreach (var color in colors)
            {
                //각 픽셀별로 구해야 함. 멀티스레드 작동이 필요함.
                // ColorHSV firstHsv = ColorHSV.rgb2hsv()
            }

            return output;
        }

        //work only grayscale
        public static float[,] to2DArrayFromTexture2D(Texture2D target)
        {
            float[,] output = new float[target.width, target.height];

            for (int w = 0; w < target.width; w++)
            {
                for (int h = 0; h < target.height; h++)
                {
                    //주의!: Unity Color는 모두 0~1로 정규화 되어 있음.
                    Color color = target.GetPixel(w, h);
                    //output[w, h] = (color.r + color.g + color.b) / 3;
                    output[w, h] = ((color.r * 255) + (color.g * 255) + (color.b * 255)) / 3;
                }
            }

            return output;
        }

        //MARK: Filter Utils

        //For info : only for grayscale
        public static Texture2D gaussianFilter(Texture2D target, int size, float sigma)
        {
            float[,] kernel = gaussianKernel(size, sigma);
            int kernelSize = size / 2;

            for (int x = 0; x < target.width; x++)
            {
                for (int y = 0; y < target.height; y++)
                {
                    float avgSum = 0;
                    float a = 0;

                    for (int kernelX = -kernelSize; kernelX < kernelSize; kernelX++)
                    {
                        for (int kernelY = -kernelSize; kernelY < kernelSize; kernelY++)
                        {
                            int kx2tx = Math.Clamp(x + kernelX, 0, target.width - 1);
                            int ky2ty = Math.Clamp(y + kernelY, 0, target.height - 1);

                            Color oldColor = target.GetPixel(kx2tx, ky2ty);
                            float kernelResult = kernel[kernelX + kernelSize, kernelY + kernelSize];

                            a += oldColor.a * kernelResult;
                            avgSum += kernelResult;
                        }
                    }

                    target.SetPixel(x, y, new Color(0, 0, 0, a / avgSum));
                }
            }

            return target;
        }


        //MARK: MASK Or Kernel

        //컨볼루션 연산으로 계산
        public static float[,] applyConvKernel(float[,] target, float[,] filter, int stride = 1, int padding = 0)
        {
            int img_w = target.GetLength(0);
            int img_h = target.GetLength(1);
            int filter_w = filter.GetLength(0);
            int filter_h = filter.GetLength(1);

            //참고: https://amber-chaeeunk.tistory.com/24
            //참고: https://velog.io/@7ryean/CNN-Convolution-%EC%97%B0%EC%82%B0
            //cnn연산 시 픽셀 수가 감소되는 현상을 없애기 위해 padding을 이용하여 연산
            int output_w = (img_w - filter_w + 2 * padding) / stride + 1;
            int output_h = (img_h - filter_h + 2 * padding) / stride + 1;

            float[,] kernel = new float[output_w, output_h];
            float[,] paddedTarget = new float[img_w + 2 * padding, img_h + 2 * padding];

            for (int x = 0; x < img_w; x++)
            {
                for (int y = 0; y < img_h; y++)
                {
                    paddedTarget[x + padding, y + padding] = target[x, y] / 255;
                }
            }

            float filterSum = 0;
            for (int i = 0; i < filter_w; i++)
            {
                for (int j = 0; j < filter_h; j++)
                {
                    filterSum += filter[i, j];
                }
            }

            for (int i_w = 0; i_w < output_w; i_w++)
            {
                for (int i_h = 0; i_h < output_h; i_h++)
                {
                    float sum = 0;

                    for (int f_w = 0; f_w < filter_w; f_w++)
                    {
                        for (int f_h = 0; f_h < filter_h; f_h++)
                        {
                            int i_r = i_w * stride + f_w;
                            int i_c = i_h * stride + f_h;

                            // if (i_r < img_w && i_c < img_h)
                            // {
                            //     sum += target[i_r, i_c] * filter[f_w, f_h];
                            // }

                            sum += paddedTarget[i_r, i_c] * filter[f_w, f_h];
                        }
                    }

                    kernel[i_w, i_h] = filterSum != 0 ? sum / filterSum : sum;
                }
            }

            return kernel;
        }

        private static float[,] gaussianKernel(int size, float sigma)
        {
            float[,] kernel = new float[size, size];
            float sum = 0;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int kernelX = x - size / 2;
                    int kernelY = y - size / 2;
                    kernel[x, y] = (float)Math.Exp(-(Math.Pow(kernelX, 2) + Math.Pow(kernelY, 2)) / (2 * Math.Pow(sigma, 2)));
                    sum += kernel[x, y];
                }
            }

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    kernel[x, y] /= sum;
                }
            }

            return kernel;
        }

        //MARK: Calculate Size

        public static bool shapeIsSame(int[,] mask1, int[,] mask2)
        {
            return mask1.GetLength(0) == mask2.GetLength(0) &&
                    mask1.GetLength(1) == mask2.GetLength(1);
        }

        public static bool isBoundary(int x, int y, Vector2 maxRowCol)
        {
            return ((x >= 0 && x < maxRowCol.x) && (y >= 0 && y < maxRowCol.y));
        }
    }
}