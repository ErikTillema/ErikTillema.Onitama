using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErikTillema.Onitama.Domain {

    /// <summary>
    /// Immutable.
    /// </summary>
    public class TurnResult {

        public PieceType? CapturedPieceType { get; }

        public bool GameIsFinished { get; }

        public TurnResult(Piece capturedPiece, bool gameIsFinished) {
            CapturedPieceType = capturedPiece?.PieceType;
            GameIsFinished = gameIsFinished;
        }

    }
}
