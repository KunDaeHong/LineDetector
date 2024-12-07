using System;
using System.Linq;
using System.Collections.Generic;

public class LinearRegression
{
    /// <summary>
    /// Linear Test
    /// </summary>
    public static void testLinearRegression()
    {
        /**
        테스트 방식
        1. 영화관 방문 수를 X
        2. 영화관 방문 후 영화를 관람 수를 Y
        //**/

        //값은 각각 1000씩 나누어 정규화 하였음.
        List<List<int>> movie_audience = new List<List<int>>()
        {
            new List<int>(){10, 4},
            new List<int>(){20, 3},
            new List<int>(){30, 7},
            new List<int>(){40, 2},
            new List<int>(){50, 5},
            new List<int>(){60, 4},
            new List<int>(){70, 8}
        };

        List<List<float>> expect = new List<List<float>>()
        {
            new List<float>(){80},
            new List<float>(){90},
            new List<float>(){100},
            new List<float>(){110},
        };

        List<float> result = fitting(movie_audience, expect, 500000, (float)0.0001);

        for (int x = 0; x < result.Count; x++)
        {
            Console.WriteLine(result[x]);
        }

        for (int ex = 0; ex < expect.Count; ex++)
        {
            Console.WriteLine($"{expect[ex][0]}, {expect[ex][1]}");
        }

    }

    //dataFrame[[x, y]]
    public static List<float> fitting(List<List<int>> dataFrame, List<List<float>> expect, int epoch, float learningRate = 1)
    {
        //LMS(Least Mean Square ERR)
        //값을 미리 초기화
        float w = 0; //가중치
        float b = 0; //편향
        List<float> result_x = new List<float>();
        (w, b) = gradDescent(w, b, dataFrame, learningRate, epoch);

        for (int cnt = 0; cnt < dataFrame.Count; cnt++)
        {
            //x에 대한 y값
            result_x.Add(predict(w, b, dataFrame[cnt][0]));
        }

        for (int x = 0; x < expect.Count; x++)
        {
            expect[x].Add(predict(w, b, expect[x][0]));
        }

        return result_x;
    }

    /// <summary>
    /// 예측 함수(mx+b)
    /// </summary>
    /// <param name="w">가중치</param>
    /// <param name="b">편향</param>
    /// <param name="x">값</param>
    /// <returns></returns>
    public static float predict(float w, float b, float x)
    {
        return w * x + b;
    }

    //참고: https://www.youtube.com/watch?v=06EqCxjrmX0&t=381s
    //참고: https://velog.io/@amobmocmo/Python-%EB%8B%A8%EC%88%9C-%EC%84%A0%ED%98%95-%ED%9A%8C%EA%B7%80-Linear-Regression-%EA%B5%AC%ED%98%84-9ik2uej68q
    /// <summary>
    /// 경사 하강법 함수
    /// </summary>
    /// <param name="w">가중치</param>
    /// <param name="b">편향</param>
    /// <param name="dataFrame">데이터프레임</param>
    /// <param name="learningRate">학습률</param>
    public static (float, float) gradDescent(float w, float b, List<List<int>> dataFrame, float learningRate, int epoch, float decay_rate = (float)0.9)
    {
        DLUtils.rmsPropInit();

        for (int epochCnt = 0; epochCnt <= epoch; epochCnt++)
        {
            //err
            //float err = dataFrame[cnt][1] - predict(w, b, dataFrame[cnt][0]);

            float grad_w = dataFrame.Sum(a => (a[1] - (w * a[0] + b)) * a[0]) * -2 / dataFrame.Count; // mse를 w에 대해 편미분
            float grad_b = dataFrame.Sum(a => a[1] - (w * a[0] + b)) * -2 / dataFrame.Count; //mse를 b에 대해 편미분

            // w -= learningRate * grad_w; //기존 경사하강법 업데이트 제거
            // b -= learningRate * grad_b; // 기존 경사하강법 업데이트 제거

            (w, b) = DLUtils.rmsProp(w, b, grad_w, grad_b, learningRate, decay_rate);

            //Mean Squared Error 알고리즘 참고
            float cost = dataFrame.Sum(coord => (float)Math.Pow(coord[1] - predict(w, b, coord[0]), 2)) / dataFrame.Count;
            Console.WriteLine($"비용 값 {cost} epoch {epochCnt} w {w} b {b}");

            if (cost < 0.00001) break; //비용함수가 0과 가까워진 값이 될 경우 종료
        }

        return (w, b);
    }
}