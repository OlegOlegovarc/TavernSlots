using System;
using System.Collections.Generic;

namespace SlotsTavern.Runtime
{
    [Serializable]
    public class TurnStartResult
    {
        public string activePlayerId;
        public string opponentPlayerId;

        public MatchPhase phaseBefore;
        public MatchPhase phaseAfter;

        public List<StatusTickResult> statusTickResults = new List<StatusTickResult>();

        public bool activePlayerDiedOnTurnStart;
        public bool roundEnded;

        public string roundWinnerPlayerId;
        public string roundLoserPlayerId;

        public bool canActivePlayerAct;

        public TurnStartResult(
            string activePlayerId,
            string opponentPlayerId,
            MatchPhase phaseBefore)
        {
            this.activePlayerId = activePlayerId;
            this.opponentPlayerId = opponentPlayerId;
            this.phaseBefore = phaseBefore;

            phaseAfter = phaseBefore;
            canActivePlayerAct = false;
        }

        public void AddStatusTickResults(List<StatusTickResult> results)
        {
            if (results == null || results.Count == 0)
                return;

            statusTickResults.AddRange(results);
        }

        public void SetPlayerCanAct(MatchPhase phaseAfter)
        {
            this.phaseAfter = phaseAfter;
            canActivePlayerAct = true;
            activePlayerDiedOnTurnStart = false;
            roundEnded = false;
        }

        public void SetPlayerDiedOnTurnStart(
            string winnerPlayerId,
            string loserPlayerId,
            MatchPhase phaseAfter)
        {
            this.phaseAfter = phaseAfter;

            activePlayerDiedOnTurnStart = true;
            roundEnded = true;
            canActivePlayerAct = false;

            roundWinnerPlayerId = winnerPlayerId;
            roundLoserPlayerId = loserPlayerId;
        }
    }
}