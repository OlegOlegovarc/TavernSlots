using System;
using System.Collections.Generic;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class ResolvedSymbolGroup
    {
        public string actingPlayerId;
        public string symbolId;

        public int startIndex;
        public int length;

        public int baseValuePerSymbol;
        public int finalValue;

        public List<int> affectedSlotIndices = new List<int>();

        public bool IsStack
        {
            get { return length > 1; }
        }

        public ResolvedSymbolGroup(
            string actingPlayerId,
            string symbolId,
            int startIndex,
            int length,
            int baseValuePerSymbol)
        {
            this.actingPlayerId = actingPlayerId;
            this.symbolId = symbolId;
            this.startIndex = startIndex;
            this.length = length;
            this.baseValuePerSymbol = baseValuePerSymbol;

            finalValue = CalculateFinalValue(baseValuePerSymbol, length);

            affectedSlotIndices = new List<int>();
            for (int i = 0; i < length; i++)
                affectedSlotIndices.Add(startIndex + i);
        }

        public static int CalculateFinalValue(int baseValuePerSymbol, int groupLength)
        {
            if (groupLength <= 0)
                return 0;

            int sum = baseValuePerSymbol * groupLength;
            return sum * groupLength;
        }
    }
}