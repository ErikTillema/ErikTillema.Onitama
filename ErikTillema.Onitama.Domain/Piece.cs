using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Stateful, mutable: contains changing Position, IsCaptured.
    /// </summary>
    public abstract class Piece : IEquatable<Piece> {

        public PieceType PieceType { get; }

        public int PlayerIndex { get; }

        public Vector Position { get; set; }

        public bool IsCaptured { get; set; }

        public Piece(PieceType pieceType, int playerIndex, Vector position) {
            PieceType = pieceType;
            PlayerIndex = playerIndex;
            Position = position;
            IsCaptured = false;
        }

        public abstract Piece Clone();

        public override string ToString() {
            return PieceType.ToString();
        }

        public abstract bool Equals(Piece other);

        [Pure]
        protected static bool Equals(Piece a, Piece other) {
            return object.Equals(a.PieceType, other.PieceType) && object.Equals(a.PlayerIndex, other.PlayerIndex) && object.Equals(a.Position, other.Position) && object.Equals(a.IsCaptured, other.IsCaptured);
        }

        public override bool Equals(object obj) {
            if (!(obj is Piece)) return false;
            else return Equals(obj as Piece);
        }

        public override int GetHashCode() {
            return PieceType.GetHashCode() ^ PlayerIndex.GetHashCode() ^ Position.GetHashCode() ^ IsCaptured.GetHashCode();
        }

    }

    public class Pawn : Piece {

        public Pawn(int playerIndex, Vector position) : base(PieceType.Pawn, playerIndex, position) { }

        public override Piece Clone() {
            return new Pawn(PlayerIndex, Position) {
                IsCaptured = this.IsCaptured
            };
        }

        public override bool Equals(Piece other) {
            if (!(other is Pawn)) return false;
            else {
                Pawn o = (Pawn)other;
                return Equals(this, o);
            }
        }

    }

    public class King : Piece {

        public King(int playerIndex, Vector position) : base(PieceType.King, playerIndex, position) { }

        public override Piece Clone() {
            return new King(PlayerIndex, Position) {
                IsCaptured = this.IsCaptured
            };
        }

        public override bool Equals(Piece other) {
            if (!(other is King)) return false;
            else {
                King o = (King)other;
                return Equals(this, o);
            }
        }

    }

}
