using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

namespace CV
{
    public class CarLane
    {

        //다른 색상 차선 전용
        private static List<List<ColorHSV>> hsvList = new List<List<ColorHSV>>()
        {
            new List<ColorHSV>() {LaneHSVRange.min_blueHSV, LaneHSVRange.max_blueHSV },
            new List<ColorHSV>() {LaneHSVRange.min_yellowHSV, LaneHSVRange.max_yellowHSV }
        };

        //흰색 차선 전용
        private static List<Color> colorList = new List<Color>()
        {
           new Color(160, 160, 160)
        };

        public static async Task detectCarLane(Texture2D target)
        {
            int inputWidth = (int)(target.width * (50 / 100f));
            int inputHeight = (int)(target.height * (50 / 100f));
            Texture2D smallSize = CVUtils.resizeTexture2D(target, inputWidth, inputHeight);
            Texture2D hsvTexture = await CVUtils.hsvColorFilter(smallSize, hsvList); //hsv 컬러필터로 차선 컬러만 검출
            float[,] cannyTextureFloat = await CannyEdge.cannyEdgeDetector(hsvTexture, 50, 150); // 캐니엣지로 차선을 라인으로 변경(자동 그레이스케일로 변환)
            Texture2D cannyTexture = new Texture2D(cannyTextureFloat.GetLength(0), cannyTextureFloat.GetLength(1));

            for (int x = 0; x < cannyTextureFloat.GetLength(0); x++)
            {
                for (int y = 0; y < cannyTextureFloat.GetLength(1); y++)
                {
                    float newColor = cannyTextureFloat[x, y] / 255;
                    cannyTexture.SetPixel(x, y, new Color(newColor, newColor, newColor));
                }
            }

            cannyTexture.Apply();
            List<List<int>> houghLines = await HoughTransform.houghTransformDetector(cannyTexture, 50, 150, 1, 1, 97);

            //TODO: 직선 중 차선에 가까운 직선을 선형회귀를 통해 출력한다.
            LinearRegression.testLinearRegression();
        }
    }

}