using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public abstract class MovePawnsToTargetGameClient : IGameClient {

        public abstract Vector GetTarget(Game game);

        [Pure]
        private static int Compare<T>(IList<T> a, IList<T> b) where T : IComparable<T> {
            if (a.Count != b.Count) throw new ArgumentException($"{nameof(a)} and {nameof(b)} must have the same Count");
            for (int i = 0; i < a.Count; i++) {
                int c = a[i].CompareTo(b[i]);
                if (c != 0) return c;
            }
            return 0;
        }

        public Turn GetTurn(Game game) {
            var tup = new MiniMax(game, 2, doChecks: false).GetGameResult();
            if (tup.Item1 == MiniMax.GameResultLosing) {
                // we're fucked
            } else if (tup.Item1 == MiniMax.GameResultWinning) {
                // we're winning.
                // let's not extend the misery of the opponent and take a directly winning turn when we get one.
                Turn winningTurn = game.GetDirectlyWinningTurn();
                if (winningTurn != null) return winningTurn;

                var turn = tup.Item2.First(kvp => kvp.Value == MiniMax.GameResultWinning).Key;
                return turn;
            }

            //Turn capturingTurn = MiniMax.GetCapturingTurn(game);
            //if (capturingTurn != null) return capturingTurn;

            // make the move that brings the pawns closest to the target
            Vector target = GetTarget(game);
            var pawns = game.GameState.InTurnPlayerPieces.Where(p => p is Pawn).ToList();
            List<int> bestDistances = pawns.Select(p => Board.GetDistance(target, p.Position)).OrderBy(_ => _).ToList();
            Turn bestTurn = null;
            foreach(Turn turn in game.GetValidTurns()) { 
                Vector position = turn.OriginalPosition;
                Piece piece = game.GameState.Board[position.X, position.Y];
                piece.Position = turn.OriginalPosition.Add(turn.Move);
                List<int> trialDistances = pawns.Select(p => Board.GetDistance(target, p.Position)).OrderBy(_ => _).ToList();
                piece.Position = position;

                if (Compare(trialDistances, bestDistances) < 0) {
                    if (tup.Item2.ContainsKey(turn) && tup.Item2[turn] == MiniMax.GameResultLosing) {
                        // skip, don't play a losing move (if possible)
                    } else {
                        bestDistances = trialDistances;
                        bestTurn = turn;
                    }
                }
            }
            if (bestTurn != null) return bestTurn;

            if(tup.Item1 != MiniMax.GameResultLosing) {
                // make any non-losing move
                return tup.Item2.First(kvp => kvp.Value != MiniMax.GameResultLosing).Key;
            } else {
                // we're fucked anyway, make any move
                return game.GetAnyTurn();
            }
        }

    }

}
