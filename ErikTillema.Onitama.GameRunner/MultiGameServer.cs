using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Onitama.Domain;
using System.Threading;

namespace ErikTillema.Onitama.GameRunner {

    public class MultiGameServer {

        private Player Player1;
        private Player Player2;
        private int GameCount;
        private int? MaxGameTurns;
        private bool WriteResults;
        private readonly ICardDeckGenerator cardDeckGenerator;

        public int WinsPlayer1 { get; private set; }
        public int WinsPlayer2 { get; private set; }
        public int Draws { get; private set; }

        public MultiGameServer(Player player1, Player player2, int gameCount, int? maxGameTurns = null, bool writeResults = false, ICardDeckGenerator cardDeckGenerator = null) {
            Player1 = player1;
            Player2 = player2;
            GameCount = gameCount;
            MaxGameTurns = maxGameTurns;
            WriteResults = writeResults;
            this.cardDeckGenerator = cardDeckGenerator;
        }

        public static Object locker = new Object();

        public void Run() {
            int total = 0;
            int wins1 = 0;
            int wins2 = 0;
            int draws = 0;
            Parallel.For(0, GameCount, i => { 
                //if (i > 0 && i % 10 == 0) {
                //    Console.Out.WriteLine($"Played {i} games. {Player1.Name} won {wins1} ({((double)wins1 / i):0.000}) {Player2.Name} won {wins2} ({((double)wins2 / i):0.000})");
                //}
                var cardDeck = cardDeckGenerator?.GetCardDeck(i);
                var gameServer = new GameServer(Player1, Player2, MaxGameTurns, cardDeck);
                GameResult gameResult = gameServer.Run();
                lock (locker) {
                    total++;
                    if (gameResult is WinningGameResult) {
                        if (((WinningGameResult)gameResult).WinningPlayer.Player == Player1) wins1++;
                        else wins2++;
                    } else {
                        draws++;
                    }
                    if (WriteResults) {
                        Console.Out.Write("\r");
                        Console.Out.Write($"Played {total} games. {Player1.Name} won {wins1} ({((double)wins1 / GameCount):0.000}). {Player2.Name} won {wins2} ({((double)wins2 / GameCount):0.000}). Draws {draws} ({((double)draws / GameCount):0.000}).");
                    }
                }
            });
            if (WriteResults) {
                Console.Out.WriteLine();
            }
            WinsPlayer1 = wins1;
            WinsPlayer2 = wins2;
            Draws = draws;
        }

    }

}
