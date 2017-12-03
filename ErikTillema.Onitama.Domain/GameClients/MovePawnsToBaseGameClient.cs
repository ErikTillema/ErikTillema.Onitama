using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class MovePawnsToBaseGameClient : MovePawnsToTargetGameClient {

        public override Vector GetTarget(Game game) {
            return Board.PlayerBases[1 - game.GameState.InTurnPlayerIndex];
        }

    }

}
