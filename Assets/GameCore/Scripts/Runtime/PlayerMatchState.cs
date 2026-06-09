using System;
using System.Collections.Generic;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class PlayerMatchState
    {
        public string playerId;
        public string displayName;
        public bool isBot;

        public int roundWins;
        public int currentCrystals;

        public List<string> ownedSymbolIds = new List<string>();
        public List<string> ownedItemIds = new List<string>();

        public List<UpgradeLevelState> symbolUpgradeLevels = new List<UpgradeLevelState>();
        public List<UpgradeLevelState> itemUpgradeLevels = new List<UpgradeLevelState>();

        public int maxHealthUpgradeLevel;
        public int shieldCapacityUpgradeLevel;

        public PlayerMatchState(string playerId, string displayName, bool isBot)
        {
            this.playerId = playerId;
            this.displayName = displayName;
            this.isBot = isBot;

            roundWins = 0;
            currentCrystals = 0;
            maxHealthUpgradeLevel = 0;
            shieldCapacityUpgradeLevel = 0;
        }

        public bool HasSymbol(string symbolId)
        {
            return ownedSymbolIds.Contains(symbolId);
        }

        public bool HasItem(string itemId)
        {
            return ownedItemIds.Contains(itemId);
        }

        public void AddSymbol(string symbolId)
        {
            if (string.IsNullOrWhiteSpace(symbolId))
                return;

            if (!ownedSymbolIds.Contains(symbolId))
                ownedSymbolIds.Add(symbolId);
        }

        public void RemoveSymbol(string symbolId)
        {
            ownedSymbolIds.Remove(symbolId);
            ClearSymbolUpgrade(symbolId);
        }

        public void AddItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return;

            if (!ownedItemIds.Contains(itemId))
                ownedItemIds.Add(itemId);
        }

        public int GetSymbolUpgradeLevel(string symbolId)
        {
            return GetUpgradeLevel(symbolUpgradeLevels, symbolId);
        }

        public void SetSymbolUpgradeLevel(string symbolId, int level)
        {
            SetUpgradeLevel(symbolUpgradeLevels, symbolId, level);
        }

        public void ClearSymbolUpgrade(string symbolId)
        {
            RemoveUpgradeLevel(symbolUpgradeLevels, symbolId);
        }

        public int GetItemUpgradeLevel(string itemId)
        {
            return GetUpgradeLevel(itemUpgradeLevels, itemId);
        }

        public void SetItemUpgradeLevel(string itemId, int level)
        {
            SetUpgradeLevel(itemUpgradeLevels, itemId, level);
        }

        private int GetUpgradeLevel(List<UpgradeLevelState> levels, string targetId)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].targetId == targetId)
                    return levels[i].level;
            }

            return 0;
        }

        private void SetUpgradeLevel(List<UpgradeLevelState> levels, string targetId, int level)
        {
            if (string.IsNullOrWhiteSpace(targetId))
                return;

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].targetId == targetId)
                {
                    levels[i].level = level;
                    return;
                }
            }

            levels.Add(new UpgradeLevelState(targetId, level));
        }

        private void RemoveUpgradeLevel(List<UpgradeLevelState> levels, string targetId)
        {
            for (int i = levels.Count - 1; i >= 0; i--)
            {
                if (levels[i].targetId == targetId)
                    levels.RemoveAt(i);
            }
        }
    }

    [Serializable]
    public class UpgradeLevelState
    {
        public string targetId;
        public int level;

        public UpgradeLevelState(string targetId, int level)
        {
            this.targetId = targetId;
            this.level = level;
        }
    }
}