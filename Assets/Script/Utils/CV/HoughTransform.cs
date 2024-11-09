using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CV
{
    public class HoughTransform
    {
        //직선의 방정식 y = ax + b

        public static async Task<float[,]> houghTransformDetector(Texture2D target, int lowThreshold, int highThreshold, float thetaResolution = 1, float verticalResolution = 1, int tCount = 220)
        {
            //이미지 사이즈가 큰 경우를 위해 자동으로 이미지를 50%로 리사이징 함.(나중에 리사이징 된만큼 다시 원복함.)
            //이미지에서 손쉽게 직선을 검출 할 수 있도록 캐니엣지로 엣지 부분만 검출
            int inputWidth = (int)(target.width * (50 / 100f));
            int inputHeight = (int)(target.height * (50 / 100f));
            Texture2D smallSize = CVUtils.resizeTexture2D(target, inputWidth, inputHeight);

            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(smallSize, lowThreshold, highThreshold);
            Texture2D output = new Texture2D(outputTexture.GetLength(0), outputTexture.GetLength(1));

            List<List<int>> edge_points = CannyEdge.getEdgePoints(outputTexture);
            //List<EdgePoints> centered_edge_points = 
            float diag = (float)Math.Round(Math.Sqrt(Math.Pow(2, outputTexture.GetLength(0) + Math.Pow(2, outputTexture.GetLength(1))))); //사각형 대각선길이

            float verticalRho = 2 * diag / verticalResolution; //직선이 원점에서 떨어진 수직거리 (이를 verticalResolution 만큼 축을 나눔.)
            List<float> thetas = arange(0, 180, thetaResolution); //직선의 기울기 범위 (이미지의 축이 아닌 0,0으로 설정하기에 -90, 90 이 아닌 0, 180으로 설정)
            List<float> verticalRows = arange(-diag, diag, verticalRho); // 사각형안 직선의 범위

            List<float> cos_thetas = thetas.Select(x => (float)Math.Cos(x)).ToList(); //theta cos로 미리 계산
            List<float> sin_thetas = thetas.Select(x => (float)Math.Sin(x)).ToList(); //theta sin로 미리 계산
            List<List<float>> thetasMat = new List<List<float>>() { cos_thetas, sin_thetas }; //theta cos,sin를 매트릭스로 생성
            //edge_point좌표에 theta값을 적용하여 실제 허프공간(이미지 0,0에서 중심좌표로 옮김)에서의 좌표를 구함



            float[,] accumulator = new float[verticalRows.Count, thetas.Count]; //기울기 범위와 직선 대각선의 범위를 누적 배열로 제작


            for (int x = 0; x < outputTexture.GetLength(0); x++)
            {
                for (int y = 0; y < outputTexture.GetLength(1); y++)
                {

                }
            }
            return outputTexture;
        }

        private static List<float> arange(float start, float finish, float increase)
        {
            List<float> arange = new List<float>();

            for (float i = 0; i < finish; i += increase)
            {
                arange.Add(i);
            }

            return arange;
        }
    }
}