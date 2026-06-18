using System;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class TurnEndResult
    {
        public bool success;
        public string reason;

        public string endingPlayerId;
        public string nextPlayerId;

        public MatchPhase phaseBefore;
        public MatchPhase phaseAfter;

        public int endingPlayerUsedItemsThisTurnBefore;
        public int endingPlayerUsedItemsThisTurnAfter;

        public int nextPlayerPhysicalShieldBefore;
        public int nextPlayerMagicalShieldBefore;
        public int nextPlayerOtherShieldBefore;

        public int nextPlayerPhysicalShieldAfter;
        public int nextPlayerMagicalShieldAfter;
        public int nextPlayerOtherShieldAfter;

        public bool nextTurnStarted;
        public bool nextPlayerCanAct;

        public bool roundEndedDuringHandoff;
        public bool matchEndedDuringHandoff;

        public TurnStartResult nextTurnStartResult;

        public TurnEndResult(
            string endingPlayerId,
            string nextPlayerId,
            MatchPhase phaseBefore)
        {
            this.endingPlayerId = endingPlayerId;
            this.nextPlayerId = nextPlayerId;
            this.phaseBefore = phaseBefore;

            phaseAfter = phaseBefore;
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