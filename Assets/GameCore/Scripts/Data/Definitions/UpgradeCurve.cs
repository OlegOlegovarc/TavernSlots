using System.Collections.Generic;
using UnityEngine;

namespace SlotsTavern.Data
{
    [CreateAssetMenu(
        fileName = "UpgradeCurve_New",
        menuName = "Slots Tavern/Data/Upgrade Curve")]
    public class UpgradeCurve : ScriptableObject
    {
        [Header("Level Values")]
        [Tooltip("Index = level. Example: index 0 = level 0 value, index 1 = level 1 value.")]
        public List<int> valuesByLevel = new List<int>();

        [Header("Upgrade Costs")]
        [Tooltip("Index = current level. Example: index 0 = cost from level 0 to level 1.")]
        public List<int> upgradeCostsByCurrentLevel = new List<int>();

        public int MaxLevel
        {
            get
            {
                if (valuesByLevel == null || valuesByLevel.Count == 0)
                    return 0;

                return valuesByLevel.Count - 1;
            }
        }

        public int GetValueAtLevel(int level)
        {
            if (valuesByLevel == null || valuesByLevel.Count == 0)
                return 0;

            int safeLevel = Mathf.Clamp(level, 0, valuesByLevel.Count - 1);
            return valuesByLevel[safeLevel];
        }

        public bool CanUpgradeFromLevel(int currentLevel)
        {
            if (valuesByLevel == null || valuesByLevel.Count == 0)
                return false;

            if (currentLevel < 0)
                return false;

            if (currentLevel >= MaxLevel)
                return false;

            if (upgradeCostsByCurrentLevel == null)
                return false;

            return currentLevel < upgradeCostsByCurrentLevel.Count;
        }

        public int GetUpgradeCostFromLevel(int currentLevel)
        {
            if (!CanUpgradeFromLevel(currentLevel))
                return 0;

            return upgradeCostsByCurrentLevel[currentLevel];
        }
    }
}