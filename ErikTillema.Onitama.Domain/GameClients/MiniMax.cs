using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// This MiniMax implementation (as opposed to my Alpha Beta Search implementation) looks at the board, the cards and the in turn player
    /// and then says if the game is Winning/Losing/Undecided FOR THE PLAYER IN TURN at that moment, in that node in the tree of states.
    /// In other words, this algorithm does not have one player in mind all the time. The tree of states therefore could look like this:
    /// WINNING
    ///    |
    ///  LOSING
    ///    |
    /// WINNING
    /// In order to look at the board and the cards objectively, regardless of the in turn player, when calculating the unique identifier of a 
    /// game state we do the following:
    /// - view the board from the perspective of the in turn player, so flip the board and flip all piece positions if in turn player = 1.
    /// - view the cards from the perspective of the in turn player, so cards of the in turn player first, then middle card, then other players' cards.
    /// </summary>
    public class MiniMax {

        public static int GetNeighboursCount; // NB: these are not correct anymore when using Parallel
        public static int GetValidTurnsCount;
        public static int GetUniqueIdentifierCount;
        public static int PlayTurnCount;
        public static int UndoTurnCount;

        private Game Game;
        private int MaxMoves;
        private bool DoChecks;

        // byte represents GameResult:
        public const byte GameResultLosing = 0;
        public const byte GameResultWinning = 1;
        public const byte GameResultCalculating = 2;
        // Undecided in the next 0 move(s) = 3, Undecided with 0 moves to do
        // Undecided in the next 1 move(s) = 4, 
        // Undecided in the next 2 move(s) = 5,
        // ...
        // long = unique GameState identifier, functioning as hashcode (without having to implement IEquatable for GameState)
        private Dictionary<long, byte> GameResults;
        private Dictionary<Turn, byte> DirectTurns;

        public MiniMax(Game game, int maxMoves, bool doChecks = true) {
            GetNeighboursCount = 0;
            GetValidTurnsCount = 0;
            GetUniqueIdentifierCount = 0;
            PlayTurnCount = 0;
            UndoTurnCount = 0;
            GameResults = new Dictionary<long, byte>();
            DirectTurns = new Dictionary<Turn, byte>();
            Game = game.Clone();
            MaxMoves = maxMoves;
            DoChecks = doChecks;

            if (DoChecks) {
                if (!game.GameState.Board.Cast<Piece>().SequenceEqual(Game.GameState.Board.Cast<Piece>())) throw new Exception("oops");
                if (game.GameState.InTurnPlayerIndex != Game.GameState.InTurnPlayerIndex) throw new Exception("oops");
            }
        }

        /// <summary>
        /// Returns the GameResult and GameResults per direct Turn.
        /// </summary>
        /// <returns></returns>
        public Tuple<byte, Dictionary<Turn, byte>> GetGameResult() {
            long gameStateId = GetUniqueIdentifier(Game.GameState);
            byte gameResult = GetGameResult(gameStateId, 0, MaxMoves);
            return Tuple.Create(gameResult, DirectTurns);
        }

        private byte GetGameResult(long gameStateId, int movesDone, int maxMoves) {
            int movesTodo = maxMoves - movesDone;
            if (GameResults.ContainsKey(gameStateId)) {
                var res = GameResults[gameStateId];
                if (res == GameResultLosing || res == GameResultWinning || (res >= 3 && movesTodo <= res - 3)) {
                    return res;
                } else if (res >= 3 && movesTodo > res - 3) {
                    GameResults.Remove(gameStateId);
                } else {
                    throw new InvalidOperationException("should not happen");
                }
            }

            byte result = GameResultCalculating;
            GameResults.Add(gameStateId, result);
            if (movesDone >= maxMoves) {
                result = (byte)(3 + movesTodo);
            } else {
                result = GameResultLosing;
                foreach (var tup in GetNeighbours()) {
                    bool isWinningMove = tup.Item2;
                    if (isWinningMove) {
                        result = GameResultWinning;
                        if (movesDone == 0) DirectTurns.Add(tup.Item1, result);
                        break;
                    } else {
                        long gameStateIdNb = GetUniqueIdentifier(Game.GameState);
                        if (GameResults.ContainsKey(gameStateIdNb) && GameResults[gameStateIdNb] == GameResultCalculating) {
                            continue; // skip states that we've already seen. But shouldn't we only avoid states for which InTurnPlayerIndex is same? I don't think it really matters: our algorithm doesn't have one of the two players in mind.
                        }

                        byte resultNb = GetGameResult(gameStateIdNb, movesDone + 1, maxMoves);
                        if (resultNb == GameResultLosing) {
                            result = GameResultWinning;
                            if (movesDone == 0) DirectTurns.Add(tup.Item1, GameResultWinning); // this move is winning, so also result for current state is winning
                            break;
                        } else if (resultNb == GameResultWinning) {
                            if (movesDone == 0) DirectTurns.Add(tup.Item1, GameResultLosing); // this move is losing (but that doesn't mean that result for current state is losing)
                        } else { // not 0 and not 1, so outcome is undecided
                            result = (byte)(3 + movesTodo);
                            if (movesDone == 0) DirectTurns.Add(tup.Item1, result); // this move is undecided
                        }
                    }
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

            foreach(Turn turn in Game.GetValidTurns()) { 
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

        public static long GetUniqueIdentifier(GameState gameState) {
            GetUniqueIdentifierCount++;

            // flip board if necessary
            bool flipHorizontal = gameState.InTurnPlayerIndex == 1;
            // don't flip vertical: the board may be symmetric but the moves of the cards are not. So flipping the board is NOT the same state.

            long result = 0;

            for (int i = 0; i < 2; i++) {
                int a = (i == 0) ? gameState.InTurnPlayerIndex : 1 - gameState.InTurnPlayerIndex;
                List<Piece> playerPieces = gameState.PlayerPieces[a]
                    .OrderBy(p => GetOrderingIndex(p))
                    .ThenBy(p => p.IsCaptured)
                    .ThenBy(p => p.Position.X)
                    .ThenBy(p => flipHorizontal ? -p.Position.Y : p.Position.Y).ToList();
                for (int j = 0; j < playerPieces.Count; j++) {
                    Piece piece = playerPieces[j];
                    result *= 25 + 1; // 0 = captured, else on board.
                    result += GetUniqueIdentifier(piece, flipHorizontal);
                }
            }

            // take cards into account for state
            int inTurnPlayerCardNumber1 = gameState.CardNumbers[gameState.GameCards[gameState.InTurnPlayerCardIndices[0]]];
            int inTurnPlayerCardNumber2 = gameState.CardNumbers[gameState.GameCards[gameState.InTurnPlayerCardIndices[1]]];
            int middleCardNumber = gameState.CardNumbers[gameState.MiddleCard];
            result *= 5;
            result += Math.Max(inTurnPlayerCardNumber1, inTurnPlayerCardNumber2); // first "largest" card
            result *= 4;
            result += Math.Min(inTurnPlayerCardNumber1, inTurnPlayerCardNumber2); // then "smallest"
            result *= 5;
            result += middleCardNumber;

            return result;
        }

        [Pure]
        private static int GetOrderingIndex(Piece piece) {
            if (piece is King) return 0;
            else return 1; // First King, then Pawns
        }

        private static int GetUniqueIdentifier(Piece piece, bool flipHorizontal) {
            if (piece.IsCaptured)
                return 0;
            else {
                return 1 + GetUniqueIdentifier(piece.Position, flipHorizontal);
            }
        }

        private static int GetUniqueIdentifier(Vector position, bool flipHorizontal) {
            Vector flipped = position;
            flipped = flipHorizontal ? Domain.Board.FlipHorizontal(flipped) : flipped;
            return flipped.Y * Domain.Board.Width + flipped.X;
        }

    }
}
