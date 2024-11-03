using System;
using System.Threading.Tasks;

using UnityEngine;

namespace CV
{
    public class BilateralFilter
    {
        MultiThreadUtils tasker = new MultiThreadUtils();
        public BilateralFilter()
        {
            tasker.listen = resListen;
        }

        //함수 설명
        //커널 사이즈가 클수록 넓은 영역을 필터링하여 부드러워지지만 너무 크면 경계가 없어짐
        //공간적 거리 가중치를 제어하는제 가까운 픽셀일 수록 정밀하게 필터링 하며 멀 수록 경계가 모호해짐
        //색상 가중치는 비슷한 색상끼리 블러링 처리되며 다를 수록 정밀해짐.
        public async Task<float[,]> bilateralFilter(float[,] grayScaleFloat, int kernelSize, int spaceWeight, int colorWeight)
        {
            try
            {
                float[,] output = new float[grayScaleFloat.GetLength(0), grayScaleFloat.GetLength(1)];
                Vector2 maxRowCol = new Vector2(grayScaleFloat.GetLength(0) - 1, grayScaleFloat.GetLength(1) - 1);
                Debug.Log("BilateralFilter Start");

                for (int y = 0; y < maxRowCol.y; y++)
                {
                    //await UniTask.RunOnThreadPool(() => bilateralFilterSubTask(grayScaleFloat, kernelSize, spaceWeight, colorWeight, y));
                    await tasker.SpawnAsync(() => bilateralFilterSubTask(grayScaleFloat, output, kernelSize, spaceWeight, colorWeight, y));
                    //Console.WriteLine($"bilateralFilter x coords are finished but y coord is {y}");
                }

                await tasker.WaitUntil(() =>
                {
                    return tasker.threadCnt == 0;
                });

                Debug.Log("BilateralFilter END");

                return output;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private async Task<bool> bilateralFilterSubTask(float[,] grayScaleFloat, float[,] output, int kernelSize, int spaceWeight, int colorWeight, int yIdx)
        {
            Vector2 maxRowCol = new Vector2(grayScaleFloat.GetLength(0) - 1, grayScaleFloat.GetLength(1) - 1);

            for (int x = 0; x < grayScaleFloat.GetLength(0) - 1; x++)
            {
                float avg = await bilateralWorker(maxRowCol, grayScaleFloat, kernelSize, x, yIdx, spaceWeight, colorWeight);
                output[x, yIdx] = avg;
                Console.WriteLine($"bilateralFilter x: {x} y: {yIdx} avg: {avg}");
            }
            return true;
        }

        private async Task<float> bilateralWorker(Vector2 maxRowCol, float[,] grayScaleFloat, int kernelSize, int x, int y, int spaceWeight, int colorWeight)
        {
            float sumWeight = 0;
            float sumIntensity = 0;
            float currentColorFloat = grayScaleFloat[x, y]; //현재 색상
            float avg = 0;

            await Task.Run(() =>
            {
                for (int c = -kernelSize; c <= kernelSize; c++)
                {
                    for (int r = -kernelSize; r <= kernelSize; r++)
                    {
                        if (CVUtils.isBoundary(x + c, y + r, maxRowCol)) // 현재 픽셀이 이미지의 픽셀안에 존재 하는지 체크
                        {
                            //선택된 주변 픽셀 색상
                            float currentNeighborColor = grayScaleFloat[x + c, y + r];
                            //공간 가중치 (타겟 픽셀과 가까울 수록 값이 커짐)(가우시안)
                            double nSpaceWeight = Math.Exp(-((c * c + r * r) / 2 * spaceWeight * spaceWeight));
                            //색상 가중치 (타겟 픽셀과 색상이 비슷할 수록 값이 커짐)
                            double nColorIntensity = Math.Exp(-Math.Abs(currentColorFloat - currentNeighborColor) / (2 * colorWeight * colorWeight));

                            //kernel에 있는 가중을 모두 더함
                            sumWeight += (float)(nSpaceWeight * nColorIntensity);
                            //전체 가중치
                            sumIntensity += (float)(nSpaceWeight * nColorIntensity * currentNeighborColor);
                        }
                    }
                }

                //가중평균 구함
                if (sumWeight > 0)
                {
                    avg = sumIntensity / sumWeight;
                }
                else
                {
                    avg = currentColorFloat;
                }

            });

            return avg;
        }

        private async Task resListen(object res)
        {
            await Task.Delay(0);
            return;
        }

    }

}