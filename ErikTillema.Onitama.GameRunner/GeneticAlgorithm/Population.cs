using System;
using System.Collections.Generic;
using System.Text;

namespace ErikTillema.Onitama.GameRunner {

    public class Population {

        public IReadOnlyList<Individual> Individuals => _Individuals;
        public List<Individual> _Individuals;

        public Population() {
            _Individuals = new List<Individual>();
        }

        public void AddIndividual(Individual individual) {
            _Individuals.Add(individual);
        }

        public void RemoveIndividual(Individual individual) {
            _Individuals.Remove(individual);
        }

    }

}
