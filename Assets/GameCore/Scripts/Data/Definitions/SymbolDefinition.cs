using System.Collections.Generic;
using UnityEngine;

namespace SlotsTavern.Data
{
    [CreateAssetMenu(
        fileName = "Symbol_New",
        menuName = "Slots Tavern/Data/Symbol Definition")]
    public class SymbolDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea(2, 5)]
        public string description;

        [Header("Visuals")]
        public Sprite icon;
        public Sprite diceFaceIcon;

        [Header("Upgrade")]
        [Tooltip("Default max level is 10, but each symbol can override this.")]
        public int maxUpgradeLevel = 10;

        [Tooltip("Cost curve for upgrading this symbol.")]
        public UpgradeCurve upgradeCostCurve;

        [Header("Effects")]
        public List<EffectDefinition> effects = new List<EffectDefinition>();

        [Header("Meta")]
        public bool canAppearInOffers = true;
        public bool isUnlockedByDefault = false;

        public int GetUpgradeCostFromLevel(int currentLevel)
        {
            if (upgradeCostCurve == null)
                return 0;

            return upgradeCostCurve.GetUpgradeCostFromLevel(currentLevel);
        }
    }
}