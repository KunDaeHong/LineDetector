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

        public static async Task<List<List<int>>> houghTransformDetector(Texture2D target, int lowThreshold, int highThreshold, float thetaResolution = 180, float verticalResolution = 180, int tCount = 220)
        {
            int inputWidth = (int)(target.width * (50 / 100f));
            int inputHeight = (int)(target.height * (50 / 100f));
            // int inputWidth = target.width;
            // int inputHeight = target.height;
            Texture2D smallSize = CVUtils.resizeTexture2D(target, inputWidth, inputHeight);
            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(smallSize, lowThreshold, highThreshold);

            float halfOutputWidth = inputWidth / 2;
            float halfOutputHeight = inputHeight / 2;
            float diag = (float)Math.Sqrt(inputWidth * inputWidth + inputHeight * inputHeight); //사각형 대각선길이

            float verticalRho = 2 * diag / verticalResolution; //직선이 원점에서 떨어진 수직거리 (이를 verticalResolution 만큼 축을 나눔.)
            List<float> thetas = arange(0, 180, thetaResolution); //직선의 기울기 범위
            List<float> verticalRhos = arange(-diag, diag, verticalResolution); // 사각형안 직선의 범위
            List<double> cos_thetas = thetas.Select(x => Math.Cos(x * Math.PI / 180)).ToList(); //theta cos로 미리 계산 (라디안)
            List<double> sin_thetas = thetas.Select(x => Math.Sin(x * Math.PI / 180)).ToList(); //theta sin로 미리 계산 (라디안)

            List<List<int>> edge_points = CannyEdge.getEdgePoints(outputTexture);
            List<List<float>> centered_edge_points = edge_points.Select(inner => inner.Select((coord, index) => //이미지 좌표는 왼쪽 위에서 시작하므로 모든 점을 좌표계 원점으로 셋팅
            { return index == 0 ? coord - halfOutputWidth : coord - halfOutputHeight; }).ToList()).ToList();

            List<List<double>> thetasMat = new List<List<double>>() { cos_thetas, sin_thetas }; //theta cos,sin를 매트릭스로 생성
            double[,] verticalRho_values = CVUtils.matMul(centered_edge_points, thetasMat); //edge_point좌표에 theta값을 적용하여 실제 허프공간의 수직거리를 구함(여러 diag 값)
            double[] flat_verticalRho_values = CVUtils.flat2DMatrix(verticalRho_values);

            List<float> thetasRowCnt = Enumerable.Repeat(thetas, verticalRho_values.GetLength(0)).SelectMany(x => x).ToList();
            int[,] polarLinesVote = votePolarLine(thetasRowCnt, flat_verticalRho_values, thetas.ToArray(), verticalRhos.ToArray()); //기울기 범위와 직선 대각선의 범위를 누적 배열로 제작
            int[,] polarLinesTranspose = CVUtils.matrixTranspose(polarLinesVote);
            List<List<int>> lines = new List<List<int>>();
            List<List<int>> imgCoordLines = new List<List<int>>();

            for (int x = 0; x < polarLinesTranspose.GetLength(0); x++)
            {
                for (int y = 0; y < polarLinesTranspose.GetLength(1); y++)
                {
                    int vote = polarLinesTranspose[x, y];

                    if (vote > tCount)
                    {
                        lines.Add(new List<int>() { x, y });
                    }
                }
            }

            //극좌표를 다시 일반좌표로 변환
            foreach (var polarCoord in lines)
            {
                int lineLength = Math.Min(inputWidth, inputHeight);

                int rhoIdx = polarCoord[0];
                int thetaIdx = polarCoord[1];

                // if (rhoIdx >= verticalRhos.Count || thetaIdx >= thetas.Count) continue;

                float rho = verticalRhos[rhoIdx]; //원점에서 직선까지 거리
                float theta = thetas[thetaIdx]; //직선의 기울기

                float cosTheta = (float)Math.Cos(theta * Math.PI / 180); //직선의 방정식(방향벡터)
                float sinTheta = (float)Math.Sin(theta * Math.PI / 180); //작선의 방정식(방향벡터)

                float x0 = (cosTheta * rho) + halfOutputWidth; //원점을 다시 이미지 원점으로 이동
                float y0 = (sinTheta * rho) + halfOutputHeight; //원점을 다시 이미지 원점으로 이동

                //수직벡터
                int x1 = (int)(x0 + lineLength / 2 * (-sinTheta)); // 직선의 한 방향으로 확장
                int y1 = (int)(y0 + lineLength / 2 * cosTheta);    // 직선의 한 방향으로 확장
                int x2 = (int)(x0 - lineLength / 2 * (-sinTheta)); // 직선의 반대 방향으로 확장
                int y2 = (int)(y0 - lineLength / 2 * cosTheta);    // 직선의 반대 방향으로 확장

                // x1 = Math.Max(0, Math.Min(inputWidth - 1, x1));
                // y1 = Math.Max(0, Math.Min(inputHeight - 1, y1));
                // x2 = Math.Max(0, Math.Min(inputWidth - 1, x2));
                // y2 = Math.Max(0, Math.Min(inputHeight - 1, y2));

                imgCoordLines.Add(new List<int>() { x1, y1, x2, y2 });
            }

            return imgCoordLines;
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
            int[,] output = new int[thetaRange.Count(), rhoRange.Count()];

            //thetas 카운트와 verticalRhos의 카운트는 같을 수 밖에 없음.
            for (int x = 0; x < verticalRhos.Count(); x++)
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