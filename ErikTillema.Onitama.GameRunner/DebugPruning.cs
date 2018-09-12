using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Onitama.Domain;
using System.Globalization;

namespace ErikTillema.Onitama.GameRunner {

    public class DebugPruning {

        static void DoDebugPruning() {
            var all1 = RunSingleTurn(false);
            var all2 = RunSingleTurn(true);
            var scores1 = all1.Item1;
            var scores2 = all2.Item1;
            var keys = scores1.Keys.Intersect(scores2.Keys);
            var diffs = keys.Where(key => scores1[key] != scores2[key]).ToList();
            Console.Out.WriteLine(diffs.Count);
            foreach(long key in diffs) {
                Console.Out.WriteLine($"{key} {scores1[key]:0.000} vs {scores2[key]:0.000}");
            }
            long cur = 1655486673765524;
            while(cur != -1 && all1.Item2.ContainsKey(cur)) {
                var inf = all1.Item2[cur];
                Console.Out.WriteLine($"{GetScoreAsString(inf.Score)} [{GetScoreAsString(inf.Min)},{GetScoreAsString(inf.Max)}] depth={inf.Depth} child={inf.ChildGameStateId}");
                cur = inf.ChildGameStateId;
            }
            cur = 1655486673765524;
            while (cur != -1 && all2.Item2.ContainsKey(cur)) {
                var inf = all2.Item2[cur];
                Console.Out.WriteLine($"{GetScoreAsString(inf.Score)} [{GetScoreAsString(inf.Min)},{GetScoreAsString(inf.Max)}] depth={inf.Depth} child={inf.ChildGameStateId}");
                cur = inf.ChildGameStateId;
            }
        }

        private static string GetScoreAsString(double score) {
            return score == AlphaBetaSearch.GameResultWinning ? "WIN" : score == AlphaBetaSearch.GameResultLosing ? "LOSS" : $"{score:0.000000}";
        }

        static Tuple<Dictionary<long, double>, Dictionary<long, AlphaBetaSearch.NodeInfo>> RunSingleTurn(bool doPrune) {
            var statsCollector = new GameClientStatsCollector();
            var evaluator = new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0);
            AlphaBetaSearchGameClient absGameClient = new AlphaBetaSearchGameClient(evaluator, 7, statsCollector, doPrune); //5
            Player player1 = new Player("AlphaBetaSearch", () => absGameClient); 
            Player player2 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            var gameClient = player1.CreateGameClient.Invoke();
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    ".o..." +
                    "o.koo";
            IEnumerable<Card> cards = new List<Card>() { Card.Boar, Card.Ox, Card.Tiger, Card.Dragon, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            Turn turn = gameClient.GetTurn(new Game(player1, player2, GameUtil.ParseGameState(b, cards, 0, cardNumbers)));
            Console.Out.WriteLine(AlphaBetaSearch.GetStats());
            Console.Out.WriteLine(statsCollector.GetReport());
            // test again with maxMoves 5: result should be 0.256...
            //if (turn.ToString() != "card Boar, moving Pawn from (3,0) to (3,1)") throw new Exception("Bad result");
            return Tuple.Create(absGameClient.EvaluationScore, absGameClient.NodeInfos);
        }

    }
}
