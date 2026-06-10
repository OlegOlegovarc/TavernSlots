using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class TurnActionValidator : MonoBehaviour
    {
        public TurnActionValidationResult ValidateAction(
            MatchState matchState,
            string requestingPlayerId,
            TurnActionType actionType)
        {
            if (matchState == null)
            {
                TurnActionValidationResult nullStateResult =
                    new TurnActionValidationResult(
                        actionType,
                        requestingPlayerId,
                        string.Empty,
                        MatchPhase.None);

                nullStateResult.Deny("MatchState is null.");
                return nullStateResult;
            }

            TurnActionValidationResult result =
                new TurnActionValidationResult(
                    actionType,
                    requestingPlayerId,
                    matchState.activePlayerId,
                    matchState.currentPhase);

            if (string.IsNullOrWhiteSpace(requestingPlayerId))
            {
                result.Deny("Requesting player id is empty.");
                return result;
            }

            if (matchState.isMatchEnded)
            {
                result.Deny("Match has already ended.");
                return result;
            }

            if (actionType == TurnActionType.None)
            {
                result.Deny("Action type is None.");
                return result;
            }

            if (!IsKnownPlayer(matchState, requestingPlayerId))
            {
                result.Deny($"Requesting player is not part of this match: {requestingPlayerId}");
                return result;
            }

            if (IsBuildPhaseAction(actionType))
                return ValidateBuildPhaseAction(matchState, requestingPlayerId, actionType, result);

            if (IsTurnAction(actionType))
                return ValidateTurnAction(matchState, requestingPlayerId, actionType, result);

            result.Deny($"Unsupported action type: {actionType}");
            return result;
        }

        public TurnActionValidationResult ValidateUseItem(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.UseItem);
        }

        public TurnActionValidationResult ValidateStartSpin(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.StartSpin);
        }

        public TurnActionValidationResult ValidateReadyBuild(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.ReadyBuild);
        }

        public TurnActionValidationResult ValidateBuySymbol(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.BuySymbol);
        }

        public TurnActionValidationResult ValidateBuyItem(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.BuyItem);
        }

        public TurnActionValidationResult ValidateRemoveSymbol(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.RemoveSymbol);
        }

        public TurnActionValidationResult ValidateUpgradeSymbol(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.UpgradeSymbol);
        }

        public TurnActionValidationResult ValidateUpgradeItem(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.UpgradeItem);
        }

        public TurnActionValidationResult ValidateUpgradePlayerStat(
            MatchState matchState,
            string requestingPlayerId)
        {
            return ValidateAction(
                matchState,
                requestingPlayerId,
                TurnActionType.UpgradePlayerStat);
        }

        private TurnActionValidationResult ValidateTurnAction(
            MatchState matchState,
            string requestingPlayerId,
            TurnActionType actionType,
            TurnActionValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(matchState.activePlayerId))
            {
                result.Deny("There is no active player.");
                return result;
            }

            if (matchState.activePlayerId != requestingPlayerId)
            {
                result.Deny(
                    $"Only the active player can perform this action. Active={matchState.activePlayerId}, Requesting={requestingPlayerId}");

                return result;
            }

            PlayerRoundState requestingRoundState =
                matchState.GetPlayerRoundState(requestingPlayerId);

            if (requestingRoundState == null)
            {
                result.Deny($"Requesting player round state not found: {requestingPlayerId}");
                return result;
            }

            if (requestingRoundState.IsDead())
            {
                result.Deny("Requesting player is dead.");
                return result;
            }

            switch (actionType)
            {
                case TurnActionType.UseItem:
                    return ValidateUseItemPhase(matchState, result);

                case TurnActionType.StartSpin:
                    return ValidateStartSpinPhase(matchState, result);

                default:
                    result.Deny($"Unsupported turn action: {actionType}");
                    return result;
            }
        }

        private TurnActionValidationResult ValidateBuildPhaseAction(
            MatchState matchState,
            string requestingPlayerId,
            TurnActionType actionType,
            TurnActionValidationResult result)
        {
            if (matchState.currentPhase != MatchPhase.BuildPhase)
            {
                result.Deny($"Build action is only allowed during BuildPhase. Current phase: {matchState.currentPhase}");
                return result;
            }

            PlayerMatchState requestingMatchState =
                matchState.GetPlayerMatchState(requestingPlayerId);

            if (requestingMatchState == null)
            {
                result.Deny($"Requesting player match state not found: {requestingPlayerId}");
                return result;
            }

            result.Allow();
            return result;
        }

        private TurnActionValidationResult ValidateUseItemPhase(
            MatchState matchState,
            TurnActionValidationResult result)
        {
            if (matchState.currentPhase != MatchPhase.PreSpinItemPhase)
            {
                result.Deny($"Item use is only allowed during PreSpinItemPhase. Current phase: {matchState.currentPhase}");
                return result;
            }

            result.Allow();
            return result;
        }

        private TurnActionValidationResult ValidateStartSpinPhase(
            MatchState matchState,
            TurnActionValidationResult result)
        {
            if (matchState.currentPhase != MatchPhase.PreSpinItemPhase)
            {
                result.Deny($"Spin can only be started during PreSpinItemPhase. Current phase: {matchState.currentPhase}");
                return result;
            }

            result.Allow();
            return result;
        }

        private bool IsKnownPlayer(
            MatchState matchState,
            string playerId)
        {
            if (matchState == null)
                return false;

            return matchState.GetPlayerMatchState(playerId) != null;
        }

        private bool IsTurnAction(TurnActionType actionType)
        {
            return actionType == TurnActionType.UseItem
                   || actionType == TurnActionType.StartSpin;
        }

        private bool IsBuildPhaseAction(TurnActionType actionType)
        {
            return actionType == TurnActionType.ReadyBuild
                   || actionType == TurnActionType.BuySymbol
                   || actionType == TurnActionType.BuyItem
                   || actionType == TurnActionType.RemoveSymbol
                   || actionType == TurnActionType.UpgradeSymbol
                   || actionType == TurnActionType.UpgradeItem
                   || actionType == TurnActionType.UpgradePlayerStat;
        }
    }
}