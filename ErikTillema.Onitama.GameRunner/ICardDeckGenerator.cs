using ErikTillema.Onitama.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ErikTillema.Onitama.GameRunner {
    public interface ICardDeckGenerator {
        IReadOnlyList<Card> GetCardDeck(int gameIndex); 
    }
}
