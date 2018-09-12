using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace ErikTillema.Onitama.Domain.Tests {

    public class MiniMaxTest {

        [Test]
        public void GetGameResult_Should_ReturnWinningWhenInDirectWinningPosition_South() {
            var b = "OOKOO"+
                    "....."+
                    "..o.." +
                    "....." +
                    "o.koo";
            var cards = new[] { Card.Boar, Card.Tiger, Card.Dragon, Card.Eel, Card.Frog };
            var game = GameUtil.ParseGame(b, cards, 0);
            var tup = new MiniMax(game, 1).GetGameResult();
            tup.Item1.Should().Be(MiniMax.GameResultWinning);
            tup.Item2.Where(kvp => kvp.Value == MiniMax.GameResultWinning).Select(kvp => kvp.Key).Should().Contain(new Turn(game.InTurnPlayer, Card.Tiger, 1, PieceType.Pawn, new Vector(2,2), new Vector(0,2)));
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
            var tup = new MiniMax(game, 1).GetGameResult();
            tup.Item1.Should().Be(MiniMax.GameResultWinning);
            tup.Item2.Where(kvp => kvp.Value == MiniMax.GameResultWinning).Select(kvp => kvp.Key).Should().Contain(new Turn(game.InTurnPlayer, Card.Tiger, 1, PieceType.Pawn, new Vector(2, 2), new Vector(0, -2)));
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
            var tup = new MiniMax(game, 1).GetGameResult();
            tup.Item1.Should().Be(4); // undecided in 1
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
            var tup = new MiniMax(game, 2).GetGameResult();
            tup.Item1.Should().Be(MiniMax.GameResultLosing);
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
            var tup = new MiniMax(game, 2).GetGameResult();
            tup.Item1.Should().Be(5); // undecided in 2
            // After the new rule, there are in fact multiple moves to take that lead to undecided in 2.
            tup.Item2.Where(kvp => kvp.Value == 5).Select(_ => _.Key).Should().Contain(new Turn(game.InTurnPlayer, Card.Elephant, 1, PieceType.Pawn, new Vector(1, 1), new Vector(1, 1)));
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
            var tup = new MiniMax(game, 3).GetGameResult();
            tup.Item1.Should().Be(MiniMax.GameResultWinning);
            tup.Item2.Single(kvp => kvp.Value == MiniMax.GameResultWinning).Key.Should().Be(new Turn(game.InTurnPlayer, Card.Monkey, 0, PieceType.Pawn, new Vector(3, 3), new Vector(-1, -1)));
        }

        [Test]
        public void GetGameResult_Should_ReturnLosingWhenOpponentInTwoStepWinningPosition() {
            var b = "OO.OO" +
                    "...K." +
                    "....." +
                    "....." +
                    "ok.oo";
            var cards = new[] { Card.Frog, Card.Rabbit, Card.Monkey, Card.Tiger, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new MiniMax(game, 4).GetGameResult();
            tup.Item1.Should().Be(MiniMax.GameResultLosing);
        }

        [Test]
        public void GetGameResult_Should_ReturnUndecidedWhenOpponentNotInTwoStepWinningPosition() {
            // defense is moving pawn to (2,2) in two steps, for example through (2,1)
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Frog, Card.Rabbit, Card.Monkey, Card.Tiger, Card.Boar };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = GameUtil.ParseGame(b, cards, 0, cardNumbers);
            var tup = new MiniMax(game, 4).GetGameResult();
            tup.Item1.Should().Be(7); // undecided in 4
            tup.Item2.Where(kvp => kvp.Value == 7).Select(kvp => kvp.Key).Should().Contain(new Turn(game.InTurnPlayer, Card.Frog, 0, PieceType.Pawn, new Vector(3, 0), new Vector(-1, 1)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers2)));
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers3)));
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers4)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnSameValuesForFlippedBoard_1() {
            var b = "OOKOO" +
                    "....." +
                    "....." +
                    "....." +
                    "ookoo";
            var cardNumbers1 = new[] { 0, 1, 2, 3, 4 };
            var cardNumbers2 = new[] { 2, 3, 0, 1, 4 };
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 0, cardNumbers: cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 1, cardNumbers: cardNumbers2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnSameValuesForFlippedBoard_2() {
            var b1 = ".OKOO" +
                     "....." +
                     ".o..." +
                     "o...." +
                     "...ko";
            var b2 = "...KO" +
                     "O...." +
                     ".O..." +
                     "....." +
                     ".okoo";
            var cardNumbers1 = new[] { 0, 1, 2, 3, 4 };
            var cardNumbers2 = new[] { 2, 3, 0, 1, 4 };
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b1, inTurnPlayerIndex: 0, cardNumbers: cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b2, inTurnPlayerIndex: 1, cardNumbers: cardNumbers2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValuesForDifferentInTurnPlayer() {
            var b = "OOKOO" +
                    "....." +
                    "....." +
                    "....." +
                    "ookoo";
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 0)).Should().NotBe(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, inTurnPlayerIndex: 1)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers1)).Should().NotBe(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b, cards, 0, cardNumbers2)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().NotBe(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().NotBe(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
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
            MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b1)).Should().NotBe(MiniMax.GetUniqueIdentifier(GameUtil.ParseGameState(b2)));
        }

    }
}
 