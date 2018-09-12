using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class ThrowExceptionGameClient : IGameClient {

        public Turn GetTurn(Game game) {
            throw new Exception("This GameClient always throws an exception. This helps to be sure that I always use the right GameClient in the InfiBot project.");
        }

    }

}
