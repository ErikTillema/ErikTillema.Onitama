using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public interface IEvaluator {

        /// <summary>
        /// A higher score means a higher probability of winning for the in view player.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="inViewPlayerIndex"></param>
        /// <returns></returns>
        double Evaluate(Game game, int inViewPlayerIndex);

    }

}
