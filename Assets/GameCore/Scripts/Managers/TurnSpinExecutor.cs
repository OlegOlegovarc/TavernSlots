using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Data;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class TurnSpinExecutor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnActionValidator turnActionValidator;
        [SerializeField] private SpinManager spinManager;
        [SerializeField] private GroupResolver groupResolver;
        [SerializeField] private ResolveManager resolveManager;
        [SerializeField] private EffectApplier effectApplier;

        public TurnSpinExecutionResult ExecuteSpin(
            MatchState matchState,
            string requestingPlayerId)
        {
            string opponentPlayerId = matchState != null
                ? matchState.GetOpponentPlayerId(requestingPlayerId)
                : string.Empty;

            MatchPhase phaseBefore = matchState != null
                ? matchState.currentPhase
                : MatchPhase.None;

            TurnSpinExecutionResult executionResult =
                new TurnSpinExecutionResult(
                    requestingPlayerId,
                    opponentPlayerId,
                    phaseBefore);

            if (!ValidateReferences(executionResult))
                return executionResult;

            if (matchState == null)
            {
                executionResult.Deny("MatchState is null.");
                return executionResult;
            }

            TurnActionValidationResult validationResult =
                turnActionValidator.ValidateStartSpin(
                    matchState,
                    requestingPlayerId);

            executionResult.actionValidationResult = validationResult;

            if (validationResult == null || !validationResult.isValid)
            {
                executionResult.Deny(
                    validationResult != null
                        ? validationResult.reason
                        : "Start spin validation failed.");

                return executionResult;
            }

            PlayerMatchState actingPlayer =
                matchState.GetPlayerMatchState(requestingPlayerId);

            PlayerRoundState actingPlayerRound =
                matchState.GetPlayerRoundState(requestingPlayerId);

            PlayerRoundState opponentRound =
                matchState.GetPlayerRoundState(opponentPlayerId);

            if (actingPlayer == null)
            {
                executionResult.Deny(
                    $"Acting player match state not found: {requestingPlayerId}");

                return executionResult;
            }

            if (actingPlayerRound == null)
            {
                executionResult.Deny(
                    $"Acting player round state not found: {requestingPlayerId}");

                return executionResult;
            }

            if (opponentRound == null)
            {
                executionResult.Deny(
                    $"Opponent round state not found: {opponentPlayerId}");

                return executionResult;
            }

            RoundConfig roundConfig = matchState.GetCurrentRoundConfig();

            if (roundConfig == null)
            {
                executionResult.Deny(
                    $"RoundConfig not found for round {matchState.currentRoundIndex}.");

                return executionResult;
            }

            if (!ValidateSymbolPool(
                    matchState,
                    actingPlayer,
                    executionResult))
            {
                return executionResult;
            }

            matchState.SetPhase(MatchPhase.SpinPhase);

            SpinResult spinResult =
                spinManager.CreateSpinResult(
                    actingPlayer,
                    roundConfig);

            if (spinResult == null || !spinResult.IsValid())
            {
                matchState.SetPhase(phaseBefore);

                executionResult.phaseAfter = matchState.currentPhase;
                executionResult.Deny("SpinManager returned an invalid SpinResult.");

                return executionResult;
            }

            List<ResolvedSymbolGroup> resolvedGroups =
                groupResolver.ResolveGroups(spinResult);

            List<EffectPacket> allEffectPackets =
                resolveManager.CreateEffectPacketsFromGroups(
                    actingPlayer,
                    opponentPlayerId,
                    resolvedGroups);

            TurnResultData turnResultData =
                new TurnResultData(
                    requestingPlayerId,
                    opponentPlayerId);

            turnResultData.SetSpinResult(spinResult);

            for (int i = 0; i < resolvedGroups.Count; i++)
                turnResultData.AddResolvedGroup(resolvedGroups[i]);

            matchState.SetPhase(MatchPhase.ResolvePhase);

            List<EffectApplicationResult> applicationResults =
                effectApplier.ApplyEffectPackets(
                    matchState,
                    allEffectPackets);

            executionResult.AddApplicationResults(applicationResults);

            AddAppliedPacketsToTurnResult(
                turnResultData,
                applicationResults);

            HandleRoundAndMatchEnd(
                matchState,
                requestingPlayerId,
                opponentPlayerId,
                turnResultData);

            executionResult.turnResultData = turnResultData;
            executionResult.phaseAfter = matchState.currentPhase;

            if (turnResultData.matchEnded)
            {
                executionResult.Allow(
                    "Spin executed successfully. Match ended.");
            }
            else if (turnResultData.roundEnded)
            {
                executionResult.Allow(
                    "Spin executed successfully. Round ended.");
            }
            else
            {
                executionResult.Allow(
                    "Spin executed successfully. Awaiting turn presentation.");
            }

            return executionResult;
        }

        private bool ValidateReferences(
            TurnSpinExecutionResult executionResult)
        {
            if (turnActionValidator == null)
            {
                executionResult.Deny(
                    "TurnActionValidator reference is missing.");

                return false;
            }

            if (spinManager == null)
            {
                executionResult.Deny(
                    "SpinManager reference is missing.");

                return false;
            }

            if (groupResolver == null)
            {
                executionResult.Deny(
                    "GroupResolver reference is missing.");

                return false;
            }

            if (resolveManager == null)
            {
                executionResult.Deny(
                    "ResolveManager reference is missing.");

                return false;
            }

            if (effectApplier == null)
            {
                executionResult.Deny(
                    "EffectApplier reference is missing.");

                return false;
            }

            return true;
        }

        private bool ValidateSymbolPool(
            MatchState matchState,
            PlayerMatchState actingPlayer,
            TurnSpinExecutionResult executionResult)
        {
            if (actingPlayer.ownedSymbolIds == null
                || actingPlayer.ownedSymbolIds.Count == 0)
            {
                executionResult.Deny("Acting player's symbol pool is empty.");
                return false;
            }

            int minimumPoolSize = matchState.matchConfig != null
                ? matchState.matchConfig.minSymbolPoolSize
                : 1;

            if (actingPlayer.ownedSymbolIds.Count < minimumPoolSize)
            {
                executionResult.Deny(
                    $"Symbol pool is below the minimum size. " +
                    $"Current={actingPlayer.ownedSymbolIds.Count}, " +
                    $"Required={minimumPoolSize}");

                return false;
            }

            return true;
        }

        private void AddAppliedPacketsToTurnResult(
            TurnResultData turnResultData,
            List<EffectApplicationResult> applicationResults)
        {
            if (turnResultData == null
                || applicationResults == null
                || applicationResults.Count == 0)
            {
                return;
            }

            for (int i = 0; i < applicationResults.Count; i++)
            {
                EffectApplicationResult applicationResult =
                    applicationResults[i];

                if (applicationResult == null
                    || applicationResult.sourcePacket == null)
                {
                    continue;
                }

                turnResultData.AddEffectPacket(
                    applicationResult.sourcePacket);
            }
        }

        private void HandleRoundAndMatchEnd(
            MatchState matchState,
            string actingPlayerId,
            string opponentPlayerId,
            TurnResultData turnResultData)
        {
            PlayerRoundState actingPlayerRound =
                matchState.GetPlayerRoundState(actingPlayerId);

            PlayerRoundState opponentRound =
                matchState.GetPlayerRoundState(opponentPlayerId);

            if (actingPlayerRound == null || opponentRound == null)
                return;

            bool actingPlayerDied = actingPlayerRound.IsDead();
            bool opponentDied = opponentRound.IsDead();

            if (!actingPlayerDied && !opponentDied)
                return;

            if (actingPlayerDied && opponentDied)
            {
                Debug.LogWarning(
                    "TurnSpinExecutor: Both players are dead. " +
                    "Draw handling is not implemented yet.");

                return;
            }

            string winnerPlayerId = opponentDied
                ? actingPlayerId
                : opponentPlayerId;

            string loserPlayerId = opponentDied
                ? opponentPlayerId
                : actingPlayerId;

            PlayerMatchState winnerMatchState =
                matchState.GetPlayerMatchState(winnerPlayerId);

            if (winnerMatchState == null)
            {
                Debug.LogWarning(
                    $"TurnSpinExecutor: Winner match state not found: {winnerPlayerId}");

                return;
            }

            winnerMatchState.roundWins++;

            turnResultData.SetRoundEnded(
                winnerPlayerId,
                loserPlayerId);

            matchState.SetActivePlayer(string.Empty);

            if (matchState.HasPlayerWonMatch(winnerPlayerId))
            {
                turnResultData.SetMatchEnded(
                    winnerPlayerId,
                    loserPlayerId);

                matchState.EndMatch();
                return;
            }

            matchState.SetPhase(MatchPhase.RoundEnd);
        }
    }
}