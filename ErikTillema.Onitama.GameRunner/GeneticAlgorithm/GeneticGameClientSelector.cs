using System;
using ErikTillema.Onitama.Domain;

namespace ErikTillema.Onitama.GameRunner {

    public class GeneticGameClientSelector {

        public static Evaluator GetBestEvaluator(int challengeCount, int gameCount, int maxMoves, Evaluator initialEvaluator = null, int? maxGameTurns = null) {
            if (initialEvaluator == null) initialEvaluator = Evaluator.GetRandomEvaluator();
            Evaluator champion = initialEvaluator;
            for (int i=0; i < challengeCount; i++) {
                Evaluator challenger = Evaluator.GetRandomEvaluator();

                Player player1 = new Player("Champion", () => new AlphaBetaSearchGameClient(champion, maxMoves));
                Player player2 = new Player("Challenger", () => new AlphaBetaSearchGameClient(challenger, maxMoves));
                var multiGameServer = new MultiGameServer(player1, player2, gameCount, maxGameTurns);
                multiGameServer.Run();
                if (multiGameServer.WinsPlayer2 > multiGameServer.WinsPlayer1) {
                    champion = challenger;
                    Console.Out.WriteLine($"We have a new champion: {champion}");
                } else {
                    Console.Out.WriteLine($"Current champion won");
                }
            }
            return champion;
        }

    }

}
