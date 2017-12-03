using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace ErikTillema.Onitama.Domain.Tests {

    public class AlphaBetaSearchTest {

        class IndifferentEvaluator : IEvaluator {
            public double Evaluate(Game game, int originalInTurnPlayerIndex) {
                return 0;
            }
        }

        public static GameState ParseGameState(string board, IEnumerable<Card> cards, int inTurnPlayerIndex, IList<int> cardNumbers) {
            return MiniMaxTest.ParseGameState(board, cards, inTurnPlayerIndex, cardNumbers);
        }

        public static Game ParseGame(string board, IEnumerable<Card> cards, int inTurnPlayerIndex, IList<int> cardNumbers) {
            return MiniMaxTest.ParseGame(board, cards, inTurnPlayerIndex, cardNumbers);
        }

        private IList<Card> GetCards() {
            return new[] { Card.Boar, Card.Cobra, Card.Dragon, Card.Eel, Card.Frog };
        }

        private IList<int> GetCardNumbers() {
            return new[] { 0, 1, 2, 3, 4 };
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
            var game = ParseGame(b, cards, 0, cardNumbers);
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
            var game = ParseGame(b, cards, 1, cardNumbers);
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
            var game = ParseGame(b, cards, 1, cardNumbers);
            var tup = new AlphaBetaSearch(game, 1, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(0); // value from indifferent evaluator
        }

        [Test]
        public void GetGameResult_Should_ReturnLosingWhenOpponentInDirectWinningPosition() {
            var b = "OOK.O" +
                    "....." +
                    "..O.." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Boar, Card.Dragon, Card.Eel, Card.Tiger, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = ParseGame(b, cards, 0, cardNumbers);
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
            var game = ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 2, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(0); // undecided
            tup.Item2.Single(kvp => kvp.Value == 0).Key.Should()
                .Be(new Turn(game.InTurnPlayer, Card.Elephant, 1, PieceType.Pawn, new Vector(1, 1), new Vector(1, 1)));
        }

        [Test]
        public void GetGameResult_Should_ReturnWinningWhenInTwoStepWinningPosition() {
            // winning in two steps: play Monkey, moving pawn to (2,2), then Tiger to capture base.
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = new[] { Card.Boar, Card.Dragon, Card.Monkey, Card.Tiger, Card.Frog };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = ParseGame(b, cards, 1, cardNumbers);
            var tup = new AlphaBetaSearch(game, 3, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(AlphaBetaSearch.GameResultWinning);
            tup.Item2.Single(kvp => kvp.Value == AlphaBetaSearch.GameResultWinning).Key.Should()
                .Be(new Turn(game.InTurnPlayer, Card.Monkey, 0, PieceType.Pawn, new Vector(3, 3), new Vector(-1, -1)));
        }

        [Test]
        public void GetGameResult_Should_ReturnLosingWhenOpponentInTwoStepWinningPosition() {
            // opponent winning in two steps: play Monkey, moving pawn to (2,2), then Tiger to capture base.
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    "....." +
                    "ok.oo";
            var cards = new[] { Card.Frog, Card.Rabbit, Card.Monkey, Card.Tiger, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 4, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(AlphaBetaSearch.GameResultLosing);
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
            var game = ParseGame(b, cards, 0, cardNumbers);
            var tup = new AlphaBetaSearch(game, 4, new IndifferentEvaluator()).GetGameResult();
            tup.Item1.Should().Be(0); // undecided
            tup.Item2.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key).Should()
                .Contain(new Turn(game.InTurnPlayer, Card.Frog, 0, PieceType.Pawn, new Vector(3, 0), new Vector(-1, 1)));
        }

        // @@@ write test:
        // with evaluator that counts only material, move that captures pawn should be picked.

        [Test]
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
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
        }

        [Test]
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
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
        }

        [Test]
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
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers2)));
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers3)));
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers4)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnSameValueForDifferentInTurnPlayer_1() {
            var b = "OOKOO" +
                    "....." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 1, cardNumbers)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnSameValueForDifferentInTurnPlayer_2() {
            var b = ".OK.." +
                    "...O." +
                    ".o..O" +
                    "....." +
                    "o.k..";
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers)).Should().Be(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 1, cardNumbers)));
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
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers2)));
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
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().NotBe(AlphaBetaSearch.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
        }


    }
}
 