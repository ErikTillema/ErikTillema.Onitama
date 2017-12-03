using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public static class GameUtil {

        public static IEnumerable<Turn> GetValidTurns(this Game game, int inTurnPlayerIndex) {
            if(game.GameState.InTurnPlayerIndex == inTurnPlayerIndex) {
                return GetValidTurns(game);
            } else {
                return GetOtherPlayerValidTurns(game);
            }
        }

        /// <summary>
        /// Returns all valid moves of the player in turn.
        /// Safe for changes due to PlayTurn and UndoTurn while looping over this IEnumerable.
        /// Due to use of arrays and array index and for instead of foreach on lists.
        /// </summary>
        [Pure]
        public static IEnumerable<Turn> GetValidTurns(this Game game) {
            int cardCount = game.GameState.InTurnPlayerCardIndices.Length;
            for (int i = 0; i < cardCount; i++) {
                Card card = game.GameState.GameCards[game.GameState.InTurnPlayerCardIndices[i]];
                foreach (Vector move in card.GetMoves(game.GameState.InTurnPlayerIndex)) {
                    int pieceCount = game.GameState.PlayerPieces[game.GameState.InTurnPlayerIndex].Count;
                    for (int j = 0; j < pieceCount; j++) {
                        Piece piece = game.GameState.PlayerPieces[game.GameState.InTurnPlayerIndex][j];
                        if (!piece.IsCaptured) {
                            if (game.GameState.IsValidMove(piece, move)) {
                                Turn turn = new Turn(game.InTurnPlayer, card, i, piece, move);
                                yield return turn;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the valid turns of the other player (the player who's not in turn), as if it were his turn.
        /// Safe for changes due to PlayTurn and UndoTurn while looping over this IEnumerable.
        /// Due to use of arrays and array index and for instead of foreach on lists.
        /// </summary>
        [Pure]
        public static IEnumerable<Turn> GetOtherPlayerValidTurns(this Game game) {
            int cardCount = game.GameState.OtherPlayerCardIndices.Length;
            for (int i = 0; i < cardCount; i++) {
                Card card = game.GameState.GameCards[game.GameState.OtherPlayerCardIndices[i]];
                foreach (Vector move in card.GetMoves(1 - game.GameState.InTurnPlayerIndex)) {
                    int pieceCount = game.GameState.PlayerPieces[1 - game.GameState.InTurnPlayerIndex].Count;
                    for (int j = 0; j < pieceCount; j++) {
                        Piece piece = game.GameState.PlayerPieces[1 - game.GameState.InTurnPlayerIndex][j];
                        if (!piece.IsCaptured) {
                            if (game.GameState.IsValidMove(piece, move)) {
                                Turn turn = new Turn(game.OtherPlayer, card, i, piece, move);
                                yield return turn;
                            }
                        }
                    }
                }
            }
        }

        [Pure]
        private static bool IsDirectlyWinningTurn(Game game, Turn turn) {
            Vector newPosition = turn.OriginalPosition.Add(turn.Move);
            Piece captured = game.GameState.Board[newPosition.X, newPosition.Y];

            bool gameIsFinshed = false;
            if (captured != null && captured is King)
                gameIsFinshed = true;
            else if (newPosition.Equals(Board.PlayerBases[1 - game.GameState.InTurnPlayerIndex]))
                gameIsFinshed = true;
            return gameIsFinshed;
        }

        [Pure]
        public static Turn GetDirectlyWinningTurn(this Game game) {
            return game.GetValidTurns().Where(t => IsDirectlyWinningTurn(game, t)).FirstOrDefault();
        }

        [Pure]
        private static bool IsCapturingTurn(Game game, Turn turn) {
            Vector newPosition = turn.OriginalPosition.Add(turn.Move);
            Piece captured = game.GameState.Board[newPosition.X, newPosition.Y];
            return (captured != null);
        }

        [Pure]
        public static Turn GetCapturingTurn(this Game game) {
            return game.GetValidTurns().Where(t => IsCapturingTurn(game, t)).FirstOrDefault();
        }

        [Pure]
        public static Turn GetAnyTurn(this Game game) {
            Turn result = game.GetValidTurns().FirstOrDefault();
            if (result != null) return result;
            else throw new InvalidOperationException("No moves possible.");
        }

    }
}
