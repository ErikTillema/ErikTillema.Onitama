using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class Evaluator : IEvaluator {

        public double AlphaMaterial { get; }
        public double AlphaMobility { get; }
        public double AlphaExposure { get; }
        public double AlphaKingSafety { get; }
        public double AlphaBaseSafety { get; }

        public Evaluator(double alphaMaterial, double alphaMobility, double alphaExposure, double alphaKingSafety, double alphaBaseSafety) {
            AlphaMaterial = alphaMaterial;
            AlphaMobility = alphaMobility;
            AlphaExposure = alphaExposure;
            AlphaKingSafety = alphaKingSafety;
            AlphaBaseSafety = alphaBaseSafety;
        }

        public double Evaluate(Game game, int originalInTurnPlayerIndex) {
            return AlphaMaterial * Evaluation.EvaluateMaterial(game, originalInTurnPlayerIndex)
                + AlphaMobility * Evaluation.EvaluateMobility(game, originalInTurnPlayerIndex)
                + AlphaExposure * Evaluation.EvaluateExposure(game, originalInTurnPlayerIndex)
                + AlphaKingSafety * Evaluation.EvaluateKingSafety(game, originalInTurnPlayerIndex)
                + AlphaBaseSafety * Evaluation.EvaluateBaseSafety(game, originalInTurnPlayerIndex)
                ;
        }

        public override string ToString() {
            return $"({AlphaMaterial}, {AlphaMobility}, {AlphaExposure}, {AlphaKingSafety}, {AlphaBaseSafety})";
        }

    }

    public static class Evaluation {

        public static double EvaluateMaterial(Game game, int originalInTurnPlayerIndex) {
            int cnt1 = game.GameState.PlayerPieces[originalInTurnPlayerIndex].Count(p => !p.IsCaptured);
            int cnt2 = game.GameState.PlayerPieces[1 - originalInTurnPlayerIndex].Count(p => !p.IsCaptured);
            return cnt1 - cnt2;
        }

        public static double EvaluateMobility(Game game, int originalInTurnPlayerIndex) {
            int moves1 = game.GetValidTurns(originalInTurnPlayerIndex).Count(); // @@@ expensive!
            int moves2 = game.GetValidTurns(1 - originalInTurnPlayerIndex).Count();
            return moves1 - moves2;
        }

        public static double EvaluateExposure(Game game, int originalInTurnPlayerIndex) {
            int exposure1 = game.GameState.PlayerPieces[originalInTurnPlayerIndex][0].Position.Equals(Board.PlayerBases[originalInTurnPlayerIndex]) ? 1 : 2;
            int exposure2 = game.GameState.PlayerPieces[1 - originalInTurnPlayerIndex][0].Position.Equals(Board.PlayerBases[1 - originalInTurnPlayerIndex]) ? 1 : 2;
            return exposure1 - exposure2; // @@@ *-1 ? now it's the other way round...
        }

        public static double EvaluateKingSafety(Game game, int originalInTurnPlayerIndex) {
            int distanceFromEnemyToKing1 = GetDistanceFromEnemyToKing(game, originalInTurnPlayerIndex);
            int distanceFromEnemyToKing2 = GetDistanceFromEnemyToKing(game, 1 - originalInTurnPlayerIndex);
            return distanceFromEnemyToKing1 - distanceFromEnemyToKing2;
        }

        private static int GetDistanceFromEnemyToKing(Game game, int playerIndex) {
            Vector positionKing = game.GameState.PlayerPieces[playerIndex][0].Position;
            return game.GameState.PlayerPieces[1 - playerIndex].Min(p => p.Position.ManhattanDistance(positionKing));
        }

        public static double EvaluateBaseSafety(Game game, int originalInTurnPlayerIndex) {
            int distanceFromEnemyToBase1 = GetDistanceFromEnemyToBase(game, originalInTurnPlayerIndex);
            int distanceFromEnemyToBase2 = GetDistanceFromEnemyToBase(game, 1 - originalInTurnPlayerIndex);
            return distanceFromEnemyToBase1 - distanceFromEnemyToBase2;
        }

        private static int GetDistanceFromEnemyToBase(Game game, int playerIndex) {
            return game.GameState.PlayerPieces[1 - playerIndex].Min(p => p.Position.ManhattanDistance(Board.PlayerBases[playerIndex]));
        }


    }

}
