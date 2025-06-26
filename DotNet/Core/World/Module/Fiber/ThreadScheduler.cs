using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    // 一个Fiber一个固定的线程
    internal class ThreadScheduler: IScheduler
    {
        private readonly ConcurrentDictionary<int, Thread> dict = new();
        
        private readonly FiberManager fiberManager;

        public ThreadScheduler(FiberManager fiberManager)
        {
            this.fiberManager = fiberManager;
        }

        private void Loop(int fiberId)
        {
            Fiber fiber = fiberManager.Get(fiberId);
            Fiber.Instance = fiber;
            SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
            
            while (true)
            {
                if (this.fiberManager.IsDisposed())
                {
                    return;
                }
                
                fiber = fiberManager.Get(fiberId);
                if (fiber == null)
                {
                    this.dict.TryRemove(fiberId, out _);
                    return;
                }
                if (fiber.IsDisposed)
                {
                    this.dict.TryRemove(fiberId, out _);
                    return;
                }
                
                fiber.Update();
                fiber.LateUpdate();

                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            foreach (var kv in this.dict.ToArray())
            {
                kv.Value.Join();
            }
        }

        public void Add(int fiberId)
        {
            if (this.dict.ContainsKey(fiberId))
            {
                Log.Warning($"ThreadScheduler.Add repeated fiberId={fiberId}");
                return;
            }
            Thread thread = new(() => this.Loop(fiberId));
            this.dict.TryAdd(fiberId, thread);
            thread.Start();
        }
    }
}