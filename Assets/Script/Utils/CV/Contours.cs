using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using Unity.VisualScripting;

namespace CV
{
    public class Contours
    {
        private static MultiThreadUtils tasker = new MultiThreadUtils();

        //상하좌우 대각선 좌표 순서
        private static int[] dx = new int[8] { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static int[] dy = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };
        private static int[] dAngle = new int[8] { 90, 45, 0, -45, -90, -135, 180, 135 };

        //컨투어 박스 인식 사이즈 (단위 px)
        private static int lowest_box_width = 15;
        private static int lowest_box_height = 2;


        //MARK: Canny Contours
        public static async Task<Texture2D> contoursDetectorByCanny(Texture2D target)
        {
            bool[,] visited_pixels = new bool[target.width + 1, target.height + 1];
            List<List<Vector2>> contour_pixels = new List<List<Vector2>>();
            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(target, 50, 150);
            Debug.Log("CannyEdge 엣지 라인 생성 완료");

            //yx 순 이여야 x축으로 이동하고 아래로 내려감.
            for (int y = 0; y < target.height; y++)
            {
                for (int x = 0; x < target.width; x++)
                {
                    //현재 픽셀이 윤곽선이 아닐 때, 현재 픽셀이 방문된 픽셀인 경우 넘김.
                    if (visited_pixels[x, y] || outputTexture[x, y] < 255) continue;

                    List<Vector2> contour_pixel = new List<Vector2>();
                    testNextChainCodeCanny(outputTexture, ref contour_pixel, ref visited_pixels, x, y);

                    if (contour_pixel.Count() > 0) contour_pixels.Add(contour_pixel);
                }
            }

            Debug.Log("Contours 좌표 생성 완료");
            Texture2D output = drawContour(target, contour_pixels);

            return output;
        }

