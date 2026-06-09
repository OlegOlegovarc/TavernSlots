using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Data;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class ResolveManager : MonoBehaviour
    {
        [Header("Symbol Data")]
        [SerializeField] private List<SymbolDefinition> symbolDefinitions = new List<SymbolDefinition>();

        private readonly Dictionary<string, SymbolDefinition> symbolById = new Dictionary<string, SymbolDefinition>();

        private void Awake()
        {
            RebuildSymbolLookup();
        }

        [ContextMenu("Rebuild Symbol Lookup")]
        public void RebuildSymbolLookup()
        {
            symbolById.Clear();

            if (symbolDefinitions == null)
                return;

            for (int i = 0; i < symbolDefinitions.Count; i++)
            {
                SymbolDefinition symbol = symbolDefinitions[i];

                if (symbol == null)
                    continue;

                if (string.IsNullOrWhiteSpace(symbol.id))
                {
                    Debug.LogWarning($"ResolveManager: Symbol at index {i} has empty id.");
                    continue;
                }

                if (symbolById.ContainsKey(symbol.id))
                {
                    Debug.LogWarning($"ResolveManager: Duplicate symbol id detected: {symbol.id}");
                    continue;
                }

                symbolById.Add(symbol.id, symbol);
            }
        }

        public List<EffectPacket> CreateEffectPacketsFromGroups(
            PlayerMatchState actingPlayer,
            string opponentPlayerId,
            List<ResolvedSymbolGroup> groups)
        {
            List<EffectPacket> packets = new List<EffectPacket>();

            if (actingPlayer == null)
            {
                Debug.LogWarning("ResolveManager: Acting player is null.");
                return packets;
            }

            if (groups == null || groups.Count == 0)
            {
                Debug.LogWarning("ResolveManager: No groups to resolve.");
                return packets;
            }

            if (symbolById.Count == 0)
                RebuildSymbolLookup();

            for (int i = 0; i < groups.Count; i++)
            {
                ResolvedSymbolGroup group = groups[i];

                if (group == null)
                    continue;

                SymbolDefinition symbol = GetSymbolDefinition(group.symbolId);

                if (symbol == null)
                {
                    Debug.LogWarning($"ResolveManager: Symbol definition not found for id: {group.symbolId}");
                    continue;
                }

                int symbolUpgradeLevel = actingPlayer.GetSymbolUpgradeLevel(group.symbolId);

                CreatePacketsForSymbolGroup(
                    packets,
                    actingPlayer.playerId,
                    opponentPlayerId,
                    symbol,
                    symbolUpgradeLevel,
                    group);
            }

            return packets;
        }

        private void CreatePacketsForSymbolGroup(
            List<EffectPacket> packets,
            string sourcePlayerId,
            string opponentPlayerId,
            SymbolDefinition symbol,
            int symbolUpgradeLevel,
            ResolvedSymbolGroup group)
        {
            if (symbol.effects == null || symbol.effects.Count == 0)
            {
                Debug.LogWarning($"ResolveManager: Symbol has no effects: {symbol.id}");
                return;
            }

            for (int i = 0; i < symbol.effects.Count; i++)
            {
                EffectDefinition effect = symbol.effects[i];

                if (effect == null)
                    continue;

                if (effect.effectType == EffectType.None)
                    continue;

                int valuePerSymbol = effect.GetValueAtLevel(symbolUpgradeLevel);
                int stackedValue = CalculateStackedValue(valuePerSymbol, group.length);

                string targetPlayerId = GetTargetPlayerId(
                    effect.target,
                    sourcePlayerId,
                    opponentPlayerId);

                EffectPacket packet = EffectPacket.FromSymbolGroup(
                    sourcePlayerId,
                    targetPlayerId,
                    symbol.id,
                    effect.effectType,
                    effect.target,
                    stackedValue,
                    effect.damageType,
                    effect.statusId,
                    effect.statusDuration,
                    group);

                packets.Add(packet);
            }
        }

        private SymbolDefinition GetSymbolDefinition(string symbolId)
        {
            if (string.IsNullOrWhiteSpace(symbolId))
                return null;

            if (symbolById.TryGetValue(symbolId, out SymbolDefinition symbol))
                return symbol;

            return null;
        }

        private int CalculateStackedValue(int valuePerSymbol, int groupLength)
        {
            if (valuePerSymbol <= 0)
                return 0;

            if (groupLength <= 0)
                return 0;

            return valuePerSymbol * groupLength * groupLength;
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