using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    internal class ThreadPoolScheduler: IScheduler
    {
        private readonly List<Thread> threads;
        
        private readonly ManualResetEvent workEvent = new ManualResetEvent(false);

        private readonly ConcurrentQueue<int> idQueue = new();
        private readonly ConcurrentDictionary<int, int> idDict = new();
        
        private readonly FiberManager fiberManager;

        public ThreadPoolScheduler(FiberManager fiberManager)
        {
            this.fiberManager = fiberManager;
            int threadCount = Environment.ProcessorCount;
            Log.Info("ThreadPoolScheduler Create threadCount=" + threadCount);
            this.threads = new List<Thread>(threadCount);
            for (int i = 0; i < threadCount; ++i)
            {
                Thread thread = new(this.Loop);
                this.threads.Add(thread);
                thread.Start();
            }
        }

        private void Loop()
        {
            int count = 0;
            while (true)
            {
                if (count <= 0)
                {
                    Thread.Sleep(1);
                    
                    // count最小为1
                    count = this.fiberManager.Count() / this.threads.Count + 1;
                }
                --count;
                
                if (this.fiberManager.IsDisposed())
                {
                    return;
                }
                
                if (!this.idQueue.TryDequeue(out int fiberId))
                {
                    this.workEvent.Reset(); // 准备休眠
                    this.workEvent.WaitOne(); // 等待新任务
                    // Log.Debug($"ThreadPoolScheduler.Loop wait threadId={System.Environment.CurrentManagedThreadId}");
                    continue;
                }
                this.idDict.TryRemove(fiberId, out _);

                Fiber fiber = this.fiberManager.Get(fiberId);
                if (fiber == null || fiber.IsDisposed)
                {
                    continue;
                }

                Fiber.Instance = fiber;
                SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
                fiber.Update();
                fiber.LateUpdate();
                SynchronizationContext.SetSynchronizationContext(null);
                Fiber.Instance = null;

                if (fiber.HasTask())
                {
                    this.idQueue.Enqueue(fiberId);
                    this.idDict.TryAdd(fiberId, fiberId);
                }
            }
        }

        public void Dispose()
        {
            foreach (Thread thread in this.threads)
            {
                this.workEvent.Set();
                thread.Join();
            }
        }

        public void Add(int fiberId)
        {
            if (!this.idDict.ContainsKey(fiberId))
            {
                this.idQueue.Enqueue(fiberId);
                this.idDict.TryAdd(fiberId, fiberId);
                this.workEvent.Set(); // 唤醒线程  
            }
        }
    }
}