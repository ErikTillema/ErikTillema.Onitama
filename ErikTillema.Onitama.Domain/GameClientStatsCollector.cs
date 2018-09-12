using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ErikTillema.Onitama.Domain {

    public class GameClientStatsCollector : IGameClientStatsCollector {

        private Stopwatch stopwatch;
        private List<TurnStats> turnStats;

        public GameClientStatsCollector() {
            turnStats = new List<TurnStats>();
        }

        public void StartGetTurn(Game game) {
            stopwatch = Stopwatch.StartNew();
        }

        public void EndGetTurn() {
            stopwatch.Stop();
            double timeS = stopwatch.Elapsed.TotalSeconds;
            turnStats.Add(new TurnStats(timeS));
        }

        public string GetReport() {
            StatResults timeStatResults = new StatResults("time", turnStats.Select(_ => _.TimeS));
            return timeStatResults.ToString();
        }

        public class TurnStats {
            public double TimeS { get; }
            public TurnStats(double timeS) {
                TimeS = timeS;
            }
        }

        public class StatResults {

            string Name { get; }
            double Min { get; }
            double Max { get; }
            double Average { get; }

            public StatResults(string name, IEnumerable<double> values) {
                var tempValues = values.ToList();
                Name = name;
                if (tempValues.Count > 0) {
                    Min = tempValues.Min();
                    Max = tempValues.Max();
                    Average = tempValues.Average();
                }
            }

            public override string ToString() {
                return $"{Name:20}: average={Average}, min={Min}, max={Max}";
            }
        }

    }

}
