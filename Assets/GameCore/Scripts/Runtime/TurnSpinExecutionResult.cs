using System;
using System.Collections.Generic;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class TurnSpinExecutionResult
    {
        public string actingPlayerId;
        public string opponentPlayerId;

        public bool success;
        public string reason;

        public MatchPhase phaseBefore;
        public MatchPhase phaseAfter;

        public TurnActionValidationResult actionValidationResult;
        public TurnResultData turnResultData;

        public List<EffectApplicationResult> applicationResults =
            new List<EffectApplicationResult>();

        public TurnSpinExecutionResult(
            string actingPlayerId,
            string opponentPlayerId,
            MatchPhase phaseBefore)
        {
            this.actingPlayerId = actingPlayerId;
            this.opponentPlayerId = opponentPlayerId;
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

        public void AddApplicationResults(
            List<EffectApplicationResult> results)
        {
            if (results == null || results.Count == 0)
                return;

            applicationResults.AddRange(results);
        }
    }
}