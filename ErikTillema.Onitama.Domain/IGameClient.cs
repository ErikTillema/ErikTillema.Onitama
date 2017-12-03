using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public interface IGameClient {
        Turn GetTurn(Game game);
    }

    public class GameClient {

        public static readonly IReadOnlyList<IGameClient> AllGameClients = new IGameClient[] {
            new MoveForwardGameClient(),
            new MovePawnsToBaseGameClient(),
            new MovePawnsToKingGameClient(),
        };

    }

}
