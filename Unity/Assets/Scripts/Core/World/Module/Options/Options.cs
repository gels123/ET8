using CommandLine;
using System;
using System.Collections.Generic;

namespace ET
{
    public enum AppType
    {
        Server = 1,
        Watcher = 2, // 每台物理机一个守护进程，用来启动该物理机上的所有进程
        GameTool = 3,
        
        ExcelExporter = 11,
        Proto2CS = 12,
        BenchmarkClient = 13,
        BenchmarkServer = 14,
        Demo = 15,
        LockStep = 16,
    }
    
    public class Options: Singleton<Options>
    {
        [Option("AppType", Required = false, Default = AppType.Server, HelpText = "AppType enum")]
        public AppType AppType { get; set; }

        [Option("StartConfig", Required = false, Default = "StartConfig/Localhost")]
        public string StartConfig { get; set; }

        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }
        
        [Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }

        [Option("LogLevel", Required = false, Default = 0)]
        public int LogLevel { get; set; }
        
        [Option("Console", Required = false, Default = 0)]
        public int Console { get; set; }
    }
}