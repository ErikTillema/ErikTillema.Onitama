using System;
using System.Collections.Generic;
using System.Linq;
using ErikTillema.Collections;
using ErikTillema.Onitama.Domain;

namespace ErikTillema.Onitama.GameRunner {

    public class GameClientEvolutionRunner {

        public int EvolveCount { get; }
        public int IndividualCount { get; }
        public int GameCount { get; }
        public int AbsMaxMoves { get; }
        public List<Evaluator> InitialEvaluators { get; }
        public Population Population { get; private set; }
        public PopulationFitnessJudge PopulationFitnessJudge { get; }

        private int NextPlayerIndex;

        public GameClientEvolutionRunner(int evolveCount, int individualCount, int absMaxMoves, PopulationFitnessJudge populationFitnessJudge, IEnumerable<Evaluator> initialEvaluators = null) {
            EvolveCount = evolveCount;
            IndividualCount = individualCount;
            AbsMaxMoves = absMaxMoves;
            PopulationFitnessJudge = populationFitnessJudge;
            InitialEvaluators = initialEvaluators.ToList();
            NextPlayerIndex = 1;
        }

        public void Run() {
            Init();
            for (int i = 0; i < EvolveCount; i++) {
                Evolve();
                PopulationFitnessJudge.WriteResults();
            }
        }

        private void Init() {
            Population = new Population();
            for (int i = 0; i < IndividualCount; i++) {
                Evaluator evaluator = i < InitialEvaluators.Count ? InitialEvaluators[i] : Evaluator.GetRandomEvaluator(false);
                Population.AddIndividual(CreateIndividual(evaluator, null));
            }
            PopulationFitnessJudge.Population = Population;
        }

        /// <summary>
        /// Removes losers and adds random challengers and children of winners.
        /// Children of winners: for example, randomly choose one or two among the winners, create a child.
        /// Create a child by for example:
        /// - take 1 parent, change some value(s) randomly, keep the rest
        /// - take some values from one parent, some values from another parent, some random values, some average values.
        /// </summary>
        private void Evolve() {
            int randomChallengerCount = 2;
            int childrenOfWinnersCount = 10;
            int loserCount = randomChallengerCount + childrenOfWinnersCount;
            int winnersToCreateChildrenFromCount = 8;
            if (loserCount + winnersToCreateChildrenFromCount != IndividualCount)
                throw new InvalidOperationException("winnersToCreateChildrenFromCount + loserCount should equal IndividualCount, so the population remains equal in size over time.");

            List<Individual> losers = Population.Individuals.OrderBy(individual => GetFitness(individual)).Take(loserCount).ToList();
            List<Individual> winners = Population.Individuals.OrderByDescending(individual => GetFitness(individual)).Take(winnersToCreateChildrenFromCount).ToList();

            List<Individual> randomChallengers = Enumerable.Range(1, randomChallengerCount).Select(_ => CreateIndividual(Evaluator.GetRandomEvaluator(false), null)).ToList();
            List<Individual> childrenOfWinners = Enumerable.Range(1, childrenOfWinnersCount).Select(_ => CreateIndividual(winners)).ToList();
            foreach (var loser in losers) {
                Population.RemoveIndividual(loser);
            }
            foreach (var challenger in randomChallengers.Union(childrenOfWinners)) {
                Population.AddIndividual(challenger);
            }
        }

        public Individual CreateIndividual(List<Individual> winners) {
            int index = (int)RandomExt.NextLong(winners.Count);
            Individual winner = winners[index];
            return CreateIndividual(Evaluator.GetRandomlyEvolvedEvaluator(winner.Evaluator, true), winner);
        }

        public Individual CreateIndividual(Evaluator evaluator, Individual parent) {
            var result = new Individual(NextPlayerIndex, evaluator, AbsMaxMoves, parent);
            NextPlayerIndex++;
            return result;
        }

        public double GetFitness(Individual individual) {
            return PopulationFitnessJudge.GetFitness(individual);
        }
    
    }
}
