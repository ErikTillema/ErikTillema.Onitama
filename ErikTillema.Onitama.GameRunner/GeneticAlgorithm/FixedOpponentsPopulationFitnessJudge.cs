using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ErikTillema.Onitama.Domain;

namespace ErikTillema.Onitama.GameRunner {

    public class FixedOpponentsPopulationFitnessJudge : PopulationFitnessJudge {

        public int GameCount { get; }
        public int? MaxGameTurns { get; }

        public IReadOnlyList<Player> Opponents => _Opponents;
        private List<Player> _Opponents;

        private Dictionary<Individual, double> fitness;
        private readonly ICardDeckGenerator cardDeckGenerator;

        public FixedOpponentsPopulationFitnessJudge(IEnumerable<Player> opponents, int gameCount, int? maxGameTurns, ICardDeckGenerator cardDeckGenerator = null) {
            _Opponents = opponents.ToList();
            GameCount = gameCount;
            MaxGameTurns = maxGameTurns;
            this.cardDeckGenerator = cardDeckGenerator;
            fitness = new Dictionary<Individual, double>();
        }

        private double CalculateFitness(Individual individual) {
            int wins = 0;
            int draws = 0;
            foreach(Player opponent in Opponents) {
                MultiGameServer multiGameServer = new MultiGameServer(individual.Player, opponent, GameCount, MaxGameTurns, cardDeckGenerator: cardDeckGenerator);
                multiGameServer.Run();
                wins += multiGameServer.WinsPlayer1;
                draws += multiGameServer.Draws;
            }
            return GetPlayerScore(wins, draws);
        }

        public override double GetFitness(Individual individual) {
            if (!fitness.ContainsKey(individual)) fitness.Add(individual, CalculateFitness(individual));
            return fitness[individual];
        }

        public static double GetPlayerScore(int wins, int draws) {
            return wins + 0.5 * draws;
        }

        public override void WriteResults() {
            foreach(Individual individual in fitness.Keys) {
                if (Population.Individuals.Contains(individual)) {
                    Console.Out.WriteLine($"{individual.Player.Name,-20} {GetFitness(individual):00.0}");
                }
            }
            Console.Out.WriteLine();
        }

    }
}
