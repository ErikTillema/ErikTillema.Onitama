using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public static class GameUtil {

        /// <summary>
        /// Returns all valid moves of the player in view.
        /// Safe for changes due to PlayTurn and UndoTurn while looping over this IEnumerable,
        /// due to use of arrays and array index and for instead of foreach on lists.
        /// </summary>
        [Pure]
        public static IEnumerable<Turn> GetValidTurns(this Game game, int? inViewPlayerIndex = null) {
            MiniMax.GetValidTurnsCount++;
            AlphaBetaSearch.GetValidTurnsCount++;

            int playerIndex = inViewPlayerIndex ?? game.GameState.InTurnPlayerIndex;
            // Tested to see if an ArrayList is faster than yield return: answer, no, it's a tiny bit slower.
            for (int i = 0; i < 2; i++) {
                Card card = game.GameState.GameCards[GameState.PlayerCardIndices[playerIndex][i]];
                foreach (Vector move in card.GetMoves(playerIndex)) { // 2-4
                    int pieceCount = game.GameState.PlayerPieces[playerIndex].Count;
                    for (int j = 0; j < pieceCount; j++) { // 1-5
                        Piece piece = game.GameState.PlayerPieces[playerIndex][j];
                        if (!piece.IsCaptured) {
                            if (game.GameState.IsValidMove(piece, move)) {
                                Turn turn = new Turn(game.Players[playerIndex], card, i, piece, move);
                                yield return turn;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of moves the given inViewPlayer can make.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="inViewPlayerIndex"></param>
        /// <returns></returns>
        public static int GetValidTurnCount(this Game game, int? inViewPlayerIndex = null) {
            AlphaBetaSearch.GetValidTurnCountCount++;

            int playerIndex = inViewPlayerIndex ?? game.GameState.InTurnPlayerIndex;
            int result = 0;
            for (int i = 0; i < 2; i++) {
                Card card = game.GameState.GameCards[GameState.PlayerCardIndices[playerIndex][i]];
                foreach (Vector move in card.GetMoves(playerIndex)) { // 2-4
                    int pieceCount = game.GameState.PlayerPieces[playerIndex].Count;
                    for (int j = 0; j < pieceCount; j++) { // 1-5
                        Piece piece = game.GameState.PlayerPieces[playerIndex][j];
                        if (!piece.IsCaptured) {
                            if (game.GameState.IsValidMove(piece, move)) {
                                result++;
                            }
                        }
                    }
                }
            }
            return result;
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

        public static GameState ParseGameState(string board = null, IEnumerable<Card> cards = null, int inTurnPlayerIndex = 0, IList<int> cardNumbers = null) {
            if (board == null) board = GetDefaultBoard();
            if (cards == null) cards = GetDefaultCards();
            if (cardNumbers == null) cardNumbers = GetDefaultCardNumbers();

            Piece[][] playerPieces = new Piece[2][];
            int[] playerPiecesCount = new int[2];
            for (int i = 0; i < 2; i++) {
                playerPiecesCount[i] = 0;
                playerPieces[i] = new Piece[5];
            }
            // always set King as first Piece of playerPieces, then pawns.
            for (int i = 0; i < board.Length; i++) {
                int y = 4 - (i / 5);
                int x = i % 5;
                char c = board[i];
                if (c == 'k') playerPieces[0][playerPiecesCount[0]++] = new King(0, new Vector(x, y));
                else if (c == 'K') playerPieces[1][playerPiecesCount[1]++] = new King(1, new Vector(x, y));
            }
            for (int i = 0; i < board.Length; i++) {
                int y = 4 - (i / 5);
                int x = i % 5;
                char c = board[i];
                if (c == 'o') playerPieces[0][playerPiecesCount[0]++] = new Pawn(0, new Vector(x, y));
                else if (c == 'O') playerPieces[1][playerPiecesCount[1]++] = new Pawn(1, new Vector(x, y));
            }
            for (int i = 0; i < 2; i++) {
                while (playerPiecesCount[i] < 5)
                    playerPieces[i][playerPiecesCount[i]++] = new Pawn(i, new Vector(0, 0)) { IsCaptured = true };
            }
            return new GameState(playerPieces, cards, inTurnPlayerIndex, cardNumbers);
        }

        public static Game ParseGame(string board = null, IEnumerable<Card> cards = null, int inTurnPlayerIndex = 0, IList<int> cardNumbers = null) {
            if (board == null) board = GetDefaultBoard();
            if (cards == null) cards = GetDefaultCards();
            if (cardNumbers == null) cardNumbers = GetDefaultCardNumbers();
            var players = new[] { new Player("player1", null), new Player("player2", null) };
            var gameState = ParseGameState(board, cards, inTurnPlayerIndex, cardNumbers);
            return new Game(players[0], players[1], gameState);
        }

        private static string GetDefaultBoard() {
            return "OOKOO" +
                   "....." +
                   "....." +
                   "....." +
                   "ookoo";
        }

        private static IList<Card> GetDefaultCards() {
            return new[] { Card.Boar, Card.Cobra, Card.Dragon, Card.Eel, Card.Frog };
        }

        private static IList<int> GetDefaultCardNumbers() {
            return new[] { 0, 1, 2, 3, 4 };
        }

    }
}
