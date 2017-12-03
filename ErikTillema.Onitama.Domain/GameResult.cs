using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable
    /// </summary>
    public class GameResult {

        public GamePlayer WinningPlayer { get; }

        public GameResult(GamePlayer winningPlayer) {
            WinningPlayer = winningPlayer;
        }

    }
}
