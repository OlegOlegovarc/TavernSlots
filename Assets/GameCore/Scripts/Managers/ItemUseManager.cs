using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Data;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class ItemUseManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnActionValidator turnActionValidator;

        [Header("Item Data")]
        [SerializeField] private List<ItemDefinition> itemDefinitions = new List<ItemDefinition>();

        private readonly Dictionary<string, ItemDefinition> itemById =
            new Dictionary<string, ItemDefinition>();

        private readonly Dictionary<string, HashSet<string>> usedOncePerMatchItemsByPlayer =
            new Dictionary<string, HashSet<string>>();

        private void Awake()
        {
            RebuildItemLookup();
        }

        [ContextMenu("Rebuild Item Lookup")]
        public void RebuildItemLookup()
        {
            itemById.Clear();

            if (itemDefinitions == null)
                return;

            for (int i = 0; i < itemDefinitions.Count; i++)
            {
                ItemDefinition item = itemDefinitions[i];

                if (item == null)
                    continue;

                if (string.IsNullOrWhiteSpace(item.id))
                {
                    Debug.LogWarning($"ItemUseManager: Item at index {i} has empty id.");
                    continue;
                }

                if (itemById.ContainsKey(item.id))
                {
                    Debug.LogWarning($"ItemUseManager: Duplicate item id detected: {item.id}");
                    continue;
                }

                itemById.Add(item.id, item);
            }
        }

        public void ClearMatchUsage()
        {
            usedOncePerMatchItemsByPlayer.Clear();
        }

        public ItemUseResult UseItem(
            MatchState matchState,
            string playerId,
            string itemId)
        {
            string opponentPlayerId = matchState != null
                ? matchState.GetOpponentPlayerId(playerId)
                : string.Empty;

            ItemUseResult result = new ItemUseResult(
                playerId,
                opponentPlayerId,
                itemId);

            if (matchState == null)
            {
                result.Deny("MatchState is null.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                result.Deny("Player id is empty.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(itemId))
            {
                result.Deny("Item id is empty.");
                return result;
            }

            if (turnActionValidator == null)
            {
                result.Deny("TurnActionValidator reference is missing.");
                return result;
            }

            TurnActionValidationResult validationResult =
                turnActionValidator.ValidateUseItem(matchState, playerId);

            result.actionValidationResult = validationResult;

            if (validationResult == null || !validationResult.isValid)
            {
                string reason = validationResult != null
                    ? validationResult.reason
                    : "Action validation failed.";

                result.Deny(reason);
                return result;
            }

            if (itemById.Count == 0)
                RebuildItemLookup();

            ItemDefinition item = GetItemDefinition(itemId);

            if (item == null)
            {
                result.Deny($"Item definition not found: {itemId}");
                return result;
            }

            PlayerMatchState playerMatchState =
                matchState.GetPlayerMatchState(playerId);

            PlayerRoundState playerRoundState =
                matchState.GetPlayerRoundState(playerId);

            if (playerMatchState == null)
            {
                result.Deny($"Player match state not found: {playerId}");
                return result;
            }

            if (playerRoundState == null)
            {
                result.Deny($"Player round state not found: {playerId}");
                return result;
            }

            if (!playerMatchState.HasItem(itemId))
            {
                result.Deny($"Player does not own item: {itemId}");
                return result;
            }

            if (!IsActivationWindowAllowed(item))
            {
                result.Deny($"Item activation window is not supported now: {item.activationWindow}");
                return result;
            }

            if (!CanUseByPolicy(playerId, playerRoundState, item))
            {
                result.Deny($"Item use policy blocks this use: {item.usePolicy}");
                return result;
            }

            result.activationCost = Mathf.Max(0, item.activationCost);
            result.crystalsBefore = playerMatchState.currentCrystals;
            result.usePolicy = item.usePolicy;

            if (playerMatchState.currentCrystals < result.activationCost)
            {
                result.Deny(
                    $"Not enough match crystals. Required={result.activationCost}, Available={playerMatchState.currentCrystals}");

                return result;
            }

            playerMatchState.currentCrystals -= result.activationCost;
            result.crystalsAfter = playerMatchState.currentCrystals;

            MarkItemUsed(playerId, playerRoundState, item);

            int itemUpgradeLevel = playerMatchState.GetItemUpgradeLevel(itemId);

            CreateEffectPackets(
                result,
                playerId,
                opponentPlayerId,
                item,
                itemUpgradeLevel);

            result.Allow();
            return result;
        }

        private ItemDefinition GetItemDefinition(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return null;

            if (itemById.TryGetValue(itemId, out ItemDefinition item))
                return item;

            return null;
        }

        private bool IsActivationWindowAllowed(ItemDefinition item)
        {
            return item.activationWindow == ItemActivationWindow.PreSpinOnly;
        }

        private bool CanUseByPolicy(
            string playerId,
            PlayerRoundState playerRoundState,
            ItemDefinition item)
        {
            switch (item.usePolicy)
            {
                case ItemUsePolicy.OncePerTurn:
                    return !playerRoundState.HasUsedItemThisTurn(item.id);

                case ItemUsePolicy.OncePerRound:
                    return !playerRoundState.HasUsedItemThisRound(item.id);

                case ItemUsePolicy.OncePerMatch:
                    return !HasUsedOncePerMatchItem(playerId, item.id);

                case ItemUsePolicy.Reusable:
                    return true;

                default:
                    return false;
            }
        }

        private void MarkItemUsed(
            string playerId,
            PlayerRoundState playerRoundState,
            ItemDefinition item)
        {
            switch (item.usePolicy)
            {
                case ItemUsePolicy.OncePerTurn:
                    playerRoundState.MarkItemUsedThisTurn(item.id);
                    break;

                case ItemUsePolicy.OncePerRound:
                    playerRoundState.MarkItemUsedThisRound(item.id);
                    break;

                case ItemUsePolicy.OncePerMatch:
                    MarkUsedOncePerMatchItem(playerId, item.id);
                    break;

                case ItemUsePolicy.Reusable:
                    break;
            }
        }

        private bool HasUsedOncePerMatchItem(string playerId, string itemId)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(itemId))
                return false;

            if (!usedOncePerMatchItemsByPlayer.TryGetValue(
                    playerId,
                    out HashSet<string> usedItems))
            {
                return false;
            }

            return usedItems.Contains(itemId);
        }

        private void MarkUsedOncePerMatchItem(string playerId, string itemId)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(itemId))
                return;

            if (!usedOncePerMatchItemsByPlayer.TryGetValue(
                    playerId,
                    out HashSet<string> usedItems))
            {
                usedItems = new HashSet<string>();
                usedOncePerMatchItemsByPlayer.Add(playerId, usedItems);
            }

            usedItems.Add(itemId);
        }

        private void CreateEffectPackets(
            ItemUseResult result,
            string sourcePlayerId,
            string opponentPlayerId,
            ItemDefinition item,
            int itemUpgradeLevel)
        {
            if (item.effects == null || item.effects.Count == 0)
            {
                Debug.LogWarning($"ItemUseManager: Item has no effects: {item.id}");
                return;
            }

            for (int i = 0; i < item.effects.Count; i++)
            {
                EffectDefinition effect = item.effects[i];

                if (effect == null)
                    continue;

                if (effect.effectType == EffectType.None)
                    continue;

                int value = effect.GetValueAtLevel(itemUpgradeLevel);

                string targetPlayerId = GetTargetPlayerId(
                    effect.target,
                    sourcePlayerId,
                    opponentPlayerId);

                EffectPacket packet = EffectPacket.FromItem(
                    sourcePlayerId,
                    targetPlayerId,
                    item.id,
                    effect.effectType,
                    effect.target,
                    value,
                    effect.damageType,
                    effect.statusId,
                    effect.statusDuration);

                result.AddEffectPacket(packet);
            }
        }

        private string GetTargetPlayerId(
            EffectTarget target,
            string sourcePlayerId,
            string opponentPlayerId)
        {
            switch (target)
            {
                case EffectTarget.Self:
                    return sourcePlayerId;

                case EffectTarget.Opponent:
                    return opponentPlayerId;

                default:
                    return opponentPlayerId;
            }
        }
    }
}