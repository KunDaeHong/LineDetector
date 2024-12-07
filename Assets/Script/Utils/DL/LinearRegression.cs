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

        List<float> result = fitting(movie_audience, expect, 50000, (float)0.001, "ADAM");

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
    public static List<float> fitting(List<List<int>> dataFrame, List<List<float>> expect, int epoch, float learningRate = 1, string optimizer = "")
    {
        //LMS(Least Mean Square ERR)
        //값을 미리 초기화
        float w = 0; //가중치
        float b = 0; //편향
        List<float> result_x = new List<float>();
        (w, b) = DLUtils.gradDescent(w, b, dataFrame, learningRate, epoch, (float)0.9, optimizer);

        for (int cnt = 0; cnt < dataFrame.Count; cnt++)
        {
            //x에 대한 y값
            result_x.Add(DLUtils.predict(w, b, dataFrame[cnt][0]));
        }

        for (int x = 0; x < expect.Count; x++)
        {
            expect[x].Add(DLUtils.predict(w, b, expect[x][0]));
        }

        return result_x;
    }
}