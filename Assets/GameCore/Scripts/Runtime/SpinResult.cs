using System;
using System.Collections.Generic;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class SpinResult
    {
        public string actingPlayerId;
        public int slotCount;
        public List<string> orderedSymbolIds = new List<string>();

        public SpinResult(string actingPlayerId, List<string> orderedSymbolIds)
        {
            this.actingPlayerId = actingPlayerId;
            this.orderedSymbolIds = orderedSymbolIds != null
                ? new List<string>(orderedSymbolIds)
                : new List<string>();

            slotCount = this.orderedSymbolIds.Count;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(actingPlayerId)
                   && orderedSymbolIds != null
                   && orderedSymbolIds.Count > 0
                   && orderedSymbolIds.Count == slotCount;
        }

        public string GetSymbolIdAt(int index)
        {
            if (orderedSymbolIds == null)
                return string.Empty;

            if (index < 0 || index >= orderedSymbolIds.Count)
                return string.Empty;

            return orderedSymbolIds[index];
        }
    }
}