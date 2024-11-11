using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

namespace CV
{
    public class HoughTransform
    {
        //직선의 방정식 y = ax + b
        //참고 : https://throwexception.tistory.com/1071

        public static async Task<float[,]> houghTransformDetector(Texture2D target, int lowThreshold, int highThreshold, float thetaResolution = 180, float verticalResolution = 180, int tCount = 220)
        {
            //이미지 사이즈가 큰 경우를 위해 자동으로 이미지를 50%로 리사이징 함.(나중에 리사이징 된만큼 다시 원복함.)
            //이미지에서 손쉽게 직선을 검출 할 수 있도록 캐니엣지로 엣지 부분만 검출
            int inputWidth = (int)(target.width * (50 / 100f));
            int inputHeight = (int)(target.height * (50 / 100f));
            Texture2D smallSize = CVUtils.resizeTexture2D(target, inputWidth, inputHeight);
            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(smallSize, lowThreshold, highThreshold);
            Texture2D output = new Texture2D(outputTexture.GetLength(0), outputTexture.GetLength(1));

            float halfOutputWidth = output.width / 2;
            float halfOutputHeight = output.height / 2;
            float diag = (float)Math.Sqrt(Math.Pow(output.width, 2) + Math.Pow(output.height, 2)); //사각형 대각선길이

            float verticalRho = 2 * diag / verticalResolution; //직선이 원점에서 떨어진 수직거리 (이를 verticalResolution 만큼 축을 나눔.)
            List<float> thetas = arange(0, 180, 180 / thetaResolution); //직선의 기울기 범위 (이미지의 축이 아닌 0,0으로 설정하기에 -90, 90 이 아닌 0, 180으로 설정)
            List<float> verticalRows = arange(-diag, diag, verticalRho); // 사각형안 직선의 범위

            List<List<int>> edge_points = CannyEdge.getEdgePoints(outputTexture);
            List<List<float>> centered_edge_points = edge_points.Select(inner => inner.Select((coord, index) => //이미지 좌표는 왼쪽 위에서 시작하므로 모든 점을 좌표계 원점으로 셋팅
            { return index == 0 ? coord - halfOutputWidth : coord - halfOutputHeight; }).ToList()).ToList();

            List<float> cos_thetas = thetas.Select(x => (float)Math.Cos(x * Math.PI / 180)).ToList(); //theta cos로 미리 계산 (라디안)
            List<float> sin_thetas = thetas.Select(x => (float)Math.Sin(x * Math.PI / 180)).ToList(); //theta sin로 미리 계산 (라디안)
            List<List<float>> thetasMat = new List<List<float>>() { cos_thetas, sin_thetas }; //theta cos,sin를 매트릭스로 생성
            float[,] verticalRho_values = CVUtils.matMul(centered_edge_points, thetasMat); //edge_point좌표에 theta값을 적용하여 실제 허프공간의 수직거리를 구함(여러 diag 값)
            double[] flat_verticalRho_values = CVUtils.flat2DMatrix(verticalRho_values);

            foreach (var item in flat_verticalRho_values)
            {
                if (item != 0)
                {
                    Console.WriteLine($"대각선 길이 값 {item}");
                }
            }

            List<float> thetasRowCnt = Enumerable.Repeat(thetas, verticalRho_values.GetLength(0)).SelectMany(x => x).ToList();
            int[,] polarLinesVote = votePolarLine(thetasRowCnt, flat_verticalRho_values, thetas.ToArray(), verticalRows.ToArray()); //기울기 범위와 직선 대각선의 범위를 누적 배열로 제작
            List<List<int>> lines = new List<List<int>>();

            for (int x = 0; x < polarLinesVote.GetLength(0); x++)
            {
                for (int y = 0; y < polarLinesVote.GetLength(1); y++)
                {
                    int vote = polarLinesVote[x, y];

                    if (vote > tCount)
                    {
                        lines.Add(new List<int>() { x, y });
                        Console.WriteLine($"hough Result x: {x} y: {y}");
                    }
                }
            }

            return outputTexture;
        }

        /// <summary>
        /// 해당 이미지 구간에 있는 직선 중 범위에 있는 모든 직선을 누적계산 하여 카운트를 함.(누적 계산 accumulation array)
        /// </summary>
        /// <param name="thetas">verticalRho_values 열에 있는 갯수만큼의 theta 값들</param>
        /// <param name="verticalRhos">1차원 verticalRho_values(수직거리(diag를 구한 값))</param>
        /// <param name="xRange">x 구간</param>
        /// <param name="yRange">y 구간</param>
        /// <returns>float[,]</returns>
        private static int[,] votePolarLine(List<float> thetas, double[] verticalRhos, float[] thetaRange, float[] rhoRange)
        {
            //TODO: verticalRho_values 열에 있는 갯수만큼 theta 값 검증
            //TODO: verticalRho_values를 1차원으로 변경
            //TODO: 범위 설정

            int[,] output = new int[thetaRange.Count() - 1, rhoRange.Count() - 1];

            //thetas 카운트와 verticalRhos의 카운트는 같을 수 밖에 없음.
            for (int x = 0; x < thetaRange.Count(); x++)
            {
                int xIdx = FindBinIdx(thetas[x], thetaRange);
                int yIdx = FindBinIdx((float)verticalRhos[x], rhoRange);

                if (xIdx >= 0 && yIdx >= 0 && xIdx < output.GetLength(0) && yIdx < output.GetLength(1))
                {
                    output[xIdx, yIdx]++;
                }
            }

            return output;
        }

        private static int FindBinIdx(float target, float[] range)
        {
            for (int i = 0; i < range.Count() - 1; i++)
            {
                //range에 + 1 하는 이유는 range의 갯수를 -1 하여 for문을 돌기 때문.
                if (target >= range[i] && target < range[i + 1])
                {
                    return i;
                }
            }

            return -1;
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
    }
}