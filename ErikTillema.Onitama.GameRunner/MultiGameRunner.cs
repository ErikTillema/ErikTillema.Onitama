using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Onitama.Domain;
using System.Threading;

namespace ErikTillema.Onitama.GameRunner {

    public class MultiGameRunner {

        private Player Player1;
        private Player Player2;
        private int GameCount;

        public int WinsPlayer1 { get; private set; }
        public int WinsPlayer2 { get; private set; }

        public MultiGameRunner(Player player1, Player player2, int gameCount) {
            Player1 = player1;
            Player2 = player2;
            GameCount = gameCount;
        }

        public void Run() {
            int wins1 = 0;
            int wins2 = 0;
            Parallel.For(0, GameCount, i => { // @@@
            //for(int i=0; i < GameCount; i++) {
                //if (i > 0 && i % 10 == 0) {
                //    Console.Out.WriteLine($"Played {i} games. {Player1.Name} won {wins1} ({((double)wins1 / i):0.000}) {Player2.Name} won {wins2} ({((double)wins2 / i):0.000})");
                //}
                var gameServer = new GameServer(Player1, Player2);
                GameResult gameResult = gameServer.Run();
                if (gameResult.WinningPlayer.Player == Player1)
                    Interlocked.Increment(ref wins1);
                else
                    Interlocked.Increment(ref wins2);
                Console.Out.Write(".");
            //}
            });
            Console.Out.WriteLine();
            Console.Out.WriteLine($"Played {GameCount} games. {Player1.Name} won {wins1} ({((double)wins1 / GameCount):0.000}) {Player2.Name} won {wins2} ({((double)wins2 / GameCount):0.000})");
            WinsPlayer1 = wins1;
            WinsPlayer2 = wins2;
        }

    }

}
