using UnityEngine;
using SlotsTavern.Data;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class RoundManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private UpgradeManager upgradeManager;

        [SerializeField]
        private TurnManager turnManager;

        public RoundTransitionResult EnterBuildPhase(
            MatchState matchState)
        {
            RoundTransitionResult result =
                CreateResult(matchState);

            if (matchState == null)
            {
                result.Deny("MatchState is null.");
                return result;
            }

            if (matchState.isMatchEnded
                || matchState.currentPhase == MatchPhase.MatchEnd)
            {
                result.Deny(
                    "Cannot enter BuildPhase. Match has ended.");

                return result;
            }

            if (matchState.currentPhase != MatchPhase.RoundEnd)
            {
                result.Deny(
                    $"BuildPhase can only begin after RoundEnd. " +
                    $"Current phase: {matchState.currentPhase}");

                return result;
            }

            matchState.SetActivePlayer(string.Empty);
            matchState.SetPhase(MatchPhase.BuildPhase);

            result.enteredBuildPhase = true;
            result.phaseAfter = matchState.currentPhase;
            result.roundIndexAfter = matchState.currentRoundIndex;

            result.Allow(
                "BuildPhase started successfully.");

            return result;
        }

        public RoundTransitionResult StartCurrentRound(
            MatchState matchState,
            string firstPlayerId)
        {
            return StartRoundInternal(
                matchState,
                firstPlayerId,
                false);
        }

        public RoundTransitionResult StartNextRound(
            MatchState matchState,
            string firstPlayerId)
        {
            return StartRoundInternal(
                matchState,
                firstPlayerId,
                true);
        }

        private RoundTransitionResult StartRoundInternal(
            MatchState matchState,
            string firstPlayerId,
            bool advanceRoundIndex)
        {
            RoundTransitionResult result =
                CreateResult(matchState);

            if (!ValidateRoundStart(
                    matchState,
                    firstPlayerId,
                    result))
            {
                return result;
            }

            int targetRoundIndex =
                matchState.currentRoundIndex;

            if (advanceRoundIndex)
                targetRoundIndex++;

            RoundConfig roundConfig =
                matchState.matchConfig.GetRoundConfig(
                    targetRoundIndex);

            if (roundConfig == null)
            {
                result.Deny(
                    $"RoundConfig not found for round " +
                    $"{targetRoundIndex}.");

                return result;
            }

            PlayerMatchState playerA =
                matchState.playerA;

            PlayerMatchState playerB =
                matchState.playerB;

            if (playerA == null || playerB == null)
            {
                result.Deny(
                    "One or both PlayerMatchState objects are missing.");

                return result;
            }

            int playerAMaxHealth =
                upgradeManager.CalculateMaxHealth(
                    playerA,
                    roundConfig);

            int playerAShieldCapacity =
                upgradeManager.CalculateShieldCapacity(
                    playerA,
                    roundConfig);

            int playerBMaxHealth =
                upgradeManager.CalculateMaxHealth(
                    playerB,
                    roundConfig);

            int playerBShieldCapacity =
                upgradeManager.CalculateShieldCapacity(
                    playerB,
                    roundConfig);

            matchState.currentRoundIndex =
                targetRoundIndex;

            matchState.playerARound =
                new PlayerRoundState(
                    playerA.playerId,
                    playerAMaxHealth,
                    playerAShieldCapacity);

            matchState.playerBRound =
                new PlayerRoundState(
                    playerB.playerId,
                    playerBMaxHealth,
                    playerBShieldCapacity);

            matchState.SetActivePlayer(string.Empty);
            matchState.SetPhase(MatchPhase.RoundStart);

            result.advancedRoundIndex =
                advanceRoundIndex;

            result.createdRoundStates = true;
            result.firstPlayerId = firstPlayerId;

            result.playerAMaxHealth =
                playerAMaxHealth;

            result.playerAShieldCapacity =
                playerAShieldCapacity;

            result.playerBMaxHealth =
                playerBMaxHealth;

            result.playerBShieldCapacity =
                playerBShieldCapacity;

            result.roundIndexAfter =
                matchState.currentRoundIndex;

            TurnStartResult firstTurnResult =
                turnManager.BeginTurn(
                    matchState,
                    firstPlayerId);

            result.firstTurnStartResult =
                firstTurnResult;

            result.startedFirstTurn =
                firstTurnResult != null;

            result.phaseAfter =
                matchState.currentPhase;

            if (firstTurnResult == null)
            {
                result.Deny(
                    "Round states were created, but the first turn " +
                    "could not start.");

                return result;
            }

            if (!firstTurnResult.canActivePlayerAct)
            {
                result.Deny(
                    "Round started, but the first player cannot act.");

                return result;
            }

            result.Allow(
                advanceRoundIndex
                    ? "Next round started successfully."
                    : "Current round started successfully.");

            return result;
        }

        private bool ValidateRoundStart(
            MatchState matchState,
            string firstPlayerId,
            RoundTransitionResult result)
        {
            if (matchState == null)
            {
                result.Deny("MatchState is null.");
                return false;
            }

            if (upgradeManager == null)
            {
                result.Deny(
                    "UpgradeManager reference is missing.");

                return false;
            }

            if (turnManager == null)
            {
                result.Deny(
                    "TurnManager reference is missing.");

                return false;
            }

            if (matchState.matchConfig == null)
            {
                result.Deny(
                    "MatchConfig is missing.");

                return false;
            }

            if (matchState.isMatchEnded
                || matchState.currentPhase == MatchPhase.MatchEnd)
            {
                result.Deny(
                    "Cannot start round. Match has ended.");

                return false;
            }

            if (matchState.currentPhase != MatchPhase.BuildPhase)
            {
                result.Deny(
                    $"Round can only start from BuildPhase. " +
                    $"Current phase: {matchState.currentPhase}");

                return false;
            }

            if (string.IsNullOrWhiteSpace(firstPlayerId))
            {
                result.Deny(
                    "First player id is empty.");

                return false;
            }

            if (matchState.GetPlayerMatchState(firstPlayerId) == null)
            {
                result.Deny(
                    $"First player is not part of this match: " +
                    $"{firstPlayerId}");

                return false;
            }

            return true;
        }

        private RoundTransitionResult CreateResult(
            MatchState matchState)
        {
            MatchPhase phaseBefore =
                matchState != null
                    ? matchState.currentPhase
                    : MatchPhase.None;

            int roundIndexBefore =
                matchState != null
                    ? matchState.currentRoundIndex
                    : 0;

            return new RoundTransitionResult(
                phaseBefore,
                roundIndexBefore);
        }
    }
}