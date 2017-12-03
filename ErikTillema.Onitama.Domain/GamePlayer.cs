using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable.
    /// </summary>
    public class GamePlayer : IEquatable<GamePlayer> {

        public Player Player { get; }

        public int PlayerIndex { get; }

        public GamePlayer(Player player, int playerIndex) {
            Player = player;
            PlayerIndex = playerIndex;
        }

        public override string ToString() {
            return $"{(PlayerIndex == 0 ? "South" : "North")}";
        }

        public override bool Equals(object obj) {
            if (!(obj is GamePlayer)) return false;
            else return Equals(obj as GamePlayer);
        }

        public bool Equals(GamePlayer other) {
            return this.PlayerIndex == other.PlayerIndex;
        }

        public override int GetHashCode() {
            return PlayerIndex.GetHashCode();
        }

    }

}
