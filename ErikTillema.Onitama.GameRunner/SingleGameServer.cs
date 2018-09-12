using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Onitama.Domain;

namespace ErikTillema.Onitama.GameRunner {

    public class SingleGameServer {

        private GameServer GameServer;

        public SingleGameServer(Player player1, Player player2) {
            GameServer.GameCreated += GameServer_GameCreated;
            GameServer = new GameServer(player1, player2);
        }

        public void Run() {
            GameServer.TurnPlay += GameServer_TurnPlay;
            GameServer.TurnPlayed += GameServer_TurnPlayed;
            GameResult gameResult = GameServer.Run();
            String gameResultString = gameResult is DrawingGameResult ? "draw" : $"winner is {((WinningGameResult)gameResult).WinningPlayer} ({((WinningGameResult)gameResult).WinningPlayer.Player})";
            Console.Out.WriteLine($"Game finished, {gameResultString} in {GameServer.Game.PlayedTurns.Count} turns (total from both players).");
        }

        private void GameServer_GameCreated(object sender, GameEventArgs eventArgs) {
            var game = eventArgs.Game;
            //foreach (Card card in Card.AllCards) {
            //    Console.Out.WriteLine($"{card.Name}:");
            //    Console.Out.WriteLine(card.GetMovesAsBoardString());
            //}

            Console.Out.WriteLine($"Cards: {string.Join(", ", game.GameState.GameCards.ToList())}");
            foreach (Card card in game.GameState.GameCards) {
                Console.Out.WriteLine($"{card.Name}:");
                Console.Out.WriteLine(card.GetMovesAsBoardString());
            }
            Console.Out.WriteLine($"Players: {string.Join(", ", game.Players.Select(p => p.ToString() + $" ({p.Player})"))}");
            Console.Out.WriteLine($"StartingPlayer: {game.InTurnPlayer}");
            Console.Out.WriteLine();
        }

        private void GameServer_TurnPlay(object sender, GameEventArgs eventArgs) {
            var game = eventArgs.Game;
            string s = "";
            //var tup = new MiniMax(game, 5, doChecks: false).GetGameResult();
            //if (tup.Item1 == 0) s = $"{game.InTurnPlayer} is in a losing position. ";
            //else if (tup.Item1 == 1) s = $"{game.InTurnPlayer} is in a winning position. ";
            Console.Out.WriteLine($"{game.PlayedTurns.Count + 1}. {s}{game.InTurnPlayer} has {string.Join(", ", game.GameState.InTurnPlayerCards)}");
        }

        private void GameServer_TurnPlayed(object sender, TurnEventArgs eventArgs) {
            var game = eventArgs.Game;
            var turn = eventArgs.Turn;
            var turnResult = eventArgs.TurnResult;

            Console.Out.Write($"{game.PlayedTurns.Count}. {turn.Player} played {turn}");
            //Console.Out.WriteLine($"{Game.OtherPlayer} had {string.Join(", ", otherPlayerCards)}. Middle card {middleCard}");

            if (turnResult.CapturedPieceType != null)
                Console.Out.WriteLine($", capturing {turnResult.CapturedPieceType}");
            else
                Console.Out.WriteLine();

            Console.Out.WriteLine($"{game.Board}");
        }

    }

}
