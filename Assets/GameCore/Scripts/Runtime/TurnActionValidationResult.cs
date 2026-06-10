using System;
using SlotsTavern.Core;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class TurnActionValidationResult
    {
        public TurnActionType actionType;

        public string requestingPlayerId;
        public string activePlayerId;

        public MatchPhase currentPhase;

        public bool isValid;
        public string reason;

        public TurnActionValidationResult(
            TurnActionType actionType,
            string requestingPlayerId,
            string activePlayerId,
            MatchPhase currentPhase)
        {
            this.actionType = actionType;
            this.requestingPlayerId = requestingPlayerId;
            this.activePlayerId = activePlayerId;
            this.currentPhase = currentPhase;

            isValid = false;
            reason = string.Empty;
        }

        public void Allow()
        {
            isValid = true;
            reason = "Allowed.";
        }

        public void Deny(string reason)
        {
            isValid = false;
            this.reason = reason;
        }
    }
}