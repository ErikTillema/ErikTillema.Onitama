using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class MoveForwardGameClient : IGameClient {

        public Turn GetTurn(Game game) {
            Turn winningTurn = game.GetDirectlyWinningTurn();
            if (winningTurn != null) return winningTurn;

            Turn capturingTurn = game.GetCapturingTurn();
            if (capturingTurn != null) return capturingTurn;

            // make any move forward with that card
            Turn result = game.GetValidTurns().FirstOrDefault(IsForwardTurn);
            if (result != null) return result;
            else return game.GetAnyTurn();
        }

        private bool IsForwardTurn(Turn turn) {
            return turn.Move.Y > 0 && turn.Player.PlayerIndex == 0 || turn.Move.Y < 0 && turn.Player.PlayerIndex == 1;
        }

    }

}
