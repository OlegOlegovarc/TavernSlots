using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class EffectApplierDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EffectApplier effectApplier;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;

        [Header("Player A")]
        [SerializeField] private string playerAId = "player";
        [SerializeField] private int playerAHealth = 30;
        [SerializeField] private int playerAShieldCapacity = 20;
        [SerializeField] private int playerAStartCrystals = 0;

        [Header("Player B")]
        [SerializeField] private string playerBId = "bot";
        [SerializeField] private int playerBHealth = 30;
        [SerializeField] private int playerBShieldCapacity = 20;
        [SerializeField] private int playerBStartCrystals = 0;

        private void Start()
        {
            if (runOnStart)
                RunEffectApplierTest();
        }

        [ContextMenu("Run Effect Applier Test")]
        public void RunEffectApplierTest()
        {
            if (effectApplier == null)
            {
                Debug.LogWarning("EffectApplierDebugTester: EffectApplier reference is missing.");
                return;
            }

            MatchState matchState = CreateTestMatchState();
            List<EffectPacket> packets = CreateTestPackets();

            LogState("BEFORE", matchState);

            List<EffectApplicationResult> results = effectApplier.ApplyEffectPackets(
                matchState,
                packets);

            LogResults(results);
            LogState("AFTER", matchState);
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState playerA = new PlayerMatchState(playerAId, "Player", false);
            PlayerMatchState playerB = new PlayerMatchState(playerBId, "Bot", true);

            playerA.currentCrystals = playerAStartCrystals;
            playerB.currentCrystals = playerBStartCrystals;

            MatchState matchState = new MatchState(null, playerA, playerB);

            matchState.playerARound = new PlayerRoundState(
                playerAId,
                playerAHealth,
                playerAShieldCapacity);

            matchState.playerBRound = new PlayerRoundState(
                playerBId,
                playerBHealth,
                playerBShieldCapacity);

            return matchState;
        }

        private List<EffectPacket> CreateTestPackets()
        {
            List<EffectPacket> packets = new List<EffectPacket>();

            packets.Add(EffectPacket.FromItem(
                playerAId,
                playerAId,
                "debug_ore",
                EffectType.GainCrystals,
                EffectTarget.Self,
                10,
                DamageType.Other,
                string.Empty,
                1));

            packets.Add(EffectPacket.FromItem(
                playerAId,
                playerAId,
                "debug_shield",
                EffectType.GainShield,
                EffectTarget.Self,
                8,
                DamageType.Physical,
                string.Empty,
                1));

            packets.Add(EffectPacket.FromItem(
                playerBId,
                playerAId,
                "debug_musket",
                EffectType.DealDamage,
                EffectTarget.Opponent,
                12,
                DamageType.Physical,
                string.Empty,
                1));

            packets.Add(EffectPacket.FromItem(
                playerAId,
                playerAId,
                "debug_heal",
                EffectType.Heal,
                EffectTarget.Self,
                5,
                DamageType.Other,
                string.Empty,
                1));

            packets.Add(EffectPacket.FromItem(
                playerAId,
                playerBId,
                "debug_poison",
                EffectType.ApplyStatus,
                EffectTarget.Opponent,
                3,
                DamageType.Other,
                "poison",
                2));

            return packets;
        }

        private void LogState(string label, MatchState matchState)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"=== EFFECT APPLIER STATE {label} ===");

            AppendPlayerState(builder, matchState.playerA, matchState.playerARound);
            AppendPlayerState(builder, matchState.playerB, matchState.playerBRound);

            builder.AppendLine("====================================");

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

        private void LogResults(List<EffectApplicationResult> results)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== EFFECT APPLICATION RESULTS ===");

            if (results == null || results.Count == 0)
            {
                builder.AppendLine("No results.");
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
                        $"StatusPower={result.appliedStatusPower}, " +
                        $"StatusDuration={result.appliedStatusDuration}, " +
                        $"TargetDied={result.targetDied}");
                }
            }

            builder.AppendLine("==================================");

            Debug.Log(builder.ToString());
        }
    }
}