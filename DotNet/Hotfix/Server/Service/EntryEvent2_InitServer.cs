using System;
using System.Net;

namespace ET.Server
{
    [Event(SceneType.Main)]
    public class EntryEvent2_InitServer: AEvent<Scene, EntryEvent2>
    {
        protected override async ETTask Run(Scene root, EntryEvent2 args)
        {
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    Log.Info("EntryEvent2_InitServer begin AppType=Server");
                    int process = root.Fiber.Process;
                    // 创建网络纤程
                    StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(process);
                    if (startProcessConfig.Port > 0)
                    {
                        await FiberManager.Instance.Create(SchedulerType.ThreadPool, ConstFiberId.NetInner, 0, SceneType.NetInner, "NetInner");
                    }
                    // 创建业务纤程
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(process);
                    foreach (StartSceneConfig sceneConfig in processScenes)
                    {
                        await FiberManager.Instance.Create(SchedulerType.ThreadPool, sceneConfig.Id, sceneConfig.Zone, sceneConfig.Type, sceneConfig.Name);
                    }
                    Log.Info("EntryEvent2_InitServer end AppType=Server");
                    break;
                }
                case AppType.Watcher:
                {
                    Log.Info("EntryEvent2_InitServer begin AppType=Watcher");
                    root.AddComponent<WatcherComponent>();
                    Log.Info("EntryEvent2_InitServer end AppType=Watcher");
                    break;
                }
            }
            
            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
        }
    }
}