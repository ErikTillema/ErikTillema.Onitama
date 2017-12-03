using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable.
    /// </summary>
    public class Player {

        public string Name { get; }

        public Func<IGameClient> CreateGameClient { get; }

        public Player(string name, Func<IGameClient> createGameClient) {
            Name = name;
            CreateGameClient = createGameClient;
        }

        public override string ToString() {
            return Name;
        }

    }

}
