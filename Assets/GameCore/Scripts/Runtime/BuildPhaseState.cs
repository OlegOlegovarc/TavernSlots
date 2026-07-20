using System;
using System.Collections.Generic;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class BuildPhaseState
    {
        public int sourceRoundIndex;
        public bool advanceRoundOnReady;

        public List<BuildPlayerOfferState> playerOfferStates =
            new List<BuildPlayerOfferState>();

        public BuildPhaseState(
            int sourceRoundIndex,
            bool advanceRoundOnReady)
        {
            this.sourceRoundIndex = sourceRoundIndex;
            this.advanceRoundOnReady = advanceRoundOnReady;
        }

        public BuildPlayerOfferState GetPlayerOfferState(
            string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                return null;

            for (int i = 0; i < playerOfferStates.Count; i++)
            {
                BuildPlayerOfferState state =
                    playerOfferStates[i];

                if (state != null && state.playerId == playerId)
                    return state;
            }

            return null;
        }

        public BuildPlayerOfferState EnsurePlayerOfferState(
            string playerId)
        {
            BuildPlayerOfferState existingState =
                GetPlayerOfferState(playerId);

            if (existingState != null)
                return existingState;

            BuildPlayerOfferState newState =
                new BuildPlayerOfferState(playerId);

            playerOfferStates.Add(newState);

            return newState;
        }

        public bool IsPlayerReady(string playerId)
        {
            BuildPlayerOfferState state =
                GetPlayerOfferState(playerId);

            return state != null && state.isReady;
        }

        public void SetPlayerReady(
            string playerId,
            bool isReady)
        {
            BuildPlayerOfferState state =
                EnsurePlayerOfferState(playerId);

            state.isReady = isReady;
        }

        public bool AreBothPlayersReady(
            MatchState matchState)
        {
            if (matchState == null
                || matchState.playerA == null
                || matchState.playerB == null)
            {
                return false;
            }

            return IsPlayerReady(matchState.playerA.playerId)
                   && IsPlayerReady(matchState.playerB.playerId);
        }
    }

    [Serializable]
    public class BuildPlayerOfferState
    {
        public string playerId;
        public bool isReady;

        public List<string> offeredSymbolIds =
            new List<string>();

        public List<string> offeredItemIds =
            new List<string>();

        public BuildPlayerOfferState(string playerId)
        {
            this.playerId = playerId;
            isReady = false;
        }
    }
}