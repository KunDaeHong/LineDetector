using System;
using System.Collections.Generic;

using CV;
using UnityEngine;

namespace DNN
{
    class CNN
    {

        //TODO: 컨투어를 통해 나온 이미지를 cnn으로 분석 하는 알고리즘 제작

        //debug를 위해 List 사용 나중에 모두 배열로 바꿀 예정
        public static List<List<ColorRGB>> img2RGBList(ref Texture2D target)
        {
            int width = target.width;
            int height = target.height;
            List<List<ColorRGB>> rgbList = new List<List<ColorRGB>>(); // h x w 순

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Color color = target.GetPixel(w, h);
                    ColorRGB pixel_rgb = new ColorRGB(color.r, color.g, color.b);
                    rgbList[h].Add(pixel_rgb);
                }
            }

            return rgbList;
        }

        //필터를 넣는 것이 아닌 필터 사이즈를 기준으로 4차원 텐서를 2차원 배열로 변경
        public static void img2col(ref List<List<ColorRGB>> imgData, int filter_size, int stride = 1, int pad = 0) //kernel == filter
        {
            int imgData_h = imgData.Count;
            int imgData_w = imgData[0].Count;

            //이미지 사이즈에서 필터 사이즈를 빼면 실제 출력물 갯수 img - filterSize
            //패딩은 1당 2씩 증가 2 * pad
            //stride는 주어진 숫자만큼 건너뛰면서 감 / stride
            // + 1은 첫번째 위치 계산도 계산이므로 보정 역할로 + 1
            int out_h = (imgData_h - filter_size + 2 * pad) / stride + 1;
            int out_w = (imgData_w - filter_size + 2 * pad) / stride + 1;

            //패딩 추가
            for (int p_h = 0; p_h < imgData_h; p_h++)
            {
                for (int p_w = 0; p_w < imgData_w; p_w++)
                {

                }
            }


        }
    }
}