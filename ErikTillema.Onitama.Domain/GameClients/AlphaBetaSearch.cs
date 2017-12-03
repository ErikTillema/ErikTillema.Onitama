using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// NB: the alpha beta search tree of states is a bit different from the minimax tree of states (at least, my implementation of it)
    /// This tree of states with a result value for each node always is with respect to the same in turn player.
    /// So the whole tree is in the perspective of only one player. 
    /// A node with value loss means a loss for this only player, irrespective of the depth in the tree 
    /// and irrespective of whose turn it is at that node.
    /// </summary>
    public class AlphaBetaSearch {

        public static int GetNeighboursCount; // NB: these are not correct anymore when using Parallel
        public static int GetUniqueIdentifierCount;
        public static int PlayTurnCount;
        public static int UndoTurnCount;

        private Game Game;
        private int OriginalInTurnPlayerIndex;
        private IEvaluator Evaluator;
        private int MaxMoves;
        private bool DoChecks;

        // GameResult = int = score from evaluation function.
        // A higher score means a higher probability of winning
        public const double GameResultCalculating = double.MinValue;
        public const double GameResultLosing = double.MinValue / 2;
        public const double GameResultWinning = double.MaxValue;
        // long = unique GameState identifier, functioning as hashcode (without having to implement IEquatable for GameState)
        private Dictionary<long, double> GameResults;
        private Dictionary<long, int> GameMovesDone;
        private Dictionary<Turn, double> DirectTurns; // @@@ actually these are only the best direct turns, not all of them

        public AlphaBetaSearch(Game game, int maxMoves, IEvaluator evaluator, bool doChecks = true) {
            GetNeighboursCount = 0;
            GetUniqueIdentifierCount = 0;
            PlayTurnCount = 0;
            UndoTurnCount = 0;
            Game = game.Clone();
            OriginalInTurnPlayerIndex = Game.GameState.InTurnPlayerIndex;
            Evaluator = evaluator;
            MaxMoves = maxMoves;
            DoChecks = doChecks;
            GameResults = new Dictionary<long, double>();
            GameMovesDone = new Dictionary<long, int>();
            DirectTurns = new Dictionary<Turn, double>();
        }

        public Tuple<double, Dictionary<Turn, double>> GetGameResult() {
            long hashcode = GetUniqueIdentifier(Game.GameState);
            //Node root = new Node(Game.Board.ToString(), true, GameResultLosing, GameResultWinning, MaxMoves);
            double gameResult = GetGameResult(hashcode, 0, MaxMoves, GameResultLosing, GameResultWinning); //, root);
            //root.Value = gameResult;
            return Tuple.Create(gameResult, DirectTurns);
        }

        private double GetGameResult(long gameStateId, int movesDone, int maxMoves, double min, double max) { //, Node node) {
            if (GameResults.ContainsKey(gameStateId)) {
                double res = GameResults[gameStateId];
                if (res == GameResultLosing || res == GameResultWinning || movesDone >= GameMovesDone[gameStateId]) { 
                    return res;
                } else {
                    GameResults.Remove(gameStateId);
                    GameMovesDone.Remove(gameStateId);
                }
            }

            double result = GameResultCalculating;
            GameResults.Add(gameStateId, result);
            GameMovesDone.Add(gameStateId, movesDone);
            if (movesDone >= maxMoves) {
                result = Evaluator.Evaluate(Game, OriginalInTurnPlayerIndex);
            } else {
                if (movesDone % 2 == 0) { // current node is a max node
                    double value = min;
                    foreach (var tup in GetNeighbours()) {
                        bool isWinningMove = tup.Item2;

                        long gameStateIdNb = GetUniqueIdentifier(Game.GameState);
                        if (GameResults.ContainsKey(gameStateIdNb) && GameResults[gameStateIdNb] == GameResultCalculating)
                            continue; // skip states that we've already seen

                        double trialValue;
                        //Node neighbour = new Node(Game.Board.ToString(), false, value, max, maxMoves - movesDone - 1);
                        if (isWinningMove) {
                            trialValue = GameResultWinning; // it's my turn, so a winning move is winning for me.
                        } else {
                            trialValue = GetGameResult(gameStateIdNb, movesDone + 1, maxMoves, value, max); //, neighbour);
                        }
                        //neighbour.Value = trialValue;
                        //node.Neighbours.Add(Tuple.Create(tup.Item1, neighbour));
                        if (movesDone == 0) {
                            if (DirectTurns.Count == 0 || trialValue > value) {
                                // only add when really higher. Else the min and max will mean that trialValue is not the real trialValue.
                                DirectTurns.Add(tup.Item1, trialValue);
                            }
                        }

                        if (trialValue > value) {
                            value = trialValue;
                        }
                        if (value > max) {
                            value = max;
                            break;
                        }
                    }
                    result = value;
                } else { // n is a min node
                    double value = max;
                    foreach (var tup in GetNeighbours()) {
                        bool isWinningMove = tup.Item2;

                        long gameStateIdNb = GetUniqueIdentifier(Game.GameState);
                        if (GameResults.ContainsKey(gameStateIdNb) && GameResults[gameStateIdNb] == GameResultCalculating)
                            continue; // skip states that we've already seen

                        double trialValue;
                        //Node neighbour = new Node(Game.Board.ToString(), true, min, value, maxMoves - movesDone - 1);
                        if (isWinningMove) {
                            trialValue = GameResultLosing; // it's my opponent's turn, so a winning move is a losing move for me.
                        } else {
                            trialValue = GetGameResult(gameStateIdNb, movesDone + 1, maxMoves, min, value); //, neighbour);
                        }
                        //neighbour.Value = trialValue;
                        //node.Neighbours.Add(Tuple.Create(tup.Item1, neighbour));

                        if (trialValue < value) value = trialValue;
                        if (value < min) {
                            value = min;
                            break;
                        }
                    }
                    result = value;
                }
            }
            GameResults[gameStateId] = result;
            return result;
        }

        /// <summary>
        /// Returns all neighbouring states and whether or not the move to the neighbouring state is winning or not.
        /// Changes Game.GameState during loop.
        /// </summary>
        private IEnumerable<Tuple<Turn, bool>> GetNeighbours() {
            GetNeighboursCount++;

            foreach (Turn turn in Game.GetValidTurns()) {
                TurnResult turnResult = null;
                try {
                    turnResult = Game.GameState.PlayTurn(turn, DoChecks);
                    if (turnResult.GameIsFinished) yield return Tuple.Create(turn, true);
                    else yield return Tuple.Create(turn, false);
                } finally {
                    // roll back
                    Game.GameState.UndoTurn(turn, turnResult, DoChecks);
                }
            }
        }

        // adjusted function for alpha beta search, which doesn't need flipping board horizontally
        // due to always same player perspective.
        // assume for now that the evaluation function does not that the GameState.InTurnPlayerIndex into account.
        // I think that this wouldn't make sense anyway, since all leaves have the same depth and therefore would get 
        // the same added value for this.
        public static long GetUniqueIdentifier(GameState gameState) {
            GetUniqueIdentifierCount++;

            // flip board if necessary
            Piece firstPlayerKing = gameState.PlayerPieces[0][0];
            Piece secondPlayerKing = gameState.PlayerPieces[1][0];
            bool flipVertical = firstPlayerKing.Position.X > 2 || (firstPlayerKing.Position.X == 2 && secondPlayerKing.Position.X > 2);

            long result = 0;

            for (int i = 0; i < 2; i++) {
                IEnumerable<Piece> playerPieces = gameState.PlayerPieces[i]
                    .OrderBy(p => GetOrderingIndex(p))
                    .ThenBy(p => p.IsCaptured)
                    .ThenBy(p => flipVertical ? -p.Position.X : p.Position.X)
                    .ThenBy(p => p.Position.Y); //.ToList(); // @@@ run tests to see if this list was indeed unnecessary.
                //for (int j = 0; j < playerPieces.Count; j++) {
                    //Piece piece = playerPieces[j];
                foreach (Piece piece in playerPieces) { 
                    result *= 25 + 1; // 0 = captured, else on board.
                    result += GetUniqueIdentifier(piece, flipVertical);
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

        private static int GetUniqueIdentifier(Vector position, bool flipVertical) {
            Vector flipped = position;
            flipped = flipVertical ? Domain.Board.FlipVertical(flipped) : flipped;
            return flipped.Y * Domain.Board.Width + flipped.X;
        }

        private static int GetUniqueIdentifier(Piece piece, bool flipVertical) {
            if (piece.IsCaptured)
                return 0;
            else {
                return 1 + GetUniqueIdentifier(piece.Position, flipVertical);
            }
        }

        //public class Node {
        //    public string Board;

        //    public double Min;
        //    public double Max;
        //    public double Value;
        //    public List<Tuple<Turn, Node>> Neighbours;
        //    public bool IsMaxNode;
        //    public int MovesLeft;

        //    public Node(string board, bool isMaxNode, double min, double max, int movesLeft) {
        //        Board = board;
        //        Min = min;
        //        Max = max;
        //        IsMaxNode = isMaxNode;
        //        MovesLeft = movesLeft;
        //        Neighbours = new List<Tuple<Turn, Node>>();
        //    }

        //    public override string ToString() {
        //        return $"{(IsMaxNode?"MaxNode":"MinNode")} {MovesLeft}, {Value} [{Min},{Max}] \n{Board}";
        //    }

        //}

    }

}
