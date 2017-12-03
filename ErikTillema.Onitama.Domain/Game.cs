using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Collections;

namespace ErikTillema.Onitama.Domain {
    public class Game {

        public IReadOnlyList<GamePlayer> Players { get; }

        public Board Board { get; }

        public GameState GameState { get; }

        public GamePlayer InTurnPlayer => Players[GameState.InTurnPlayerIndex];

        public GamePlayer OtherPlayer => Players[1 - GameState.InTurnPlayerIndex];

        public PlayerColor InTurnPlayerColor => PlayerColors[GameState.InTurnPlayerIndex];

        public IList<Tuple<Turn, TurnResult>> PlayedTurns { get; }

        public GamePlayer WinningPlayer => GameState.WinningPlayerIndex.HasValue ? Players[GameState.WinningPlayerIndex.Value] : null;

        public bool IsFinished => GameState.WinningPlayerIndex.HasValue;

        public Game(Player player1, Player player2) : this(player1, player2, null) { }

        public Game(Player player1, Player player2, GameState gameState) {
            GameState = gameState ?? new GameState();
            Players = new[] { new GamePlayer(player1, 0), new GamePlayer(player2, 1) };
            Board = new Board(GameState);
            PlayedTurns = new List<Tuple<Turn, TurnResult>>();
        }

        public Game Clone() {
            return new Game(Players[0].Player, Players[1].Player, GameState.Clone());
        }

        public TurnResult PlayTurn(Turn turn) {
            TurnResult turnResult = GameState.PlayTurn(turn);
            PlayedTurns.Add(Tuple.Create(turn, turnResult));
            Board.LastTurn = turn;
            return turnResult;
        }

        public static readonly Bijection<int, PlayerColor> PlayerColors = new Bijection<int, Domain.PlayerColor>() {
            { 0, PlayerColor.Blue },
            { 1, PlayerColor.Red },
        };

    }
}
