using System;
using System.Linq;
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
        public static void im2col(ref List<List<ColorRGB>> imgData, int filter_size, int stride = 1, int pad = 0) //kernel == filter
        {
            int imgData_h = imgData.Count;
            int imgData_w = imgData[0].Count;
            List<List<ColorRGB>> newImgData = imgData;
            //이미지 사이즈에서 필터 사이즈를 빼면 실제 출력물 갯수 img - filterSize
            //패딩은 1당 2씩 증가 2 * pad
            //stride는 주어진 숫자만큼 건너뛰면서 감 / stride
            // + 1은 첫번째 위치 계산도 계산이므로 보정 역할로 + 1
            int out_h = (imgData_h - filter_size + 2 * pad) / stride + 1;
            int out_w = (imgData_w - filter_size + 2 * pad) / stride + 1;

            ///Padding
            int p_h = 0;

            while (p_h < imgData_h + pad * 2)
            {
                //맨 위와 맨 아래 추가
                if (p_h == 0 || p_h == imgData_h + pad * 2)
                {
                    imgData[p_h].AddRange(Enumerable.Repeat(new ColorRGB(0f, 0f, 0f), imgData_w + pad * 2));
                }

                //맨 위와 맨 아래를 제외한 나머지 부분에 추가
                if (p_h > 0 || p_h < imgData_h + pad * 2)
                {
                    imgData[p_h].InsertRange(0, Enumerable.Repeat(new ColorRGB(0f, 0f, 0f), pad)); //처음 부분에 추가
                    imgData[p_h].AddRange(Enumerable.Repeat(new ColorRGB(0f, 0f, 0f), pad)); //마지막 부분에 추가
                }

                p_h++;
            }

        }
    }

    public class perceptron
    {
        //활성화 함수

        //활성화(계단)
        public static int step_func(double x, double threshold)
        {
            if (x < threshold)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        //학습 데이터

        //테스트 학습데이터
        public static Tuple<double[], int>[] gen_train_data_random(int max)
        {
            System.Random random = new System.Random();

            double[] x1 = Enumerable
                .Repeat(0, max)
                .Select(i => random.NextDouble())
                .ToArray();
            double[] x2 = Enumerable
                .Repeat(0, max)
                .Select(i => random.NextDouble())
                .ToArray();
            int[] y = Enumerable
                .Repeat(0, max)
                .Select(i => x1[i] + x2[i] > 0.5 ? 1 : 0)
                .ToArray();

            Tuple<double[], int>[] train_data = Enumerable
                .Repeat(0, max)
                .Select(i => new Tuple<double[], int>(new double[2] { x1[i], x2[i] }, y[i]))
                .ToArray();

            return train_data;
        }

        public static void train_start()
        {
            float threshold = 0.5f;
            double[] weights = new double[2] { 0.3f, 0.9f }; //시작은 랜덤된 가중치
            float lr = 0.1f;
            int data_max = 100;
            int epoch = 10; //학습 횟수
            var training_set = gen_train_data_random(data_max);

            double[] x_values = Enumerable //실제론 x의 1번째 값
            .Range(0, 50)
            .Select(i => (double)(0 + i * (50 - 0) / (50 - 1)))
            .ToArray();
            double[] xi_values; //y축 실제론 x의 i 번째 값


            foreach (int i in Enumerable.Range(1, epoch))
            {
                foreach (var data in training_set)
                {
                    double[] x_input = data.Item1; //x 값들
                    int y = data.Item2;
                    double netInput = x_input.Select((val, i) => val * weights[i]).Sum(); // x1*w1 + x2*w2... 한거
                    double err = y - step_func(netInput, threshold); //오차 뺌
                    weights = x_input.Select((val, i) => weights[i] + val * err * lr).ToArray(); //현재 가중치 + 현재 입력 * 오차 * 학습률

                    /**
                    여기서 결정 경계
                    직선의 방정식은 f(x) = ax + b인데
                    netInput = threshold인데 이걸 풀어쓰면
                    w1 * x1 + w2 * x2 + w.. * x.. = threshold임
                    x의 feature가 2개라고 가정 시 
                    만약 x1에 대한 직선을 가져와야 한다면 x1에 대한 식으로 변경해야 함(일차방정식?)
                    w1 * x1 = -w2 * x2 + threshold (즉 w1*x1를 제외한 모든 항을 우변으로 이동)
                    x1 = -w2/w1 * x2 + threshold/w1 (좌변에 있던 w1를 없애기 위해 우변에 있는 모든 항에 w1로 나눠줌)
                    식은 위 식이 됨.
                    **/

                    xi_values = x_values
                        .Select((val, i) => -weights[1] / weights[0] * val + threshold / weights[0])
                        .ToArray(); //위식을 토대로 1차 방정식을 통해 xi에 대한 식으로 변경해서 만듬.
                }
            }
        }
    }
}