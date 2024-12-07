using System;
using System.Collections.Generic;

public class DLUtils
{

    List<string> optimizerList = new List<string>() { "MOMENTUM", "RMSPROP", "ADAM" };

    //RMS Prop Optimizer, Adam Values
    private static float sDw = 0;
    private static float sDb = 0;

    //Momentum, Adam Optimizer Values
    private static float vDw = 0;
    private static float vDb = 0;

    public static void toCategorical()
    {

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
    }

    /// <summary>
    /// RMS Prop과 Momentum을 합성한 것입니다.
    /// https://gaussian37.github.io/dl-dlai-Adam/
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
        vDw = decay_rate * vDw + (1 - decay_rate) * grad_w;
        vDb = decay_rate * vDw + (1 - decay_rate) * grad_b;
        sDw = decay_rate * sDw + (1 - decay_rate) * grad_w * grad_w;
        sDb = decay_rate * sDb + (1 - decay_rate) * grad_b * grad_b;



        return (vDw, vDb);
    }
}