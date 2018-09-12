using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Collections;
using ErikTillema.Onitama.Domain;
using System.Threading;

namespace ErikTillema.Onitama.GameRunner {

    public class TournamentServer {

        public IReadOnlyList<Player> Players => _Players;
        private List<Player> _Players;
        public int GameCount { get; }
        public int? MaxGameTurns { get; }
        private ICardDeckGenerator cardDeckGenerator;

        /// <summary>
        /// Wins[x,y] represents the number of times player x has won from player y.
        /// So Wins[y,x] is the number of times x has lost from player y.
        /// That means, that player x drawed against y gameCount-Wins[x,y]-Wins[y,x] times.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> Wins => _Wins;
        private List<List<int>> _Wins;

        public TournamentServer(IEnumerable<Player> players, int gameCount, int? maxGameTurns = null, ICardDeckGenerator cardDeckGenerator = null) {
            _Players = players.ToList();
            GameCount = gameCount;
            MaxGameTurns = maxGameTurns;
            this.cardDeckGenerator = cardDeckGenerator;
            _Wins = new List<List<int>>();
            for (int i=0; i < _Players.Count; i++) {
                _Wins.Add(new List<int>());
                for(int j=0; j < _Players.Count; j++) {
                    _Wins[i].Add(0);
                }
            }
        }

        public void Run() {
            for(int i=0; i < Players.Count; i++) {
                var player = Players[i];
                Console.Out.WriteLine($"{i} {player.Name}");
            }

            var indices = Enumerable.Range(0, Players.Count).ToList();
            var combinations = indices.GetChooseCombinations(Players.Count, 2).Select(_ => _.ToList()).ToList();
            //Parallel.ForEach(combinations, (gamePlayers) => {
            foreach(var gamePlayers in combinations) { 
                PlayGames(gamePlayers[0], gamePlayers[1]);
                Console.Out.Write("*");
            }
            //});
        }

        public void RemovePlayer(Player player) {
            int index = _Players.SelectWithIndex().Where(tup => tup.Item2 == player).Single().Item1;
            _Players.Remove(player);
            _Wins.RemoveAt(index);
            for (int i = 0; i < _Wins.Count; i++) _Wins[i].RemoveAt(index);
        }

        public void AddPlayer(Player player) {
            _Players.Add(player);
            for (int i = 0; i < _Wins.Count; i++) _Wins[i].Add(0);
            _Wins.Add(new List<int>());
            for (int j = 0; j < _Players.Count; j++) _Wins[_Wins.Count - 1].Add(0);
            // Play games, fill out _Wins
            for (int i=0; i < _Players.Count-1; i++) {
                // Play i against _Players.Count-1
                PlayGames(i, _Players.Count - 1);
            }
        }

        public void WriteResult() {
            Console.Out.WriteLine();
            for (int y = 0; y < Players.Count; y++) {
                for (int x = 0; x < Players.Count; x++) {
                    double ratio = (double)_Wins[x][y] / GameCount;
                    if (x != y) {
                        Console.Out.Write($"{Wins[x][y],3} ({ratio:0.000}) ");
                    } else {
                        Console.Out.Write($"            ");
                    }
                }
                Console.Out.WriteLine();
            }

            for (int i = 0; i < Players.Count; i++) {
                var player = Players[i];
                int totalWins = _Wins[i].Sum();
                double ratio = (double)totalWins / ((Players.Count - 1) * GameCount);
                Console.Out.WriteLine($"{i} {player.Name,-20} {totalWins,3} ({ratio:0.000})");
            }
        }

        public PlayerTournamentResults GetPlayerResults(Player player) {
            int i = Players.SelectWithIndex().Where(tup => tup.Item2 == player).Single().Item1;
            int wins = _Wins[i].Sum();
            int losses = _Wins.Select(row => row[i]).Sum();
            var result = new PlayerTournamentResults((Players.Count-1)*GameCount, wins, losses);
            return result;
        }

        private Object lockObject = new object();
        private void PlayGames(int playerIndex1, int playerIndex2) {
            var player1 = Players[playerIndex1];
            var player2 = Players[playerIndex2];

            Parallel.For(0, GameCount, i => {
            //for (int i = 0; i < GameCount; i++) {
                var cardDeck = cardDeckGenerator?.GetCardDeck(i);
                var gameServer = new GameServer(player1, player2, MaxGameTurns, cardDeck);
                GameResult gameResult = gameServer.Run();
                lock (lockObject) {
                    if (gameResult is WinningGameResult) {
                        if (((WinningGameResult)gameResult).WinningPlayer.Player == player1) {
                            _Wins[playerIndex1][playerIndex2]++;
                        } else {
                            _Wins[playerIndex2][playerIndex1]++;
                        }
                    } else {
                        // draw, no increments
                    }
                }
                Console.Out.Write(".");
            //}
            });
        }

    }

    public class PlayerTournamentResults {
        public int Wins { get; }
        public int Losses { get; }
        public int Draws { get; }
        public PlayerTournamentResults(int total, int wins, int losses) {
            Wins = wins;
            Losses = losses;
            Draws = total - wins - losses;
        }
    }

}
