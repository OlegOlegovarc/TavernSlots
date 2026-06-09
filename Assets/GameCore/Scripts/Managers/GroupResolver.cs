using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class GroupResolver : MonoBehaviour
    {
        public List<ResolvedSymbolGroup> ResolveGroups(SpinResult spinResult)
        {
            List<ResolvedSymbolGroup> groups = new List<ResolvedSymbolGroup>();

            if (spinResult == null)
            {
                Debug.LogWarning("GroupResolver: Spin result is null.");
                return groups;
            }

            if (!spinResult.IsValid())
            {
                Debug.LogWarning("GroupResolver: Spin result is invalid.");
                return groups;
            }

            int startIndex = 0;
            string currentSymbolId = spinResult.GetSymbolIdAt(0);

            for (int i = 1; i < spinResult.slotCount; i++)
            {
                string nextSymbolId = spinResult.GetSymbolIdAt(i);

                if (nextSymbolId == currentSymbolId)
                    continue;

                AddGroup(
                    groups,
                    spinResult.actingPlayerId,
                    currentSymbolId,
                    startIndex,
                    i - startIndex);

                startIndex = i;
                currentSymbolId = nextSymbolId;
            }

            AddGroup(
                groups,
                spinResult.actingPlayerId,
                currentSymbolId,
                startIndex,
                spinResult.slotCount - startIndex);

            return groups;
        }

        private void AddGroup(
            List<ResolvedSymbolGroup> groups,
            string actingPlayerId,
            string symbolId,
            int startIndex,
            int length)
        {
            if (string.IsNullOrWhiteSpace(symbolId))
                return;

            if (length <= 0)
                return;

            // Temporary value.
            // Real effect values will be calculated later in ResolveManager
            // using SymbolDefinition effects and player upgrade levels.
            const int temporaryBaseValuePerSymbol = 0;

            ResolvedSymbolGroup group = new ResolvedSymbolGroup(
                actingPlayerId,
                symbolId,
                startIndex,
                length,
                temporaryBaseValuePerSymbol);

            groups.Add(group);
        }
    }
}