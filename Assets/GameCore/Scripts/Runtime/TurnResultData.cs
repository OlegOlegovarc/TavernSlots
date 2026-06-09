using System;
using System.Collections.Generic;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class TurnResultData
    {
        public string actingPlayerId;
        public string targetPlayerId;

        public SpinResult spinResult;

        public List<ResolvedSymbolGroup> resolvedGroups = new List<ResolvedSymbolGroup>();
        public List<EffectPacket> effectPackets = new List<EffectPacket>();

        public bool roundEnded;
        public string roundWinnerPlayerId;
        public string roundLoserPlayerId;

        public bool matchEnded;
        public string matchWinnerPlayerId;
        public string matchLoserPlayerId;

        public TurnResultData(string actingPlayerId, string targetPlayerId)
        {
            this.actingPlayerId = actingPlayerId;
            this.targetPlayerId = targetPlayerId;
        }

        public void SetSpinResult(SpinResult spinResult)
        {
            this.spinResult = spinResult;
        }

        public void AddResolvedGroup(ResolvedSymbolGroup group)
        {
            if (group != null)
                resolvedGroups.Add(group);
        }

        public void AddEffectPacket(EffectPacket packet)
        {
            if (packet != null)
                effectPackets.Add(packet);
        }

        public void SetRoundEnded(string winnerPlayerId, string loserPlayerId)
        {
            roundEnded = true;
            roundWinnerPlayerId = winnerPlayerId;
            roundLoserPlayerId = loserPlayerId;
        }

        public void SetMatchEnded(string winnerPlayerId, string loserPlayerId)
        {
            matchEnded = true;
            matchWinnerPlayerId = winnerPlayerId;
            matchLoserPlayerId = loserPlayerId;
        }
    }
}