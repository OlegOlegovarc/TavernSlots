using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class TurnManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatusManager statusManager;

        public TurnStartResult BeginTurn(
            MatchState matchState,
            string activePlayerId)
        {
            if (matchState == null)
            {
                Debug.LogWarning("TurnManager: Cannot begin turn. MatchState is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(activePlayerId))
            {
                Debug.LogWarning("TurnManager: Cannot begin turn. Active player id is empty.");
                return null;
            }

            PlayerMatchState activePlayerMatchState =
                matchState.GetPlayerMatchState(activePlayerId);

            PlayerRoundState activePlayerRoundState =
                matchState.GetPlayerRoundState(activePlayerId);

            if (activePlayerMatchState == null)
            {
                Debug.LogWarning($"TurnManager: Active player match state not found: {activePlayerId}");
                return null;
            }

            if (activePlayerRoundState == null)
            {
                Debug.LogWarning($"TurnManager: Active player round state not found: {activePlayerId}");
                return null;
            }

            string opponentPlayerId = matchState.GetOpponentPlayerId(activePlayerId);

            if (string.IsNullOrWhiteSpace(opponentPlayerId))
            {
                Debug.LogWarning($"TurnManager: Opponent player id not found for active player: {activePlayerId}");
                return null;
            }

            TurnStartResult result = new TurnStartResult(
                activePlayerId,
                opponentPlayerId,
                matchState.currentPhase);

            matchState.SetActivePlayer(activePlayerId);
            matchState.SetPhase(MatchPhase.TurnStart);

            ProcessTurnReceivedStatuses(
                matchState,
                activePlayerId,
                result);

            if (activePlayerRoundState.IsDead())
            {
                HandleActivePlayerDeathOnTurnStart(
                    matchState,
                    activePlayerId,
                    opponentPlayerId,
                    result);

                return result;
            }

            matchState.SetPhase(MatchPhase.PreSpinItemPhase);

            result.SetPlayerCanAct(matchState.currentPhase);

            return result;
        }

        public void EndTurn(MatchState matchState)
        {
            if (matchState == null)
            {
                Debug.LogWarning("TurnManager: Cannot end turn. MatchState is null.");
                return;
            }

            matchState.SetActivePlayer(string.Empty);
            matchState.SetPhase(MatchPhase.TurnStart);
        }

        private void ProcessTurnReceivedStatuses(
            MatchState matchState,
            string activePlayerId,
            TurnStartResult result)
        {
            if (statusManager == null)
            {
                Debug.LogWarning("TurnManager: StatusManager reference is missing. Status ticks will not be processed.");
                return;
            }

            List<StatusTickResult> statusTickResults =
                statusManager.ProcessTurnReceivedStatuses(
                    matchState,
                    activePlayerId);

            result.AddStatusTickResults(statusTickResults);
        }

        private void HandleActivePlayerDeathOnTurnStart(
            MatchState matchState,
            string activePlayerId,
            string opponentPlayerId,
            TurnStartResult result)
        {
            matchState.SetActivePlayer(string.Empty);
            matchState.SetPhase(MatchPhase.RoundEnd);

            PlayerMatchState winnerMatchState =
                matchState.GetPlayerMatchState(opponentPlayerId);

            if (winnerMatchState != null)
                winnerMatchState.roundWins++;

            result.SetPlayerDiedOnTurnStart(
                opponentPlayerId,
                activePlayerId,
                matchState.currentPhase);
        }
    }
}