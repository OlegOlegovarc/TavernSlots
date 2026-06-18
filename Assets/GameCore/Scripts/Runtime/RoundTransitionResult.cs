using System;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class RoundTransitionResult
    {
        public bool success;
        public string reason;

        public MatchPhase phaseBefore;
        public MatchPhase phaseAfter;

        public int roundIndexBefore;
        public int roundIndexAfter;

        public bool enteredBuildPhase;
        public bool advancedRoundIndex;
        public bool createdRoundStates;
        public bool startedFirstTurn;

        public string firstPlayerId;

        public int playerAMaxHealth;
        public int playerAShieldCapacity;

        public int playerBMaxHealth;
        public int playerBShieldCapacity;

        public TurnStartResult firstTurnStartResult;

        public RoundTransitionResult(
            MatchPhase phaseBefore,
            int roundIndexBefore)
        {
            this.phaseBefore = phaseBefore;
            this.roundIndexBefore = roundIndexBefore;

            phaseAfter = phaseBefore;
            roundIndexAfter = roundIndexBefore;

            success = false;
            reason = string.Empty;
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