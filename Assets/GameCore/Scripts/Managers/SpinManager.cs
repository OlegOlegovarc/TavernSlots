using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Data;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class SpinManager : MonoBehaviour
    {
        [Header("Random")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int fixedSeed = 12345;

        private System.Random random;

        private void Awake()
        {
            ResetRandom();
        }

        public void ResetRandom()
        {
            random = useRandomSeed
                ? new System.Random()
                : new System.Random(fixedSeed);
        }

        public SpinResult CreateSpinResult(PlayerMatchState actingPlayer, RoundConfig roundConfig)
        {
            if (actingPlayer == null)
            {
                Debug.LogWarning("SpinManager: Cannot create spin result. Acting player is null.");
                return new SpinResult(string.Empty, new List<string>());
            }

            if (roundConfig == null)
            {
                Debug.LogWarning("SpinManager: Cannot create spin result. Round config is null.");
                return new SpinResult(actingPlayer.playerId, new List<string>());
            }

            return CreateSpinResult(
                actingPlayer.playerId,
                actingPlayer.ownedSymbolIds,
                roundConfig.slotCount);
        }

        public SpinResult CreateSpinResult(string actingPlayerId, List<string> symbolPoolIds, int slotCount)
        {
            if (random == null)
                ResetRandom();

            List<string> orderedSymbolIds = new List<string>();

            if (string.IsNullOrWhiteSpace(actingPlayerId))
            {
                Debug.LogWarning("SpinManager: Acting player id is empty.");
                return new SpinResult(string.Empty, orderedSymbolIds);
            }

            if (slotCount <= 0)
            {
                Debug.LogWarning($"SpinManager: Invalid slot count: {slotCount}");
                return new SpinResult(actingPlayerId, orderedSymbolIds);
            }

            if (symbolPoolIds == null || symbolPoolIds.Count == 0)
            {
                Debug.LogWarning("SpinManager: Symbol pool is empty.");
                return new SpinResult(actingPlayerId, orderedSymbolIds);
            }

            for (int i = 0; i < slotCount; i++)
            {
                int randomIndex = random.Next(0, symbolPoolIds.Count);
                string selectedSymbolId = symbolPoolIds[randomIndex];

                orderedSymbolIds.Add(selectedSymbolId);
            }

            return new SpinResult(actingPlayerId, orderedSymbolIds);
        }
    }
}