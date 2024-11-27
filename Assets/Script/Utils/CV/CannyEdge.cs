using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

namespace CV
{
    public class CannyEdge
    {
        public static float[,] sobel_X_mask = new float[3, 3] {
        {-1, 0, 1},
        { -2, 0, 2},
        { -1, 0, 1}
    };
        public static float[,] sobel_Y_mask = new float[3, 3]{
        {1, 2, 1},
        {0, 0, 0},
        {-1, -2, -1}
    };
        public static async Task<float[,]> cannyEdgeDetector(Texture2D target, int lowThreshold, int highThreshold)
        {
            Texture2D targetGrayscale = CVUtils.toGrayScale(target);
            float[,] target2DArray = CVUtils.to2DArrayFromTexture2D(targetGrayscale);
            float[,] filtering = await bilateralFilterCoroutine(target2DArray); //후추 잡음 제거 

            //sobel 마스크 적용 [비전 책 118쪽]
            float[,] sobel_x = CVUtils.applyConvKernel(filtering, sobel_X_mask, 1, 1);
            float[,] sobel_y = CVUtils.applyConvKernel(filtering, sobel_Y_mask, 1, 1);

            //엣지 강도와 엣지 방향 체크 [비전 책 119쪽]
            /**
            이유를 설명해드림.
            캐니 엣지는 색상에 차이 두지 않고 걍 모양(Shape) 가져갈려고 하는 것임.
            우선 먼저 이미지 잡음을 최소로 하기 위해 보통 가우시안 블러 또는 바이레터럴 필터 or 미디언 필터를 통해 잡음을 잡음
            다만 잡음을 잡으면 부드러운 선과 Gradient가 존재하는데 해당 선의 Gradient 방향을 찾아서 해당 선을 다시 Gradient를 없애 버림.
            그럼 우리가 아는 깔끔하고 얇은 선만 남게 됨. 이걸 해야 차선 또는 물체 특징점을 찾기가 쉬워짐.
            //**/
            float[,] edge_direction = new float[sobel_x.GetLength(0), sobel_x.GetLength(1)];
            float[,] grad_direction = new float[sobel_x.GetLength(0), sobel_x.GetLength(1)];

            for (int x = 0; x < sobel_x.GetLength(0); x++)
            {
                for (int y = 0; y < sobel_x.GetLength(1); y++)
                {
                    edge_direction[x, y] = (float)Math.Sqrt(Math.Pow(sobel_x[x, y], 2) + Math.Pow(sobel_y[x, y], 2));
                    grad_direction[x, y] = (float)Math.Atan2(sobel_y[x, y], sobel_x[x, y]);
                }
            }

            //edge가 gradation grad는 gradation의 방향
            float[,] nms = nonMaximumSuppression(edge_direction, grad_direction);
            //float[,] doubleThreshold = doubleThresholdEdges(nms, lowThreshold, highThreshold);
            float[,] hysteresisThreshold = hysteresisThresholding(nms, lowThreshold, highThreshold);

            return hysteresisThreshold;
        }

        //함수 설명 참고: https://velog.io/@mykirk98/Canny-Edge-Detection
        public static float[,] nonMaximumSuppression(float[,] edge, float[,] grad)
        {
            int w = grad.GetLength(0);
            int h = grad.GetLength(1);
            float[,] output = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float left = 0;
                    float right = 0;
                    var angle = grad[x, y] * 180 / Math.PI;
                    angle = angle < 0 ? angle + 180 : angle;

                    if ((0 <= angle && angle < 22.5) || (157.5 <= angle && angle <= 180))
                    {
                        if (CVUtils.isBoundary(x, y - 1, new Vector2(w, h))) left = edge[x, y - 1];
                        if (CVUtils.isBoundary(x, y + 1, new Vector2(w, h))) right = edge[x, y + 1];
                    }
                    else if (22.5 <= angle && angle < 67.5)
                    {
                        if (CVUtils.isBoundary(x - 1, y + 1, new Vector2(w, h))) left = edge[x - 1, y + 1];
                        if (CVUtils.isBoundary(x + 1, y - 1, new Vector2(w, h))) right = edge[x + 1, y - 1];
                    }
                    else if (67.5 <= angle && angle < 112.5)
                    {
                        if (CVUtils.isBoundary(x - 1, y, new Vector2(w, h))) left = edge[x - 1, y];
                        if (CVUtils.isBoundary(x + 1, y, new Vector2(w, h))) right = edge[x + 1, y];
                    }
                    else if (112.5 <= angle && angle < 157.5)
                    {
                        if (CVUtils.isBoundary(x - 1, y - 1, new Vector2(w, h))) left = edge[x - 1, y - 1];
                        if (CVUtils.isBoundary(x + 1, y + 1, new Vector2(w, h))) right = edge[x + 1, y + 1];
                    }

                    if (edge[x, y] > left && edge[x, y] > right)
                    {
                        output[x, y] = edge[x, y];
                    }
                    else
                    {
                        output[x, y] = 0;
                    }
                }
            }

