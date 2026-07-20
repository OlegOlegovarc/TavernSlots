using System;
using SlotsTavern.Core;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class BuildActionResult
    {
        public BuildActionType actionType;

        public bool success;
        public string reason;

        public string playerId;
        public string targetId;

        public BuildOfferType offerType;
        public BuildPlayerStatType playerStatType;

        public int cost;
        public int crystalsBefore;
        public int crystalsAfter;

        public int levelBefore;
        public int levelAfter;

        public int ownedSymbolsBefore;
        public int ownedSymbolsAfter;

        public int ownedItemsBefore;
        public int ownedItemsAfter;

        public int offeredSymbolsBefore;
        public int offeredSymbolsAfter;

        public int offeredItemsBefore;
        public int offeredItemsAfter;

        public bool playerReady;
        public bool bothPlayersReady;

        public bool startedRound;
        public RoundTransitionResult roundTransitionResult;

        public TurnActionValidationResult actionValidationResult;

        public BuildActionResult(
            BuildActionType actionType,
            string playerId,
            string targetId)
        {
            this.actionType = actionType;
            this.playerId = playerId;
            this.targetId = targetId;

            success = false;
            reason = string.Empty;
            offerType = BuildOfferType.None;
            playerStatType = BuildPlayerStatType.None;
        }

        public void Allow(string successReason)
        {
            success = true;
            reason = successReason;
        }

        public void Deny(string failureReason)
        {
            success = false;
            reason = failureReason;
        }
    }
}