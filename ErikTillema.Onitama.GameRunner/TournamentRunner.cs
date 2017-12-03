using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Collections;
using ErikTillema.Onitama.Domain;
using System.Threading;

namespace ErikTillema.Onitama.GameRunner {

    public class TournamentRunner {

        private IList<Player> TournamentPlayers;
        private int GameCount;

        /// <summary>
        /// wins[x,y] represents the number of times player x has won from player y
        /// </summary>
        private int[,] Wins;

        public TournamentRunner(IEnumerable<Player> players, int gameCount) {
            TournamentPlayers = players.ToList();
            GameCount = gameCount;
            Wins = new int[TournamentPlayers.Count, TournamentPlayers.Count];
        }

        public void Run() {
            for(int i=0; i < TournamentPlayers.Count; i++) {
                var player = TournamentPlayers[i];
                Console.Out.WriteLine($"{i} {player.Name}");
            }

            var indices = Enumerable.Range(0, TournamentPlayers.Count).ToList();
            var combinations = indices.GetChooseCombinations(TournamentPlayers.Count, 2).Select(_ => _.ToList()).ToList();
            //Parallel.ForEach(combinations, (gamePlayers) => {
            foreach(var gamePlayers in combinations) { 
                PlayGames(gamePlayers[0], gamePlayers[1]);
                Console.Out.Write("*");
            } 
            //});

            Console.Out.WriteLine();
            for (int y=0; y < TournamentPlayers.Count; y++) {
                for (int x=0; x < TournamentPlayers.Count; x++) {
                    double ratio = (double)Wins[x, y] / GameCount;
                    if(x != y) {
                        Console.Out.Write($"{Wins[x,y],3} ({ratio:0.000}) ");
                    } else {
                        Console.Out.Write($"            ");
                    }
                }
                Console.Out.WriteLine();
            }

            for (int i = 0; i < TournamentPlayers.Count; i++) {
                var player = TournamentPlayers[i];
                int totalWins = Wins.Slice(i, 1, 0, TournamentPlayers.Count).Cast<int>().Sum();
                double ratio = (double)totalWins / ((TournamentPlayers.Count-1)*GameCount);
                Console.Out.WriteLine($"{i} {player.Name, -20} {totalWins,3} ({ratio:0.000})");
            }
        }

        private void PlayGames(int playerIndex1, int playerIndex2) {
            var player1 = TournamentPlayers[playerIndex1];
            var player2 = TournamentPlayers[playerIndex2];
            Parallel.For(0, GameCount, i => { 
            //for (int i = 0; i < GameCount; i++) {
                var gameServer = new GameServer(player1, player2);
                GameResult gameResult = gameServer.Run();
                if (gameResult.WinningPlayer.Player == player1)
                    Interlocked.Increment(ref Wins[playerIndex1, playerIndex2]);
                else
                    Interlocked.Increment(ref Wins[playerIndex2, playerIndex1]);
                Console.Out.Write(".");
            //}
            });
        }

    }

}
