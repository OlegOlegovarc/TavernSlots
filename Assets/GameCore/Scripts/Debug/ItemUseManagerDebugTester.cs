using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class ItemUseManagerDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ItemUseManager itemUseManager;
        [SerializeField] private EffectApplier effectApplier;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private string playerId = "player";
        [SerializeField] private string botId = "bot";
        [SerializeField] private string itemId = "musket";

        [Header("Player State")]
        [SerializeField] private int playerHealth = 30;
        [SerializeField] private int playerShieldCapacity = 20;
        [SerializeField] private int playerStartCrystals = 20;

        [Header("Bot State")]
        [SerializeField] private int botHealth = 30;
        [SerializeField] private int botShieldCapacity = 20;

        [Header("Apply Effects")]
        [SerializeField] private bool applyEffectsAfterUse = true;

        private void Start()
        {
            if (runOnStart)
                RunItemUseTest();
        }

        [ContextMenu("Run Item Use Test")]
        public void RunItemUseTest()
        {
            if (itemUseManager == null)
            {
                Debug.LogWarning("ItemUseManagerDebugTester: ItemUseManager reference is missing.");
                return;
            }

            if (applyEffectsAfterUse && effectApplier == null)
            {
                Debug.LogWarning("ItemUseManagerDebugTester: EffectApplier reference is missing.");
                return;
            }

            MatchState matchState = CreateTestMatchState();

            LogMatchState("BEFORE ITEM USE", matchState);

            ItemUseResult itemUseResult = itemUseManager.UseItem(
                matchState,
                playerId,
                itemId);

            LogItemUseResult(itemUseResult);

            if (applyEffectsAfterUse && itemUseResult.success)
            {
                List<EffectApplicationResult> applicationResults =
                    effectApplier.ApplyEffectPackets(
                        matchState,
                        itemUseResult.effectPackets);

                LogApplicationResults(applicationResults);
            }

            LogMatchState("AFTER ITEM USE", matchState);
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState player = new PlayerMatchState(
                playerId,
                "Debug Player",
                false);

            PlayerMatchState bot = new PlayerMatchState(
                botId,
                "Debug Bot",
                true);

            player.currentCrystals = playerStartCrystals;
            player.AddItem(itemId);
            player.SetItemUpgradeLevel(itemId, 0);

            MatchState matchState = new MatchState(null, player, bot);

            matchState.playerARound = new PlayerRoundState(
                playerId,
                playerHealth,
                playerShieldCapacity);

            matchState.playerBRound = new PlayerRoundState(
                botId,
                botHealth,
                botShieldCapacity);

            matchState.SetPhase(MatchPhase.PreSpinItemPhase);
            matchState.SetActivePlayer(playerId);

            return matchState;
        }

        private void LogItemUseResult(ItemUseResult result)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== ITEM USE RESULT ===");

            if (result == null)
            {
                builder.AppendLine("Result is null.");
                builder.AppendLine("=======================");

                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine($"Player: {result.playerId}");
            builder.AppendLine($"Opponent: {result.opponentPlayerId}");
            builder.AppendLine($"Item: {result.itemId}");
            builder.AppendLine($"Success: {result.success}");
            builder.AppendLine($"Reason: {result.reason}");
            builder.AppendLine($"Use Policy: {result.usePolicy}");
            builder.AppendLine($"Activation Cost: {result.activationCost}");
            builder.AppendLine($"Crystals Before: {result.crystalsBefore}");
            builder.AppendLine($"Crystals After: {result.crystalsAfter}");

            if (result.actionValidationResult != null)
            {
                builder.AppendLine("");
                builder.AppendLine("Action Validation:");
                builder.AppendLine($"Valid: {result.actionValidationResult.isValid}");
                builder.AppendLine($"Reason: {result.actionValidationResult.reason}");
                builder.AppendLine($"Phase: {result.actionValidationResult.currentPhase}");
                builder.AppendLine($"Active Player: {result.actionValidationResult.activePlayerId}");
            }

            builder.AppendLine("");
            builder.AppendLine("Effect Packets:");

            if (result.effectPackets == null || result.effectPackets.Count == 0)
            {
                builder.AppendLine("No packets.");
            }
            else
            {
                for (int i = 0; i < result.effectPackets.Count; i++)
                {
                    EffectPacket packet = result.effectPackets[i];

                    builder.AppendLine(
                        $"Packet {i}: " +
                        $"SourceItem={packet.sourceItemId}, " +
                        $"Type={packet.effectType}, " +
                        $"Target={packet.targetPlayerId}, " +
                        $"Value={packet.value}, " +
                        $"DamageType={packet.damageType}, " +
                        $"Status={packet.statusId}, " +
                        $"Duration={packet.statusDuration}");
                }
            }

            builder.AppendLine("=======================");

            Debug.Log(builder.ToString());
        }

        private void LogApplicationResults(List<EffectApplicationResult> results)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== ITEM EFFECT APPLICATION RESULTS ===");

            if (results == null || results.Count == 0)
            {
                builder.AppendLine("No application results.");
            }
            else
            {
                for (int i = 0; i < results.Count; i++)
                {
                    EffectApplicationResult result = results[i];

                    builder.AppendLine(
                        $"Result {i}: " +
                        $"Type={result.effectType}, " +
                        $"Target={result.targetPlayerId}, " +
                        $"Requested={result.requestedValue}, " +
                        $"HealthDamage={result.actualHealthDamage}, " +
                        $"Heal={result.actualHeal}, " +
                        $"ShieldGain={result.actualShieldGain}, " +
                        $"Crystals={result.actualCrystalGain}, " +
                        $"Status={result.appliedStatusId}, " +
                        $"TargetDied={result.targetDied}");
                }
            }

            builder.AppendLine("=======================================");

            Debug.Log(builder.ToString());
        }

        private void LogMatchState(string label, MatchState matchState)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"=== ITEM USE STATE {label} ===");
            builder.AppendLine($"Current Phase: {matchState.currentPhase}");
            builder.AppendLine($"Active Player Id: {matchState.activePlayerId}");
            builder.AppendLine("");

            AppendPlayerState(builder, matchState.playerA, matchState.playerARound);
            AppendPlayerState(builder, matchState.playerB, matchState.playerBRound);

            builder.AppendLine("================================");

            Debug.Log(builder.ToString());
        }

        private void AppendPlayerState(
            StringBuilder builder,
            PlayerMatchState matchState,
            PlayerRoundState roundState)
        {
            builder.AppendLine($"Player: {matchState.playerId}");
            builder.AppendLine($"Crystals: {matchState.currentCrystals}");
            builder.AppendLine($"HP: {roundState.currentHealth}/{roundState.maxHealth}");
            builder.AppendLine(
                $"Shields: Physical={roundState.physicalShield}, Magical={roundState.magicalShield}, Other={roundState.otherShield}");

            if (roundState.usedItemsThisTurn == null || roundState.usedItemsThisTurn.Count == 0)
            {
                builder.AppendLine("Used Items This Turn: none");
            }
            else
            {
                builder.AppendLine(
                    $"Used Items This Turn: {string.Join(", ", roundState.usedItemsThisTurn)}");
            }

            if (roundState.usedItemsThisRound == null || roundState.usedItemsThisRound.Count == 0)
            {
                builder.AppendLine("Used Items This Round: none");
            }
            else
            {
                builder.AppendLine(
                    $"Used Items This Round: {string.Join(", ", roundState.usedItemsThisRound)}");
            }

            builder.AppendLine("");
        }
    }
}