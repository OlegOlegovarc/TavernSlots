using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Core;

namespace SlotsTavern.Data
{
    [CreateAssetMenu(
        fileName = "Item_New",
        menuName = "Slots Tavern/Data/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea(2, 5)]
        public string description;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject worldPrefab;

        [Header("Costs")]
        [Tooltip("Cost to buy this item during BuildPhase.")]
        public int acquireCost = 0;

        [Tooltip("Cost to activate this item during the player's turn.")]
        public int activationCost = 0;

        [Header("Use Rules")]
        public ItemUsePolicy usePolicy = ItemUsePolicy.OncePerTurn;
        public ItemActivationWindow activationWindow = ItemActivationWindow.PreSpinOnly;

        [Header("Upgrade")]
        public int maxUpgradeLevel = 10;
        public UpgradeCurve upgradeCostCurve;

        [Header("Effects")]
        public List<EffectDefinition> effects = new List<EffectDefinition>();

        [Header("Meta")]
        public bool canAppearInOffers = true;
        public bool isUnlockedByDefault = false;
        public bool isRare = false;

        public int GetUpgradeCostFromLevel(int currentLevel)
        {
            if (upgradeCostCurve == null)
                return 0;

            return upgradeCostCurve.GetUpgradeCostFromLevel(currentLevel);
        }
    }
}