using System;
using System.Collections.Generic;
using System.Linq;

namespace ErikTillema.Onitama.Domain {

    public class Evaluator : IEvaluator {

        public IReadOnlyList<double> Alphas => _Alphas;
        private List<double> _Alphas;
        private static int AlphaCount = 9;
        private static int[] expensiveAlphas = new int[] { }; // @@@ { 1, 7 };

        public double AlphaMaterial => Alphas[0];
        public double AlphaMobility => Alphas[1];
        public double AlphaExposure => Alphas[2];
        public double AlphaKingSafety => Alphas[3];
        public double AlphaBaseSafety => Alphas[4];

        // Evaluate cards: some cards I think have higher value than others.
        // Own cards and evaluate opponent cards
        // Take also middle card into account: better if a good card is in the middle and it's my turn (so I will receive that card) than when there's a bad card in the middle.
        // We could do like this: MyCardStrength = own cards + 0.75 of middle card IF it's my turn
        //                        OpponentCardStrength = their cards + 0.75 of middle card IF it's their turn
        public double AlphaCardStrength => Alphas[5];

        public double AlphaCenterControl => Alphas[6];
        public double AlphaCenterCoverage => Alphas[7];

        public double AlphaKingMobility => Alphas[8];

        // On second thought, this is pretty useless since we can't influence this. So all max/min nodes will have the same penalty/bonus for this.
        // And we search in Alpha Beta search always until a fixed depth (or leave nodes of course, so WINNING/LOSING)
        // All in all, as longs as we compare min-nodes to min-nodes and max-nodes to max-nodes (which I think we always should), then this Evaluator contributes nothing.
        //public double AlphaInitiative { get; } // is it my turn?

        public Evaluator(double alphaMaterial, double alphaMobility, double alphaExposure, double alphaKingSafety, double alphaBaseSafety, double alphaCardStrength, double alphaCenterControl, double alphaCenterCoverage, double alphaKingMobility) {
            List<double> alphas = new List<double>();
            alphas.Add(alphaMaterial);
            alphas.Add(alphaMobility);
            alphas.Add(alphaExposure);
            alphas.Add(alphaKingSafety);
            alphas.Add(alphaBaseSafety);
            alphas.Add(alphaCardStrength);
            alphas.Add(alphaCenterControl);
            alphas.Add(alphaCenterCoverage);
            alphas.Add(alphaKingMobility);
            Init(alphas);
        }

        public Evaluator(IEnumerable<double> alphas) {
            Init(alphas);
        }

        private void Init(IEnumerable<double> alphas) {
            _Alphas = alphas.ToList();
            if (_Alphas.Count != AlphaCount) throw new ArgumentException($"alphas should contain {AlphaCount} values");
            double maxValue = _Alphas.Max();
            // Probably it's better to re-normalize every evaluator to have max value 1.0, in order to make genetic cross-producing more easy.
            double multiplier = 1.0 / maxValue;
            for (int i = 0; i < _Alphas.Count; i++) {
                _Alphas[i] *= multiplier;
            }
        }

        public double Evaluate(Game game, int inViewPlayerIndex) {
            return 
                  AlphaMaterial * Evaluation.EvaluateMaterial(game, inViewPlayerIndex)
                + (AlphaMobility == 0.0 
                    ? 0.0
                    : AlphaMobility * Evaluation.EvaluateMobility(game, inViewPlayerIndex))
                + AlphaExposure * Evaluation.EvaluateExposure(game, inViewPlayerIndex)
                + AlphaKingSafety * Evaluation.EvaluateKingSafety(game, inViewPlayerIndex)
                + AlphaBaseSafety * Evaluation.EvaluateBaseSafety(game, inViewPlayerIndex)
                + AlphaCardStrength * Evaluation.EvaluateCardStrength(game, inViewPlayerIndex)
                + AlphaCenterControl * Evaluation.EvaluateCenterControl(game, inViewPlayerIndex)
                + (AlphaCenterCoverage == 0.0
                    ? 0.0
                    : AlphaCenterCoverage * Evaluation.EvaluateCenterCoverage(game, inViewPlayerIndex))
                + AlphaKingMobility * Evaluation.EvaluateKingMobility(game, inViewPlayerIndex)
                ;
        }

        public bool IsSimilarTo(Evaluator e) {
            for(int i=0; i < _Alphas.Count; i++) {
                if (!IsApproximately(this.Alphas[i], e.Alphas[i]))
                    return false;
            }
            return true;
        }

