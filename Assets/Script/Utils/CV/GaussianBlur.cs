using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CV
{
    public class GaussianBlur
    {

        public static async Task<Texture2D> filtering(Texture2D target, int size, float sigma)
        {

            //선재를 위해 설명 해줌

            float[,] kernel = gaussianColorKernel(size, sigma); //커널 제작
            int kernelSize = size / 2; //커널 사이즈 조정

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
            int width = imgMat.GetLength(0); //이미지 너비
            int height = imgMat.GetLength(1); //이미지 높이

            Texture2D output = new Texture2D(width, height); //반환할 이미지 (새로운 이미지에 덮어씌워 버리면 커널 내 값이 업데이트되서 안됨.)

            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    float avgSum = 0; //기존과 같음
                    float r = 0; // red
                    float g = 0; // green
                    float b = 0; // blue

                    for (int kernelY = -kernelSize; kernelY < kernelSize; kernelY++)
                    {
                        for (int kernelX = -kernelSize; kernelX < kernelSize; kernelX++)
                        {
                            int kx2tx = Math.Clamp(x + kernelX, 0, width - 1); //커널 인덱스가 이미지 인덱스를 넘어가는지 체크
                            int kx2ty = Math.Clamp(y + kernelY, 0, height - 1); //커널 인덱스가 이미지 인덱스를 넘어가는지 체크

                            float oldR = imgMat[kx2tx, kx2ty, 0]; //imgMat에 있는 red 0 ~ 255 기준
                            float oldG = imgMat[kx2tx, kx2ty, 1]; //imgMat에 있는 green 0 ~ 255 기준
                            float oldB = imgMat[kx2tx, kx2ty, 2]; //imgMat에 있는 blue 0 ~ 255 기준
                            float kernelResult = kernel[kernelX + kernelSize, kernelY + kernelSize]; //해당 커널 인덱스에 있는 값 가져오기

                            r += oldR * kernelResult; //커널 값 가중치와 red와 곱한 후 누적
                            g += oldG * kernelResult; //커널 값 가중치와 green과 곱한 후 누적
                            b += oldB * kernelResult; //커널 값 가중치와 blue와 곱한 후 누적

                            avgSum += kernelResult; // 커널 값 가중치 누적
                        }
                    }

                    float newR = r / avgSum; // 누적된 red 색상 가중치에서 누적된 커널 가중치 나누기
                    float newG = g / avgSum; // 누적된 green 색상 가중치에서 누적된 커널 가중치 나누기
                    float newB = b / avgSum; // 누적된 blue 색상 가중치에서 누적된 커널 가중치 나누기

                    //255를 나눈 이유는 현재 색상 가중치는 0 ~ 255를 기준으로 작성되었기에 다시 0 ~ 1로 정규화 하기 위해 각 색상마다 255로 나누기
                    output.SetPixel(x, y, new Color(newR / 255, newG / 255, newB / 255));
                }
            }

            //Texture2D 모든 색상 다시 적용 함수
            output.Apply();

            // for (int x = 0; x < target.width; x++)
            // {
            //     for (int y = 0; y < target.height; y++)
            //     {


            //         for (int kernelX = -kernelSize; kernelX < kernelSize; kernelX++)
            //         {
            //             for (int kernelY = -kernelSize; kernelY < kernelSize; kernelY++)
            //             {
            //                 int kx2tx = Math.Clamp(x + kernelX, 0, target.width - 1);
            //                 int ky2ty = Math.Clamp(y + kernelY, 0, target.height - 1);

            //                 Color oldColor = target.GetPixel(kx2tx, ky2ty);
            //                 float kernelResult = kernel[kernelX + kernelSize, kernelY + kernelSize];

            //                 r = oldColor.r * kernelResult;
            //                 g = oldColor.g * kernelResult;
            //                 b = oldColor.b * kernelResult;

            //             }
            //         }

            //         target.SetPixel(x, y, new Color(r / avgSum, g / avgSum, b / avgSum));
            //     }
            // }

            await Task.Delay(0);
            return output;
        }

        private static float[,] gaussianColorKernel(int size, float sigma)
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

    }
}