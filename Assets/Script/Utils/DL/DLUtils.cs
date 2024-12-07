using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public class DLUtils
{

    private static List<string> optimizerList = new List<string>() { "MOMENTUM", "RMSPROP", "ADAM" };

    //RMS Prop Optimizer, Adam Values
    private static float sDw = 0;
    private static float sDb = 0;

    //Momentum, Adam Optimizer Values
    private static float vDw = 0;
    private static float vDb = 0;
    private static int iter = 0;

    //MARK: 자료 정리 알고리즘
    public static void toCategorical()
    {

    }

    //MARK: 확률 알고리즘

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
    public static (float, float) gradDescent(float w, float b, List<List<int>> dataFrame, float learningRate, int epoch, float decay_rate = (float)0.9, string optimizer = "")
    {
        if (optimizer != "" && optimizerList.Contains(optimizer))
        {
            throw new Exception($"Optimizer {optimizer} is not exits in this project. \n Select only 1 optimizer from ADAM, RMSPROP, MOMENTUM.");
        }

        if (optimizer == "ADAM") adamInit();
        if (optimizer == "RMSPROP") rmsPropInit();
        if (optimizer == "MOMENTUM") momentumInit();

        for (int epochCnt = 0; epochCnt <= epoch; epochCnt++)
        {
            //err
            //float err = dataFrame[cnt][1] - predict(w, b, dataFrame[cnt][0]);

            float grad_w = dataFrame.Sum(a => (a[1] - (w * a[0] + b)) * a[0]) * -2 / dataFrame.Count; // mse를 w에 대해 편미분
            float grad_b = dataFrame.Sum(a => a[1] - (w * a[0] + b)) * -2 / dataFrame.Count; //mse를 b에 대해 편미분

            switch (optimizer)
            {
                case "ADAM":
                    {
                        adam(w, b, grad_w, grad_b, learningRate, decay_rate);
                        break;
                    }
                case "RMSPROP":
                    {
                        (w, b) = rmsProp(w, b, grad_w, grad_b, learningRate, decay_rate);
                        break;
                    }
                case "MOMENTUM":
                    {
                        (w, b) = momentum(w, b, grad_w, grad_b, learningRate, decay_rate);
                        break;
                    }

                default:
                    {
                        w -= learningRate * grad_w; //기존 경사하강법 업데이트 제거
                        b -= learningRate * grad_b; // 기존 경사하강법 업데이트 제거
                        break;
                    }
            }

            //Mean Squared Error 알고리즘 참고
            float cost = dataFrame.Sum(coord => (float)Math.Pow(coord[1] - predict(w, b, coord[0]), 2)) / dataFrame.Count;
            Console.WriteLine($"비용 값 {cost} epoch {epochCnt} w {w} b {b}");

            if (cost < 0.00001) break; //비용함수가 0과 가까워진 값이 될 경우 종료
        }

        return (w, b);
    }

    //MARK: 최적화 알고리즘
    public static void momentumInit()
    {
        vDw = 0;
        vDb = 0;
    }

    /// <summary>
    /// Momentum Optimizer 입니다.
    /// 기존 경사하강법에서 진동폭을 줄이고 기울기를 일정비율로 계속 반영할 수 있도록 관성을 적용합니다.
    /// https://gaussian37.github.io/dl-dlai-gradient_descent_with_momentum/
    /// </summary>
    /// <param name="w">w</param>
    /// <param name="b">b</param>
    /// <param name="grad_w">가중치가 적용된 w</param>
    /// <param name="grad_b">가중치가 적용된 b</param>
    /// <param name="lr">학습률</param>
    /// <param name="decay_rate">관성</param>
    /// <returns>업데이트된 w, b</returns>
    public static (float, float) momentum(float w, float b, float grad_w, float grad_b, float lr, float decay_rate = (float)0.9)
    {
        vDw = decay_rate * vDw + (1 - decay_rate) * grad_w;
        vDb = decay_rate * vDw + (1 - decay_rate) * grad_b;

        float w_updated = lr * vDw;
        float b_updated = lr * vDb;

        return (w_updated, b_updated);
    }

    public static void rmsPropInit()
    {
        sDw = 0;
        sDb = 0;
    }

    /// <summary>
    /// Root Mean Square Propagation 최적화 (적응적 조정 학습률 알고리즘)
    /// 기울기의 크기에 따라 학습률이 다르게 설정됨. (기울기가 클 경우 작은 학습률, 기울기가 작은 경우 큰 학습률)
    /// https://gaussian37.github.io/dl-dlai-RMSProp/
    /// </summary>
    public static (float, float) rmsProp(float w, float b, float grad_w, float grad_b, float lr, float decay_rate = (float)0.9)
    {
        sDw = decay_rate * sDw + (1 - decay_rate) * grad_w * grad_w;
        sDb = decay_rate * sDb + (1 - decay_rate) * grad_b * grad_b;
        float w_updated = w - lr * grad_w / (float)(Math.Sqrt(sDw) + 1e-7);
        float b_updated = b - lr * grad_b / (float)(Math.Sqrt(sDb) + 1e-7);

        return (w_updated, b_updated);
    }

    public static void adamInit()
    {
        sDw = 0;
        sDb = 0;
        vDw = 0;
        vDb = 0;
        iter = 0;
    }

    /// <summary>
    /// RMS Prop과 Momentum을 합성한 것입니다.
    /// https://gaussian37.github.io/dl-dlai-Adam/
    /// https://velog.io/@viriditass/%EB%82%B4%EA%B0%80-%EB%B3%B4%EB%A0%A4%EA%B3%A0-%EB%A7%8C%EB%93%A0-Optimizier-%EC%A0%95%EB%A6%AC
    /// </summary>
    /// <param name="w">w</param>
    /// <param name="b">b</param>
    /// <param name="grad_w">가중치가 적용된 grad_w</param>
    /// <param name="grad_b">가중치가 적용된 grad_b</param>
    /// <param name="lr">학습률</param>
    /// <param name="decay_rate">관성 방향</param>
    /// <returns></returns>
    public static (float, float) adam(float w, float b, float grad_w, float grad_b, float lr, float decay_rate = (float)0.9)
    {
        iter += 1;
        float decay_rate_1 = (float)(decay_rate + 0.0999);
        float decay_rate_2 = decay_rate;

        vDw = decay_rate * vDw + (1 - decay_rate_1) * grad_w;
        vDb = decay_rate * vDw + (1 - decay_rate_1) * grad_b;
        sDw = decay_rate * sDw + (1 - decay_rate_2) * grad_w * grad_w;
        sDb = decay_rate * sDb + (1 - decay_rate_2) * grad_b * grad_b;

        float lr_t = (float)(lr * Math.Sqrt(1 - Math.Pow(decay_rate_2, 2) * iter) / (1 - Math.Pow(decay_rate_1, 2) * iter));

        float w_updated = (float)(w - lr_t * (vDw / Math.Sqrt(sDw) + 1e-7));
        float b_updated = (float)(b - lr_t * (vDb / Math.Sqrt(sDb) + 1e-7));

        return (w_updated, b_updated);
    }
}