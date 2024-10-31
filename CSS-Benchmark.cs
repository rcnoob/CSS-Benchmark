using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Timers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Benchmark
{
    [MinimumApiVersion(284)]
    public class Benchmark : BasePlugin
    {
        public override string ModuleName => "CSS-Benchmark";
        public override string ModuleVersion => $"1.0.0";
        public override string ModuleAuthor => "rc https://github.com/rcnoob/";
        public override string ModuleDescription => "A CS# Benchmark Plugin";

        private string? currentTime;
        private Timer writeTimer = new Timer(30000); 
        private string benchmarkDir;
        private string fileDir;
        private int index = 0;
        private List<float> PlayerCount = new List<float>();
        private List<float> FrameTime = new List<float>();

        private class IndexedChartData
        {
            public int Index { get; set; }
            public float AvgPlayerCount { get; set; }
            public float AvgFrameTimeTicks { get; set; }
            public float FrameTimeTicksPeak { get; set; }
        }

        public override void Load(bool hotReload)
        {
            if(hotReload)
            {
                Logger.LogError($"[CSS-Benchmark] Please do not hotload this plugin");
                return;
            }

            benchmarkDir = Path.Join(Server.GameDirectory, "csgo", "benchmark");
            if (!Directory.Exists(benchmarkDir))
            {
                Directory.CreateDirectory(benchmarkDir);
            }

            writeTimer.Elapsed += OnWriteTimedEvent;

            currentTime = DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss");
            fileDir = Path.Join(benchmarkDir, $"benchmark-{currentTime}.json");

            Logger.LogInformation($"[CSS-Benchmark] Starting benchmark...");
            RegisterListener<Listeners.OnTick>(LogOnTick);
            StartBenchmark();
        }

        private void StartBenchmark()
        {
            writeTimer.Start();
        }

        private void LogOnTick()
        {
            LogData();
        }

        private void OnWriteTimedEvent(Object source, ElapsedEventArgs e)
        {
            WriteData();
        }

        private void LogData()
        {
            Server.NextFrame(() => 
            {
                FrameTime.Add(Server.FrameTime);
                PlayerCount.Add(Utilities.GetPlayers().Count);
            });
        }

        private void WriteData()
        {
            float avgPlayerCount = CalculateAverage(PlayerCount);
            float avgFrameTime = CalculateAverage(FrameTime);
            var indexedChartData = new IndexedChartData
            {
                Index = index,
                AvgPlayerCount = avgPlayerCount,
                AvgFrameTimeTicks = CalculateFrameTimeTicks(avgFrameTime),
                FrameTimeTicksPeak = FrameTime.Count > 0 ? CalculateFrameTimeTicks(FrameTime.Max()) : 0
            };

            using (Stream stream = new FileStream(fileDir, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                if (stream.Length > 1)
                {
                    stream.WriteByte((byte)',');
                }

                JsonSerializer.Serialize(stream, indexedChartData);
            }

            PlayerCount.Clear();
            FrameTime.Clear();
            index++;
        }

        private float CalculateAverage(List<float> numbers)
        {
            if (numbers == null || numbers.Count == 0) return 0;

            float sum = 0;
            foreach (float number in numbers)
            {
                sum += number;
            }
            
            return sum / numbers.Count;
        }

        private float CalculateFrameTimeTicks(float frametime)
        {
            return frametime * 64;
        }
    }
}
