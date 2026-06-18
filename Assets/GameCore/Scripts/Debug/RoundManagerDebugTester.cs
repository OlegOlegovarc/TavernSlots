using System.Text;
using UnityEngine;
using SlotsTavern.Data;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class RoundManagerDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private RoundManager roundManager;

        [SerializeField]
        private MatchConfig matchConfig;

        [Header("Players")]
        [SerializeField]
        private string playerId = "player";

        [SerializeField]
        private string botId = "bot";

        [Header("Player Upgrades")]
        [SerializeField]
        private int playerHealthUpgradeLevel = 1;

        [SerializeField]
        private int playerShieldUpgradeLevel = 1;

        [Header("Bot Upgrades")]
        [SerializeField]
        private int botHealthUpgradeLevel = 2;

        [SerializeField]
        private int botShieldUpgradeLevel = 2;

        [Header("Next Round")]
        [SerializeField]
        private string firstPlayerId = "player";

        [SerializeField]
        private bool runOnStart = false;

        private void Start()
        {
            if (runOnStart)
                RunRoundTransitionTest();
        }

        [ContextMenu("Run Round Transition Test")]
        public void RunRoundTransitionTest()
        {
            if (roundManager == null)
            {
                Debug.LogWarning(
                    "RoundManagerDebugTester: RoundManager " +
                    "reference is missing.");

                return;
            }

            if (matchConfig == null)
            {
                Debug.LogWarning(
                    "RoundManagerDebugTester: MatchConfig " +
                    "reference is missing.");

                return;
            }

            MatchState matchState =
                CreateTestMatchState();

            LogMatchState(
                "BEFORE BUILD PHASE",
                matchState);

            RoundTransitionResult buildResult =
                roundManager.EnterBuildPhase(
                    matchState);

            LogTransitionResult(
                "ENTER BUILD PHASE",
                buildResult);

            LogMatchState(
                "IN BUILD PHASE",
                matchState);

            RoundTransitionResult roundResult =
                roundManager.StartNextRound(
                    matchState,
                    firstPlayerId);

            LogTransitionResult(
                "START NEXT ROUND",
                roundResult);

            LogMatchState(
                "AFTER NEXT ROUND START",
                matchState);
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState player =
                new PlayerMatchState(
                    playerId,
                    "Debug Player",
                    false);

            PlayerMatchState bot =
                new PlayerMatchState(
                    botId,
                    "Debug Bot",
                    true);

            player.maxHealthUpgradeLevel =
                playerHealthUpgradeLevel;

            player.shieldCapacityUpgradeLevel =
                playerShieldUpgradeLevel;

            bot.maxHealthUpgradeLevel =
                botHealthUpgradeLevel;

            bot.shieldCapacityUpgradeLevel =
                botShieldUpgradeLevel;

            MatchState matchState =
                new MatchState(
                    matchConfig,
                    player,
                    bot);

            matchState.currentRoundIndex = 1;

            matchState.playerARound =
                new PlayerRoundState(
                    playerId,
                    30,
                    20);

            matchState.playerBRound =
                new PlayerRoundState(
                    botId,
                    30,
                    20);

            matchState.playerARound.currentHealth = 7;
            matchState.playerARound.physicalShield = 9;
            matchState.playerARound.statuses.Add(
                new AppliedStatusState(
                    "poison",
                    3,
                    2));

            matchState.playerARound.MarkItemUsedThisTurn(
                "musket");

            matchState.playerARound.MarkItemUsedThisRound(
                "musket");

            matchState.playerBRound.currentHealth = 0;
            matchState.playerBRound.magicalShield = 8;
            matchState.playerBRound.statuses.Add(
                new AppliedStatusState(
                    "poison",
                    5,
                    1));

            matchState.SetActivePlayer(string.Empty);
            matchState.SetPhase(MatchPhase.RoundEnd);

            return matchState;
        }

        private void LogTransitionResult(
            string label,
            RoundTransitionResult result)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                $"=== ROUND TRANSITION: {label} ===");

            if (result == null)
            {
                builder.AppendLine("Result is null.");
                builder.AppendLine(
                    "================================");

                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine(
                $"Success: {result.success}");

            builder.AppendLine(
                $"Reason: {result.reason}");

            builder.AppendLine(
                $"Phase Before: {result.phaseBefore}");

            builder.AppendLine(
                $"Phase After: {result.phaseAfter}");

            builder.AppendLine(
                $"Round Before: {result.roundIndexBefore}");

            builder.AppendLine(
                $"Round After: {result.roundIndexAfter}");

            builder.AppendLine(
                $"Entered Build Phase: " +
                $"{result.enteredBuildPhase}");

            builder.AppendLine(
                $"Advanced Round Index: " +
                $"{result.advancedRoundIndex}");

            builder.AppendLine(
                $"Created Round States: " +
                $"{result.createdRoundStates}");

            builder.AppendLine(
                $"Started First Turn: " +
                $"{result.startedFirstTurn}");

            builder.AppendLine(
                $"First Player: {result.firstPlayerId}");

            builder.AppendLine(
                $"Player A Max Health: " +
                $"{result.playerAMaxHealth}");

            builder.AppendLine(
                $"Player A Shield Capacity: " +
                $"{result.playerAShieldCapacity}");

            builder.AppendLine(
                $"Player B Max Health: " +
                $"{result.playerBMaxHealth}");

            builder.AppendLine(
                $"Player B Shield Capacity: " +
                $"{result.playerBShieldCapacity}");

            if (result.firstTurnStartResult != null)
            {
                builder.AppendLine("");
                builder.AppendLine("First Turn:");

                builder.AppendLine(
                    $"Active Player: " +
                    $"{result.firstTurnStartResult.activePlayerId}");

                builder.AppendLine(
                    $"Can Act: " +
                    $"{result.firstTurnStartResult.canActivePlayerAct}");

                builder.AppendLine(
                    $"Phase After: " +
                    $"{result.firstTurnStartResult.phaseAfter}");
            }

            builder.AppendLine(
                "================================");

            Debug.Log(builder.ToString());
        }

        private void LogMatchState(
            string label,
            MatchState matchState)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                $"=== ROUND MANAGER STATE: {label} ===");

            builder.AppendLine(
                $"Round Index: {matchState.currentRoundIndex}");

            builder.AppendLine(
                $"Phase: {matchState.currentPhase}");

            builder.AppendLine(
                $"Active Player: {matchState.activePlayerId}");

            builder.AppendLine("");

            AppendPlayerState(
                builder,
                matchState.playerA,
                matchState.playerARound);

            AppendPlayerState(
                builder,
                matchState.playerB,
                matchState.playerBRound);

            builder.AppendLine(
                "================================");

            Debug.Log(builder.ToString());
        }

        private void AppendPlayerState(
            StringBuilder builder,
            PlayerMatchState matchState,
            PlayerRoundState roundState)
        {
            builder.AppendLine(
                $"Player: {matchState.playerId}");

            builder.AppendLine(
                $"Health Upgrade Level: " +
                $"{matchState.maxHealthUpgradeLevel}");

            builder.AppendLine(
                $"Shield Upgrade Level: " +
                $"{matchState.shieldCapacityUpgradeLevel}");

            if (roundState == null)
            {
                builder.AppendLine(
                    "Round State: null");

                builder.AppendLine("");
                return;
            }

            builder.AppendLine(
                $"HP: {roundState.currentHealth}/" +
                $"{roundState.maxHealth}");

            builder.AppendLine(
                $"Shield Capacity: " +
                $"{roundState.shieldCapacity}");

            builder.AppendLine(
                $"Shields: " +
                $"Physical={roundState.physicalShield}, " +
                $"Magical={roundState.magicalShield}, " +
                $"Other={roundState.otherShield}");

            builder.AppendLine(
                $"Status Count: " +
                $"{roundState.statuses.Count}");

            builder.AppendLine(
                $"Used This Turn Count: " +
                $"{roundState.usedItemsThisTurn.Count}");

            builder.AppendLine(
                $"Used This Round Count: " +
                $"{roundState.usedItemsThisRound.Count}");

            builder.AppendLine("");
        }
    }
}