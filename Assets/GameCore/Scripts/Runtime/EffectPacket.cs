using System;
using System.Collections.Generic;
using SlotsTavern.Core;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class EffectPacket
    {
        public string sourcePlayerId;
        public string targetPlayerId;

        public EffectType effectType = EffectType.None;
        public EffectTarget effectTarget = EffectTarget.Opponent;

        public int value;
        public DamageType damageType = DamageType.Physical;

        public string statusId;
        public int statusDuration;

        public string sourceSymbolId;
        public string sourceItemId;

        public bool isFromSymbolGroup;
        public int sourceGroupStartIndex;
        public int sourceGroupLength;

        public List<int> sourceSlotIndices = new List<int>();

        public EffectPacket()
        {
        }

        public static EffectPacket FromSymbolGroup(
            string sourcePlayerId,
            string targetPlayerId,
            string sourceSymbolId,
            EffectType effectType,
            EffectTarget effectTarget,
            int value,
            DamageType damageType,
            string statusId,
            int statusDuration,
            ResolvedSymbolGroup group)
        {
            EffectPacket packet = new EffectPacket
            {
                sourcePlayerId = sourcePlayerId,
                targetPlayerId = targetPlayerId,
                sourceSymbolId = sourceSymbolId,
                sourceItemId = string.Empty,
                effectType = effectType,
                effectTarget = effectTarget,
                value = value,
                damageType = damageType,
                statusId = statusId,
                statusDuration = statusDuration,
                isFromSymbolGroup = true,
                sourceGroupStartIndex = group != null ? group.startIndex : -1,
                sourceGroupLength = group != null ? group.length : 0,
                sourceSlotIndices = group != null
                    ? new List<int>(group.affectedSlotIndices)
                    : new List<int>()
            };

            return packet;
        }

        public static EffectPacket FromItem(
            string sourcePlayerId,
            string targetPlayerId,
            string sourceItemId,
            EffectType effectType,
            EffectTarget effectTarget,
            int value,
            DamageType damageType,
            string statusId,
            int statusDuration)
        {
            EffectPacket packet = new EffectPacket
            {
                sourcePlayerId = sourcePlayerId,
                targetPlayerId = targetPlayerId,
                sourceSymbolId = string.Empty,
                sourceItemId = sourceItemId,
                effectType = effectType,
                effectTarget = effectTarget,
                value = value,
                damageType = damageType,
                statusId = statusId,
                statusDuration = statusDuration,
                isFromSymbolGroup = false,
                sourceGroupStartIndex = -1,
                sourceGroupLength = 0,
                sourceSlotIndices = new List<int>()
            };

            return packet;
        }
    }
}