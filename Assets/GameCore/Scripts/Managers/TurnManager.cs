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
                Debug.LogWarning(
                    "TurnManager: Cannot begin turn. MatchState is null.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(activePlayerId))
            {
                Debug.LogWarning(
                    "TurnManager: Cannot begin turn. Active player id is empty.");

                return null;
            }

            if (matchState.isMatchEnded
                || matchState.currentPhase == MatchPhase.MatchEnd)
            {
                Debug.LogWarning(
                    "TurnManager: Cannot begin turn. Match has already ended.");

                return null;
            }

            PlayerMatchState activePlayerMatchState =
                matchState.GetPlayerMatchState(activePlayerId);

            PlayerRoundState activePlayerRoundState =
                matchState.GetPlayerRoundState(activePlayerId);

            if (activePlayerMatchState == null)
            {
                Debug.LogWarning(
                    $"TurnManager: Active player match state not found: " +
                    $"{activePlayerId}");

                return null;
            }

            if (activePlayerRoundState == null)
            {
                Debug.LogWarning(
                    $"TurnManager: Active player round state not found: " +
                    $"{activePlayerId}");

                return null;
            }

            string opponentPlayerId =
                matchState.GetOpponentPlayerId(activePlayerId);

            if (string.IsNullOrWhiteSpace(opponentPlayerId))
            {
                Debug.LogWarning(
                    $"TurnManager: Opponent player id not found for " +
                    $"active player: {activePlayerId}");

                return null;
            }

            TurnStartResult result =
                new TurnStartResult(
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

            result.SetPlayerCanAct(
                matchState.currentPhase);

            return result;
        }

        public TurnEndResult EndTurn(
            MatchState matchState)
        {
            string endingPlayerId = matchState != null
                ? matchState.activePlayerId
                : string.Empty;

            string nextPlayerId =
                matchState != null
                && !string.IsNullOrWhiteSpace(endingPlayerId)
                    ? matchState.GetOpponentPlayerId(endingPlayerId)
                    : string.Empty;

            MatchPhase phaseBefore = matchState != null
                ? matchState.currentPhase
                : MatchPhase.None;

            TurnEndResult result =
                new TurnEndResult(
                    endingPlayerId,
                    nextPlayerId,
                    phaseBefore);

            if (matchState == null)
            {
                result.Deny("MatchState is null.");
                return result;
            }

            if (matchState.isMatchEnded
                || matchState.currentPhase == MatchPhase.MatchEnd)
            {
                result.Deny("Cannot end turn. Match has already ended.");
                return result;
            }

            if (matchState.currentPhase == MatchPhase.RoundEnd)
            {
                result.Deny(
                    "Cannot end turn. Round has already ended.");

                return result;
            }

            if (matchState.currentPhase != MatchPhase.ResolvePhase)
            {
                result.Deny(
                    $"Turn can only end after ResolvePhase. " +
                    $"Current phase: {matchState.currentPhase}");

                return result;
            }

            if (string.IsNullOrWhiteSpace(endingPlayerId))
            {
                result.Deny("There is no active player.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(nextPlayerId))
            {
                result.Deny(
                    $"Next player was not found for: {endingPlayerId}");

                return result;
            }

            PlayerRoundState endingPlayerRoundState =
                matchState.GetPlayerRoundState(endingPlayerId);

            PlayerRoundState nextPlayerRoundState =
                matchState.GetPlayerRoundState(nextPlayerId);

            if (endingPlayerRoundState == null)
            {
                result.Deny(
                    $"Ending player round state not found: " +
                    $"{endingPlayerId}");

                return result;
            }

            if (nextPlayerRoundState == null)
            {
                result.Deny(
                    $"Next player round state not found: " +
                    $"{nextPlayerId}");

                return result;
            }

            CaptureStateBeforeTurnEnd(
                result,
                endingPlayerRoundState,
                nextPlayerRoundState);

            endingPlayerRoundState.ResetTurnItemUsage();

            /*
             * Temporary shields live through:
             *
             * 1. The owner's current turn.
             * 2. The opponent's following turn.
             *
             * Therefore, when the current player finishes their turn,
             * the NEXT player's old shields have completed their lifetime.
             */
            nextPlayerRoundState.ClearTemporaryShields();

            CaptureStateAfterTurnEnd(
                result,
                endingPlayerRoundState,
                nextPlayerRoundState);

            matchState.SetActivePlayer(string.Empty);
            matchState.SetPhase(MatchPhase.TurnStart);

            result.phaseAfter = matchState.currentPhase;

            result.Allow(
                "Turn ended successfully. Next player is ready.");

            return result;
        }

        public TurnEndResult EndTurnAndBeginNextTurn(
            MatchState matchState)
        {
            TurnEndResult result =
                EndTurn(matchState);

            if (result == null || !result.success)
                return result;

            TurnStartResult nextTurnStartResult =
                BeginTurn(
                    matchState,
                    result.nextPlayerId);

            result.nextTurnStartResult =
                nextTurnStartResult;

            result.nextTurnStarted =
                nextTurnStartResult != null;

            result.phaseAfter =
                matchState.currentPhase;

            result.matchEndedDuringHandoff =
                matchState.isMatchEnded
                || matchState.currentPhase == MatchPhase.MatchEnd;

            if (nextTurnStartResult == null)
            {
                result.nextPlayerCanAct = false;

                result.Deny(
                    "Current turn ended, but the next turn could not start.");

                return result;
            }

            result.nextPlayerCanAct =
                nextTurnStartResult.canActivePlayerAct;

            result.roundEndedDuringHandoff =
                nextTurnStartResult.roundEnded;

            if (result.matchEndedDuringHandoff)
            {
                result.Allow(
                    "Turn ended. The next player died on turn start. " +
                    "Match ended.");

                return result;
            }

            if (result.roundEndedDuringHandoff)
            {
                result.Allow(
                    "Turn ended. The next player died on turn start. " +
                    "Round ended.");

                return result;
            }

            result.Allow(
                "Turn ended and the next player can act.");

            return result;
        }

        private void ProcessTurnReceivedStatuses(
            MatchState matchState,
            string activePlayerId,
            TurnStartResult result)
        {
            if (statusManager == null)
            {
                Debug.LogWarning(
                    "TurnManager: StatusManager reference is missing. " +
                    "Status ticks will not be processed.");

                return;
            }

            List<StatusTickResult> statusTickResults =
                statusManager.ProcessTurnReceivedStatuses(
                    matchState,
                    activePlayerId);

            result.AddStatusTickResults(
                statusTickResults);
        }

        private void HandleActivePlayerDeathOnTurnStart(
            MatchState matchState,
            string activePlayerId,
            string opponentPlayerId,
            TurnStartResult result)
        {
            matchState.SetActivePlayer(string.Empty);

            PlayerMatchState winnerMatchState =
                matchState.GetPlayerMatchState(opponentPlayerId);

            if (winnerMatchState == null)
            {
                Debug.LogWarning(
                    $"TurnManager: Winner match state not found: " +
                    $"{opponentPlayerId}");

                matchState.SetPhase(MatchPhase.RoundEnd);

                result.SetPlayerDiedOnTurnStart(
                    opponentPlayerId,
                    activePlayerId,
                    matchState.currentPhase);

                return;
            }

            winnerMatchState.roundWins++;

            if (matchState.HasPlayerWonMatch(opponentPlayerId))
            {
                matchState.EndMatch();
            }
            else
            {
                matchState.SetPhase(MatchPhase.RoundEnd);
            }

            result.SetPlayerDiedOnTurnStart(
                opponentPlayerId,
                activePlayerId,
                matchState.currentPhase);
        }

        private void CaptureStateBeforeTurnEnd(
            TurnEndResult result,
            PlayerRoundState endingPlayerRoundState,
            PlayerRoundState nextPlayerRoundState)
        {
            result.endingPlayerUsedItemsThisTurnBefore =
                endingPlayerRoundState.usedItemsThisTurn != null
                    ? endingPlayerRoundState.usedItemsThisTurn.Count
                    : 0;

            result.nextPlayerPhysicalShieldBefore =
                nextPlayerRoundState.physicalShield;

            result.nextPlayerMagicalShieldBefore =
                nextPlayerRoundState.magicalShield;

            result.nextPlayerOtherShieldBefore =
                nextPlayerRoundState.otherShield;
        }

        private void CaptureStateAfterTurnEnd(
            TurnEndResult result,
            PlayerRoundState endingPlayerRoundState,
            PlayerRoundState nextPlayerRoundState)
        {
            result.endingPlayerUsedItemsThisTurnAfter =
                endingPlayerRoundState.usedItemsThisTurn != null
                    ? endingPlayerRoundState.usedItemsThisTurn.Count
                    : 0;

            result.nextPlayerPhysicalShieldAfter =
                nextPlayerRoundState.physicalShield;

            result.nextPlayerMagicalShieldAfter =
                nextPlayerRoundState.magicalShield;

            result.nextPlayerOtherShieldAfter =
                nextPlayerRoundState.otherShield;
        }
    }
}