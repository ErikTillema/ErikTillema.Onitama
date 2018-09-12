using System;
using System.Collections.Generic;
using System.Text;
using ErikTillema.Onitama.Domain;

namespace ErikTillema.Onitama.GameRunner {

    public class Individual {

        public Evaluator Evaluator { get; }
        public Player Player { get; }
        public int Index { get; }

        public Individual(int i, Evaluator evaluator, int maxMoves, Individual parent) {
            Index = i;
            Evaluator = evaluator;
            String s = parent == null ? "random" : $"child of {parent.Index}";
            Player = new Player($"Player {i:000} {s,12} - {evaluator}", () => new AlphaBetaSearchGameClient(evaluator, maxMoves));
        }

    }

}
