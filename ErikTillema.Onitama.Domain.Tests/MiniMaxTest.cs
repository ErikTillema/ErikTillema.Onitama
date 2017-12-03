using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace ErikTillema.Onitama.Domain.Tests {

    public class MiniMaxTest {

        public static GameState ParseGameState(string board, IEnumerable<Card> cards, int inTurnPlayerIndex, IList<int> cardNumbers) {
            var result = new GameState();
            Piece[][] playerPieces = new Piece[2][];
            int[] playerPiecesCount = new int[2];
            for (int i = 0; i < 2; i++) {
                playerPiecesCount[i] = 0;
                playerPieces[i] = new Piece[5];
            }
            // always set King as first Piece of playerPieces, then pawns.
            for (int i = 0; i < board.Length; i++) {
                int y = 4 - (i / 5);
                int x = i % 5;
                char c = board[i];
                if (c == 'k') playerPieces[0][playerPiecesCount[0]++] = new King(0, new Vector(x, y));
                else if (c == 'K') playerPieces[1][playerPiecesCount[1]++] = new King(1, new Vector(x, y));
            }
            for (int i = 0; i < board.Length; i++) {
                int y = 4 - (i / 5);
                int x = i % 5;
                char c = board[i];
                if (c == 'o') playerPieces[0][playerPiecesCount[0]++] = new Pawn(0, new Vector(x, y));
                else if (c == 'O') playerPieces[1][playerPiecesCount[1]++] = new Pawn(1, new Vector(x, y));
            }
            for (int i = 0; i < 2; i++) {
                while (playerPiecesCount[i] < 5)
                    playerPieces[i][playerPiecesCount[i]++] = new Pawn(0, new Vector(0, 0)) { IsCaptured = true };
            }
            return new GameState(playerPieces, cards, inTurnPlayerIndex, cardNumbers);
        }

        public static Game ParseGame(string board, IEnumerable<Card> cards, int inTurnPlayerIndex, IList<int> cardNumbers) {
            var players = new[] { new Player("player1", null), new Player("player2", null) };
            var gameState = ParseGameState(board, cards, inTurnPlayerIndex, cardNumbers);
            return new Game(players[0], players[1], gameState);
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
            var game = ParseGame(b, cards, 1, cardNumbers);
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
            var game = ParseGame(b, cards, 1, cardNumbers);
            var tup = new MiniMax(game, 1).GetGameResult();
            tup.Item1.Should().Be(4); // undecided in 1
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
            var game = ParseGame(b, cards, 0, cardNumbers);
            var tup = new MiniMax(game, 2).GetGameResult();
            tup.Item1.Should().Be(5); // undecided in 2
            tup.Item2.Single(kvp => kvp.Value == 5).Key.Should().Be(new Turn(game.InTurnPlayer, Card.Elephant, 1, PieceType.Pawn, new Vector(1, 1), new Vector(1, 1)));
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
            var tup = new MiniMax(game, 3).GetGameResult();
            tup.Item1.Should().Be(MiniMax.GameResultWinning);
            tup.Item2.Single(kvp => kvp.Value == MiniMax.GameResultWinning).Key.Should().Be(new Turn(game.InTurnPlayer, Card.Monkey, 0, PieceType.Pawn, new Vector(3, 3), new Vector(-1, -1)));
        }

        [Test]
        public void GetGameResult_Should_ReturnLosingWhenOpponentInTwoStepWinningPosition() {
            var b = "OOK.O" +
                    "...O." +
                    "....." +
                    "....." +
                    "ok.oo";
            var cards = new[] { Card.Frog, Card.Rabbit, Card.Monkey, Card.Tiger, Card.Eel };
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            var game = ParseGame(b, cards, 0, cardNumbers);
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
            var game = ParseGame(b, cards, 0, cardNumbers);
            var tup = new MiniMax(game, 4).GetGameResult();
            tup.Item1.Should().Be(7); // undecided in 4
            tup.Item2.Where(kvp => kvp.Value == 7).Select(kvp => kvp.Key).Should().Contain(new Turn(game.InTurnPlayer, Card.Frog, 0, PieceType.Pawn, new Vector(3, 0), new Vector(-1, 1)));
        }

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
            MiniMax.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            MiniMax.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            MiniMax.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers2)));
            MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers3)));
            MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers4)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnSameValuesForRotatedBoard_1() {
            var b = "OOKOO" +
                    "....." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = GetCards();
            var cardNumbers1 = new[] { 0, 1, 2, 3, 4 };
            var cardNumbers2 = new[] { 2, 3, 0, 1, 4 };
            MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 1, cardNumbers2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnSameValuesForRotatedBoard_2() {
            var b1 = ".OKOO" +
                     "....." +
                     ".o..." +
                     "o...." +
                     "...ko";
            var b2 = "OK..." +
                     "....O" +
                     "...O." +
                     "....." +
                     "ooko.";
            var cards = GetCards();
            var cardNumbers1 = new[] { 0, 1, 2, 3, 4 };
            var cardNumbers2 = new[] { 2, 3, 0, 1, 4 };
            MiniMax.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers1)).Should().Be(MiniMax.GetUniqueIdentifier(ParseGameState(b2, cards, 1, cardNumbers2)));
        }

        [Test]
        public void GetUniqueIdentifier_should_returnDifferentValuesForDifferentInTurnPlayer() {
            var b = "OOKOO" +
                    "....." +
                    "....." +
                    "....." +
                    "ookoo";
            var cards = GetCards();
            var cardNumbers = GetCardNumbers();
            MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers)).Should().NotBe(MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 1, cardNumbers)));
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
            MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers1)).Should().NotBe(MiniMax.GetUniqueIdentifier(ParseGameState(b, cards, 0, cardNumbers2)));
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
            MiniMax.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().NotBe(MiniMax.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            MiniMax.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().NotBe(MiniMax.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
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
            MiniMax.GetUniqueIdentifier(ParseGameState(b1, cards, 0, cardNumbers)).Should().NotBe(MiniMax.GetUniqueIdentifier(ParseGameState(b2, cards, 0, cardNumbers)));
        }

    }
}
 