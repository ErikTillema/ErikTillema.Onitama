using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable.
    /// </summary>
    public class Card : IEquatable<Card> {

        public string Name { get; }

        public PlayerColor StartingPlayerColor { get; }

        private IReadOnlyList<Vector> Moves;

        public Card(string name, PlayerColor startingPlayerColor, IEnumerable<Vector> moves) {
            Name = name;
            StartingPlayerColor = startingPlayerColor;
            Moves = new List<Vector>(moves);
        }

        public IEnumerable<Vector> GetMoves(int playerInTurnIndex) {
            if (playerInTurnIndex == 0) return Moves;
            else return Moves.Select(m => new Vector(-m.X, -m.Y));
        }

        public bool Equals(Card other) {
            return this.Name == other.Name;
        }

        public override bool Equals(object obj) {
            if(!(obj is Card)) return false;
            else return Equals(obj as Card);
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public string GetMovesAsBoardString() {
            char[,] result = new char[Board.Width, Board.Height];
            for (int y = 0; y < Board.Height; y++)
                for (int x = 0; x < Board.Width; x++)
                    result[x, y] = '.';

            Vector middle = new Vector(2, 2);
            result[middle.X, middle.Y] = '+';
            foreach (Vector move in Moves) {
                Vector target = middle.Add(move);
                result[target.X, target.Y] = 'X';
            }

            return Board.GetAsBoardString(result);
        }

        public override string ToString() {
            return Name;
        }

        public static readonly Card Tiger = new Card("Tiger", PlayerColor.Blue, new[] { new Vector(0, -1), new Vector(0, 2) });
        public static readonly Card Dragon = new Card("Dragon", PlayerColor.Red, new[] { new Vector(-1, -1), new Vector(-2, 1), new Vector(1, -1), new Vector(2, 1) });
        public static readonly Card Frog = new Card("Frog", PlayerColor.Red, new[] { new Vector(-2, 0), new Vector(-1, 1), new Vector(1, -1) });
        public static readonly Card Rabbit = new Card("Rabbit", PlayerColor.Blue, new[] { new Vector(-1, -1), new Vector(1, 1), new Vector(2, 0) });

        public static readonly Card Crab = new Card("Crab", PlayerColor.Blue, new[] { new Vector(-2, 0), new Vector(0, 1), new Vector(2, 0) });
        public static readonly Card Elephant = new Card("Elephant", PlayerColor.Red, new[] { new Vector(-1, 0), new Vector(-1, 1), new Vector(1, 0), new Vector(1, 1) });
        public static readonly Card Goose = new Card("Goose", PlayerColor.Blue, new[] { new Vector(-1, 0), new Vector(-1, 1), new Vector(1, 0), new Vector(1, -1) });
        public static readonly Card Rooster = new Card("Rooster", PlayerColor.Red, new[] { new Vector(-1, -1), new Vector(-1, 0), new Vector(1, 0), new Vector(1, 1) });

        public static readonly Card Monkey = new Card("Monkey", PlayerColor.Blue, new[] { new Vector(-1, -1), new Vector(-1, 1), new Vector(1, -1), new Vector(1, 1) });
        public static readonly Card Mantis = new Card("Mantis", PlayerColor.Red, new[] { new Vector(0, -1), new Vector(-1, 1), new Vector(1, 1) });
        public static readonly Card Horse = new Card("Horse", PlayerColor.Red, new[] { new Vector(-1, 0), new Vector(0, -1), new Vector(0, 1) });
        public static readonly Card Ox = new Card("Ox", PlayerColor.Blue, new[] { new Vector(0, -1), new Vector(0, 1), new Vector(1, 0) });

        public static readonly Card Crane = new Card("Crane", PlayerColor.Blue, new[] { new Vector(-1, -1), new Vector(0, 1), new Vector(1, -1) });
        public static readonly Card Boar = new Card("Boar", PlayerColor.Red, new[] { new Vector(-1, 0), new Vector(0, 1), new Vector(1, 0) });
        public static readonly Card Eel = new Card("Eel", PlayerColor.Blue, new[] { new Vector(-1, -1), new Vector(-1, 1), new Vector(1, 0) });
        public static readonly Card Cobra = new Card("Cobra", PlayerColor.Red, new[] { new Vector(-1, 0), new Vector(1, 1), new Vector(1, -1) });

        public static readonly IReadOnlyList<Card> AllCards = new[] {
            Tiger, Dragon, Frog, Rabbit,
            Crab, Elephant, Goose, Rooster,
            Monkey, Mantis, Horse, Ox,
            Crane, Boar, Eel, Cobra
        };

    }
}
