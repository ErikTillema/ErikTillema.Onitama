using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable.
    /// </summary>
    public struct Vector : IEquatable<Vector>, IComparable<Vector> {

        public int X { get; }
        public int Y { get; }

        public Vector(int x, int y) {
            X = x;
            Y = y;
        }

        public static Vector Add(Vector a, Vector b) {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public Vector Add(Vector v) {
            return Add(this, v);
        }

        public Vector Subtract(Vector v) {
            return new Vector(X - v.X, Y - v.Y);
        }

        public static int ManhattanDistance(Vector a, Vector b) {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public int ManhattanDistance(Vector v) {
            return ManhattanDistance(this, v);
        }

        public bool Equals(Vector other) {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj) {
            if (!(obj is Vector)) return false;
            else return Equals((Vector)obj);
        }

        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public int CompareTo(Vector other) {
            if (this.X != other.X) return this.X - other.X;
            return this.Y - other.Y;
        }

        public override string ToString() {
            return $"({X},{Y})";
        }

    }
}
