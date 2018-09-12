using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using ErikTillema.Collections;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// NB: the alpha beta search tree of states is a bit different from the minimax tree of states (at least, my implementation of it)
    /// This tree of states with a result value for each node always is with respect to the same in turn player all the time.
    /// So the whole tree is in the perspective of only one player. 
    /// A node with value loss means a loss for this only player, irrespective of the depth in the tree 
    /// and irrespective of whose turn it is at that node.
    /// Tree could look something like this:
    ///        6          -- max nodes, we move, we choose the max of child nodes
    ///       / \
    ///      /   \
    ///     6     L       -- min nodes, opponent moves, opponent chooses the min of child nodes
    ///    / \    |\
    ///   6   8   L W     -- max nodes
    ///  /|\  |\    |\
    /// 1 5 6 8 3   1 W   -- leaves
    /// </summary>
    public class AlphaBetaSearch {

        public static int GetNeighboursCount; // NB: these are not correct anymore when using Parallel
        public static int GetValidTurnsCount;
        public static int GetValidTurnCountCount;
        public static int GetUniqueIdentifierCount;
        public static int PlayTurnCount;
        public static int UndoTurnCount;
        public static int EvaluationCount;
        public static int PruneCount;
        public static int SkippedStatesCount; // we skip states that we have already seen before during ongoing calculation
        public static int SkippedStatesCount2; // we skip states that we have already seen before (with finished calculation)
        public static int RemovingStateCount;

        private Game Game;
        private int OriginalInTurnPlayerIndex;
        private IEvaluator Evaluator;
        private int MaxMoves;
        private bool DoChecks;
        private bool DoLog;
        private bool CollectStats;
        private bool DoPrune; // actually, this is whether or not we do sorting of nodes before pruning...
        private bool Invert; // if inverted, even though player 2 is in turn, we are looking at the tree from the perspective of player 1.
        private double StartingMin;

        // so, evaluation functions will be run from view of player 1
        // and min nodes become max nodes and vice versa.

        // GameResult = double = score from evaluation function.
        // A higher score means a higher probability of winning for the OriginalInTurnPlayerIndex
        public const double GameResultCalculating = double.MinValue;
        public const double GameResultLosing = double.MinValue / 2;
        public const double GameResultWinning = double.MaxValue;
        // long = unique GameState identifier, functioning as hashcode (without having to implement IEquatable for GameState)
        private Dictionary<long, double> GameResults;
        private Dictionary<long, int> GameMovesDone;
        private Dictionary<Turn, double> DirectTurns; // Actually these are only the best direct turns, not all of them
        public Dictionary<long, double> EvaluationScore;
        public Dictionary<long, NodeInfo> NodeInfos;

        public AlphaBetaSearch(Game game, int maxMoves, IEvaluator evaluator, bool doChecks = true, bool doPrune = true, bool collectStats = false, bool doLog = false, bool invert = false, double startingMin = GameResultLosing) {
            GetNeighboursCount = 0;
            GetValidTurnsCount = 0; // * 40
            GetValidTurnCountCount = 0; // * 40, but without yield return so hopefully faster
            GetUniqueIdentifierCount = 0; // * 2*(5 log 5 + 5) = 40
            PlayTurnCount = 0; // * 1
            UndoTurnCount = 0; // * 1
            EvaluationCount = 0;
            PruneCount = 0;
            SkippedStatesCount = 0;
            SkippedStatesCount2 = 0;
            RemovingStateCount = 0;

            Game = game.Clone();
            OriginalInTurnPlayerIndex = Game.GameState.InTurnPlayerIndex;
            Evaluator = evaluator;
            MaxMoves = maxMoves;
            DoChecks = doChecks;
            DoLog = doLog;
            CollectStats = collectStats;
            DoPrune = doPrune;
            Invert = invert;
            StartingMin = startingMin;
            GameResults = new Dictionary<long, double>();
            GameMovesDone = new Dictionary<long, int>();
            DirectTurns = new Dictionary<Turn, double>();
            EvaluationScore = new Dictionary<long, double>();
            NodeInfos = new Dictionary<long, NodeInfo>();
        }

        /// <summary>
        /// Returns the GameResult and GameResults per direct Turn (but only for some direct Turns, not all of them).
        /// </summary>
        /// <returns></returns>
        public Tuple<double, Dictionary<Turn, double>> GetGameResult() {
            long gameStateId = GetUniqueIdentifier(Game.GameState);
            double gameResult = GetGameResult(gameStateId, 0, MaxMoves, StartingMin, GameResultWinning);
            return Tuple.Create(gameResult, DirectTurns);
        }

        private double GetGameResult(long gameStateId, int movesDone, int maxMoves, double min, double max) {
            if (gameStateId == -1) { 
                // for leaves, simply return the evaluation score
                // this saves us calculating many unique identifiers (of all leaves)
                // but will come at a slight cost of calculating the evaluation score of some leaves with the same unique identifier multiple times,
                // or calculating the evaluation score of some leaves for which the evaluation score was already known from non-leaves.
                // Measurements show a slight improvement.
                double evaluationScore = Evaluator.Evaluate(Game, Invert ? 1 - OriginalInTurnPlayerIndex : OriginalInTurnPlayerIndex);
                EvaluationCount++;
                return evaluationScore;
            }

            if (GameResults.ContainsKey(gameStateId)) {
                double res = GameResults[gameStateId];
                if (res == GameResultLosing || res == GameResultWinning || movesDone >= GameMovesDone[gameStateId]) {
                    // @@@ is this still ok now that I'm dividing the result values....
                    SkippedStatesCount2++;
                    return res;
                } else {
                    GameResults.Remove(gameStateId);
                    GameMovesDone.Remove(gameStateId);
                    RemovingStateCount++;
                }
            }

            double result = GameResultCalculating;
            GameResults.Add(gameStateId, result);
            GameMovesDone.Add(gameStateId, movesDone);
            if (movesDone >= maxMoves) {
                throw new Exception("Leaves should be handled above already");
                result = GetEvaluationScore(gameStateId);
                if(DoLog) Log(movesDone, result, gameStateId);
            } else {
                if ((movesDone + (Invert ? 1 : 0))%2 == 0) { // current node is a max node
                    double value = GameResultLosing;
                    long childGameStateId = -1; int neighbours = 0;
                    if (CollectStats) {
                        neighbours = Game.GetValidTurnCount(); // Expensive
                    }
                    foreach (var tupWithIndex in GetNeighbours(true, movesDone).SelectWithIndex()) {
                        int i = tupWithIndex.Item1;
                        var tup = tupWithIndex.Item2;
                        bool isWinningMove = tup.Item2;

                        long gameStateIdNb = movesDone == maxMoves - 1 ? -1 : tup.Item3 ?? GetUniqueIdentifier(Game.GameState); // Don't calculate the unique identifier for leaves.
                        if (GameResults.ContainsKey(gameStateIdNb) && GameResults[gameStateIdNb] == GameResultCalculating) {
                            SkippedStatesCount++;
                            continue; // skip states that we've already seen (and in this case, we've really seen it exactly the same before, including which player was in turn)
                        }

                        double trialValue;
                        if (isWinningMove) {
                            // it's my turn, so a winning move is winning for me.
                            trialValue = GameResultWinning / (movesDone + 1); // a win in 1 is better than a win in 3
                        } else {
                            trialValue = GetGameResult(gameStateIdNb, movesDone + 1, maxMoves, min, max);
                        }

                        if (movesDone == 0) {
                            if (DirectTurns.Count == 0 || trialValue > value) {
                                // only add when really higher. Else the min and max will mean that trialValue is not the real trialValue.
                                DirectTurns.Add(tup.Item1, trialValue);
                            }
                        }

                        if (trialValue > value) {
                            value = trialValue;
                            if (CollectStats) childGameStateId = gameStateIdNb;
                        }
                        min = Math.Max(min, value);
                        if (min >= max) {
                            if (CollectStats) PruneCount += neighbours - (i + 1);
                            break;
                        }
                    }
                    result = value;
                    if (CollectStats) NodeInfos[gameStateId] = new NodeInfo(result, min, max, childGameStateId, movesDone, true);
                    if (DoLog) Log(movesDone, result, gameStateId, true, min, max);
                } else { // current node is a min node
                    double value = GameResultWinning;
                    long childGameStateId = -1; int neighbours = 0;
                    if (CollectStats) neighbours = Game.GetValidTurnCount(); // Expensive
                    foreach (var tupWithIndex in GetNeighbours(false, movesDone).SelectWithIndex()) {
                        int i = tupWithIndex.Item1;
                        var tup = tupWithIndex.Item2;
                        bool isWinningMove = tup.Item2;

                        long gameStateIdNb = movesDone == maxMoves - 1 ? -1 : tup.Item3 ?? GetUniqueIdentifier(Game.GameState); // Don't calculate the unique identifier for leaves.
                        if (GameResults.ContainsKey(gameStateIdNb) && GameResults[gameStateIdNb] == GameResultCalculating) {
                            SkippedStatesCount++;
                            continue; // skip states that we've already seen (and in this case, we've really seen it exactly the same before, including which player was in turn)
                        }

                        double trialValue;
                        if (isWinningMove) {
                            // it's my opponent's turn, so a winning move is a losing move for me.
                            trialValue = GameResultLosing / (movesDone + 1); // a loss in 4 is better than a loss in 2
                        } else {
                            trialValue = GetGameResult(gameStateIdNb, movesDone + 1, maxMoves, min, max);
                        }

                        if (trialValue < value) {
                            value = trialValue;
                            if (CollectStats) childGameStateId = gameStateIdNb;
                        }
                        max = Math.Min(max, value);
                        if (min >= max) {
                            if (CollectStats) PruneCount += neighbours - (i + 1);
                            break;
                        }
                    }
                    result = value;
                    if (CollectStats) NodeInfos[gameStateId] = new NodeInfo(result, min, max, childGameStateId, movesDone, false);
                    if (DoLog) Log(movesDone, result, gameStateId, false, min, max);
                }
            }
            GameResults[gameStateId] = result;
            return result;
        }

        /// <summary>
        /// Returns all neighbouring states and whether or not the move to the neighbouring state is winning or not and the neighbouring gameStateId.
        /// Changes Game.GameState during loop.
        /// Return neighbours in order of score of evaluation function if DoPrune is true.
        /// </summary>
        private IEnumerable<Tuple<Turn, bool, long?>> GetNeighbours(bool isMaxNode, int movesDone) {
            GetNeighboursCount++;

            if (movesDone == MaxMoves - 1 || !DoPrune) {
                // we don't want to calculate the evaluation score of all leave nodes (to sort on them)
                // because that's slower than traversing them unsorted and then (due to ABS pruning) NOT calculating the evaluation score of some of the leaves at all.
                // so, getting neighbours for the almost-leave-nodes is different than for the other nodes.
                foreach (Turn turn in Game.GetValidTurns()) {
                    TurnResult turnResult = null;
                    try {
                        turnResult = Game.GameState.PlayTurn(turn, DoChecks);
                        if (turnResult.GameIsFinished) yield return Tuple.Create(turn, true, (long?)null);
                        else yield return Tuple.Create(turn, false, (long?)null);
                    } finally {
                        // roll back
                        Game.GameState.UndoTurn(turn, turnResult, DoChecks);
                    }
                }
            } else {
                List<Tuple<Turn, double, long?>> turnsWithScore = new List<Tuple<Turn, double, long?>>();

                foreach (Turn turn in Game.GetValidTurns()) {
                    TurnResult turnResult = null;
                    try {
                        turnResult = Game.GameState.PlayTurn(turn, DoChecks);
                        long? gameStateId = GetUniqueIdentifier(Game.GameState); // don't calculate gameStateId again in the ABS min/max loop, so pass it on.
                        double score = GetEvaluationScore(gameStateId.Value);
                        turnsWithScore.Add(Tuple.Create(turn, score, gameStateId));
                    } finally {
                        // roll back
                        Game.GameState.UndoTurn(turn, turnResult, DoChecks);
                    }
                }

                if (isMaxNode) {
                    turnsWithScore.Sort((tup1, tup2) => -1 * tup1.Item2.CompareTo(tup2.Item2)); // descending by score for max nodes.
                } else {
                    turnsWithScore.Sort((tup1, tup2) => 1 * tup1.Item2.CompareTo(tup2.Item2)); // ascending by score for min nodes.
                }

                foreach (var tup in turnsWithScore) {
                    Turn turn = tup.Item1;
                    TurnResult turnResult = null;
                    long? gameStateId = tup.Item3;
                    try {
                        turnResult = Game.GameState.PlayTurn(turn, DoChecks);
                        if (turnResult.GameIsFinished) yield return Tuple.Create(turn, true, gameStateId);
                        else yield return Tuple.Create(turn, false, gameStateId);
                    } finally {
                        // roll back
                        Game.GameState.UndoTurn(turn, turnResult, DoChecks);
                    }
                }
            }
        }

        private double GetEvaluationScore(long gameStateId) {
            if (!EvaluationScore.ContainsKey(gameStateId)) {
                double evaluationScore = Evaluator.Evaluate(Game, Invert ? 1 - OriginalInTurnPlayerIndex : OriginalInTurnPlayerIndex);
                EvaluationCount++;
                EvaluationScore[gameStateId] = evaluationScore;
            }
            return EvaluationScore[gameStateId];
        }

        // adjusted function for alpha beta search, which doesn't need flipping board horizontally
        // due to always same player perspective.
        // But shouldn't the game state into account whose turn it is? I think yes, for sure! We wouldn't want that
        // a min-node (which has exactly the same board and cards as a max node) takes over the value from the max node. Certainly not,
        // because the min-node would take the min of its child nodes and the max node the max.
        public static long GetUniqueIdentifier(GameState gameState) {
            GetUniqueIdentifierCount++;

            // don't flip board, because card moves are not always symmetrical. See todo file.

            long result = 0;
            // take whose turn is it into account
            result *= 2;
            result += gameState.InTurnPlayerIndex;

            // take pieces into account, whether they are captured and location.
            for (int i = 0; i < 2; i++) {
                IEnumerable<Piece> playerPieces = gameState.PlayerPieces[i]
                    .OrderBy(p => GetOrderingIndex(p)) // Not having to do this sorting would save 0.5 second on a depth=6 ABS turn.
                    .ThenBy(p => p.IsCaptured)
                    .ThenBy(p => p.Position.X)
                    .ThenBy(p => p.Position.Y);
                foreach (Piece piece in playerPieces) { 
                    result *= 25 + 1; // 0 = captured, else on board.
                    result += GetUniqueIdentifier(piece);
                }
            }

            // take cards into account for state
            int firstPlayerCardNumber1 = gameState.CardNumbers[gameState.GameCards[GameState.PlayerCardIndices[0][0]]];
            int firstPlayerCardNumber2 = gameState.CardNumbers[gameState.GameCards[GameState.PlayerCardIndices[0][1]]];
            int middleCardNumber = gameState.CardNumbers[gameState.MiddleCard];
            result *= 5;
            result += Math.Max(firstPlayerCardNumber1, firstPlayerCardNumber2); // first "largest" card
            result *= 4;
            result += Math.Min(firstPlayerCardNumber1, firstPlayerCardNumber2); // then "smallest"
            result *= 5;
            result += middleCardNumber;

            return result;
        }

        [Pure]
        private static int GetOrderingIndex(Piece piece) {
            if (piece is King) return 0;
            else return 1; // First King, then Pawns
        }

        private static int GetUniqueIdentifier(Piece piece) {
            if (piece.IsCaptured)
                return 0;
            else {
                return 1 + GetUniqueIdentifier(piece.Position);
            }
        }

        private static int GetUniqueIdentifier(Vector position) {
            return position.Y * Domain.Board.Width + position.X;
        }

        private void Log(int movesDone, double score, long gameStateId) {
            String ws = String.Join("", Enumerable.Range(0, movesDone).Select(_ => "     "));
            Console.Out.WriteLine(ws + GetScoreAsString(score) + " " + gameStateId + " " + String.Join(",", Game.GameState.GameCards.ToList()) + " " + Game.InTurnPlayer + "\n" + Game.Board.ToString());
        }

        private void Log(int movesDone, double score, long gameStateId, bool isMaxNode, double min, double max) {
            String ws = String.Join("", Enumerable.Range(0, movesDone).Select(_ => "     "));
            String addition = isMaxNode ? "max node" : "min node";
            String addition2 = $"[{GetScoreAsString(min)},{GetScoreAsString(max)}]";
            Console.Out.WriteLine(ws + GetScoreAsString(score) + " " + gameStateId + " " + addition + " " + addition2);
        }

        private string GetScoreAsString(double score) {
            return score == GameResultWinning ? "WIN" : score == GameResultLosing ? "LOSS" : $"{score:0.000}";
        }

        public static string GetStats() {
            return 
$@"GetNeighboursCount:       {GetNeighboursCount,10}
GetValidTurnsCount:       {GetValidTurnsCount,10}
GetValidTurnCountCount:   {GetValidTurnCountCount,10}
GetUniqueIdentifierCount: {GetUniqueIdentifierCount,10}
PlayTurnCount:            {PlayTurnCount,10}
UndoTurnCount:            {UndoTurnCount,10}
EvaluationCount:          {EvaluationCount,10}
PruneCount:               {PruneCount,10}
SkippedStatesCount:       {SkippedStatesCount,10}
SkippedStatesCount2:      {SkippedStatesCount2,10}
RemovingStateCount:       {RemovingStateCount,10}";
        }

        public class NodeInfo {
            public double Score { get; }
            public double Min { get; }
            public double Max { get; }
            public long ChildGameStateId { get; }
            public int Depth { get; }
            public bool IsMaxNode { get; }

            public NodeInfo(double score, double min, double max, long childGameStateId, int depth, bool isMaxNode) {
                Score = score;
                Min = min;
                Max = max;
                ChildGameStateId = childGameStateId;
                Depth = depth;
                IsMaxNode = isMaxNode;
            }
        }

    }
}
