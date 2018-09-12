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
            //RunSingleTurn5();
            //DebugPruning();
            RunSingleGame();
            //RunMultiGame();
            //RunTournament();
            //RunOneOnOneGeneticSelector();
            //RunTournamentGeneticSelector();
            //RunFixedOpponentGeneticSelector();
        }

        static void RunSingleTurn() {
            var statsCollector = new GameClientStatsCollector();
            //var evaluator = new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0);
            var evaluator = new Evaluator(0.654, 1.000, 0.125, 0.674, 0.241, 0.346, 0.867, 0.554, 0.338);
            AlphaBetaSearchGameClient absGameClient = new AlphaBetaSearchGameClient(evaluator, 6, statsCollector, true, true, runParallel: true);
            Player player1 = new Player("AlphaBetaSearch", () => absGameClient); 
            Player player2 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            var gameClient = player1.CreateGameClient.Invoke();
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    ".o..." +
                    "o.koo";
            IEnumerable<Card> cards = new List<Card>() { Card.Boar, Card.Ox, Card.Tiger, Card.Dragon, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            Turn turn = gameClient.GetTurn(new Game(player1, player2, GameUtil.ParseGameState(b, cards, 0, cardNumbers)));
            Console.Out.WriteLine($"Score: {absGameClient.GameResult:0.000000}");
            Console.Out.WriteLine(AlphaBetaSearch.GetStats());
            Console.Out.WriteLine(statsCollector.GetReport());
        }

        static void RunSingleTurn2() {
            var statsCollector = new GameClientStatsCollector();
            var evaluator = new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0);
            AlphaBetaSearchGameClient absGameClient = new AlphaBetaSearchGameClient(evaluator, 7, statsCollector, true, true, runParallel: true);
            Player player1 = new Player("AlphaBetaSearch", () => absGameClient);
            Player player2 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            var gameClient = player1.CreateGameClient.Invoke();
            var b = "OOKO." +
                    "...O." +
                    "....." +
                    ".oo.." +
                    "o.k.o";
            IEnumerable<Card> cards = new List<Card>() { Card.Tiger, Card.Dragon, Card.Boar, Card.Elephant, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            Turn turn = gameClient.GetTurn(new Game(player1, player2, GameUtil.ParseGameState(b, cards, 1, cardNumbers)));
            Console.Out.WriteLine($"Score: {absGameClient.GameResult:0.000000}");
            Console.Out.WriteLine(AlphaBetaSearch.GetStats());
            Console.Out.WriteLine(statsCollector.GetReport());
        }

        static void RunSingleTurn3() {
            var statsCollector = new GameClientStatsCollector();
            var evaluator = new Evaluator(0.876, 0.000, 0.137, 0.625, 1.000, 0.289, 0.696, 0.000, 0.522);
            AlphaBetaSearchGameClient absGameClient = new AlphaBetaSearchGameClient(evaluator, 7, statsCollector, true, true, runParallel: true);
            Player player = new Player("AlphaBetaSearch", () => absGameClient);
            var gameClient = player.CreateGameClient.Invoke();
            var b = "....." +
                    ".Ko.." +
                    ".o..." +
                    "....." +
                    "..k..";
            IEnumerable<Card> cards = new List<Card>() { Card.Elephant, Card.Monkey, Card.Cobra, Card.Frog, Card.Horse };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            Game game = new Game(player, null, GameUtil.ParseGameState(b, cards, 0, cardNumbers));
            Turn turn = gameClient.GetTurn(game);
            game.PlayTurn(turn);
            Console.Out.WriteLine($"Score: {absGameClient.GameResult:0.000000}");
            Console.Out.WriteLine(AlphaBetaSearch.GetStats());
            Console.Out.WriteLine(statsCollector.GetReport());
            Console.Out.WriteLine(turn);
            Console.Out.WriteLine(game.ToString());
        }

        static void RunSingleTurn4() {
            var statsCollector = new GameClientStatsCollector();
            var evaluator = new Evaluator(1.000, 0.000, 0.043, 0.829, 0.654, 0.082, 0.797, 0.000, 0.437);
            AlphaBetaSearchGameClient absGameClient = new AlphaBetaSearchGameClient(evaluator, 6, statsCollector, true, true, runParallel: true);
            Player player = new Player("AlphaBetaSearch", () => absGameClient);
            var gameClient = player.CreateGameClient.Invoke();
            var b = ".O.OO" +
                    "k...K" +
                    "..o.." +
                    "....." +
                    ".o..o";
            IEnumerable<Card> cards = new List<Card>() { Card.Dragon, Card.Rooster, Card.Goose, Card.Frog, Card.Mantis };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            Game game = new Game(player, null, GameUtil.ParseGameState(b, cards, 0, cardNumbers));
            Turn turn = gameClient.GetTurn(game);
            game.PlayTurn(turn);
            Console.Out.WriteLine($"Score: {absGameClient.GameResult:0.000000}");
            Console.Out.WriteLine(AlphaBetaSearch.GetStats());
            Console.Out.WriteLine(statsCollector.GetReport());
            Console.Out.WriteLine(turn);
            Console.Out.WriteLine(game.ToString());
        }

        static void RunSingleTurn5() {
            var statsCollector = new GameClientStatsCollector();
            var evaluator = new Evaluator(0.353, 0.000, 0.030, 1.000, 0.406, 0.111, 0.103, 0.000, 0.389);
            AlphaBetaSearchGameClient absGameClient = new AlphaBetaSearchGameClient(evaluator, 7, statsCollector, true, true, runParallel: false);
            Player player = new Player("AlphaBetaSearch", () => absGameClient);
            var gameClient = player.CreateGameClient.Invoke();
            var b = "...O." +
                    ".k..." +
                    "...K." +
                    "....." +
                    "o...o";
            IEnumerable<Card> cards = new List<Card>() { Card.Eel, Card.Boar, Card.Crane, Card.Rabbit, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            Game game = new Game(player, null, GameUtil.ParseGameState(b, cards, 0, cardNumbers));
            Turn turn = gameClient.GetTurn(game);
            game.PlayTurn(turn);
            Console.Out.WriteLine($"Score: {absGameClient.GameResult:0.000000}");
            Console.Out.WriteLine(AlphaBetaSearch.GetStats());
            Console.Out.WriteLine(statsCollector.GetReport());
            Console.Out.WriteLine(turn);
            Console.Out.WriteLine(game.ToString());
        }

        static void RunSingleGame() {
            var statsCollector1 = new GameClientStatsCollector();
            var statsCollector2 = new GameClientStatsCollector();
            Player player1 = new Player("MoveForward", () => new MoveForwardGameClient());
            //Player player4 = new Player("AlphaBetaSearch2", () => new AlphaBetaSearchGameClient(new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0), 7, statsCollector));
            Player player3 = new Player("AlphaBetaSearch2", () => new AlphaBetaSearchGameClient(new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0), 5, statsCollector1, runParallel: false));
            Player player4 = new Player("AlphaBetaSearch2", () => new AlphaBetaSearchGameClient(new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0), 5, statsCollector2, runParallel: false));
            var singleGameServer = new SingleGameServer(player3, player4);
            singleGameServer.Run();
            Console.Out.WriteLine(statsCollector1.GetReport());
            Console.Out.WriteLine(statsCollector2.GetReport());
        }

        static void RunMultiGame() {
            Player player1 = new Player("MovePawnsToBase", () => new MovePawnsToBaseGameClient());
            Player player2 = new Player("MovePawnsToKing", () => new MovePawnsToKingGameClient());
            Player player3 = new Player("MoveForward1", () => new MoveForwardGameClient());
            Player player4 = new Player("AlphaBetaSearch1", () => new AlphaBetaSearchGameClient(new Evaluator(0.654, 1.000, 0.125, 0.674, 0.241, 0.346, 0.867, 0.554, 0.338), 6));
            Player player5 = new Player("AlphaBetaSearch2", () => new AlphaBetaSearchGameClient(new Evaluator(1.000, 0.000, 0.043, 0.829, 0.654, 0.082, 0.797, 0.000, 0.437), 6));
            var multiGameServer = new MultiGameServer(player4, player5, 20, 50, true);
            multiGameServer.Run();
        }

        static void RunTournament() {
            List<Player> players = new List<Player>();
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.80, 0.64, 0.29, 0.20, 0.38, 0.30, 0.5, 0.5, 0.5), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(1.000, 0.548, 0.258, 0.121, 0.348, 0.049, 0.5, 0.5, 0.5), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(1.000, 0.970, 0.258, 0.121, 0.348, 0.353, 0.758, 0.5, 0.5), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.353, 0.000, 0.030, 1.000, 0.406, 0.111, 0.103, 0.000, 0.389), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.318, 0.0, 0.008, 1.000, 0.643, 0.133, 0.583, 0.0, 0.296), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.318, 0.000, 0.008, 1.000, 0.643, 0.133, 0.583, 0.000, 0.296), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.562, 0.0, 0.006, 0.516, 0.188, 0.328, 1.000, 0.0, 0.000), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.717, 0.0, 0.026, 0.966, 0.881, 0.430, 1.000, 0.0, 0.000), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.753, 0.0, 0.029, 0.801, 1.000, 0.574, 0.962, 0.0, 0.000), 3)));

            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.544, 0.038, 0.004, 1.000, 0.411, 0.072, 0.394, 0.000, 0.120), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.654, 1.000, 0.125, 0.674, 0.241, 0.346, 0.867, 0.554, 0.338), 3)));
            players.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(1.000, 0.000, 0.043, 0.829, 0.654, 0.082, 0.797, 0.000, 0.437), 3)));

            var tournamentServer = new TournamentServer(players, 50, 100, new FixedCardDeckGenerator());
            tournamentServer.Run();
            tournamentServer.WriteResult();
        }

        static void RunOneOnOneGeneticSelector() {
            Evaluator initialEvaluator = null; // new Evaluator(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0); ;
            Evaluator champion = GeneticGameClientSelector.GetBestEvaluator(50, 10, 3, initialEvaluator, 50);
            Console.Out.WriteLine($"Last champion: {champion}");
        }

        static void RunTournamentGeneticSelector() {
            var populationFitnessJudge = new TournamentPopulationFitnessJudge(10, 50);
            List<Evaluator> initialEvaluators = new List<Evaluator>() { new Evaluator(0.66, 0.64, 0.17, 0.08, 0.23, 0.31, 0.5, 0.5, 0.5) };
            new GameClientEvolutionRunner(20, 6, 2, populationFitnessJudge, initialEvaluators).Run();
        }

        static void RunFixedOpponentGeneticSelector() {
            List<Player> opponents = new List<Player>();
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.80, 0.64, 0.29, 0.20, 0.38, 0.30, 0.5, 0.5, 0.5),    3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(1.000, 0.548, 0.258, 0.121, 0.348, 0.049, 0.5, 0.5, 0.5), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(1.000, 0.970, 0.258, 0.121, 0.348, 0.353, 0.758, 0.5, 0.5), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.353, 0.000, 0.030, 1.000, 0.406, 0.111, 0.103, 0.000, 0.389), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.318, 0.0, 0.008, 1.000, 0.643, 0.133, 0.583, 0.0, 0.296), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.318, 0.000, 0.008, 1.000, 0.643, 0.133, 0.583, 0.000, 0.296), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.562, 0.0, 0.006, 0.516, 0.188, 0.328, 1.000, 0.0, 0.000), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.717, 0.0, 0.026, 0.966, 0.881, 0.430, 1.000, 0.0, 0.000), 3)));
            opponents.Add(new Player("AlphaBetaSearch", () => new AlphaBetaSearchGameClient(new Evaluator(0.753, 0.0, 0.029, 0.801, 1.000, 0.574, 0.962, 0.0, 0.000), 3)));
            var populationFitnessJudge = new FixedOpponentsPopulationFitnessJudge(opponents, 20, 50, new FixedCardDeckGenerator());

            List<Evaluator> initialEvaluators = new List<Evaluator>() {
                new Evaluator(1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00),
                new Evaluator(0.80, 0.64, 0.29, 0.20, 0.38, 0.30, 0.5, 0.5, 0.5),
                new Evaluator(1.000, 0.548, 0.258, 0.121, 0.348, 0.049, 0.5, 0.5, 0.5),
                new Evaluator(1.000, 0.970, 0.258, 0.121, 0.348, 0.353, 0.758, 0.5, 0.5),
                new Evaluator(0.353, 0.000, 0.030, 1.000, 0.406, 0.111, 0.103, 0.000, 0.389),
                new Evaluator(0.318, 0.0, 0.008, 1.000, 0.643, 0.133, 0.583, 0.0, 0.296),
                new Evaluator(0.318, 0.000, 0.008, 1.000, 0.643, 0.133, 0.583, 0.000, 0.296),
                new Evaluator(0.562, 0.0, 0.006, 0.516, 0.188, 0.328, 1.000, 0.0, 0.000),
                new Evaluator(0.717, 0.0, 0.026, 0.966, 0.881, 0.430, 1.000, 0.0, 0.000),
                new Evaluator(0.753, 0.0, 0.029, 0.801, 1.000, 0.574, 0.962, 0.0, 0.000),
            };
            new GameClientEvolutionRunner(1000, 20, 3, populationFitnessJudge, initialEvaluators).Run();
        }

    }
}
