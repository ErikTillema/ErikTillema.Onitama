using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTillema.Collections;
using System.Diagnostics.Contracts;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Stateful, mutable: contains changing InTurnPlayerIndex, PlayerPieces, GameCards, Board.
    /// 
    /// The game state contains the positions of the pieces of both players
    /// and which cards are in the players' hands.
    /// 
    /// Possible states:
    /// Different board states with all pieces:
    /// Kings: 25 (k) x 24 (K)                                        =  600 = 6*10^2
    /// Pawns: 23 x 22 x 21 x 20    x    19 x 18 x 17 x 16 (or less)  =  20*10^9
    /// But pawns are interchangable: / 4! x 4!                       =  24^2 = 576
    /// And board is symmetrical in mirror through both bases         =  2           (more or less)  WRONG! King mobility for example is not the same for mirrored boards, because not all cards have symmetrical moves. See todo file.
    /// Total  =  10*10^9
    /// 
    /// But pieces cannot occupy the enemies base (yet), so actually it's less. Or at least those states are final states.
    /// But there are also board states with less pawns, so actually it's more.
    /// 
    /// Symmetrical states: 
    ///         mirror
    /// 
    /// .K+..   ..+K.
    /// ...OO   OO...
    /// .....   .....
    /// ..oo.   .oo..
    /// .k+..   ..+k.
    /// 
    /// Different card states:
    /// (5 2) x (3 1) x (2 2) = 10 x 3 x 1 = 30
    /// 
    /// Total games state possibilities:
    /// 3*10^11 = 300*10^9
    /// 
    /// Moves:
    /// A player can make a number of different moves. It depends on the cards available and the positions of the pieces,
    /// since you cannot move to a position of your own pieces.
    /// 
    /// Cards to choose from = 2
    /// Moves possible per card = 2-4, max 4
    /// Pieces to choose from for move = max 5
    /// 
    /// Total move possibilities = max 2*4*5 = 40
    /// </summary>
    public class GameState {

        /// <summary>
        /// Index of the player who's turn it is. 0 or 1.
        /// 0 = South = Blue
        /// 1 = North = Red
        /// Stateful.
        /// </summary>
        public int InTurnPlayerIndex { get; private set; }

        /// <summary>
        /// Stateful (in the sense that the list is not changing, but the Pieces inside are changing).
        /// </summary>
        public IReadOnlyList<IReadOnlyList<Piece>> PlayerPieces { get; private set; }

        /// <summary>
        /// Stateful.
        /// </summary>
        public Piece[,] Board { get; private set; }

        /// <summary>
        /// Stateful.
        /// </summary>
        public Stack<Piece> CapturedPieces { get; private set; }

        /// <summary>
        /// The (five) cards in this game.
        /// The order changes, because the first two cards are South player's cards, next two cards are North's cards, last card is middle card.
        /// Stateful.
        /// </summary>
        public Card[] GameCards { get; private set; }

        /// <summary>
        /// Stateful.
        /// </summary>
        public int? WinningPlayerIndex { get; private set; } = null;

        /// <summary>
        /// We give every card a number, 0..4, so that we can use these numbers to generate a hashcode for the cards part of the state.
        /// Stateless.
        /// </summary>
        public Dictionary<Card, int> CardNumbers { get; private set; }

        public static readonly int[][] PlayerCardIndices = new int[2][] {
            new int[2] { 0, 1 },
            new int[2] { 2, 3 },
        };
        private const int MiddleCardIndex = 4;

        public IEnumerable<Piece> InTurnPlayerPieces => PlayerPieces[InTurnPlayerIndex].Where(p => !p.IsCaptured);
        public IEnumerable<Piece> OtherPlayerPieces => PlayerPieces[1 - InTurnPlayerIndex].Where(p => !p.IsCaptured);

        public int[] InTurnPlayerCardIndices => PlayerCardIndices[InTurnPlayerIndex];
        public int[] OtherPlayerCardIndices => PlayerCardIndices[1 - InTurnPlayerIndex];

        public IEnumerable<IEnumerable<Card>> PlayerCards => PlayerCardIndices.Select(indices => indices.Select(index => GameCards[index]));
        public IEnumerable<Card> InTurnPlayerCards => PlayerCardIndices[InTurnPlayerIndex].Select(index => GameCards[index]);
        public IEnumerable<Card> OtherPlayerCards => PlayerCardIndices[1 - InTurnPlayerIndex].Select(index => GameCards[index]);

        public Card MiddleCard => GameCards[MiddleCardIndex];

        public GameState(IEnumerable<Card> cards = null) {
            var gameCards = cards == null ? Card.GetRandomCardDeck() : cards.ToList();
            var cardNumbers = new[] { 0, 1, 2, 3, 4 };
            int startingPlayerIndex = Game.PlayerColors.GetKey(gameCards[MiddleCardIndex].StartingPlayerColor); // Starting player is chosen from middle card
            var playerPieces = GetStartingPieces();
            Init(playerPieces, gameCards, startingPlayerIndex, cardNumbers, new List<Piece>());
        }

        public GameState(IReadOnlyList<IReadOnlyList<Piece>> playerPieces, IEnumerable<Card> gameCards, int inTurnPlayerIndex, IList<int> cardNumbers) {
            Init(playerPieces, gameCards, inTurnPlayerIndex, cardNumbers, new List<Piece>());
        }

        private GameState(IReadOnlyList<IReadOnlyList<Piece>> playerPieces, IEnumerable<Card> gameCards, int inTurnPlayerIndex, IList<int> cardNumbers, IEnumerable<Piece> capturedPieces) {
            Init(playerPieces, gameCards, inTurnPlayerIndex, cardNumbers, capturedPieces);
        }

        private void Init(IReadOnlyList<IReadOnlyList<Piece>> playerPieces, IEnumerable<Card> gameCards, int inTurnPlayerIndex, IList<int> cardNumbers, IEnumerable<Piece> capturedPieces) {
            GameCards = gameCards.ToArray();
            CardNumbers = new Dictionary<Card, int>();
            for (int i = 0; i < 5; i++) CardNumbers[GameCards[i]] = cardNumbers[i];

            InTurnPlayerIndex = inTurnPlayerIndex;

            PlayerPieces = playerPieces;
            Board = new Piece[Domain.Board.Width, Domain.Board.Height];
            foreach (var piece in PlayerPieces.SelectMany(_ => _).Where(p => !p.IsCaptured)) {
                Board[piece.Position.X, piece.Position.Y] = piece;
            }
            CapturedPieces = new Stack<Piece>(capturedPieces);
        }

        public GameState Clone() {
            var clonedPlayerPieces = this.PlayerPieces.Select(l => l.Select(p => p.Clone()).ToList()).ToList();
            var clonedCardNumbers = this.GameCards.Select(c => this.CardNumbers[c]).ToList();
            var clonedCapturedPieces = this.CapturedPieces.ToList();
            return new GameState(clonedPlayerPieces, this.GameCards, this.InTurnPlayerIndex, clonedCardNumbers, clonedCapturedPieces);
        }

        [Pure]
        private static Piece[][] GetStartingPieces() {
            var result = new Piece[2][];
            for (int i = 0; i < 2; i++) {
                result[i] = new Piece[5];
                int y = i == 0 ? 0 : 4;
                result[i][0] = new King(i, new Vector(2, y));
                for (int j = 0; j < result[i].Length - 1; j++) {
                    int x = j >= 2 ? j + 1 : j;
                    result[i][j + 1] = new Pawn(i, new Vector(x, y));
                }
            }
            //result[0] = new Piece[5];
            //result[0][0] = new King(0, new Vector(2, 0));
            //result[0][1] = new Pawn(0, new Vector(1, 3));
            //result[0][2] = new Pawn(0, new Vector(1, 0));
            //result[0][3] = new Pawn(0, new Vector(3, 0));
            //result[0][4] = new Pawn(0, new Vector(4, 0));
            //result[1] = new Piece[1];
            //result[1][0] = new King(1, new Vector(2, 4));
            //result[1][1] = new Pawn(1, new Vector(0, 4));
            //result[1][2] = new Pawn(1, new Vector(0, 2));
            //result[1][3] = new Pawn(1, new Vector(2, 3));
            //result[1][4] = new Pawn(1, new Vector(4, 4));

            return result;
        }

        /// <summary>
        /// Returns whether or not the game is finished after this move.
        /// </summary>
        public TurnResult PlayTurn(Turn turn, bool doChecks = true) {
            MiniMax.PlayTurnCount++;
            AlphaBetaSearch.PlayTurnCount++;

            Piece piece = Board[turn.OriginalPosition.X, turn.OriginalPosition.Y];

            if (doChecks) { // set doChecks to false for performance if necessary
                if (piece == null) throw new ArgumentException("Invalid Turn, bad OriginalPosition");
                if (piece.PlayerIndex != InTurnPlayerIndex) throw new ArgumentException("Invalid Turn, wrong piece");
                if (piece.IsCaptured) throw new ArgumentException("Invalid Turn, piece was already captured");
                if (!InTurnPlayerCards.Contains(turn.Card)) throw new ArgumentException("Invalid Turn, wrong card");
                if (!turn.Card.GetMoves(InTurnPlayerIndex).Contains(turn.Move)) throw new ArgumentException("Invalid Turn, bad move for card");
                if (!IsValidMove(piece, turn.Move)) throw new ArgumentException("Invalid Turn, bad move");
            }

            // move pieces
            Vector newPosition = turn.OriginalPosition.Add(turn.Move);
            Piece captured = Board[newPosition.X, newPosition.Y];

            Board[turn.OriginalPosition.X, turn.OriginalPosition.Y] = null;
            piece.Position = newPosition;
            Board[newPosition.X, newPosition.Y] = piece;

            if (captured != null) {
                captured.IsCaptured = true;
                CapturedPieces.Push(captured);
            }

            // swap played card with middle card
            Card tmp = GameCards[MiddleCardIndex];
            GameCards[MiddleCardIndex] = GameCards[PlayerCardIndices[InTurnPlayerIndex][turn.CardIndex]];
            GameCards[PlayerCardIndices[InTurnPlayerIndex][turn.CardIndex]] = tmp;

            bool gameIsFinshed = false;
            if (captured != null && captured is King)
                gameIsFinshed = true;
            else if (piece is King && newPosition.Equals(Domain.Board.PlayerBases[1 - InTurnPlayerIndex]))
                gameIsFinshed = true;

            if (gameIsFinshed) {
                WinningPlayerIndex = InTurnPlayerIndex;
            }

            InTurnPlayerIndex = 1 - InTurnPlayerIndex; // let's toggle this for consistency, even when the game has just been finished.

            return new TurnResult(captured, gameIsFinshed);
        }

        public void UndoTurn(Turn turn, TurnResult turnResult, bool doChecks = true) {
            MiniMax.UndoTurnCount++;
            AlphaBetaSearch.UndoTurnCount++;

            InTurnPlayerIndex = 1 - InTurnPlayerIndex;
            WinningPlayerIndex = null;
            Vector movedPosition = turn.OriginalPosition.Add(turn.Move);
            Piece piece = Board[movedPosition.X, movedPosition.Y];
            if (turnResult.CapturedPieceType != null) {
                Piece capturedPiece = CapturedPieces.Pop();
                if (doChecks) {
                    if (!capturedPiece.IsCaptured) throw new Exception("Huh");
                    if (capturedPiece.PlayerIndex != 1 - InTurnPlayerIndex) throw new Exception("huh");
                    if (capturedPiece.PieceType != turnResult.CapturedPieceType) throw new ArgumentException("Bad TurnResult.CapturedPieceType");
                    if (!capturedPiece.Position.Equals(movedPosition)) throw new ArgumentException("Bad CapturedPiece Position mismatch");
                }
                capturedPiece.IsCaptured = false;
                Board[movedPosition.X, movedPosition.Y] = capturedPiece; 
            } else {
                Board[movedPosition.X, movedPosition.Y] = null;
            }
            piece.Position = turn.OriginalPosition;
            Board[turn.OriginalPosition.X, turn.OriginalPosition.Y] = piece;

            Card tmp = GameCards[MiddleCardIndex];
            GameCards[MiddleCardIndex] = GameCards[PlayerCardIndices[InTurnPlayerIndex][turn.CardIndex]];
            GameCards[PlayerCardIndices[InTurnPlayerIndex][turn.CardIndex]] = tmp;
        }

        public bool IsValidMove(Piece piece, Vector move) {
            Vector newPosition = piece.Position.Add(move);
            if (!Domain.Board.IsWithinBounds(newPosition)) return false;
            if (Board[newPosition.X, newPosition.Y] != null && Board[newPosition.X, newPosition.Y].PlayerIndex == InTurnPlayerIndex) return false;
            return true;
        }

    }

}
