using System;
using System.Collections.Generic;
using System.Text;

namespace ErikTillema.Onitama.GameRunner {

    public abstract class PopulationFitnessJudge {

        public Population Population { get; set; }

        public PopulationFitnessJudge() { }

        public abstract double GetFitness(Individual individual);

        public abstract void WriteResults();

    }
}
