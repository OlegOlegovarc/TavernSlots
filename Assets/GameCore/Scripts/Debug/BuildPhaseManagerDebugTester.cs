using System.Text;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Data;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class BuildPhaseManagerDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private BuildPhaseManager buildPhaseManager;

        [SerializeField]
        private MatchConfig matchConfig;

        [Header("Players")]
        [SerializeField]
        private string playerId = "player";

        [SerializeField]
        private string botId = "bot";

        [SerializeField]
        private string firstPlayerId = "player";

        [Header("Crystals")]
        [SerializeField]
        private int playerStartCrystals = 100;

        [SerializeField]
        private int botStartCrystals = 100;

        [Header("Runtime")]
        [SerializeField]
        private bool runOnStart = false;

        private void Start()
        {
            if (runOnStart)
                RunBuildPhaseTest();
        }

        [ContextMenu("Run Build Phase Test")]
        public void RunBuildPhaseTest()
        {
            if (buildPhaseManager == null)
            {
                Debug.LogWarning(
                    "BuildPhaseManagerDebugTester: BuildPhaseManager " +
                    "reference is missing.");

                return;
            }

            if (matchConfig == null)
            {
                Debug.LogWarning(
                    "BuildPhaseManagerDebugTester: MatchConfig " +
                    "reference is missing.");

                return;
            }

            MatchState matchState =
                CreateTestMatchState();

            LogMatchState(
                "BEFORE BUILD",
                matchState);

            BuildActionResult beginResult =
                buildPhaseManager.BeginBuildPhase(
                    matchState,
                    true);

            LogBuildActionResult(
                "BEGIN BUILD PHASE",
                beginResult);

            LogOffers(
                "OFFERS AFTER BEGIN",
                buildPhaseManager.CurrentBuildState);

            string offeredSymbolId =
                GetFirstOfferedSymbol(
                    playerId);

            if (!string.IsNullOrWhiteSpace(offeredSymbolId))
            {
                BuildActionResult addSymbolResult =
                    buildPhaseManager.AddOfferedSymbol(
                        matchState,
                        playerId,
                        offeredSymbolId);

                LogBuildActionResult(
                    "ADD OFFERED SYMBOL",
                    addSymbolResult);
            }

            string offeredItemId =
                GetFirstOfferedItem(
                    playerId);

            if (!string.IsNullOrWhiteSpace(offeredItemId))
            {
                BuildActionResult addItemResult =
                    buildPhaseManager.AddOfferedItem(
                        matchState,
                        playerId,
                        offeredItemId);

                LogBuildActionResult(
                    "ADD OFFERED ITEM",
                    addItemResult);

                if (addItemResult.success)
                {
                    BuildActionResult upgradeItemResult =
                        buildPhaseManager.UpgradeItem(
                            matchState,
                            playerId,
                            offeredItemId);

                    LogBuildActionResult(
                        "UPGRADE ADDED ITEM",
                        upgradeItemResult);
                }
            }

            BuildActionResult upgradeSymbolResult =
                buildPhaseManager.UpgradeSymbol(
                    matchState,
                    playerId,
                    "blade");

            LogBuildActionResult(
                "UPGRADE BLADE",
                upgradeSymbolResult);

            BuildActionResult upgradeHealthResult =
                buildPhaseManager.UpgradePlayerStat(
                    matchState,
                    playerId,
                    BuildPlayerStatType.MaxHealth);

            LogBuildActionResult(
                "UPGRADE MAX HEALTH",
                upgradeHealthResult);

            BuildActionResult upgradeShieldResult =
                buildPhaseManager.UpgradePlayerStat(
                    matchState,
                    playerId,
                    BuildPlayerStatType.ShieldCapacity);

            LogBuildActionResult(
                "UPGRADE SHIELD CAPACITY",
                upgradeShieldResult);

            BuildActionResult removeSymbolBlockedResult =
                buildPhaseManager.RemoveOwnedSymbol(
                    matchState,
                    playerId,
                    "blade");

            LogBuildActionResult(
                "REMOVE SYMBOL BLOCK TEST",
                removeSymbolBlockedResult);

            BuildActionResult playerReadyResult =
                buildPhaseManager.ReadyPlayer(
                    matchState,
                    playerId,
                    firstPlayerId);

            LogBuildActionResult(
                "PLAYER READY",
                playerReadyResult);

            BuildActionResult botReadyResult =
                buildPhaseManager.ReadyPlayer(
                    matchState,
                    botId,
                    firstPlayerId);

            LogBuildActionResult(
                "BOT READY / START ROUND",
                botReadyResult);

            LogMatchState(
                "AFTER BUILD",
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

            player.currentCrystals =
                playerStartCrystals;

            bot.currentCrystals =
                botStartCrystals;

            player.AddSymbol("blade");
            player.AddSymbol("shield");
            player.AddSymbol("ore");

            bot.AddSymbol("blade");
            bot.AddSymbol("shield");
            bot.AddSymbol("ore");
            bot.AddSymbol("poison_drop");

            MatchState matchState =
                new MatchState(
                    matchConfig,
                    player,
                    bot);

            matchState.currentRoundIndex = 1;
            matchState.SetPhase(MatchPhase.BuildPhase);
            matchState.SetActivePlayer(string.Empty);

            return matchState;
        }

        private string GetFirstOfferedSymbol(
            string targetPlayerId)
        {
            BuildPhaseState state =
                buildPhaseManager.CurrentBuildState;

            if (state == null)
                return string.Empty;

            BuildPlayerOfferState offerState =
                state.GetPlayerOfferState(targetPlayerId);

            if (offerState == null
                || offerState.offeredSymbolIds == null
                || offerState.offeredSymbolIds.Count == 0)
            {
                return string.Empty;
            }

            return offerState.offeredSymbolIds[0];
        }

        private string GetFirstOfferedItem(
            string targetPlayerId)
        {
            BuildPhaseState state =
                buildPhaseManager.CurrentBuildState;

            if (state == null)
                return string.Empty;

            BuildPlayerOfferState offerState =
                state.GetPlayerOfferState(targetPlayerId);

            if (offerState == null
                || offerState.offeredItemIds == null
                || offerState.offeredItemIds.Count == 0)
            {
                return string.Empty;
            }

            return offerState.offeredItemIds[0];
        }

        private void LogBuildActionResult(
            string label,
            BuildActionResult result)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                $"=== BUILD ACTION: {label} ===");

            if (result == null)
            {
                builder.AppendLine("Result is null.");
                builder.AppendLine(
                    "==============================");

                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine(
                $"Success: {result.success}");

            builder.AppendLine(
                $"Reason: {result.reason}");

            builder.AppendLine(
                $"Action: {result.actionType}");

            builder.AppendLine(
                $"Player: {result.playerId}");

            builder.AppendLine(
                $"Target: {result.targetId}");

            builder.AppendLine(
                $"Offer Type: {result.offerType}");

            builder.AppendLine(
                $"Player Stat Type: {result.playerStatType}");

            builder.AppendLine(
                $"Cost: {result.cost}");

            builder.AppendLine(
                $"Crystals Before: {result.crystalsBefore}");

            builder.AppendLine(
                $"Crystals After: {result.crystalsAfter}");

            builder.AppendLine(
                $"Level Before: {result.levelBefore}");

            builder.AppendLine(
                $"Level After: {result.levelAfter}");

            builder.AppendLine(
                $"Owned Symbols: " +
                $"{result.ownedSymbolsBefore} -> " +
                $"{result.ownedSymbolsAfter}");

            builder.AppendLine(
                $"Owned Items: " +
                $"{result.ownedItemsBefore} -> " +
                $"{result.ownedItemsAfter}");

            builder.AppendLine(
                $"Offered Symbols: " +
                $"{result.offeredSymbolsBefore} -> " +
                $"{result.offeredSymbolsAfter}");

            builder.AppendLine(
                $"Offered Items: " +
                $"{result.offeredItemsBefore} -> " +
                $"{result.offeredItemsAfter}");

            builder.AppendLine(
                $"Player Ready: {result.playerReady}");

            builder.AppendLine(
                $"Both Players Ready: " +
                $"{result.bothPlayersReady}");

            builder.AppendLine(
                $"Started Round: {result.startedRound}");

            if (result.actionValidationResult != null)
            {
                builder.AppendLine("");
                builder.AppendLine("Validation:");

                builder.AppendLine(
                    $"Valid: " +
                    $"{result.actionValidationResult.isValid}");

                builder.AppendLine(
                    $"Reason: " +
                    $"{result.actionValidationResult.reason}");

                builder.AppendLine(
                    $"Phase: " +
                    $"{result.actionValidationResult.currentPhase}");
            }

            if (result.roundTransitionResult != null)
            {
                builder.AppendLine("");
                builder.AppendLine("Round Transition:");

                builder.AppendLine(
                    $"Success: " +
                    $"{result.roundTransitionResult.success}");

                builder.AppendLine(
                    $"Reason: " +
                    $"{result.roundTransitionResult.reason}");

                builder.AppendLine(
                    $"Round: " +
                    $"{result.roundTransitionResult.roundIndexBefore} -> " +
                    $"{result.roundTransitionResult.roundIndexAfter}");

                builder.AppendLine(
                    $"Phase: " +
                    $"{result.roundTransitionResult.phaseBefore} -> " +
                    $"{result.roundTransitionResult.phaseAfter}");

                builder.AppendLine(
                    $"First Player: " +
                    $"{result.roundTransitionResult.firstPlayerId}");
            }

            builder.AppendLine(
                "==============================");

            Debug.Log(builder.ToString());
        }

        private void LogOffers(
            string label,
            BuildPhaseState state)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                $"=== BUILD OFFERS: {label} ===");

            if (state == null)
            {
                builder.AppendLine("State is null.");
                builder.AppendLine(
                    "=============================");

                Debug.Log(builder.ToString());
                return;
            }

            for (int i = 0;
                 i < state.playerOfferStates.Count;
                 i++)
            {
                BuildPlayerOfferState offerState =
                    state.playerOfferStates[i];

                builder.AppendLine(
                    $"Player: {offerState.playerId}");

                builder.AppendLine(
                    $"Ready: {offerState.isReady}");

                builder.AppendLine(
                    $"Symbols: " +
                    $"{string.Join(", ", offerState.offeredSymbolIds)}");

                builder.AppendLine(
                    $"Items: " +
                    $"{string.Join(", ", offerState.offeredItemIds)}");

                builder.AppendLine("");
            }

            builder.AppendLine(
                "=============================");

            Debug.Log(builder.ToString());
        }

        private void LogMatchState(
            string label,
            MatchState matchState)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                $"=== BUILD MATCH STATE: {label} ===");

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
                "==============================");

            Debug.Log(builder.ToString());
        }

        private void AppendPlayerState(
            StringBuilder builder,
            PlayerMatchState player,
            PlayerRoundState roundState)
        {
            builder.AppendLine(
                $"Player: {player.playerId}");

            builder.AppendLine(
                $"Crystals: {player.currentCrystals}");

            builder.AppendLine(
                $"Symbols: " +
                $"{string.Join(", ", player.ownedSymbolIds)}");

            builder.AppendLine(
                $"Items: " +
                $"{string.Join(", ", player.ownedItemIds)}");

            builder.AppendLine(
                $"Max Health Upgrade Level: " +
                $"{player.maxHealthUpgradeLevel}");

            builder.AppendLine(
                $"Shield Capacity Upgrade Level: " +
                $"{player.shieldCapacityUpgradeLevel}");

            builder.AppendLine(
                $"Symbol Upgrade Count: " +
                $"{player.symbolUpgradeLevels.Count}");

            builder.AppendLine(
                $"Item Upgrade Count: " +
                $"{player.itemUpgradeLevels.Count}");

            if (roundState == null)
            {
                builder.AppendLine("Round State: null");
            }
            else
            {
                builder.AppendLine(
                    $"Round HP: " +
                    $"{roundState.currentHealth}/" +
                    $"{roundState.maxHealth}");

                builder.AppendLine(
                    $"Round Shield Capacity: " +
                    $"{roundState.shieldCapacity}");
            }

            builder.AppendLine("");
        }
    }
}