using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Onitama.Domain;

namespace ErikTillema.Onitama.GameRunner {

    public class GeneticGameClientSelector {

        public static Evaluator GetRandomEvaluator() {
            Random random = new Random();
            double alphaMaterial = random.NextDouble();
            double alphaMobility = random.NextDouble();
            double alphaExposure = -random.NextDouble();
            double alphaKingSafety = random.NextDouble();
            double alphaBaseSafety = random.NextDouble();
            return new Evaluator(alphaMaterial, alphaMobility, alphaExposure, alphaKingSafety, alphaBaseSafety);
        }

        public static Evaluator GetBestEvaluator(int challengeCount, int gameCount, int maxMoves, Evaluator initialEvaluator = null) {
            if (initialEvaluator == null) initialEvaluator = GetRandomEvaluator();
            Evaluator champion = initialEvaluator;
            for (int i=0; i < challengeCount; i++) {
                Evaluator challenger = GetRandomEvaluator();

                Player player1 = new Player("Champion", () => new AlphaBetaSearchGameClient(champion, maxMoves));
                Player player2 = new Player("Challenger", () => new AlphaBetaSearchGameClient(challenger, maxMoves));
                var multiGameRunner = new MultiGameRunner(player1, player2, gameCount);
                multiGameRunner.Run();
                if (multiGameRunner.WinsPlayer2 > multiGameRunner.WinsPlayer1) {
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
