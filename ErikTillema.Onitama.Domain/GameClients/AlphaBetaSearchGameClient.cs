using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class AlphaBetaSearchGameClient : IGameClient {

        private IEvaluator Evaluator;
        private int MaxMoves;

        public AlphaBetaSearchGameClient(IEvaluator evaluator, int maxMoves) {
            Evaluator = evaluator;
            MaxMoves = maxMoves;
        }

        public Turn GetTurn(Game game) {
            AlphaBetaSearch abs = new AlphaBetaSearch(game, MaxMoves, Evaluator, false);
            var gameResult = abs.GetGameResult();
            if (gameResult.Item1 == AlphaBetaSearch.GameResultWinning) {
                Turn winningTurn = game.GetDirectlyWinningTurn();
                if (winningTurn != null) return winningTurn;
            }
            return gameResult.Item2.OrderByDescending(kvp => kvp.Value).First().Key;
        }

    }
}
