using System;
using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Core;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class PlayerRoundState
    {
        public string playerId;

        public int maxHealth;
        public int currentHealth;

        public int shieldCapacity;

        public int physicalShield;
        public int magicalShield;
        public int otherShield;

        public List<AppliedStatusState> statuses = new List<AppliedStatusState>();

        public List<string> usedItemsThisTurn = new List<string>();
        public List<string> usedItemsThisRound = new List<string>();

        public PlayerRoundState(string playerId, int maxHealth, int shieldCapacity)
        {
            this.playerId = playerId;
            this.maxHealth = maxHealth;
            this.currentHealth = maxHealth;
            this.shieldCapacity = shieldCapacity;

            physicalShield = 0;
            magicalShield = 0;
            otherShield = 0;
        }

        public bool IsDead()
        {
            return currentHealth <= 0;
        }

        public void Heal(int value)
        {
            if (value <= 0)
                return;

            currentHealth = Mathf.Min(currentHealth + value, maxHealth);
        }

        public void AddShield(DamageType damageType, int value)
        {
            if (value <= 0)
                return;

            switch (damageType)
            {
                case DamageType.Physical:
                    physicalShield = Mathf.Min(physicalShield + value, shieldCapacity);
                    break;

                case DamageType.Magical:
                    magicalShield = Mathf.Min(magicalShield + value, shieldCapacity);
                    break;

                case DamageType.Other:
                    otherShield = Mathf.Min(otherShield + value, shieldCapacity);
                    break;
            }
        }

        public int ApplyDamage(DamageType damageType, int incomingDamage)
        {
            if (incomingDamage <= 0)
                return 0;

            int remainingDamage = incomingDamage;

            switch (damageType)
            {
                case DamageType.Physical:
                    remainingDamage = ApplyShieldDamage(ref physicalShield, incomingDamage);
                    break;

                case DamageType.Magical:
                    remainingDamage = ApplyShieldDamage(ref magicalShield, incomingDamage);
                    break;

                case DamageType.Other:
                    remainingDamage = ApplyShieldDamage(ref otherShield, incomingDamage);
                    break;
            }

            if (remainingDamage > 0)
                currentHealth = Mathf.Max(0, currentHealth - remainingDamage);

            return remainingDamage;
        }

        public void ClearTemporaryShields()
        {
            physicalShield = 0;
            magicalShield = 0;
            otherShield = 0;
        }

        public void ResetTurnItemUsage()
        {
            usedItemsThisTurn.Clear();
        }

        public bool HasUsedItemThisTurn(string itemId)
        {
            return usedItemsThisTurn.Contains(itemId);
        }

        public bool HasUsedItemThisRound(string itemId)
        {
            return usedItemsThisRound.Contains(itemId);
        }

        public void MarkItemUsedThisTurn(string itemId)
        {
            if (!usedItemsThisTurn.Contains(itemId))
                usedItemsThisTurn.Add(itemId);
        }

        public void MarkItemUsedThisRound(string itemId)
        {
            if (!usedItemsThisRound.Contains(itemId))
                usedItemsThisRound.Add(itemId);
        }

        private int ApplyShieldDamage(ref int shieldValue, int incomingDamage)
        {
            if (shieldValue <= 0)
                return incomingDamage;

            int absorbed = Mathf.Min(shieldValue, incomingDamage);
            shieldValue -= absorbed;

            return incomingDamage - absorbed;
        }
    }

    [Serializable]
    public class AppliedStatusState
    {
        public string statusId;
        public int power;
        public int remainingDuration;

        public AppliedStatusState(string statusId, int power, int remainingDuration)
        {
            this.statusId = statusId;
            this.power = power;
            this.remainingDuration = remainingDuration;
        }
    }
}