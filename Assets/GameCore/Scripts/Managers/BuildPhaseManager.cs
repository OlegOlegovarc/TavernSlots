using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Data;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class BuildPhaseManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private TurnActionValidator turnActionValidator;

        [SerializeField]
        private RoundManager roundManager;

        [SerializeField]
        private UpgradeManager upgradeManager;

        [Header("Offer Data")]
        [SerializeField]
        private List<SymbolDefinition> symbolDefinitions =
            new List<SymbolDefinition>();

        [SerializeField]
        private List<ItemDefinition> itemDefinitions =
            new List<ItemDefinition>();

        [Header("Runtime State")]
        [SerializeField]
        private BuildPhaseState currentBuildState;

        private readonly Dictionary<string, SymbolDefinition> symbolById =
            new Dictionary<string, SymbolDefinition>();

        private readonly Dictionary<string, ItemDefinition> itemById =
            new Dictionary<string, ItemDefinition>();

        public BuildPhaseState CurrentBuildState => currentBuildState;

        private void Awake()
        {
            RebuildLookups();
        }

        [ContextMenu("Rebuild Build Data Lookups")]
        public void RebuildLookups()
        {
            RebuildSymbolLookup();
            RebuildItemLookup();
        }

        public BuildActionResult BeginBuildPhase(
            MatchState matchState,
            bool advanceRoundOnReady)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.BeginBuildPhase,
                    string.Empty,
                    string.Empty);

            if (matchState == null)
            {
                result.Deny("MatchState is null.");
                return result;
            }

            if (matchState.currentPhase != MatchPhase.BuildPhase)
            {
                result.Deny(
                    $"BuildPhaseManager can only begin during BuildPhase. " +
                    $"Current phase: {matchState.currentPhase}");

                return result;
            }

            if (matchState.playerA == null || matchState.playerB == null)
            {
                result.Deny("One or both players are missing.");
                return result;
            }

            if (matchState.matchConfig == null)
            {
                result.Deny("MatchConfig is missing.");
                return result;
            }

            RebuildLookups();

            currentBuildState =
                new BuildPhaseState(
                    matchState.currentRoundIndex,
                    advanceRoundOnReady);

            GenerateOffersForPlayer(
                matchState,
                matchState.playerA);

            GenerateOffersForPlayer(
                matchState,
                matchState.playerB);

            result.offeredSymbolsAfter =
                CountTotalOfferedSymbols();

            result.offeredItemsAfter =
                CountTotalOfferedItems();

            result.Allow("BuildPhase state created and offers generated.");
            return result;
        }

        public BuildActionResult AddOfferedSymbol(
            MatchState matchState,
            string playerId,
            string symbolId)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.AddSymbol,
                    playerId,
                    symbolId);

            result.offerType = BuildOfferType.Symbol;

            if (!ValidateCommonBuildAction(
                    matchState,
                    playerId,
                    TurnActionType.BuySymbol,
                    result))
            {
                return result;
            }

            PlayerMatchState player =
                matchState.GetPlayerMatchState(playerId);

            BuildPlayerOfferState offerState =
                currentBuildState.GetPlayerOfferState(playerId);

            if (offerState == null)
            {
                result.Deny(
                    $"Offer state not found for player: {playerId}");

                return result;
            }

            if (!offerState.offeredSymbolIds.Contains(symbolId))
            {
                result.Deny(
                    $"Symbol is not offered to this player: {symbolId}");

                return result;
            }

            SymbolDefinition symbol =
                GetSymbolDefinition(symbolId);

            if (symbol == null)
            {
                result.Deny(
                    $"Symbol definition not found: {symbolId}");

                return result;
            }

            if (player.HasSymbol(symbolId))
            {
                result.Deny(
                    $"Player already owns symbol: {symbolId}");

                return result;
            }

            result.ownedSymbolsBefore =
                player.ownedSymbolIds.Count;

            result.offeredSymbolsBefore =
                offerState.offeredSymbolIds.Count;

            player.AddSymbol(symbolId);
            offerState.offeredSymbolIds.Remove(symbolId);

            result.ownedSymbolsAfter =
                player.ownedSymbolIds.Count;

            result.offeredSymbolsAfter =
                offerState.offeredSymbolIds.Count;

            result.cost = 0;
            result.crystalsBefore = player.currentCrystals;
            result.crystalsAfter = player.currentCrystals;

            result.Allow(
                $"Symbol added to player build: {symbolId}");

            return result;
        }

        public BuildActionResult AddOfferedItem(
            MatchState matchState,
            string playerId,
            string itemId)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.AddItem,
                    playerId,
                    itemId);

            result.offerType = BuildOfferType.Item;

            if (!ValidateCommonBuildAction(
                    matchState,
                    playerId,
                    TurnActionType.BuyItem,
                    result))
            {
                return result;
            }

            PlayerMatchState player =
                matchState.GetPlayerMatchState(playerId);

            BuildPlayerOfferState offerState =
                currentBuildState.GetPlayerOfferState(playerId);

            if (offerState == null)
            {
                result.Deny(
                    $"Offer state not found for player: {playerId}");

                return result;
            }

            if (!offerState.offeredItemIds.Contains(itemId))
            {
                result.Deny(
                    $"Item is not offered to this player: {itemId}");

                return result;
            }

            ItemDefinition item =
                GetItemDefinition(itemId);

            if (item == null)
            {
                result.Deny(
                    $"Item definition not found: {itemId}");

                return result;
            }

            if (player.HasItem(itemId))
            {
                result.Deny(
                    $"Player already owns item: {itemId}");

                return result;
            }

            result.cost = Mathf.Max(0, item.acquireCost);
            result.crystalsBefore = player.currentCrystals;

            if (player.currentCrystals < result.cost)
            {
                result.Deny(
                    $"Not enough match crystals. Required={result.cost}, " +
                    $"Available={player.currentCrystals}");

                return result;
            }

            result.ownedItemsBefore =
                player.ownedItemIds.Count;

            result.offeredItemsBefore =
                offerState.offeredItemIds.Count;

            player.currentCrystals -= result.cost;
            player.AddItem(itemId);
            offerState.offeredItemIds.Remove(itemId);

            result.crystalsAfter =
                player.currentCrystals;

            result.ownedItemsAfter =
                player.ownedItemIds.Count;

            result.offeredItemsAfter =
                offerState.offeredItemIds.Count;

            result.Allow(
                $"Item bought and added to player build: {itemId}");

            return result;
        }

        public BuildActionResult RemoveOwnedSymbol(
            MatchState matchState,
            string playerId,
            string symbolId)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.RemoveSymbol,
                    playerId,
                    symbolId);

            result.offerType = BuildOfferType.Symbol;

            if (!ValidateCommonBuildAction(
                    matchState,
                    playerId,
                    TurnActionType.RemoveSymbol,
                    result))
            {
                return result;
            }

            PlayerMatchState player =
                matchState.GetPlayerMatchState(playerId);

            if (!player.HasSymbol(symbolId))
            {
                result.Deny(
                    $"Player does not own symbol: {symbolId}");

                return result;
            }

            int minimumSymbolPoolSize =
                matchState.matchConfig != null
                    ? matchState.matchConfig.minSymbolPoolSize
                    : 1;

            result.ownedSymbolsBefore =
                player.ownedSymbolIds.Count;

            if (player.ownedSymbolIds.Count <= minimumSymbolPoolSize)
            {
                result.Deny(
                    $"Cannot remove symbol below minimum pool size. " +
                    $"Current={player.ownedSymbolIds.Count}, " +
                    $"Minimum={minimumSymbolPoolSize}");

                return result;
            }

            player.RemoveSymbol(symbolId);

            result.ownedSymbolsAfter =
                player.ownedSymbolIds.Count;

            result.Allow(
                $"Symbol removed from player build: {symbolId}");

            return result;
        }

        public BuildActionResult UpgradeSymbol(
            MatchState matchState,
            string playerId,
            string symbolId)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.UpgradeSymbol,
                    playerId,
                    symbolId);

            result.offerType = BuildOfferType.Symbol;

            if (!ValidateCommonBuildAction(
                    matchState,
                    playerId,
                    TurnActionType.UpgradeSymbol,
                    result))
            {
                return result;
            }

            PlayerMatchState player =
                matchState.GetPlayerMatchState(playerId);

            if (!player.HasSymbol(symbolId))
            {
                result.Deny(
                    $"Player does not own symbol: {symbolId}");

                return result;
            }

            SymbolDefinition symbol =
                GetSymbolDefinition(symbolId);

            if (symbol == null)
            {
                result.Deny(
                    $"Symbol definition not found: {symbolId}");

                return result;
            }

            int currentLevel =
                player.GetSymbolUpgradeLevel(symbolId);

            result.levelBefore = currentLevel;

            if (currentLevel >= symbol.maxUpgradeLevel)
            {
                result.Deny(
                    $"Symbol is already at max level: {symbolId}");

                return result;
            }

            if (symbol.upgradeCostCurve == null
                || !symbol.upgradeCostCurve.CanUpgradeFromLevel(currentLevel))
            {
                result.Deny(
                    $"Symbol cannot upgrade from current level: {symbolId}");

                return result;
            }

            result.cost =
                symbol.GetUpgradeCostFromLevel(currentLevel);

            return SpendAndUpgrade(
                player,
                result,
                () =>
                {
                    player.SetSymbolUpgradeLevel(
                        symbolId,
                        currentLevel + 1);

                    result.levelAfter =
                        player.GetSymbolUpgradeLevel(symbolId);
                },
                $"Symbol upgraded: {symbolId}");
        }

        public BuildActionResult UpgradeItem(
            MatchState matchState,
            string playerId,
            string itemId)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.UpgradeItem,
                    playerId,
                    itemId);

            result.offerType = BuildOfferType.Item;

            if (!ValidateCommonBuildAction(
                    matchState,
                    playerId,
                    TurnActionType.UpgradeItem,
                    result))
            {
                return result;
            }

            PlayerMatchState player =
                matchState.GetPlayerMatchState(playerId);

            if (!player.HasItem(itemId))
            {
                result.Deny(
                    $"Player does not own item: {itemId}");

                return result;
            }

            ItemDefinition item =
                GetItemDefinition(itemId);

            if (item == null)
            {
                result.Deny(
                    $"Item definition not found: {itemId}");

                return result;
            }

            int currentLevel =
                player.GetItemUpgradeLevel(itemId);

            result.levelBefore = currentLevel;

            if (currentLevel >= item.maxUpgradeLevel)
            {
                result.Deny(
                    $"Item is already at max level: {itemId}");

                return result;
            }

            if (item.upgradeCostCurve == null
                || !item.upgradeCostCurve.CanUpgradeFromLevel(currentLevel))
            {
                result.Deny(
                    $"Item cannot upgrade from current level: {itemId}");

                return result;
            }

            result.cost =
                item.GetUpgradeCostFromLevel(currentLevel);

            return SpendAndUpgrade(
                player,
                result,
                () =>
                {
                    player.SetItemUpgradeLevel(
                        itemId,
                        currentLevel + 1);

                    result.levelAfter =
                        player.GetItemUpgradeLevel(itemId);
                },
                $"Item upgraded: {itemId}");
        }

        public BuildActionResult UpgradePlayerStat(
            MatchState matchState,
            string playerId,
            BuildPlayerStatType statType)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.UpgradePlayerStat,
                    playerId,
                    statType.ToString());

            result.playerStatType = statType;

            if (!ValidateCommonBuildAction(
                    matchState,
                    playerId,
                    TurnActionType.UpgradePlayerStat,
                    result))
            {
                return result;
            }

            if (upgradeManager == null)
            {
                result.Deny(
                    "UpgradeManager reference is missing.");

                return result;
            }

            PlayerMatchState player =
                matchState.GetPlayerMatchState(playerId);

            if (statType == BuildPlayerStatType.MaxHealth)
            {
                if (!upgradeManager.CanUpgradeMaxHealth(player))
                {
                    result.Deny(
                        "Max Health cannot be upgraded from current level.");

                    return result;
                }

                result.levelBefore =
                    player.maxHealthUpgradeLevel;

                result.cost =
                    upgradeManager.GetMaxHealthUpgradeCost(player);

                return SpendAndUpgrade(
                    player,
                    result,
                    () =>
                    {
                        player.maxHealthUpgradeLevel++;
                        result.levelAfter =
                            player.maxHealthUpgradeLevel;
                    },
                    "Max Health upgraded.");
            }

            if (statType == BuildPlayerStatType.ShieldCapacity)
            {
                if (!upgradeManager.CanUpgradeShieldCapacity(player))
                {
                    result.Deny(
                        "Shield Capacity cannot be upgraded from current level.");

                    return result;
                }

                result.levelBefore =
                    player.shieldCapacityUpgradeLevel;

                result.cost =
                    upgradeManager.GetShieldCapacityUpgradeCost(player);

                return SpendAndUpgrade(
                    player,
                    result,
                    () =>
                    {
                        player.shieldCapacityUpgradeLevel++;
                        result.levelAfter =
                            player.shieldCapacityUpgradeLevel;
                    },
                    "Shield Capacity upgraded.");
            }

            result.Deny(
                $"Unsupported player stat type: {statType}");

            return result;
        }

        public BuildActionResult ReadyPlayer(
            MatchState matchState,
            string playerId,
            string firstPlayerId)
        {
            BuildActionResult result =
                new BuildActionResult(
                    BuildActionType.Ready,
                    playerId,
                    string.Empty);

            if (!ValidateCommonBuildAction(
                    matchState,
                    playerId,
                    TurnActionType.ReadyBuild,
                    result))
            {
                return result;
            }

            currentBuildState.SetPlayerReady(
                playerId,
                true);

            result.playerReady =
                true;

            result.bothPlayersReady =
                currentBuildState.AreBothPlayersReady(
                    matchState);

            if (!result.bothPlayersReady)
            {
                result.Allow(
                    $"Player is ready: {playerId}");

                return result;
            }

            if (roundManager == null)
            {
                result.Deny(
                    "Both players are ready, but RoundManager " +
                    "reference is missing.");

                return result;
            }

            RoundTransitionResult roundTransitionResult =
                currentBuildState.advanceRoundOnReady
                    ? roundManager.StartNextRound(
                        matchState,
                        firstPlayerId)
                    : roundManager.StartCurrentRound(
                        matchState,
                        firstPlayerId);

            result.roundTransitionResult =
                roundTransitionResult;

            result.startedRound =
                roundTransitionResult != null
                && roundTransitionResult.success;

            if (!result.startedRound)
            {
                result.Deny(
                    roundTransitionResult != null
                        ? roundTransitionResult.reason
                        : "Round transition failed.");

                return result;
            }

            result.Allow(
                "Both players are ready. Round started.");

            return result;
        }

        private bool ValidateCommonBuildAction(
            MatchState matchState,
            string playerId,
            TurnActionType validationAction,
            BuildActionResult result)
        {
            if (matchState == null)
            {
                result.Deny("MatchState is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                result.Deny("Player id is empty.");
                return false;
            }

            if (currentBuildState == null)
            {
                result.Deny(
                    "BuildPhase state has not been created.");

                return false;
            }

            if (turnActionValidator == null)
            {
                result.Deny(
                    "TurnActionValidator reference is missing.");

                return false;
            }

            TurnActionValidationResult validationResult =
                turnActionValidator.ValidateAction(
                    matchState,
                    playerId,
                    validationAction);

            result.actionValidationResult =
                validationResult;

            if (validationResult == null
                || !validationResult.isValid)
            {
                result.Deny(
                    validationResult != null
                        ? validationResult.reason
                        : "Build action validation failed.");

                return false;
            }

            if (matchState.GetPlayerMatchState(playerId) == null)
            {
                result.Deny(
                    $"Player is not part of this match: {playerId}");

                return false;
            }

            return true;
        }

        private BuildActionResult SpendAndUpgrade(
            PlayerMatchState player,
            BuildActionResult result,
            System.Action applyUpgrade,
            string successReason)
        {
            result.crystalsBefore =
                player.currentCrystals;

            if (player.currentCrystals < result.cost)
            {
                result.Deny(
                    $"Not enough match crystals. Required={result.cost}, " +
                    $"Available={player.currentCrystals}");

                return result;
            }

            player.currentCrystals -= result.cost;

            applyUpgrade?.Invoke();

            result.crystalsAfter =
                player.currentCrystals;

            result.Allow(successReason);

            return result;
        }

        private void GenerateOffersForPlayer(
            MatchState matchState,
            PlayerMatchState player)
        {
            if (matchState == null || player == null)
                return;

            BuildPlayerOfferState offerState =
                currentBuildState.EnsurePlayerOfferState(
                    player.playerId);

            offerState.offeredSymbolIds.Clear();
            offerState.offeredItemIds.Clear();
            offerState.isReady = false;

            int symbolOfferCount =
                matchState.matchConfig != null
                    ? Mathf.Max(0, matchState.matchConfig.offeredSymbolCount)
                    : 0;

            int itemOfferCount =
                matchState.matchConfig != null
                    ? Mathf.Max(0, matchState.matchConfig.offeredItemCount)
                    : 0;

            List<string> availableSymbols =
                GetAvailableSymbolOfferIds(player);

            List<string> availableItems =
                GetAvailableItemOfferIds(player);

            FillRandomOffers(
                availableSymbols,
                offerState.offeredSymbolIds,
                symbolOfferCount);

            FillRandomOffers(
                availableItems,
                offerState.offeredItemIds,
                itemOfferCount);
        }

        private List<string> GetAvailableSymbolOfferIds(
            PlayerMatchState player)
        {
            List<string> ids =
                new List<string>();

            for (int i = 0; i < symbolDefinitions.Count; i++)
            {
                SymbolDefinition symbol =
                    symbolDefinitions[i];

                if (symbol == null)
                    continue;

                if (string.IsNullOrWhiteSpace(symbol.id))
                    continue;

                if (!symbol.canAppearInOffers)
                    continue;

                if (player != null && player.HasSymbol(symbol.id))
                    continue;

                ids.Add(symbol.id);
            }

            return ids;
        }

        private List<string> GetAvailableItemOfferIds(
            PlayerMatchState player)
        {
            List<string> ids =
                new List<string>();

            for (int i = 0; i < itemDefinitions.Count; i++)
            {
                ItemDefinition item =
                    itemDefinitions[i];

                if (item == null)
                    continue;

                if (string.IsNullOrWhiteSpace(item.id))
                    continue;

                if (!item.canAppearInOffers)
                    continue;

                if (player != null && player.HasItem(item.id))
                    continue;

                ids.Add(item.id);
            }

            return ids;
        }

        private void FillRandomOffers(
            List<string> availableIds,
            List<string> targetOffers,
            int maxCount)
        {
            targetOffers.Clear();

            if (availableIds == null || availableIds.Count == 0)
                return;

            List<string> shuffledIds =
                new List<string>(availableIds);

            Shuffle(shuffledIds);

            int count =
                Mathf.Min(maxCount, shuffledIds.Count);

            for (int i = 0; i < count; i++)
                targetOffers.Add(shuffledIds[i]);
        }

        private void Shuffle(List<string> ids)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                int randomIndex =
                    Random.Range(i, ids.Count);

                string temp =
                    ids[i];

                ids[i] =
                    ids[randomIndex];

                ids[randomIndex] =
                    temp;
            }
        }

        private void RebuildSymbolLookup()
        {
            symbolById.Clear();

            for (int i = 0; i < symbolDefinitions.Count; i++)
            {
                SymbolDefinition symbol =
                    symbolDefinitions[i];

                if (symbol == null)
                    continue;

                if (string.IsNullOrWhiteSpace(symbol.id))
                    continue;

                if (symbolById.ContainsKey(symbol.id))
                {
                    Debug.LogWarning(
                        $"BuildPhaseManager: Duplicate symbol id: " +
                        $"{symbol.id}");

                    continue;
                }

                symbolById.Add(
                    symbol.id,
                    symbol);
            }
        }

        private void RebuildItemLookup()
        {
            itemById.Clear();

            for (int i = 0; i < itemDefinitions.Count; i++)
            {
                ItemDefinition item =
                    itemDefinitions[i];

                if (item == null)
                    continue;

                if (string.IsNullOrWhiteSpace(item.id))
                    continue;

                if (itemById.ContainsKey(item.id))
                {
                    Debug.LogWarning(
                        $"BuildPhaseManager: Duplicate item id: " +
                        $"{item.id}");

                    continue;
                }

                itemById.Add(
                    item.id,
                    item);
            }
        }

        private SymbolDefinition GetSymbolDefinition(
            string symbolId)
        {
            if (symbolById.Count == 0)
                RebuildSymbolLookup();

            if (string.IsNullOrWhiteSpace(symbolId))
                return null;

            if (symbolById.TryGetValue(
                    symbolId,
                    out SymbolDefinition symbol))
            {
                return symbol;
            }

            return null;
        }

        private ItemDefinition GetItemDefinition(
            string itemId)
        {
            if (itemById.Count == 0)
                RebuildItemLookup();

            if (string.IsNullOrWhiteSpace(itemId))
                return null;

            if (itemById.TryGetValue(
                    itemId,
                    out ItemDefinition item))
            {
                return item;
            }

            return null;
        }

        private int CountTotalOfferedSymbols()
        {
            if (currentBuildState == null)
                return 0;

            int count = 0;

            for (int i = 0;
                 i < currentBuildState.playerOfferStates.Count;
                 i++)
            {
                BuildPlayerOfferState offerState =
                    currentBuildState.playerOfferStates[i];

                if (offerState != null
                    && offerState.offeredSymbolIds != null)
                {
                    count += offerState.offeredSymbolIds.Count;
                }
            }

            return count;
        }

        private int CountTotalOfferedItems()
        {
            if (currentBuildState == null)
                return 0;

            int count = 0;

            for (int i = 0;
                 i < currentBuildState.playerOfferStates.Count;
                 i++)
            {
                BuildPlayerOfferState offerState =
                    currentBuildState.playerOfferStates[i];

                if (offerState != null
                    && offerState.offeredItemIds != null)
                {
                    count += offerState.offeredItemIds.Count;
                }
            }

            return count;
        }
    }
}