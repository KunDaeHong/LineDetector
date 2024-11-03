using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

/*
원하는 인스턴스로 반환받기 위해 여러개의 타입으로 실행하기 보다
별개의 인스턴스로 생성하여 단일 타입으로 반환 필수
*/

namespace CV
{

    public class MultiThreadUtils
    {
        public Func<object, Task> listen;
        public SynchronizationContext unityContext;
        private readonly object lockObj = new object();
        public List<Thread> threadPoolList = new List<Thread>();
        public int threadCnt = 0;

        public MultiThreadUtils()
        {
            unityContext = SynchronizationContext.Current;
        }

        private void Run(Thread workerThread)
        {

            lock (lockObj)
            {
                if (threadCnt == 0)
                {
                    return;
                }

                if (workerThread.IsAlive)
                {
                    Debug.LogWarning("스레드가 이미 실행 중입니다.");
                    return;
                }

                workerThread.IsBackground = true;

                try
                {
                    workerThread.Start();
                    workerThread.Join();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"스레드 실행 중 오류 발생: {ex.Message}"); // 예외 처리
                }
            }

            // lock (lockObj)
            // {
            //     threadPoolList.Remove(workerThread);
            // }
        }

        public async Task SpawnAsync<T>(Func<Task<T>> startFunc)
        {
            Thread workerThread = null;

            lock (lockObj)
            {
                if (threadCnt < TaskUtilsConst.Task_Cnt)
                {
                    //실제 카운트 숫자와 idx 숫자가 일치하지 않음. idx넘버는 고정이라 변동되는 숫자로 넣어야 함.
                    workerThread = new Thread(async () => await Worker(startFunc));
                    threadCnt++;
                }
            }

            await WaitUntil(() =>
            {
                lock (lockObj)
                {
                    if (threadCnt > TaskUtilsConst.Task_Cnt)
                    {
                        Console.WriteLine("기다리는 거야");
                    }
                    return threadCnt < TaskUtilsConst.Task_Cnt;
                }
            });

            if (workerThread != null)
            {
                Run(workerThread);

            }

            Console.WriteLine($"현재 스레드 갯수 증가 {threadCnt}");

            // await WaitUntil(() =>
            // {
            //     lock (lockObj)
            //     {
            //         return threadPoolList.Count == 0;
            //     }
            // });
        }

        private async Task Worker<T>(Func<Task<T>> startFunc)
        {
            if (listen == null)
            {
                Debug.Log($"finish task: {threadCnt} left with error");
                throw new Exception("Please add listen function.\nIf task function finish then will be return value to listen function.");
            }

            try
            {
                var resData = await startFunc();
                await SwitchToContext(unityContext);
                unityContext.Post(async _ => await listen(resData), null);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                lock (lockObj)
                {
                    threadCnt--;
                    Console.WriteLine($"현재 스레드 갯수 감소 {threadCnt}");
                }
            }
        }

        public async Task WaitUntil(Func<bool> condition)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(1000);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(1000000000)))
                throw new TimeoutException();
        }


        public async Task SwitchToContext(SynchronizationContext context)
        {
            var tcs = new TaskCompletionSource<bool>();
            context.Post(_ => tcs.SetResult(true), null);
            await tcs.Task;
        }

        public Task SwitchToMainThread(Action action, SynchronizationContext context)
        {
            var tcs = new TaskCompletionSource<bool>();
            context.Post(_ =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (System.Exception e)
                {
                    tcs.SetException(e);
                }
            }, null);

            return tcs.Task;
        }

        public static async Task resListen(object res)
        {
            await Task.Delay(0);
            return;
        }
    }


    public struct TaskUtilsConst
    {
        public static int Task_Cnt = 1900;
    }

    public struct ThreadUtilsConst
    {
        public static int Thread_Cnt = 200;
    }
}