        private static bool IsApproximately(double a, double b) {
            //double absoluteDifference = Math.Abs(a - b);
            double relativeDifference = GetRelativeDifference(a, b);
            return relativeDifference < 0.08; // && absoluteDifference < 0.10
        }

        /// <summary>
        /// Returns the ratio between the difference a and b and the max of a and b.
        /// Returns a value in the interval [0,1]
        /// Returns 0 when a and b are equal, returns 1 if a or b is zero and the other is not.
        /// For example, if a=2 and b=5, then this method will return 0.6
        /// If a=3 and b=0, then this method will return 1.0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double GetRelativeDifference(double a, double b) {
            if (a == 0 && b == 0) return 0.0;
            else return Math.Abs(b-a)/Math.Max(a,b);
        }

        public override string ToString() {
            return $"({AlphaMaterial:0.000}, {AlphaMobility:0.000}, {AlphaExposure:0.000}, {AlphaKingSafety:0.000}, {AlphaBaseSafety:0.000}, {AlphaCardStrength:0.000}, {AlphaCenterControl:0.000}, {AlphaCenterCoverage:0.000}, {AlphaKingMobility:0.000})";
        }

        private static Random random = new Random();

        public static Evaluator GetRandomEvaluator(bool excludeExpensive = false) {
            return new Evaluator(Enumerable.Range(0, AlphaCount).Select(i => {
                if (excludeExpensive && expensiveAlphas.Contains(i)) return 0.0;
                else return random.NextDouble();
            }));
        }
        
        /// <summary>
        /// Randomly replace one alpha value with a random value
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Evaluator GetRandomlyEvolvedEvaluator(Evaluator parent, bool excludeExpensive = false) {
            int index1 = GetRandomAlphaIndex(excludeExpensive);
            int index2 = GetRandomAlphaIndex(excludeExpensive);
            return new Evaluator(Enumerable.Range(0, AlphaCount).Select(i => {
                if (i == index1 || i == index2) return GetRandomAndUnsimilar(parent.Alphas[i]);
                else return parent.Alphas[i];
            }));
        }

        private static int GetRandomAlphaIndex(bool excludeExpensive) {
            while(true) {
                int result = (int)RandomExt.NextLong(AlphaCount);
                if (excludeExpensive && expensiveAlphas.Contains(result)) continue;
                return result;
            }
        }

        private static double GetRandomAndUnsimilar(double currentValue) {
            while(true) {
                // if currentValue is 0.99, then a new value of 1.2 makes sense (it's like decreasing all other values).
                // so allow all values, including high values to be doubled.
                // The normalization of the Evaluator will fix things.
                double maxNewValue = Math.Max(1.0, 2.0 * currentValue);
                double result = random.NextDouble() * maxNewValue; 
                if (!IsApproximately(result, currentValue)) return result;
            }
        }

        public static Evaluator GetAverageEvaluator(IEnumerable<Evaluator> evaluators) {
            return new Evaluator(Enumerable.Range(0, AlphaCount).Select(i => evaluators.Average(e => e.Alphas[i])));
        }

