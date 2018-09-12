using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public interface IGameClientStatsCollector {
        void StartGetTurn(Game game);
        void EndGetTurn();
        string GetReport();
    }

}
