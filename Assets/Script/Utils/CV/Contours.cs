using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

namespace CV
{
    public class Contours
    {
        private static MultiThreadUtils tasker = new MultiThreadUtils();

        //상하좌우 대각선 4 꼭짓점 포함.
        private static int[] dx = new int[8] { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static int[] dy = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };

        //MARK: Canny Contours
        public static async Task<Texture2D> contoursDetectorByCanny(Texture2D target)
        {
            bool[,] visited_pixels = new bool[target.width + 1, target.height + 1];
            List<List<Vector2>> contour_pixels = new List<List<Vector2>>();
            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(target, 50, 150);

            //yx 순 이여야 x축으로 이동하고 아래로 내려감.
            for (int y = 0; y < target.height; y++)
            {
                for (int x = 0; x < target.width; x++)
                {
                    //현재 픽셀이 윤곽선이 아닐 때, 현재 픽셀이 방문된 픽셀인 경우 넘김.
                    if (visited_pixels[x, y] || outputTexture[x, y] < 255) continue;

                    List<Vector2> contour_pixel = new List<Vector2>();
                    nextChainCodeCanny(outputTexture, ref contour_pixel, x, y); // 새롭게 메모리 재할당 하지 않고 기존 변수 참조
                    contour_pixels.Add(contour_pixel);

                    contour_pixel.ForEach(i =>
                    {
                        visited_pixels[(int)i.x, (int)i.y] = true;
                    });
                }
            }

            Texture2D output = drawContour(target, contour_pixels);

            return output;
        }

        public static async Task<List<List<Vector2>>> contoursDetectorByCannyVectors(Texture2D target)
        {
            bool[,] visited_pixels = new bool[target.width, target.height];
            List<List<Vector2>> contour_pixels = new List<List<Vector2>>();
            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(target, 50, 150);

            //yx 순 이여야 x축으로 이동하고 아래로 내려감.
            for (int y = 0; y <= target.height; y++)
            {
                for (int x = 0; x <= target.width; x++)
                {
                    //현재 픽셀이 윤곽선이 아닐 때, 현재 픽셀이 방문된 픽셀인 경우 넘김.
                    if (visited_pixels[x, y] || outputTexture[x, y] < 255) continue;

                    List<Vector2> contour_pixel = new List<Vector2>();
                    nextChainCodeCanny(outputTexture, ref contour_pixel, x, y); // 새롭게 메모리 재할당 하지 않고 기존 변수 참조
                    contour_pixels.Add(contour_pixel);

                    contour_pixel.ForEach(i =>
                    {
                        visited_pixels[(int)i.x, (int)i.y] = true;
                    });
                }
            }

            return contour_pixels;
        }


        //참고: https://kipl.tistory.com/10
        /// <summary>
        /// 체인 코드로 해당 픽셀을 기준으로 주변 반경 픽셀을 검사합니다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        private static void nextChainCodeCanny(float[,] image, ref List<Vector2> contour_pixel, int x, int y)
        {
            int direction = 0;
            Vector2 startP = new Vector2(x, y);
            bool[,] visit_chain = new bool[image.GetLength(0) + 1, image.GetLength(1) + 1];

            while (true)
            {
                if (visit_chain[(int)startP.x, (int)startP.y]) break;

                bool nextPixel = false;
                contour_pixel.Add(startP); //현재 시작 픽셀을 추가해야함.
                visit_chain[(int)startP.x, (int)startP.y] = true;

                for (int dIdx = 0; dIdx < dx.Count(); dIdx++)
                {
                    int img_rows = image.GetLength(0);
                    int img_cols = image.GetLength(1);
                    int chainNum = (direction + dIdx) % 8;
                    int nx = (int)startP.x + dx[chainNum]; // 조회 할 x 좌표
                    int ny = (int)startP.y + dy[chainNum]; // 조회 할 y 좌표

                    Console.WriteLine($"현재 픽셀 순서 {chainNum} x: {dx[chainNum]} y: {dy[chainNum]}");

                    //이미지의 범위 밖 좌표의 경우 다음 좌표로 이동
                    if (!CVUtils.isBoundary(nx, ny, new Vector2(img_rows, img_cols)))
                    {
                        continue;
                    }

                    if (image[nx, ny] != 255) continue;

                    nextPixel = true;
                    startP = new Vector2(nx, ny);
                    direction = (chainNum + 4) % 8;

                    break;
                }

                if (!nextPixel || new Vector2(x, y) == startP) break;
            }
        }

        // MARK: HSV Contours
        public static async Task contoursDetectorByHsv(Texture2D target)
        {
            int batchSize = 1000;
            tasker.listen = resListen;

            int width = target.width;
            int height = target.height;
            Texture2D output = new Texture2D(width, height);
            //1: widthCnt 2: heightCnt 3: hsvColor 3D array
            Color[] colorPixels = target.GetPixels();
            float[,,] hsvColorTarget = new float[width, height, 3];

            //rgb texture2d 2 hsv2d
            for (int y = 0; y < height; y += batchSize)
            {
                for (int x = 0; x < width; x += batchSize)
                {
                    int endX = Math.Min(x + batchSize, width);
                    int endY = Math.Min(y + batchSize, height);

                    await tasker.SpawnAsync(async () =>
                    {
                        int initX = x;
                        int initY = y;

                        for (int startY = initY; startY < endY; startY++)
                        {
                            for (int startX = initX; startX < endX; startX++)
                            {
                                await CVUtils.hsvColorFilterSubTask(colorPixels[startY * width + startX], startX, startY, hsvColorTarget);
                            }
                        }

                        return true;
                    });
                }
            }

            await tasker.WaitUntil(() =>
            {
                return tasker.threadCnt == 0;
            });

            Console.WriteLine("Hsv 컬러로 변환 완료");
        }

        //MARK: Draw
        public static Texture2D drawContour(Texture2D target, List<List<Vector2>> contour_pixels)
        {
            Texture2D output = target;

            for (int i = 0; i < contour_pixels.Count(); i++)
            {
                for (int j = 0; j < contour_pixels[i].Count(); j++)
                {
                    Vector2 p1 = contour_pixels[i][j];
                    Vector2 p2 = contour_pixels[i][(j + 1) % contour_pixels[i].Count];
                    List<Vector2> points = CVUtils.getLineCoordinates(p1, p2);

                    foreach (var point in points)
                    {
                        int x = (int)point.x;
                        int y = (int)point.y;

                        if (x >= 0 && x < target.width && y >= 0 && y < target.height)
                        {
                            output.SetPixel(x, y, new Color(0, 1, 0));
                        }
                    }

                }
            }

            output.Apply();

            return output;
        }

        //MARK: Task
        private static async Task resListen(object res)
        {
            await Task.Delay(0);
            return;
        }
    }
}