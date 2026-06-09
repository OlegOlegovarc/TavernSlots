using System;
using SlotsTavern.Core;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class EffectApplicationResult
    {
        public EffectPacket sourcePacket;

        public string sourcePlayerId;
        public string targetPlayerId;

        public EffectType effectType;
        public DamageType damageType;

        public int requestedValue;
        public int actualHealthDamage;
        public int actualHeal;
        public int actualShieldGain;
        public int actualCrystalGain;

        public string appliedStatusId;
        public int appliedStatusPower;
        public int appliedStatusDuration;

        public bool targetDied;

        public EffectApplicationResult(EffectPacket sourcePacket)
        {
            this.sourcePacket = sourcePacket;

            if (sourcePacket == null)
                return;

            sourcePlayerId = sourcePacket.sourcePlayerId;
            targetPlayerId = sourcePacket.targetPlayerId;
            effectType = sourcePacket.effectType;
            damageType = sourcePacket.damageType;
            requestedValue = sourcePacket.value;
        }
    }
}