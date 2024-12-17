using System;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

namespace CV
{
    public class Contours
    {
        private static MultiThreadUtils tasker = new MultiThreadUtils();

        //상하좌우 대각선 4 꼭짓점 포함.
        private static int[] dx = new int[8] { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static int[] dy = new int[8] { -1, -1, -1, 0, 1, 1, 1, 0 };

        //캐니엣지를 활용한 방법
        public static async Task contoursDetectorByCanny(Texture2D target)
        {
            int direction = 0;
            float[,] outputTexture = await CannyEdge.cannyEdgeDetector(target, 50, 150);

            //yx 순 이여야 x축으로 이동하고 아래로 내려감.
            for (int y = 0; y <= target.height; y++)
            {
                for (int x = 0; x <= target.width; x++)
                {
                    nextChainCode(outputTexture, x, y, direction);
                }
            }
        }

        //참고: https://kipl.tistory.com/10
        /// <summary>
        /// 체인 코드로 해당 픽셀을 기준으로 주변 반경 픽셀을 검사합니다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        private static void nextChainCode(float[,] image, int x, int y, int direction)
        {
            for (int dIdx = 0; dIdx < dx.Count(); dIdx++)
            {
                int nx = x + dx[dIdx]; // 조회 할 x 좌표
                int ny = y + dy[dIdx]; // 조회 할 y 좌표
                int img_rows = image.GetLength(0);
                int img_cols = image.GetLength(1);

                //이미지의 범위 밖 좌표의 경우 다음 좌표로 이동
                if (!CVUtils.isBoundary(nx, ny, new Vector2(img_rows, img_cols)))
                {
                    continue;
                }



            }
        }

        // 비슷한 컬러끼리 묶는 방법 (hsv 사용)
        public static async Task contoursDetectorByHsv(Texture2D target)
        {
            int batchSize = 1000;
            tasker.listen = resListen;

            int width = target.width;
            int height = target.height;
            Texture2D output = new Texture2D(width, height);
            //1: widthCnt 2: heightCnt 3: hsvColor 3D array
            Color[] colorPixels = target.GetPixels();
            float[,,] hsvColorTarget = new float[width, height, 3];

            //rgb texture2d 2 hsv2d
            for (int y = 0; y < height; y += batchSize)
            {
                for (int x = 0; x < width; x += batchSize)
                {
                    int endX = Math.Min(x + batchSize, width);
                    int endY = Math.Min(y + batchSize, height);

                    await tasker.SpawnAsync(async () =>
                    {
                        int initX = x;
                        int initY = y;

                        for (int startY = initY; startY < endY; startY++)
                        {
                            for (int startX = initX; startX < endX; startX++)
                            {
                                await CVUtils.hsvColorFilterSubTask(colorPixels[startY * width + startX], startX, startY, hsvColorTarget);
                            }
                        }

                        return true;
                    });
                }
            }

            await tasker.WaitUntil(() =>
            {
                return tasker.threadCnt == 0;
            });

            Console.WriteLine("Hsv 컬러로 변환 완료");
        }

        //MARK: Task
        private static async Task resListen(object res)
        {
            await Task.Delay(0);
            return;
        }
    }
}