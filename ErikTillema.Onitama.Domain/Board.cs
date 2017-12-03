using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Stateful.
    /// 
    /// The game is played like chess: from bottom to top for blue (player 0), from top to bottom for red (player 1).
    /// Blue always starts at the bottom or South, Red starts at the top or North.
    /// Blue's pieces are lowercase (k, o), Red's pieces are uppercase (K, O)
    /// ..+..
    /// .....
    /// .....
    /// .....
    /// ..+..
    /// </summary>
    public class Board {

        public const int Width = 5;
        public const int Height = 5;

        public static readonly IReadOnlyList<Vector> PlayerBases = new[] {
            new Vector(2, 0),
            new Vector(2, 4),
        };

        public GameState GameState { get; }

        public Turn LastTurn { get; set; }

        public Board(GameState gameState) {
            GameState = gameState;
            LastTurn = null;
        }

        /// <summary>
        /// Returns the distance between the given positions if every horizontal, vertical or diagonal step counts as 1.
        /// </summary>
        [Pure]
        public static int GetDistance(Vector position1, Vector position2) {
            int dx = Math.Abs(position1.X - position2.X);
            int dy = Math.Abs(position1.Y - position2.Y);
            return Math.Max(dx, dy);
        }

        [Pure]
        public static Vector FlipVertical(Vector position) {
            return new Vector(Width-1 - position.X, position.Y);
        }

        [Pure]
        public static Vector FlipHorizontal(Vector position) {
            return new Vector(position.X, Height-1 - position.Y);
        }

        public override string ToString() {
            char[,] result = new char[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    result[x, y] = '.';

            foreach(Vector playerBase in PlayerBases) {
                result[playerBase.X, playerBase.Y] = '+';
            }

            if (LastTurn != null) {
                Vector previousPosition = LastTurn.OriginalPosition;
                result[previousPosition.X, previousPosition.Y] = '#';
            }

            for (int i=0; i < 2; i++) {
                var pieces = GameState.PlayerPieces[i];
                foreach(var piece in pieces.Where(p => !p.IsCaptured)) {
                    string s = (piece is King) ? "k" : "o";
                    if (i == 1) s = s.ToUpper();
                    result[piece.Position.X, piece.Position.Y] = s[0];
                }
            }

            return GetAsBoardString(result);
        }

        [Pure]
        public static string GetAsBoardString(char[,] chars) {
            StringBuilder sb = new StringBuilder();
            for (int y = Height - 1; y >= 0; y--) {
                for (int x = 0; x < Width; x++) {
                    sb.Append(chars[x, y]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }
}
