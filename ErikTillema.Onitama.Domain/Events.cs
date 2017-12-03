using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public delegate void GameCreatedEventHandler(object sender, GameEventArgs eventArgs);
    public delegate void TurnPlayEventHandler(object sender, GameEventArgs eventArgs);
    public delegate void TurnPlayedEventHandler(object sender, TurnEventArgs eventArgs);
    public delegate void GameFinishedEventHandler(object sender, GameEventArgs eventArgs);

    public class GameEventArgs : EventArgs {
        public Game Game { get; }
        public GameResult GameResult { get; }

        public GameEventArgs(Game game, GameResult gameResult) {
            Game = game;
            GameResult = gameResult;
        }
    }

    public class TurnEventArgs : EventArgs {
        public Game Game { get; }
        public Turn Turn { get; }
        public TurnResult TurnResult { get; }

        public TurnEventArgs(Game game, Turn turn, TurnResult turnResult) {
            Game = game;
            Turn = turn;
            TurnResult = turnResult;
        }
    }

}
