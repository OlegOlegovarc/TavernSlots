using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class FullTurnDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpinManager spinManager;
        [SerializeField] private GroupResolver groupResolver;
        [SerializeField] private ResolveManager resolveManager;
        [SerializeField] private EffectApplier effectApplier;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private string actingPlayerId = "player";
        [SerializeField] private string opponentPlayerId = "bot";
        [SerializeField] private int slotCount = 5;

        [Header("Player State")]
        [SerializeField] private int playerHealth = 30;
        [SerializeField] private int playerShieldCapacity = 30;
        [SerializeField] private int playerStartCrystals = 0;

        [Header("Bot State")]
        [SerializeField] private int botHealth = 30;
        [SerializeField] private int botShieldCapacity = 30;
        [SerializeField] private int botStartCrystals = 0;

        [Header("Symbol Upgrades")]
        [SerializeField] private int allSymbolUpgradeLevel = 0;

        [Header("Player Symbol Pool")]
        [SerializeField]
        private List<string> playerSymbolPool = new List<string>
        {
            "blade",
            "shield",
            "ore",
            "poison_drop"
        };

        private void Start()
        {
            if (runOnStart)
                RunFullTurnTest();
        }

        [ContextMenu("Run Full Turn Test")]
        public void RunFullTurnTest()
        {
            if (!HasValidReferences())
                return;

            MatchState matchState = CreateTestMatchState();

            PlayerMatchState actingPlayer = matchState.GetPlayerMatchState(actingPlayerId);

            LogMatchState("BEFORE TURN", matchState);

            SpinResult spinResult = spinManager.CreateSpinResult(
                actingPlayerId,
                playerSymbolPool,
                slotCount);

            List<ResolvedSymbolGroup> groups = groupResolver.ResolveGroups(spinResult);

            List<EffectPacket> packets = resolveManager.CreateEffectPacketsFromGroups(
                actingPlayer,
                opponentPlayerId,
                groups);

            List<EffectApplicationResult> applicationResults = effectApplier.ApplyEffectPackets(
                matchState,
                packets);

            LogFullTurnResult(
                spinResult,
                groups,
                packets,
                applicationResults);

            LogMatchState("AFTER TURN", matchState);
        }

        private bool HasValidReferences()
        {
            if (spinManager == null)
            {
                Debug.LogWarning("FullTurnDebugTester: SpinManager reference is missing.");
                return false;
            }

            if (groupResolver == null)
            {
                Debug.LogWarning("FullTurnDebugTester: GroupResolver reference is missing.");
                return false;
            }

            if (resolveManager == null)
            {
                Debug.LogWarning("FullTurnDebugTester: ResolveManager reference is missing.");
                return false;
            }

            if (effectApplier == null)
            {
                Debug.LogWarning("FullTurnDebugTester: EffectApplier reference is missing.");
                return false;
            }

            return true;
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState player = new PlayerMatchState(
                actingPlayerId,
                "Debug Player",
                false);

            PlayerMatchState bot = new PlayerMatchState(
                opponentPlayerId,
                "Debug Bot",
                true);

            player.currentCrystals = playerStartCrystals;
            bot.currentCrystals = botStartCrystals;

            for (int i = 0; i < playerSymbolPool.Count; i++)
            {
                string symbolId = playerSymbolPool[i];

                player.AddSymbol(symbolId);
                player.SetSymbolUpgradeLevel(symbolId, allSymbolUpgradeLevel);
            }

            MatchState matchState = new MatchState(null, player, bot);

            matchState.playerARound = new PlayerRoundState(
                actingPlayerId,
                playerHealth,
                playerShieldCapacity);

            matchState.playerBRound = new PlayerRoundState(
                opponentPlayerId,
                botHealth,
                botShieldCapacity);

            matchState.SetActivePlayer(actingPlayerId);

            return matchState;
        }

        private void LogFullTurnResult(
            SpinResult spinResult,
            List<ResolvedSymbolGroup> groups,
            List<EffectPacket> packets,
            List<EffectApplicationResult> applicationResults)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== FULL TURN DEBUG TEST ===");
            builder.AppendLine($"Acting Player: {actingPlayerId}");
            builder.AppendLine($"Opponent Player: {opponentPlayerId}");
            builder.AppendLine($"Slot Count: {slotCount}");
            builder.AppendLine($"All Symbol Upgrade Level: {allSymbolUpgradeLevel}");
            builder.AppendLine("");

            AppendSpinResult(builder, spinResult);
            AppendGroups(builder, groups);
            AppendEffectPackets(builder, packets);
            AppendApplicationResults(builder, applicationResults);

            builder.AppendLine("============================");

            Debug.Log(builder.ToString());
        }

        private void AppendSpinResult(StringBuilder builder, SpinResult spinResult)
        {
            builder.AppendLine("Spin Result:");

            if (spinResult == null || !spinResult.IsValid())
            {
                builder.AppendLine("Invalid spin result.");
                builder.AppendLine("");
                return;
            }

            for (int i = 0; i < spinResult.orderedSymbolIds.Count; i++)
            {
                builder.AppendLine($"Slot {i}: {spinResult.orderedSymbolIds[i]}");
            }

            builder.AppendLine("");
        }

        private void AppendGroups(StringBuilder builder, List<ResolvedSymbolGroup> groups)
        {
            builder.AppendLine("Resolved Groups:");

            if (groups == null || groups.Count == 0)
            {
                builder.AppendLine("No groups.");
                builder.AppendLine("");
                return;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                ResolvedSymbolGroup group = groups[i];

                builder.AppendLine(
                    $"Group {i}: {group.symbolId} x{group.length}, start slot {group.startIndex}");
            }

            builder.AppendLine("");
        }

        private void AppendEffectPackets(StringBuilder builder, List<EffectPacket> packets)
        {
            builder.AppendLine("Effect Packets:");

            if (packets == null || packets.Count == 0)
            {
                builder.AppendLine("No packets.");
                builder.AppendLine("");
                return;
            }

            for (int i = 0; i < packets.Count; i++)
            {
                EffectPacket packet = packets[i];

                builder.AppendLine(
                    $"Packet {i}: " +
                    $"Source={GetPacketSourceName(packet)}, " +
                    $"Type={packet.effectType}, " +
                    $"Target={packet.targetPlayerId}, " +
                    $"Value={packet.value}, " +
                    $"DamageType={packet.damageType}, " +
                    $"Status={packet.statusId}, " +
                    $"Duration={packet.statusDuration}, " +
                    $"GroupStart={packet.sourceGroupStartIndex}, " +
                    $"GroupLength={packet.sourceGroupLength}");
            }

            builder.AppendLine("");
        }

        private void AppendApplicationResults(
            StringBuilder builder,
            List<EffectApplicationResult> results)
        {
            builder.AppendLine("Application Results:");

            if (results == null || results.Count == 0)
            {
                builder.AppendLine("No application results.");
                builder.AppendLine("");
                return;
            }

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
                    $"StatusPower={result.appliedStatusPower}, " +
                    $"StatusDuration={result.appliedStatusDuration}, " +
                    $"TargetDied={result.targetDied}");
            }

            builder.AppendLine("");
        }

        private void LogMatchState(string label, MatchState matchState)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"=== MATCH STATE {label} ===");

            AppendPlayerState(builder, matchState.playerA, matchState.playerARound);
            AppendPlayerState(builder, matchState.playerB, matchState.playerBRound);

            builder.AppendLine("===========================");

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

            if (roundState.statuses == null || roundState.statuses.Count == 0)
            {
                builder.AppendLine("Statuses: none");
            }
            else
            {
                builder.AppendLine("Statuses:");

                for (int i = 0; i < roundState.statuses.Count; i++)
                {
                    AppliedStatusState status = roundState.statuses[i];

                    builder.AppendLine(
                        $"- {status.statusId}: power={status.power}, duration={status.remainingDuration}");
                }
            }

            builder.AppendLine("");
        }

        private string GetPacketSourceName(EffectPacket packet)
        {
            if (packet == null)
                return "null";

            if (!string.IsNullOrWhiteSpace(packet.sourceSymbolId))
                return packet.sourceSymbolId;

            if (!string.IsNullOrWhiteSpace(packet.sourceItemId))
                return packet.sourceItemId;

            return "unknown";
        }
    }
}