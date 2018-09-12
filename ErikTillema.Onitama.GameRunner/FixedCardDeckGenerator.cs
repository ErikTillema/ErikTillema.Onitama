using ErikTillema.Onitama.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ErikTillema.Onitama.GameRunner {

    /// <summary>
    /// Returns random CardDecks, but the same CardDeck for every game.
    /// </summary>
    public class FixedCardDeckGenerator : ICardDeckGenerator {

        private ConcurrentDictionary<int, IReadOnlyList<Card>> generatedCardDecks = new ConcurrentDictionary<int, IReadOnlyList<Card>>();

        public FixedCardDeckGenerator() { }

        public IReadOnlyList<Card> GetCardDeck(int gameIndex) {
            if (!generatedCardDecks.ContainsKey(gameIndex)) {
                generatedCardDecks[gameIndex] = Card.GetRandomCardDeck();
            }
            return generatedCardDecks[gameIndex];
        }

    }
}
