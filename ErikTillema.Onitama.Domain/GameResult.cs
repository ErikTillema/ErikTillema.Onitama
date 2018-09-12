using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable
    /// </summary>
    public abstract class GameResult { }

    public class DrawingGameResult : GameResult { }

    public class WinningGameResult : GameResult {
        public GamePlayer WinningPlayer { get; }

        public WinningGameResult(GamePlayer winningPlayer) {
            WinningPlayer = winningPlayer;
        }
    }
}
