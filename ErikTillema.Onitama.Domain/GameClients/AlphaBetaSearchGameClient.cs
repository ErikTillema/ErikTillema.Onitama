using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ErikTillema.Onitama.Domain {

    public class AlphaBetaSearchGameClient : IGameClient {

        private IEvaluator Evaluator;
        private int MaxMoves;
        private readonly IGameClientStatsCollector GameClientStatsCollector;
        private readonly bool DoPrune;
        private readonly bool DoLog;
        private readonly bool RunParallel;
        private readonly bool CollectStats;
        public Dictionary<long, double> EvaluationScore;
        public Dictionary<long, AlphaBetaSearch.NodeInfo> NodeInfos;
        public double GameResult; // score of root node

        public AlphaBetaSearchGameClient(IEvaluator evaluator, int maxMoves, IGameClientStatsCollector gameClientStatsCollector = null, bool doPrune = true, bool collectStats = false, bool doLog = false, bool runParallel = false) {
            Evaluator = evaluator;
            MaxMoves = maxMoves;
            GameClientStatsCollector = gameClientStatsCollector;
            DoPrune = doPrune;
            CollectStats = collectStats;
            DoLog = doLog;
            RunParallel = runParallel;
        }

        public Turn GetTurn(Game game) {
            if(RunParallel) {
                return GetTurnParallel(game);
            } else {
                return GetTurnNormal(game);
            }
        }

        private int GetMaxMoves(Game game) { // @@@ do the same for parallel, +1 ?
            int pieceCount = game.GameState.PlayerPieces.SelectMany(_ => _).Count(p => !p.IsCaptured);
            if (pieceCount <= 2) return 10;
            if (pieceCount <= 3) return 8;
            if (pieceCount <= 5) return 7;
            if (pieceCount <= 7) return 6;
            return 5;
        }

        private Turn GetTurnNormal(Game originalGame) {
            try {
                GameClientStatsCollector?.StartGetTurn(originalGame);
                int maxMoves = GetMaxMoves(originalGame); // MaxMoves
                AlphaBetaSearch abs = new AlphaBetaSearch(originalGame, maxMoves, Evaluator, false, DoPrune, CollectStats, DoLog);
                var gameResults = abs.GetGameResult();
                if (gameResults.Item1 == AlphaBetaSearch.GameResultWinning) {
                    Turn winningTurn = originalGame.GetDirectlyWinningTurn();
                    if (winningTurn != null) return winningTurn;
                }
                GameResult = gameResults.Item1;
                EvaluationScore = abs.EvaluationScore;
                NodeInfos = abs.NodeInfos;
                return gameResults.Item2.OrderByDescending(kvp => kvp.Value).First().Key;
            } finally {
                GameClientStatsCollector?.EndGetTurn();
            }
        }

        /// <summary>
        /// This method runs the ABS algorithm parallel, in the following way:
        /// Only parallelize the calculation of the score of the depth=1 nodes, then take the max of those.
        /// To do the calculation of the depth=1 node correctly, we need to pass on the Invert=true property and set MaxMoves one lower.
        /// Also, we need to sort on evaluation score as usual.
        /// Finally, in order for the pruning to work well, we have to pass on the updated Min (alpha) as much as possible.
        /// Here, we have a trade-off: on the one hand we want to pass on the updated Min value immediately to the next node, but then we can't parallelize anything.
        /// On the other hand, if we parallelize with 8 cores and start calculating 8 nodes immediately with Min = -inf, then those calculations might all be slow, and the parallelizing overhead results in time lost instead of time won.
        /// After some trial, parallelizing 4 seems to have the best results.
        /// </summary>
        /// <param name="originalGame"></param>
        /// <returns></returns>
        private Turn GetTurnParallel(Game originalGame) {
            try {
                GameClientStatsCollector?.StartGetTurn(originalGame);

                List<Tuple<Turn, Game, double>> games = new List<Tuple<Turn, Game, double>>();
                foreach (Turn turn in originalGame.GetValidTurns()) {
                    TurnResult turnResult = null;
                    try {
                        turnResult = originalGame.GameState.PlayTurn(turn);
                        var game = originalGame.Clone();
                        double score = Evaluator.Evaluate(game, 1 - game.GameState.InTurnPlayerIndex);
                        // @@@ if the turn is directly winning, we are still going to try to explore the subtree and that leads to problems.
                        games.Add(Tuple.Create(turn, game, score));
                    } finally {
                        // roll back
                        originalGame.GameState.UndoTurn(turn, turnResult);
                    }
                }

                ConcurrentBag<Tuple<Turn, double>> bag = new ConcurrentBag<Tuple<Turn, double>>();
                object dummyLock = new object();
                double min = AlphaBetaSearch.GameResultLosing;
                var orderedGames = games.OrderByDescending(t => t.Item3).ToList();
                Parallel.ForEach(orderedGames, 
                        new ParallelOptions() { MaxDegreeOfParallelism = Math.Min(4, Environment.ProcessorCount) }, 
                        tup => {
                    Turn turn = tup.Item1;
                    var game = tup.Item2;
                    AlphaBetaSearch abs = new AlphaBetaSearch(game, MaxMoves - 1, Evaluator, false, DoPrune, CollectStats, DoLog, invert: true, startingMin: min);
                    var gameResults = abs.GetGameResult();
                    var gameResult = gameResults.Item1;
                    lock (dummyLock) {
                        min = Math.Max(min, gameResult);
                    }
                    bag.Add(Tuple.Create(turn, gameResult));
                });

                var best = bag.OrderByDescending(tup => tup.Item2).First();
                GameResult = best.Item2;
                return best.Item1;
            } finally {
                GameClientStatsCollector?.EndGetTurn();
            }
        }

    }
}
