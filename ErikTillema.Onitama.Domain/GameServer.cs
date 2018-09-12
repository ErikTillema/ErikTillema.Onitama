using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class GameServer {

        public Game Game { get; }

        public IReadOnlyList<IGameClient> GameClients { get; }

        /// <summary>
        /// Maximum of turns before the game automatically becomes a draw.
        /// Null means infinity.
        /// </summary>
        public int? MaxGameTurns { get; }

        public GameServer(Player player1, Player player2, int? maxGameTurns = null, IReadOnlyList<Card> cardDeck = null) {
            Game = new Game(player1, player2, cardDeck);
            GameClients = new[] { player1.CreateGameClient(), player2.CreateGameClient() };
            MaxGameTurns = maxGameTurns;

            GameCreated?.Invoke(this, new GameEventArgs(Game, null));
        }

        public GameResult Run() {
            int turnsPlayed = 0;
            int maxGameTurns = MaxGameTurns ?? int.MaxValue;
            while (!Game.IsFinished && turnsPlayed < maxGameTurns) {
                Progress();
                turnsPlayed++;
            }
            GameResult result = Game.IsFinished ? new WinningGameResult(Game.WinningPlayer) as GameResult : new DrawingGameResult();
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
