using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    public class MoveForwardGameClient : IGameClient {

        private BigInteger Fib(int a) {
            //BigInteger v1 = BigInteger.One;
            //BigInteger v2 = BigInteger.One;
            //for(int i=0; i < a; i++) {
            //    BigInteger v3 = v1 + v2;
            //    v1 = v2;
            //    v2 = v3;
            //}
            //return v2;
            long dummy = 0;
            for (int i=0; i< 1_000_000_000; i++) { dummy += 1; }
            return BigInteger.One;
        }

        public Turn GetTurn(Game game) {
            //var stopwatch = Stopwatch.StartNew();
            //int start = 100000;
            //int n = 20;
            //Parallel.For(start, start+n, new ParallelOptions() {MaxDegreeOfParallelism = 8}, i => { var y = Fib(i); });
            ////for (int i = start; i < start+n; i++) { var y = Fib(i); }
            //stopwatch.Stop();
            //var duration = stopwatch.Elapsed.TotalSeconds;
            //Console.Out.WriteLine($"Elapsed: {duration:00.000}");

            Turn winningTurn = game.GetDirectlyWinningTurn();
            if (winningTurn != null) return winningTurn;

            Turn capturingTurn = game.GetCapturingTurn();
            if (capturingTurn != null) return capturingTurn;

            // make any move forward with that card
            Turn result = game.GetValidTurns().FirstOrDefault(IsForwardTurn);
            if (result != null) return result;

            return game.GetAnyTurn();
        }

        private bool IsForwardTurn(Turn turn) {
            return turn.Move.Y > 0 && turn.Player.PlayerIndex == 0 || turn.Move.Y < 0 && turn.Player.PlayerIndex == 1;
        }

    }

}
