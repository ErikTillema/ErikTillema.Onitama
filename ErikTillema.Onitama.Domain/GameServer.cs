using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class GameServer {

        public Game Game { get; }

        public IReadOnlyList<IGameClient> GameClients { get; }

        public GameServer(Player player1, Player player2) {
            Game = new Game(player1, player2);
            GameClients = new[] { player1.CreateGameClient(), player2.CreateGameClient() };

            GameCreated?.Invoke(this, new GameEventArgs(Game, null));
        }

        public GameResult Run() { // @@@ introduce a maxTurnCount, to avoid bots playing forever. Then draw?
            while(!Game.IsFinished) {
                Progress();
            }
            GameResult result = new GameResult(Game.WinningPlayer);
            GameFinished?.Invoke(this, new GameEventArgs(Game, result));
            return result;
        }

        public void Progress() {
            TurnPlay?.Invoke(this, new GameEventArgs(Game, null));
            int inTurnPlayerIndex = Game.GameState.InTurnPlayerIndex;
            Turn turn = GameClients[inTurnPlayerIndex].GetTurn(Game);
            TurnResult turnResult = Game.PlayTurn(turn);
            TurnPlayed?.Invoke(this, new TurnEventArgs(Game, turn, turnResult));
        }

        public static event GameCreatedEventHandler GameCreated;
        public event TurnPlayEventHandler TurnPlay;
        public event TurnPlayedEventHandler TurnPlayed;
        public event GameFinishedEventHandler GameFinished;

    }

}
