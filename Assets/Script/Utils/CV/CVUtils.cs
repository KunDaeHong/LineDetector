using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

namespace CV
{
    public class CVUtils
    {
        private static MultiThreadUtils tasker = new MultiThreadUtils();
        public CVUtils()
        {
            tasker.listen = resListen;
        }

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

        public static Texture2D extractSpecificColors(Texture2D target, List<List<ColorHSV>> colors)
        {
            Texture2D output = new Texture2D(target.width, target.height);

            for (int w = 0; w < target.width; w++)
            {
                for (int h = 0; h < target.height; h++)
                {
                    Color targetColor = target.GetPixel(w, h); // 0 ~ 1로 정규화 되어 있음.
                    ColorHSV targetColorHsv = ColorHSV.rgb2hsv(targetColor);

                    foreach (var cRange in colors)
                    {
                        if (cRange.Count != 2)
                        {
                            Debug.LogError("Color Range need only 2 colors");
                            break;
                        }

                        //TODO: 픽셀 hsv범위 셋팅 후 멀티스레드 작성

                    }
                }
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

        public static async Task hsvColorFilter(Texture2D target, List<ColorHSV> hsvList)
        {
            //TODO: Should be create hsv specific filter
            int width = target.width;
            int height = target.height;
            Texture2D output = new Texture2D(width, height);
            //1: widthCnt 2: heightCnt 3: hsvColor
            float[,,] hsvColorTarget = new float[width, height, 3];

            //rgb texture2d 2 hsv2d
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color targetColor = target.GetPixel(x, y);
                    await tasker.SpawnAsync(() => hsvColorFilterSubTask(targetColor, x, y, hsvColorTarget));
                }
            }

            Console.WriteLine("Hsv 컬러로 변환 완료");

            //hue 360, sat 0 ~ 100, val 0 ~ 100
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                }
            }
        }

        private static async Task<bool> hsvColorFilterSubTask(Color rgb, int xIdx, int yIdx, float[,,] output)
        {
            ColorHSV hsvResult = ColorHSV.rgb2hsv(rgb);
            output[xIdx, yIdx, 0] = hsvResult.hue;
            output[xIdx, yIdx, 1] = hsvResult.saturation;
            output[xIdx, yIdx, 2] = hsvResult.value;

            await Task.Delay(0);
            return true;
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

        /// <summary>
        /// 허프변환을 위한 행렬곱셉 함수(Matrix Multiply for Hough Transform)
        /// </summary>
        /// <param name="first">첫번째 행렬(First Matrix)</param>
        /// <param name="second">두번째 행렬(Second Matrix)</param>
        /// <returns>List<list type="List<double>"></returns>
        public static double[,] matMul(List<List<float>> first, List<List<double>> second)
        {
            int fRowCnt = first.Count; //f행
            int fColCnt = first[0].Count; //f열

            int sRowCnt = second.Count; //s행
            int sColCnt = second[0].Count; //s열

            //a x b 와 c x d 일시 b 와 c는 갯수가 같아야 함.
            if (fColCnt != sRowCnt)
            {
                Debug.LogError("Check the matrix. A and B matrix counts are different.");
                throw new Exception("Check the matrix. A and B matrix counts are different.");
            }

            double[,] output = new double[fRowCnt, sColCnt]; //행렬의 갯수는 a x b 와 c x d 일시 a x d의 형태가 됨.

            for (int i = 0; i < fRowCnt; i++)
            {
                for (int j = 0; j < sColCnt; j++)
                {
                    for (int k = 0; k < fColCnt; k++)
                    {
                        var f = first[i][k];
                        var s = second[k][j];
                        output[i, j] += f * s;
                    }
                }
            }

            return output;
        }

        public static double[] flat2DMatrix(double[,] target)
        {
            int cnt = 0;
            double[] flatArray = new double[target.GetLength(0) * target.GetLength(1)];

            for (int i = 0; i < target.GetLength(0); i++)
            {
                for (int j = 0; j < target.GetLength(1); j++)
                {
                    flatArray[cnt] = target[i, j];
                    cnt++;
                }
            }

            return flatArray;
        }

        public static T[,] matrixTranspose<T>(T[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            T[,] transposed = new T[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transposed[j, i] = matrix[i, j];
                }
            }

            return transposed;

        }

        private static List<float> arange(float start, float finish, float increase)
        {
            List<float> arange = new List<float>();

            for (float i = start; i < finish; i += increase)
            {
                arange.Add(i);
            }

            return arange;
        }

        //MARK: Draw Utils

        //bresenham algorithm 참고: https://lalyns.tistory.com/entry/%EC%A0%95%EC%88%98%EB%A7%8C-%EC%82%AC%EC%9A%A9%ED%95%B4-%EC%84%A0-%EB%B9%A0%EB%A5%B4%EA%B2%8C-%EA%B7%B8%EB%A6%AC%EA%B8%B0
        public static List<Vector2> getLineCoordinates(Vector2 startPoint, Vector2 endPoint)
        {
            List<Vector2> points = new List<Vector2>();

            try
            {
                int x1 = (int)startPoint.x;
                int y1 = (int)startPoint.y;
                int x2 = (int)endPoint.x;
                int y2 = (int)endPoint.y;

                int dx = Math.Abs(x2 - x1);
                int dy = Math.Abs(y2 - y1);

                int sx = x1 < x2 ? 1 : -1; // x 방향으로 이동할 때 증가(1) 또는 감소(-1)
                int sy = y1 < y2 ? 1 : -1; // y 방향으로 이동할 때 증가(1) 또는 감소(-1)
                int err = dx - dy; // 오차값 초기화

                while (true)
                {
                    points.Add(new Vector2(x1, y1)); // 현재 점을 리스트에 추가

                    if (x1 == x2 && y1 == y2) // 끝 점에 도달하면 종료
                        break;

                    int e2 = err * 2; // 오차값을 두 배로 늘려서 계산

                    if (e2 > -dy) // x 방향으로 이동이 우선
                    {
                        err -= dy;
                        x1 += sx;
                    }

                    if (e2 < dx) // y 방향으로 이동
                    {
                        err += dx;
                        y1 += sy;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Line Changer Error: " + ex.Message);
                throw;
            }

            return points;
        }

        //MARK: Task
        private async Task resListen(object res)
        {
            await Task.Delay(0);
            return;
        }
    }
}