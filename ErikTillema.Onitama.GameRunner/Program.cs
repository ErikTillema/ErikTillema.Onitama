using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Onitama.Domain;
using System.Globalization;

namespace ErikTillema.Onitama.GameRunner {
    class Program {

        static void Main(string[] args) {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            RunSingleTurn();
            //RunSingleGame();
            //RunMultiGame();
            //RunTournament();
            //RunGeneticSelector();
        }

        static void RunSingleTurn() {
            var evaluator = new Evaluator(1.0, 0.1, -0.5, 0.1, 0.1);
            Player player1 = new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(evaluator, 7));
            Player player2 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            var gameClient = player1.CreateGameClient.Invoke();
            gameClient.GetTurn(new Game(player1, player2));
        }

        static void RunSingleGame() {
            //Player player1 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            //Player player2 = new Player("MovePawnsToKing", () => new MovePawnsToKingGameClient());
            var evaluator = new Evaluator(1.0, 0.1, -0.5, 0.1, 0.1);
            Player player1 = new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(evaluator, 5));
            Player player2 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            var singleGameRunner = new SingleGameRunner(player1, player2);
            singleGameRunner.Run();
        }

        static void RunMultiGame() {
            Player player1 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            Player player2 = new Player("MovePawnsToKing", () => new MovePawnsToKingGameClient());
            //Player player3 = new Player("MoveForward1", () => new MoveForwardGameClient());
            //Player player4 = new Player("MoveForward2", () => new MoveForwardGameClient());
            var multiGameRunner = new MultiGameRunner(player1, player2, 100);
            multiGameRunner.Run();
        }

        static void RunTournament() {
            Player player1 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            Player player2 = new Player("MovePawnsToKing", () => new MovePawnsToKingGameClient());
            Player player3 = new Player("MoveForward", () => new MoveForwardGameClient());
            var tournamentRunner = new TournamentRunner(new[] { player1, player2, player3 }, 100);
            tournamentRunner.Run();
        }

        static void RunGeneticSelector() {
            //Evaluator initialEvaluator = new Evaluator(1.0, 0.1, -0.5, 0.1, 0.1); ;
            //Evaluator initialEvaluator = new Evaluator(0.732997958889696, 0.357760413716436, -0.228659760779077, 0.1745383381725, 0.362302185670613);
            Evaluator initialEvaluator = new Evaluator(0.747899978769897, 0.522194351312795, -0.703450416542334, 0.426722689730452, 0.0512523977324611);
            Evaluator champion = GeneticGameClientSelector.GetBestEvaluator(10, 10, 3, initialEvaluator);
            Console.Out.WriteLine($"Last champion: {champion}");
        }

    }
}