            return output;
        }

        private static float[,] doubleThresholdEdges(float[,] target, int lowThreshold, int highThreshold)
        {
            float[,] output = new float[target.GetLength(0), target.GetLength(1)];

            for (int x = 0; x < output.GetLength(0); x++)
            {
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    float strength = target[x, y];

                    if (strength < lowThreshold)
                    {
                        output[x, y] = 0;
                    }
                    else if (strength < highThreshold)
                    {
                        output[x, y] = 100;
                    }
                    else
                    {
                        output[x, y] = 255;
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 두개의 임계값을 쓰는 이력 임계값을 적용하는 함수(자동으로 0 ~ 255로 변경된 값으로 출력됩니다.)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="lowThreshold"></param>
        /// <param name="highThreshold"></param>
        /// <returns></returns>
        private static float[,] hysteresisThresholding(float[,] target, int lowThreshold, int highThreshold)
        {
            int[,] offsets = new int[8, 2] { //주변 8개 픽셀
                {-1, -1},
                {0, -1},
                {1, -1},
                {-1, 0},
                {1, 0},
                {-1, 1},
                {0, 1},
                {1, 1}
            };
            float[,] output = new float[target.GetLength(0), target.GetLength(1)];

            for (int x = 0; x < output.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < output.GetLength(1) - 1; y++)
                {
                    float targetPixel = target[x, y] * 255;

                    if (targetPixel >= highThreshold) //강한 엣지로 선택
                    {
                        output[x, y] = 255;
                        continue;
                    }
                    else if (targetPixel >= lowThreshold) //약한 엣지일 시
                    {
                        bool connectToStrongEdge = false;

                        //주변 픽셀은 존재하지 않는 지역일 수도 있음. 그땐 넘김.
                        for (int offsetIdx = 0; offsetIdx < offsets.GetLength(0); offsetIdx++)
                        {
                            int offsetX = x + offsets[offsetIdx, 0];
                            int offsetY = y + offsets[offsetIdx, 1];

                            if (offsetX < 0 || offsetY < 0) continue;
                            if (offsetX >= target.GetLength(0) || offsetY >= target.GetLength(1)) continue;

                            if (target[offsetX, offsetY] >= highThreshold)
                            {
                                connectToStrongEdge = true;
                                break;
                            }
                        }

                        output[x, y] = connectToStrongEdge ? 255 : 0;
                    }
                    else
                    {
                        output[x, y] = 0; //비 엣지로 설정
                    }
                }
            }
            return output;
        }

        public static async Task<float[,]> bilateralFilterCoroutine(float[,] convolutionList)
        {
            var bilateralFilter = new BilateralFilter();

            float[,] output = await bilateralFilter.bilateralFilter(convolutionList, 7, 75, 100);
            return output;
        }

        public static List<List<int>> getEdgePoints(float[,] target)
        {
            List<List<int>> edgePoints = new List<List<int>>();

            for (int x = 0; x < target.GetLength(0); x++)
            {
                for (int y = 0; y < target.GetLength(1); y++)
                {
                    if (target[x, y] > 0)
                    {
                        edgePoints.Add(new List<int>() { x, y });
                    }
                }
            }

            return edgePoints;
        }

    }
}