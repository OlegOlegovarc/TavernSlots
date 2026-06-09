using System;
using SlotsTavern.Core;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class StatusTickResult
    {
        public string targetPlayerId;

        public string statusId;
        public int statusPower;

        public int durationBeforeTick;
        public int durationAfterTick;

        public DamageType damageType;

        public int requestedDamage;
        public int actualHealthDamage;

        public bool targetDied;
        public bool statusRemovedAfterTick;

        public StatusTickResult(
            string targetPlayerId,
            string statusId,
            int statusPower,
            int durationBeforeTick,
            DamageType damageType)
        {
            this.targetPlayerId = targetPlayerId;
            this.statusId = statusId;
            this.statusPower = statusPower;
            this.durationBeforeTick = durationBeforeTick;
            this.damageType = damageType;

            requestedDamage = statusPower;
        }
    }
}