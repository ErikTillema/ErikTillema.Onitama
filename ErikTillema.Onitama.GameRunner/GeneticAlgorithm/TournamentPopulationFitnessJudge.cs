using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ErikTillema.Onitama.Domain;

namespace ErikTillema.Onitama.GameRunner {

    public class TournamentPopulationFitnessJudge : PopulationFitnessJudge {

        public TournamentServer TournamentServer { get; }

        public TournamentPopulationFitnessJudge(int gameCount, int? maxGameTurns) {
            TournamentServer = new TournamentServer(new Player[0], gameCount, maxGameTurns);
        }

        private void UpdateTournament() {
            foreach(Player player in TournamentServer.Players.ToList()) {
                if (!Population.Individuals.Select(ind => ind.Player).Contains(player)) {
                    TournamentServer.RemovePlayer(player);
                }
            }
            foreach(Individual individual in Population.Individuals) {
                if (!TournamentServer.Players.Contains(individual.Player)) {
                    TournamentServer.AddPlayer(individual.Player);
                }
            }
        }

        public override double GetFitness(Individual individual) {
            UpdateTournament();
            return GetPlayerScore(TournamentServer.GetPlayerResults(individual.Player));
        }

        public static double GetPlayerScore(PlayerTournamentResults results) {
            return results.Wins + 0.5 * results.Draws;
        }

        public override void WriteResults() {
            TournamentServer.WriteResult();
        }

    }
}
