using System;
using System.Collections.Generic;
using SlotsTavern.Core;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class ItemUseResult
    {
        public string playerId;
        public string opponentPlayerId;
        public string itemId;

        public bool success;
        public string reason;

        public int activationCost;
        public int crystalsBefore;
        public int crystalsAfter;

        public ItemUsePolicy usePolicy;

        public TurnActionValidationResult actionValidationResult;
        public List<EffectPacket> effectPackets = new List<EffectPacket>();

        public ItemUseResult(string playerId, string opponentPlayerId, string itemId)
        {
            this.playerId = playerId;
            this.opponentPlayerId = opponentPlayerId;
            this.itemId = itemId;

            success = false;
            reason = string.Empty;
        }

        public void Allow()
        {
            success = true;
            reason = "Item used successfully.";
        }

        public void Deny(string reason)
        {
            success = false;
            this.reason = reason;
        }

        public void AddEffectPacket(EffectPacket packet)
        {
            if (packet != null)
                effectPackets.Add(packet);
        }
    }
}