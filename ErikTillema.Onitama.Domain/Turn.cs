using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable.
    /// </summary>
    public class Turn : IEquatable<Turn> {

        public GamePlayer Player { get; }

        public Card Card { get; }

        public int CardIndex { get; }

        public PieceType PieceType { get; }

        public Vector OriginalPosition { get; }

        public Vector Move { get; }

        public Turn(GamePlayer player, Card card, int cardIndex, PieceType pieceType, Vector originalPosition, Vector move) {
            Player = player;
            Card = card;
            CardIndex = cardIndex;
            PieceType = pieceType;
            OriginalPosition = originalPosition;
            Move = move;
        }

        public Turn(GamePlayer player, Card card, int cardIndex, Piece piece, Vector move) : this(player, card, cardIndex, piece.PieceType, piece.Position, move) { }

        public override string ToString() {
            return $"card {Card}, moving {PieceType} from {OriginalPosition} to {OriginalPosition.Add(Move)}";
        }

        public override bool Equals(object obj) {
            if (!(obj is Turn)) return false;
            else return Equals(obj as Turn);
        }

        public bool Equals(Turn other) {
            return object.Equals(this.Player, other.Player)
                && object.Equals(this.Card, other.Card) 
                && object.Equals(this.OriginalPosition, other.OriginalPosition) 
                && object.Equals(this.Move, other.Move);
        }

        public override int GetHashCode() {
            return Player.GetHashCode() ^ Card.GetHashCode() ^ OriginalPosition.GetHashCode() ^ Move.GetHashCode();
        }

    }

}
