using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Benchmark
{
    public class BenchmarkConfig : BasePluginConfig
	{
        [JsonPropertyName("LogInterval")]
		public int LogInterval { get; set; } = 30000;
    }

    [MinimumApiVersion(284)]
    public class Benchmark : BasePlugin, IPluginConfig<BenchmarkConfig>
    {
        public override string ModuleName => "CSS-Benchmark";
        public override string ModuleVersion => $"1.0.0";
        public override string ModuleAuthor => "rc https://github.com/rcnoob/";
        public override string ModuleDescription => "A CS# Benchmark Plugin";

        public BenchmarkConfig Config { get; set; } = null!;
        private string? currentTime;
        private Timer writeTimer = new Timer(30000); 
        private string benchmarkDir = Path.Join(Server.GameDirectory, "csgo", "benchmark");
        private string fileDir;
        private int index = 0;

        private class IndexedChartData
        {
            public int Index { get; set; }
            public float PlayerCount { get; set; }
            public float SystemMemoryUsage { get; set; }
            public float LocalHeapSize { get; set; }
            public float TotalLocalCommitted { get; set; }
        }

        public void OnConfigParsed(BenchmarkConfig config)
        {
            Config = config;
        }

        public override void Load(bool hotReload)
        {
            if (!Directory.Exists(benchmarkDir))
            {
                Directory.CreateDirectory(benchmarkDir);
            }

            writeTimer.Interval = Config.LogInterval;
            writeTimer.Elapsed += OnWriteTimedEvent;
        }

        public void StartBenchmark()
        {
            writeTimer.Start();
            Server.ExecuteCommand("vprof_on");
            currentTime = DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss");
            fileDir = Path.Join(benchmarkDir, $"benchmark-{currentTime}.json");
        }

        public void StopBenchmark()
        {
            writeTimer.Stop();
            Server.ExecuteCommand("vprof_generate_report");
            Server.ExecuteCommand("vprof_off");
        }

        private void OnWriteTimedEvent(Object source, ElapsedEventArgs e)
        {
            WriteLogData();
        }

        private void WriteLogData()
        {
            Server.NextFrame(() => 
            {
                GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();
                float playerCount = Utilities.GetPlayers().Count;
                float memoryLoad = gcInfo.MemoryLoadBytes / (1024 * 1024 * 1024);  // Total CS2 Server Memory Usage in GB
                float heapSize = gcInfo.HeapSizeBytes / (1024 * 1024);             // CS# Heap Size in MB
                float totalCommitted = gcInfo.TotalCommittedBytes / (1024 * 1024); // CS# Total MB Committed to Memory

                var indexedChartData = new IndexedChartData
                {
                    Index = index,
                    PlayerCount = playerCount,
                    SystemMemoryUsage = memoryLoad,
                    LocalHeapSize = heapSize,
                    TotalLocalCommitted = totalCommitted
                };

                using (Stream stream = new FileStream(fileDir!, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    if (stream.Length > 1)
                    {
                        stream.WriteByte((byte)',');
                    }

                    JsonSerializer.Serialize(stream, indexedChartData);
                }
                index++;
            });
        }

        [ConsoleCommand("css_startbenchmark", "Start benchmark")]
        [RequiresPermissions("@css/root")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void StartCommand(CCSPlayerController? player, CommandInfo command)
        {
            if(player!.IsValid)
                player.PrintToChat("[CSS-Benchmark] Started benchmark");
            else
                Logger.LogInformation("[CSS-Benchmark] Started benchmark");
            StartBenchmark();
        }

        [ConsoleCommand("css_stopbenchmark", "Stop benchmark")]
        [RequiresPermissions("@css/root")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void StopCommand(CCSPlayerController? player, CommandInfo command)
        {
            if(player!.IsValid)
                player.PrintToChat("[CSS-Benchmark] Stopped benchmark. Please see console log for frametime data, and the /game/csgo/benchmark folder for memory data");
            else
                Logger.LogInformation("[CSS-Benchmark] Stopped benchmark. Please see console log for frametime data, and the /game/csgo/benchmark folder for memory data");
            StopBenchmark();
        }
    }
}
