using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class MovePawnsToKingGameClient : MovePawnsToTargetGameClient {

        public override Vector GetTarget(Game game) {
            return game.GameState.OtherPlayerPieces.Single(p => p is King).Position;
        }

    }

}
