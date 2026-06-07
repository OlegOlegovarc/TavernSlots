using System.Collections.Generic;
using UnityEngine;

namespace SlotsTavern.Data
{
    [CreateAssetMenu(
        fileName = "MatchConfig_New",
        menuName = "Slots Tavern/Data/Match Config")]
    public class MatchConfig : ScriptableObject
    {
        [Header("Match Rules")]
        [Tooltip("Best of 3 = first player to 2 round wins.")]
        public int bestOf = 3;

        [Header("Crystals")]
        public int startMatchCrystals = 30;

        [Header("Special Crystal Rewards")]
        public int winSpecialCrystals = 30;

        [Tooltip("Loser receives floor(win reward * this multiplier). Example: 0.333 = about one third.")]
        public float loseRewardMultiplier = 0.333f;

        [Header("Symbol Pool")]
        public int minSymbolPoolSize = 4;

        [Header("Build Phase")]
        public float buildPhaseTimeSeconds = 30f;
        public int offeredSymbolCount = 8;
        public int offeredItemCount = 6;

        [Header("Rounds")]
        public List<RoundConfig> roundConfigs = new List<RoundConfig>();

        public int RequiredWins
        {
            get
            {
                return (bestOf / 2) + 1;
            }
        }

        public int GetLoseSpecialCrystals()
        {
            return Mathf.FloorToInt(winSpecialCrystals * loseRewardMultiplier);
        }

        public RoundConfig GetRoundConfig(int roundIndex)
        {
            if (roundConfigs == null || roundConfigs.Count == 0)
                return null;

            for (int i = 0; i < roundConfigs.Count; i++)
            {
                if (roundConfigs[i] != null && roundConfigs[i].roundIndex == roundIndex)
                    return roundConfigs[i];
            }

            return roundConfigs[roundConfigs.Count - 1];
        }
    }
}