        public static bool AllSimilar(IEnumerable<Evaluator> evaluators) {
            evaluators = evaluators.ToList();
            Evaluator first = evaluators.First();
            return evaluators.Skip(1).All(e => first.IsSimilarTo(e));
        }

    }

    public static class Evaluation {

        /// <summary>
        /// Original range [-4, 4]
        /// How much material do we have.
        /// </summary>
        public static double EvaluateMaterial(Game game, int inViewPlayerIndex) {
            int cnt1 = game.GameState.PlayerPieces[inViewPlayerIndex].Count(p => !p.IsCaptured);
            int cnt2 = game.GameState.PlayerPieces[1 - inViewPlayerIndex].Count(p => !p.IsCaptured);
            return (cnt1 - cnt2)/4.0;
        }

        /// <summary>
        /// Original range [-40, 40]
        /// How many valid moves can we make.
        /// Expensive!
        /// </summary>
        public static double EvaluateMobility(Game game, int inViewPlayerIndex) {
            int moves1 = game.GetValidTurnCount(inViewPlayerIndex); // expensive: we could write a count method without the yields, but would that make a big difference? No: the same method but without yields has the same performance.
            int moves2 = game.GetValidTurnCount(1 - inViewPlayerIndex);
            return (moves1 - moves2)/40.0;
        }

        /// <summary>
        /// Range [-1, 1]
        /// Whether or not the King is on his base, so how many spots are exposed.
        /// </summary>
        public static double EvaluateExposure(Game game, int inViewPlayerIndex) {
            int exposure1 = game.GameState.PlayerPieces[inViewPlayerIndex][0].Position.Equals(Board.PlayerBases[inViewPlayerIndex]) ? 1 : 2;
            int exposure2 = game.GameState.PlayerPieces[1 - inViewPlayerIndex][0].Position.Equals(Board.PlayerBases[1 - inViewPlayerIndex]) ? 1 : 2;
            return -1*(exposure1 - exposure2); // *-1, because lower exposure is good.
        }

        /// <summary>
        /// Original range [-16, 16] (19 is not possible, 16 is max)
        /// How far is the enemy from our King. We have to take into account that many enemy pieces close to my King is worse than only a few pieces close to my King.
        /// @@@ this function seems to evaluate 2 things: my king safety but also the enemies king safety. But maybe we want to evaluate those independently.
        /// </summary>
        public static double EvaluateKingSafety(Game game, int inViewPlayerIndex) {
            int distanceFromEnemyToKing1 = GetDistanceFromEnemyToKing(game, inViewPlayerIndex);
            int distanceFromEnemyToKing2 = GetDistanceFromEnemyToKing(game, 1 - inViewPlayerIndex);
            return (distanceFromEnemyToKing1 - distanceFromEnemyToKing2)/16.0;
        }

        // 1 enemy with dist 2 is better than 1 enemy   with dist 1, so higher distance => higher score
        // 1 enemy with dist 1 is better than 2 enemies with dist 1, so don't sum over positive values. Sum over negative values:
        // .-1 * x = 
        // .-2 * x = 
        // .-3 * x = 
        // .-4 * x = 
        // ..K..
        // So score = sum over (dist - 5)
        // Range [-20, -1] + 20 => [0, 19]
        private static int GetDistanceFromEnemyToKing(Game game, int playerIndex) {
            Vector positionKing = game.GameState.PlayerPieces[playerIndex][0].Position;
            return game.GameState.PlayerPieces[1 - playerIndex].Where(p => !p.IsCaptured)
                        .Sum(p => Board.GetDistance(p.Position, positionKing) - 5)
                        + 20;
        }

        /// <summary>
        /// Original range [-3, 3]
        /// How far is the enemy King from our base.
        /// </summary>
        public static double EvaluateBaseSafety(Game game, int inViewPlayerIndex) {
            int distanceFromEnemyKingToBase1 = GetDistanceFromEnemyKingToBase(game, inViewPlayerIndex);
            int distanceFromEnemyKingToBase2 = GetDistanceFromEnemyKingToBase(game, 1 - inViewPlayerIndex);
            return (distanceFromEnemyKingToBase1 - distanceFromEnemyKingToBase2) / 3.0;
        }

        private static int GetDistanceFromEnemyKingToBase(Game game, int playerIndex) {
            return Board.GetDistance(game.GameState.PlayerPieces[1 - playerIndex][0].Position, Board.PlayerBases[playerIndex]);
        }

        /// <summary>
        /// Original range [-1, 1]
        /// Is it our turn, so do we have the initiative.
        /// </summary>
        public static double EvaluateInitiative(Game game, int inViewPlayerIndex) { 
            return inViewPlayerIndex == game.GameState.InTurnPlayerIndex ? 1 : -1 ;
        }

        /// <summary>
        /// Original range [-23.5, 23.5]
        /// What is the strength of the cards in our hand, taking also the middle card into account.
        /// </summary>
        public static double EvaluateCardStrength(Game game, int inViewPlayerIndex) {
            IList<IEnumerable<Card>> playerCards = game.GameState.PlayerCards.ToList();
            IEnumerable<Card> myCards = playerCards[inViewPlayerIndex];
            IEnumerable<Card> opponentCards = playerCards[1 - inViewPlayerIndex];

            double myCardsScore = GetCardsScore(myCards, game, inViewPlayerIndex);
            double opponentCardsScore = GetCardsScore(opponentCards, game, 1 - inViewPlayerIndex);
            return (myCardsScore - opponentCardsScore)/23.5;
        }

        private static double GetCardsScore(IEnumerable<Card> cards, Game game, int playerIndex) {
            double heldCardsScore = cards.Select(card => CardStrength[card]).Sum();
            double middleCardsScore = game.GameState.InTurnPlayerIndex == playerIndex 
                                        ? 0.75 * CardStrength[game.GameState.MiddleCard] 
                                        : 0.0;
            return heldCardsScore + middleCardsScore;
        }

        private static Dictionary<Card, double> CardStrength = new Dictionary<Card, double>() {
            { Card.Tiger, 15 },
            { Card.Dragon, 10 },

            { Card.Crab, 6 },

            { Card.Frog, 5 },
            { Card.Rabbit, 5 },
            { Card.Elephant, 5 },

            { Card.Monkey, 4 },
            { Card.Mantis, 4 },

            { Card.Goose, 3 },
            { Card.Rooster, 3 },
            { Card.Boar, 3 },

            { Card.Eel, 3 },
            { Card.Cobra, 3 },
            { Card.Horse, 3 },
            { Card.Ox, 3 },
            { Card.Crane, 3 },
        };

        /// <summary>
        /// Original range [-5, 5]
        /// Look at center middle 9 squares, how many pieces do we have there in the center.
        /// @@@ center control seems very important. Can we dig in deeper in this concept? Maybe split into inner center (cross, 5 spots) and outer center?
        /// Let's first see that indeed this value is the most important.
        /// </summary>
        public static double EvaluateCenterControl(Game game, int inViewPlayerIndex) {
            int countPiecesInCenter1 = game.GameState.PlayerPieces[inViewPlayerIndex].Where(p => !p.IsCaptured).Count(p => p.Position.IsInCenter()); //0-5
            int countPiecesInCenter2 = game.GameState.PlayerPieces[1 - inViewPlayerIndex].Where(p => !p.IsCaptured).Count(p => p.Position.IsInCenter());
            return (countPiecesInCenter1 - countPiecesInCenter2) / 5.0;
        }

        /// <summary>
        /// Original range [-20, 20]
        /// Look at center middle 9 squares, how many moves do we have that end up in the center.
        /// Expensive!
        /// </summary>
        public static double EvaluateCenterCoverage(Game game, int inViewPlayerIndex) {
            int movesToCenter1 = game.GetValidTurns(inViewPlayerIndex).Count(t => t.OriginalPosition.Add(t.Move).IsInCenter()); //0-20 more or less
            int movesToCenter2 = game.GetValidTurns(1 - inViewPlayerIndex).Count(t => t.OriginalPosition.Add(t.Move).IsInCenter());
            return (movesToCenter1 - movesToCenter2) / 20.0;
        }

        private static bool IsInCenter(this Vector v) {
            return 1 <= v.X && v.X <= 3 && 1 <= v.Y && v.Y <= 3;
        }

        /// <summary>
        /// Original range [-8, 8]
        /// Represents the mobility of the Kings, how many spots the King can move to. A low number means the King has no escape
        /// routes and can more easily be locked in and captured.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="inViewPlayerIndex"></param>
        /// <returns></returns>
        public static double EvaluateKingMobility(Game game, int inViewPlayerIndex) {
            IList<IEnumerable<Card>> playerCards = game.GameState.PlayerCards.ToList();
            IEnumerable<Card> myCards = playerCards[inViewPlayerIndex];
            IEnumerable<Card> opponentCards = playerCards[1 - inViewPlayerIndex];
            int myKingMoves = GetKingMoves(myCards, game, inViewPlayerIndex);
            int opponentKingMoves = GetKingMoves(opponentCards, game, 1 - inViewPlayerIndex);
            return (myKingMoves - opponentKingMoves) / 8.0;
        }

        // Range [0-8], for example king in middle + Dragon & Elephant.
        private static int GetKingMoves(IEnumerable<Card> cards, Game game, int playerIndex) {
            Vector kingPosition = game.GameState.PlayerPieces[playerIndex][0].Position;
            int result = cards.SelectMany(c => c.GetMoves(playerIndex))
                        .Distinct()
                        .Select(v => kingPosition.Add(v))
                        .Where(pos => Board.IsWithinBounds(pos))
                        .Where(pos => game.GameState.Board[pos.X, pos.Y] == null || game.GameState.Board[pos.X, pos.Y].PlayerIndex != playerIndex)
                        .Count();
            return result;
        }

    }

}