        public static async Task<List<List<Vector2>>> contoursDetectorByCannyVectors(Texture2D target)
        {
            bool[,] visited_pixels = new bool[target.width, target.height];
            List<List<Vector2>> contour_pixels = new List<List<Vector2>>();
            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(target, 50, 150);

            //yx 순 이여야 x축으로 이동하고 위로 올라감.
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

                    //Console.WriteLine($"현재 픽셀 순서 {chainNum} x: {dx[chainNum]} y: {dy[chainNum]}");

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

        private static void testNextChainCodeCanny(float[,] image, ref List<Vector2> contour_pixel, ref bool[,] visited_pixels, int x, int y)
        {
            int direction = 0;
            Vector2 startP = new Vector2(x, y);

            if (image[(int)startP.x, (int)startP.y] != 255)
            {
                return;
            }

            while (true)
            {
                if (!visited_pixels[(int)startP.x, (int)startP.y] && image[(int)startP.x, (int)startP.y] != 0)
                {
                    contour_pixel.Add(startP);
                }

                bool nextPixel = false;
                visited_pixels[(int)startP.x, (int)startP.y] = true;

                for (int dIdx = 0; dIdx < dx.Count(); dIdx++)
                {
                    int img_rows = image.GetLength(0);
                    int img_cols = image.GetLength(1);
                    int chainNum = (direction + dIdx) % 8;
                    int nx = (int)startP.x + dx[chainNum]; // 조회 할 x 좌표
                    int ny = (int)startP.y + dy[chainNum]; // 조회 할 y 좌표

                    if (!CVUtils.isBoundary(nx, ny, new Vector2(img_rows, img_cols))) continue;
                    if (visited_pixels[nx, ny]) continue;
                    if (image[nx, ny] != 255) continue;

                    nextPixel = true;
                    startP = new Vector2(nx, ny);

                    //서로의 마주보고 있는 방향을 반환. 만약 1부터 시작 시 ((chainNum + 4) % 8 ) + 1 이 되어야함.
                    direction = (chainNum + 4) % 8;
                    break;
                }

                if (!nextPixel || startP == new Vector2(x, y)) break;
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
            List<List<Vector2>> contours_rect = new List<List<Vector2>>(); //추후 반환타입으로 지정 가능성
            int[,] contourRectNum = new int[target.width, target.height]; // 해당 픽셀 방문, 해당 픽셀이 어느 컨투어에 포함되는지 기록

            Vector2 minPoint = new Vector2(-1, -1);
            Vector2 maxPoint = new Vector2(target.width + 1, target.height + 1);

            for (int i = 0; i < contour_pixels.Count() - 1; i++)
            {
                //contour pixel 리스트의 특징
                ///1. Unity의 Texture2D의 특징으로써 왼쪽 아래가 0,0 입니다.
                ///2. contour pixel 리스트는 왼쪽 아래에서 가로로 이동합니다.
                ///3. 이걸 각 오브젝트마다 인식하도록 만들어야 합니다. (동시에 몇개의 오브젝트 생성 및 인식해야 하는 것임.)
                ///4. 오브젝트는 사진에 몇개가 인식이 될진 모릅니다.

                Vector2 p1 = contour_pixels[i].First();
                Vector2 p2 = contour_pixels[i].First();

                for (int j = 0; j < contour_pixels[i].Count(); j++)
                {
                    p1 = Vector2.Min(p1, contour_pixels[i][j]);
                    p2 = Vector2.Max(p2, contour_pixels[i][j]);
                    output.SetPixel((int)contour_pixels[i][j].x, (int)contour_pixels[i][j].y, new Color(0, 1, 0));
                }

                //미할당 시
                if (minPoint == new Vector2(-1, -1))
                {
                    minPoint = contour_pixels[i].Aggregate((currentMin, v) =>
                        (v.x < currentMin.x || (v.x == currentMin.x && v.y < currentMin.y)) ? v : currentMin);
                }

                if (maxPoint == new Vector2(target.width + 1, target.height + 1))
                {
                    maxPoint = contour_pixels[i].Aggregate((currentMax, v) =>
                        (v.x > currentMax.x || (v.x == currentMax.x && v.y > currentMax.y)) ? v : currentMax);
                }

                //가장 큰점과 작은점 비교
                minPoint = Vector2.Min(minPoint, Vector2.Min(p1, p2));
                maxPoint = Vector2.Max(maxPoint, Vector2.Max(p1, p2));

                if (i == contour_pixels.Count - 1) continue;

                int arrange = 15;
                bool findRect = false;
                Vector2 nextPoint = contour_pixels[i + 1].First(); //i + 1 == contour_pixels.Count - 2 ? maxPoint : 

                float dx = nextPoint.x - maxPoint.x;
                float dy = nextPoint.y - maxPoint.y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                Console.WriteLine($"현재 좌표 x{maxPoint.x} y{maxPoint.y} 다음 좌표 x{nextPoint.x} y{nextPoint.y} 거리 {dist}");

                if (dist > arrange || contours_rect.Count() == 0)
                {
                    List<Vector2> nextPoints = contours_rect.Count() == 0 ? contour_pixels[i] : contour_pixels[i + 1];
                    Vector2 nextMin = Vector2.Min(nextPoints.First(), nextPoints.Last());
                    Vector2 nextMax = Vector2.Max(nextPoints.First(), nextPoints.Last());
                    contours_rect.Add(new List<Vector2>() { nextMin, nextMax });

                    for (int minY = (int)nextMin.y; minY <= (int)nextMax.y; minY++)
                    {
                        for (int minX = (int)nextMin.x; minX <= (int)nextMax.x; minX++)
                        {
                            contourRectNum[minX, minY] = contours_rect.Count();
                        }
                    }

                    minPoint = new Vector2(-1, -1);
                    maxPoint = new Vector2(target.width + 1, target.height + 1);
                    continue;
                }

                for (int dIdx = 0; dIdx < dAngle.Count(); dIdx++)
                {
                    if (findRect) break;

                    float rad = deg2Rad(dAngle[dIdx]);
                    Vector2 anglePoint = new Vector2((float)(arrange * Math.Cos(rad)), (float)(arrange * Math.Sin(rad)));
                    Vector2 newMinPoint = minPoint + anglePoint;
                    Vector2 newMaxPoint = maxPoint + anglePoint;

                    List<Vector2> coords = CVUtils.getLineCoordinates(minPoint, newMinPoint);
                    coords.AddRange(CVUtils.getLineCoordinates(maxPoint, newMaxPoint));

                    foreach (Vector2 coord in coords)
                    {
                        int x = Math.Clamp((int)coord.x, 0, contourRectNum.GetLength(0) - 1);
                        int y = Math.Clamp((int)coord.y, 0, contourRectNum.GetLength(1) - 1);

                        if (contourRectNum[x, y] != 0) //이전 방문 좌표 체크
                        {
                            //Vector2 Max는 각각 독립으로 비교하지만 픽셀이 같을 수 도 있음.
                            if (x <= maxPoint.x && y <= maxPoint.y)
                            {
                                contours_rect[contourRectNum[x, y] - 1][1] = maxPoint;
                                int maxX = Math.Clamp((int)maxPoint.x, 0, contourRectNum.GetLength(0) - 1);
                                int maxY = Math.Clamp((int)maxPoint.y, 0, contourRectNum.GetLength(1) - 1);
                                contourRectNum[maxX, maxY] = contourRectNum[x, y];

                                for (int minY = (int)minPoint.y; minY <= maxY; minY++)
                                {
                                    for (int minX = (int)minPoint.x; minX <= maxX; minX++)
                                    {
                                        contourRectNum[minX, minY] = contours_rect.Count();
                                    }
                                }
                            }

                            findRect = true;
                            break;
                        }
                    }
                }

                minPoint = new Vector2(-1, -1);
                maxPoint = new Vector2(target.width + 1, target.height + 1);
            }

            foreach (var contours in contours_rect)
            {
                if (contours[1].x - contours[0].x > lowest_box_width && contours[1].y - contours[0].y > lowest_box_height)
                {
                    //위쪽 변(왼쪽에서 오른쪽으로)
                    for (int x = (int)contours[0].x; x <= contours[1].x; x++)
                    {
                        output.SetPixel(x, (int)contours[0].y, new Color(0, 1, 0));
                    }

                    //오른쪽 변(위쪽에서 아래로)
                    for (int y = (int)contours[0].y; y <= contours[1].y; y++)
                    {
                        output.SetPixel((int)contours[1].x, y, new Color(0, 1, 0));
                    }

                    //아래쪽 변(오른쪽에서 왼쪽으로)
                    for (int x = (int)contours[1].x; x >= contours[0].x; x--)
                    {
                        output.SetPixel(x, (int)contours[1].y, new Color(0, 1, 0));
                    }

                    //왼쪽 변(아래에서 위로)
                    for (int y = (int)contours[1].y; y >= contours[0].y; y--)
                    {
                        output.SetPixel((int)contours[0].x, y, new Color(0, 1, 0));
                    }

                }
            }

            output.Apply();

            return output;
        }

        private static float deg2Rad(float angle)
        {
            return (float)(Math.PI * angle / 180);
        }

        //TODO: 퀵소트 만들기;
        /**
        private static List<List<Vector2>> quickSort(List<List<Vector2>> contoursPixels)
        {

        }
        **/

        //MARK: Task
        private static async Task resListen(object res)
        {
            await Task.Delay(0);
            return;
        }
    }
}