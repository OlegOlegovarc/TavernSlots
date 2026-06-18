using UnityEngine;
using SlotsTavern.Data;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Player Stat Upgrade Curves")]
        [SerializeField]
        private UpgradeCurve maxHealthBonusCurve;

        [SerializeField]
        private UpgradeCurve shieldCapacityBonusCurve;

        public int CalculateMaxHealth(
            PlayerMatchState playerMatchState,
            RoundConfig roundConfig)
        {
            if (roundConfig == null)
            {
                Debug.LogWarning(
                    "UpgradeManager: Cannot calculate max health. " +
                    "RoundConfig is null.");

                return 1;
            }

            int upgradeLevel = playerMatchState != null
                ? Mathf.Max(0, playerMatchState.maxHealthUpgradeLevel)
                : 0;

            int upgradeBonus = GetCurveValue(
                maxHealthBonusCurve,
                upgradeLevel,
                "Max Health");

            return Mathf.Max(
                1,
                roundConfig.playerBaseHealth + upgradeBonus);
        }

        public int CalculateShieldCapacity(
            PlayerMatchState playerMatchState,
            RoundConfig roundConfig)
        {
            if (roundConfig == null)
            {
                Debug.LogWarning(
                    "UpgradeManager: Cannot calculate shield capacity. " +
                    "RoundConfig is null.");

                return 0;
            }

            int upgradeLevel = playerMatchState != null
                ? Mathf.Max(
                    0,
                    playerMatchState.shieldCapacityUpgradeLevel)
                : 0;

            int upgradeBonus = GetCurveValue(
                shieldCapacityBonusCurve,
                upgradeLevel,
                "Shield Capacity");

            return Mathf.Max(
                0,
                roundConfig.playerBaseShieldCapacity + upgradeBonus);
        }

        public int GetMaxHealthUpgradeCost(
            PlayerMatchState playerMatchState)
        {
            if (maxHealthBonusCurve == null)
                return 0;

            int currentLevel = playerMatchState != null
                ? playerMatchState.maxHealthUpgradeLevel
                : 0;

            return maxHealthBonusCurve.GetUpgradeCostFromLevel(
                currentLevel);
        }

        public int GetShieldCapacityUpgradeCost(
            PlayerMatchState playerMatchState)
        {
            if (shieldCapacityBonusCurve == null)
                return 0;

            int currentLevel = playerMatchState != null
                ? playerMatchState.shieldCapacityUpgradeLevel
                : 0;

            return shieldCapacityBonusCurve.GetUpgradeCostFromLevel(
                currentLevel);
        }

        public bool CanUpgradeMaxHealth(
            PlayerMatchState playerMatchState)
        {
            if (maxHealthBonusCurve == null)
                return false;

            int currentLevel = playerMatchState != null
                ? playerMatchState.maxHealthUpgradeLevel
                : 0;

            return maxHealthBonusCurve.CanUpgradeFromLevel(
                currentLevel);
        }

        public bool CanUpgradeShieldCapacity(
            PlayerMatchState playerMatchState)
        {
            if (shieldCapacityBonusCurve == null)
                return false;

            int currentLevel = playerMatchState != null
                ? playerMatchState.shieldCapacityUpgradeLevel
                : 0;

            return shieldCapacityBonusCurve.CanUpgradeFromLevel(
                currentLevel);
        }

        private int GetCurveValue(
            UpgradeCurve curve,
            int level,
            string curveName)
        {
            if (curve == null)
            {
                Debug.LogWarning(
                    $"UpgradeManager: {curveName} curve is missing. " +
                    "Upgrade bonus will be 0.");

                return 0;
            }

            return curve.GetValueAtLevel(level);
        }
    }
}