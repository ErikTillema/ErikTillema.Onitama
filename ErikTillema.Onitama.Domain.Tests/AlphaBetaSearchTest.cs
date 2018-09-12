using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace ErikTillema.Onitama.Domain.Tests {

    public class AlphaBetaSearchTest {

        class IndifferentEvaluator : IEvaluator {
            public double Evaluate(Game game, int inViewPlayerIndex) {
                return 0;
            }
        }

        [Test]
        public void GetGameResult_Should_ReturnWinningWhenInDirectWinningPosition_South() {
            var b = "OOKOO"+
                    "....."+
                    "..o.." +
                    "....." +
                    "o.koo";
            var cards = new[] { Card.Boar, Card.Tiger, Card.Dragon, Card.Eel, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 1, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(AlphaBetaSearch.GameResultWinning);
            tup.Item2.Where(kvp => kvp.Value == AlphaBetaSearch.GameResultWinning).Select(kvp => kvp.Key).Should()
                .Contain(new Turn(game.InTurnPlayer, Card.Tiger, 1, PieceType.Pawn, new Vector(2,2), new Vector(0,2)));
        }

        [Test]
        public void GetGameResult_Should_ReturnWinningWhenInDirectWinningPosition_North() {
            var b = "OOK.O" +
                    "....." +
                    "..O.." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Boar, Card.Dragon, Card.Eel, Card.Tiger, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 1, cardNumbers);
            var tup = new AlphaBetaSearch(game, 1, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(AlphaBetaSearch.GameResultWinning);
            tup.Item2.Where(kvp => kvp.Value == AlphaBetaSearch.GameResultWinning).Select(kvp => kvp.Key).Should()
                .Contain(new Turn(game.InTurnPlayer, Card.Tiger, 1, PieceType.Pawn, new Vector(2, 2), new Vector(0, -2)));
        }

        [Test]
        public void GetGameResult_Should_ReturnUndecidedWhenNotInDirectWinningPosition() {
            var b = "OOK.O" +
                    "....." +
                    "..O.." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Boar, Card.Dragon, Card.Eel, Card.Rooster, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 1, cardNumbers);
            var tup = new AlphaBetaSearch(game, 1, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(0); // value from indifferent evaluator
        }

        [Test]
        public void GetGameResult_Should_ReturnLosingWhenOpponentInDirectWinningPosition() {
            var b = "OO.OO" +
                    "....." +
                    "..K.." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Boar, Card.Dragon, Card.Eel, Card.Tiger, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 2, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(AlphaBetaSearch.GameResultLosing);
        }

        [Test]
        public void GetGameResult_Should_ReturnUndecidedWhenOpponentNotInDirectWinningPosition() {
            var b = "OOK.O" +
                    "....." +
                    "..O.." +
                    ".o..." +
                    "o.koo";
            var cards = new[] { Card.Boar, Card.Elephant, Card.Eel, Card.Tiger, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 2, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(0); // undecided
            // After the new rule, there are in fact multiple moves to take that lead to undecided in 2.
            // due to the nature of the ABS algorithm, not all branches will be explored, so a contains() check won't work.
            // tup.Item2.Single(kvp => kvp.Value == 0).Key.Should().Be(new Turn(game.InTurnPlayer, Card.Elephant, 1, PieceType.Pawn, new Vector(1, 1), new Vector(1, 1)));
        }

        [Test]
        public void GetGameResult_Should_ReturnWinningWhenInTwoStepWinningPosition() {
            // winning in two steps: play Monkey, moving pawn to (2,2), then Tiger to capture base.
            var b = "OO.OO" +
                    "...K." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Boar, Card.Dragon, Card.Monkey, Card.Tiger, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 1, cardNumbers);
            var tup = new AlphaBetaSearch(game, 3, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(AlphaBetaSearch.GameResultWinning);
            tup.Item2.Single(kvp => kvp.Value == AlphaBetaSearch.GameResultWinning).Key.Should()
                .Be(new Turn(game.InTurnPlayer, Card.Monkey, 0, PieceType.Pawn, new Vector(3, 3), new Vector(-1, -1)));
        }

        [Test]
        public void GetGameResult_Should_ReturnLosingWhenOpponentInTwoStepWinningPosition() {
            // opponent winning in two steps: play Monkey, moving pawn to (2,2), then Tiger to capture base.
            var b = "OO.OO" +
                    "...K." +
                    "....." +
                    "....." +
                    "ok.oo";
            var cards = new[] { Card.Frog, Card.Rabbit, Card.Monkey, Card.Tiger, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 4, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(AlphaBetaSearch.GameResultLosing);
        }

        [Test]
        public void GetGameResult_Should_ReturnUndecidedWhenOpponentNotInTwoStepWinningPosition() {
            // defense is moving pawn or king to (2,2) in two steps,
            // for example through pawn (3,0) --FROG--> (2,1) --BOAR--> (2,2)
            // or through king (2,0) --FROG--> (1,1) --RABBIT--> (2,2)
            // The latter turns out to be the one chosen by the ABS algorithm after cutting off branches etc.
            var b = "OO.OO" +
                    "...K." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Frog, Card.Rabbit, Card.Monkey, Card.Tiger, Card.Boar };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 4, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(0); // undecided
            tup.Item2.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key).Should()
                //.Contain(new Turn(game.InTurnPlayer, Card.Frog, 0, PieceType.Pawn, new Vector(3, 0), new Vector(-1, 1)));
                .Contain(new Turn(game.InTurnPlayer, Card.Frog, 0, PieceType.King, new Vector(2, 0), new Vector(-1, 1)));
        }

        [Test]
        public void GetGameResult_Should_CapturePawn() {
            var b = "OOK.O" +
                    "....." +
                    "....." +
                    "o..O." +
                    ".okoo";
            var cards = new[] { Card.Frog, Card.Rabbit, Card.Monkey, Card.Tiger, Card.Boar };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 1, new Evaluator(1,0,0,0,0,0,0,0,0)).GetGameResult();
            tup.Item1.Should().Be(0.25); // +1/4 in material
        }

        [Test]
        public void GetGameResult_RegressionTest1() {
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    ".o..." +
                    "o.koo";
            var cards = new[] { Card.Boar, Card.Ox, Card.Tiger, Card.Dragon, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 5, new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0), true, true, true).GetGameResult();
            tup.Item1.Should().BeApproximately(0.128989, 0.000001);
        }

        [Test]
        public void GetGameResult_RegressionTest2() {
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    ".o..." +
                    "o.koo";
            var cards = new[] { Card.Boar, Card.Ox, Card.Tiger, Card.Dragon, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 6, new Evaluator(1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0), true, true, true).GetGameResult();
            tup.Item1.Should().BeApproximately(-0.192021, 0.000001);
        }

        [Test, Ignore("Deprecated: mirrored boards should not return the same value")]
        public void GetUniqueIdentifier_should_returnSameValueForMirroredBoards_1() {
            var b1 = ".OOKO" +
                     "O...." +
                     "....." +
                     "....." +
                     "ookoo";
            var b2 = "OKOO." +
                     "....O" +
                     "....." +
                     "....." +
                     "ookoo";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
        }

        [Test, Ignore("Deprecated: mirrored boards should not return the same value")]
        public void GetUniqueIdentifier_should_returnSameValueForMirroredBoards_2() {
            var b1 = ".O..." +
                     "OK..." +
                     "..o.." +
                     "....." +
                     "o..ko";
            var b2 = "...O." +
                     "...KO" +
                     "..o.." +
                     "....." +
                     "ok..o";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
        }

        [Test, Ignore("Deprecated: mirrored boards should not return the same value")]
        public void GetUniqueIdentifier_should_returnSameValueForMirroredBoards_3() {
            var b1 = "....K" +
                     "O...O" +
                     "....." +
                     "....." +
                     "oo..k";
            var b2 = "K...." +
                     "O...O" +
                     "....." +
                     "....." +
                     "k..oo";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnSameValuesForSameCardsInDifferentOrder() {
            var b = "O.KOO" +
                    "....." +
                    ".O..." +
                    "..oo." +
                    "..koo";
            var cards = new[] { Card.Boar, Card.Cobra, Card.Dragon, Card.Eel, Card.Frog };
            var cardNumbers1 = new[] { 0, 1, 2, 3, 4 };
            var cardNumbers2 = new[] { 1, 0, 2, 3, 4 };
            var cardNumbers3 = new[] { 0, 1, 3, 2, 4 };
            var cardNumbers4 = new[] { 1, 0, 3, 2, 4 };
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers2)));
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers3)));
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers4)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValueForDifferentInTurnPlayer_1() {
            var b = "OOKOO" +
                    "....." +
                    "....." +
                    "....." +
                    "ookoo";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 0)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 1)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValueForDifferentInTurnPlayer_2() {
            var b = ".OK.." +
                    "...O." +
                    ".o..O" +
                    "....." +
                    "o.k..";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 0)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 1)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValuesForDifferentCards() {
            var b = "OOKOO" +
                    "....." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Boar, Card.Cobra, Card.Dragon, Card.Eel, Card.Frog };
            var cardNumbers1 = new[] { 0, 1, 2, 3, 4 };
            var cardNumbers2 = new[] { 0, 1, 2, 4, 3 };
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValueForBoardsWithPawnsInDIfferentPositions1() {
            var b1 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ookoo";
            var b2 = "OOKO." +
                     "....O" +
                     "....." +
                     "....." +
                     "ookoo";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValueForBoardsWithPawnsInDIfferentPositions2() {
            var b1 = "OO..." +
                     "..K.." +
                     "...o." +
                     ".o..." +
                     "..k.o";
            var b2 = "OO..." +
                     "..K.." +
                     "...o." +
                     "..o.." +
                     "..k.o";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValueForBoardsWithPawnRemoved() {
            var b1 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ookoo";
            var b2 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ook.o";
            AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
        }


    }
}
 