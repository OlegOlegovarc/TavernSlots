using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class ResolveDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpinManager spinManager;
        [SerializeField] private GroupResolver groupResolver;
        [SerializeField] private ResolveManager resolveManager;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private string actingPlayerId = "player";
        [SerializeField] private string opponentPlayerId = "bot";
        [SerializeField] private int slotCount = 5;

        [Header("Test Upgrades")]
        [SerializeField] private int allSymbolUpgradeLevel = 0;

        [Header("Test Symbol Pool")]
        [SerializeField]
        private List<string> symbolPoolIds = new List<string>
        {
            "blade",
            "shield",
            "ore",
            "poison_drop"
        };

        private void Start()
        {
            if (runOnStart)
                RunResolveTest();
        }

        [ContextMenu("Run Resolve Test")]
        public void RunResolveTest()
        {
            if (spinManager == null)
            {
                Debug.LogWarning("ResolveDebugTester: SpinManager reference is missing.");
                return;
            }

            if (groupResolver == null)
            {
                Debug.LogWarning("ResolveDebugTester: GroupResolver reference is missing.");
                return;
            }

            if (resolveManager == null)
            {
                Debug.LogWarning("ResolveDebugTester: ResolveManager reference is missing.");
                return;
            }

            PlayerMatchState actingPlayer = CreateTestPlayer();

            SpinResult spinResult = spinManager.CreateSpinResult(
                actingPlayerId,
                symbolPoolIds,
                slotCount);

            List<ResolvedSymbolGroup> groups = groupResolver.ResolveGroups(spinResult);

            List<EffectPacket> packets = resolveManager.CreateEffectPacketsFromGroups(
                actingPlayer,
                opponentPlayerId,
                groups);

            LogResolveResult(spinResult, groups, packets);
        }

        private PlayerMatchState CreateTestPlayer()
        {
            PlayerMatchState player = new PlayerMatchState(
                actingPlayerId,
                "Debug Player",
                false);

            for (int i = 0; i < symbolPoolIds.Count; i++)
            {
                string symbolId = symbolPoolIds[i];

                player.AddSymbol(symbolId);
                player.SetSymbolUpgradeLevel(symbolId, allSymbolUpgradeLevel);
            }

            return player;
        }

        private void LogResolveResult(
            SpinResult spinResult,
            List<ResolvedSymbolGroup> groups,
            List<EffectPacket> packets)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== RESOLVE DEBUG TEST ===");

            if (spinResult == null || !spinResult.IsValid())
            {
                builder.AppendLine("Spin result is invalid.");
                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine($"Acting Player: {spinResult.actingPlayerId}");
            builder.AppendLine($"Opponent Player: {opponentPlayerId}");
            builder.AppendLine($"Slot Count: {spinResult.slotCount}");
            builder.AppendLine($"All Symbol Upgrade Level: {allSymbolUpgradeLevel}");
            builder.AppendLine("");

            builder.AppendLine("Spin Result:");
            for (int i = 0; i < spinResult.orderedSymbolIds.Count; i++)
            {
                builder.AppendLine($"Slot {i}: {spinResult.orderedSymbolIds[i]}");
            }

            builder.AppendLine("");
            builder.AppendLine("Resolved Groups:");

            if (groups == null || groups.Count == 0)
            {
                builder.AppendLine("No groups resolved.");
            }
            else
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    ResolvedSymbolGroup group = groups[i];

                    builder.AppendLine(
                        $"Group {i}: {group.symbolId} x{group.length}, start slot {group.startIndex}");
                }
            }

            builder.AppendLine("");
            builder.AppendLine("Effect Packets:");

            if (packets == null || packets.Count == 0)
            {
                builder.AppendLine("No packets created.");
            }
            else
            {
                for (int i = 0; i < packets.Count; i++)
                {
                    EffectPacket packet = packets[i];

                    builder.AppendLine(
                        $"Packet {i}: {FormatPacket(packet)}");
                }
            }

            builder.AppendLine("==========================");

            Debug.Log(builder.ToString());
        }

        private string FormatPacket(EffectPacket packet)
        {
            if (packet == null)
                return "null";

            string source = !string.IsNullOrWhiteSpace(packet.sourceSymbolId)
                ? packet.sourceSymbolId
                : packet.sourceItemId;

            return
                $"Source={source}, " +
                $"Type={packet.effectType}, " +
                $"Target={packet.targetPlayerId}, " +
                $"Value={packet.value}, " +
                $"DamageType={packet.damageType}, " +
                $"Status={packet.statusId}, " +
                $"Duration={packet.statusDuration}, " +
                $"GroupStart={packet.sourceGroupStartIndex}, " +
                $"GroupLength={packet.sourceGroupLength}";
        }
    }
}