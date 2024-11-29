using System;
using System.Linq;
using System.Collections.Generic;

public class LinearRegression
{
    //dataFrame[[x, y]]
    public static List<float> fitting<T>(List<List<int>> dataFrame, float learningRate = 1)
    {
        //LMS(Least Mean Square ERR)
        //값을 미리 초기화
        float w = 0; //가중치
        float b = 0; //편향
        List<float> result_x = new List<float>();
        gradDescent(w, b, dataFrame, (float)0.5);

        for (int cnt = 0; cnt < dataFrame.Count; cnt++)
        {
            //x에 대한 y값
            result_x.Add(predict(w, b, dataFrame[cnt][0]));
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
    public static void gradDescent(float w, float b, List<List<int>> dataFrame, float learningRate)
    {
        for (int cnt = 0; cnt < dataFrame.Count; cnt++)
        {
            //err
            float err = dataFrame[cnt][1] - predict(w, b, dataFrame[cnt][0]);
            //Mean Squared Error 알고리즘 참고
            float cost = dataFrame.Sum(coord => (float)Math.Pow(coord[1] - predict(w, b, coord[0]), 2.0)) / dataFrame.Count;

            float grad_w = dataFrame.Sum(a => (w * a[1] - (w * a[0] + b)) * -2 * a[0]) / dataFrame.Count; // mse를 w에 대해 편미분
            float grad_b = dataFrame.Sum(a => (w * a[1] - (w * a[0] + b)) * -2) / dataFrame.Count; //mse를 b에 대해 편미분

            w -= learningRate * grad_w;
            b -= learningRate * grad_b;
        }
    }
}