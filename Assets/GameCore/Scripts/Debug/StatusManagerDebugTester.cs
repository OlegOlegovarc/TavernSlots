using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class StatusManagerDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatusManager statusManager;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private bool processTwoTicks = true;

        [Header("Target")]
        [SerializeField] private string targetPlayerId = "bot";
        [SerializeField] private int targetHealth = 10;
        [SerializeField] private int targetShieldCapacity = 10;
        [SerializeField] private int targetOtherShield = 0;

        [Header("Poison")]
        [SerializeField] private int poisonPower = 4;
        [SerializeField] private int poisonDuration = 2;

        private void Start()
        {
            if (runOnStart)
                RunStatusTickTest();
        }

        [ContextMenu("Run Status Tick Test")]
        public void RunStatusTickTest()
        {
            if (statusManager == null)
            {
                Debug.LogWarning("StatusManagerDebugTester: StatusManager reference is missing.");
                return;
            }

            MatchState matchState = CreateTestMatchState();

            LogState("BEFORE STATUS TICK", matchState);

            List<StatusTickResult> firstTickResults =
                statusManager.ProcessTurnReceivedStatuses(
                    matchState,
                    targetPlayerId);

            LogTickResults("FIRST TICK RESULTS", firstTickResults);
            LogState("AFTER FIRST TICK", matchState);

            PlayerRoundState targetRoundState =
                matchState.GetPlayerRoundState(targetPlayerId);

            if (processTwoTicks && targetRoundState != null && !targetRoundState.IsDead())
            {
                List<StatusTickResult> secondTickResults =
                    statusManager.ProcessTurnReceivedStatuses(
                        matchState,
                        targetPlayerId);

                LogTickResults("SECOND TICK RESULTS", secondTickResults);
                LogState("AFTER SECOND TICK", matchState);
            }
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState player = new PlayerMatchState(
                "player",
                "Debug Player",
                false);

            PlayerMatchState bot = new PlayerMatchState(
                targetPlayerId,
                "Debug Bot",
                true);

            MatchState matchState = new MatchState(null, player, bot);

            matchState.playerARound = new PlayerRoundState(
                "player",
                30,
                10);

            matchState.playerBRound = new PlayerRoundState(
                targetPlayerId,
                targetHealth,
                targetShieldCapacity);

            matchState.playerBRound.otherShield = targetOtherShield;

            matchState.playerBRound.statuses.Add(
                new AppliedStatusState(
                    "poison",
                    poisonPower,
                    poisonDuration));

            return matchState;
        }

        private void LogState(string label, MatchState matchState)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"=== STATUS MANAGER STATE {label} ===");

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

        private void LogTickResults(
            string label,
            List<StatusTickResult> results)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"=== STATUS MANAGER {label} ===");

            if (results == null || results.Count == 0)
            {
                builder.AppendLine("No status ticks.");
            }
            else
            {
                for (int i = 0; i < results.Count; i++)
                {
                    StatusTickResult result = results[i];

                    builder.AppendLine(
                        $"Result {i}: " +
                        $"Target={result.targetPlayerId}, " +
                        $"Status={result.statusId}, " +
                        $"Power={result.statusPower}, " +
                        $"DamageType={result.damageType}, " +
                        $"RequestedDamage={result.requestedDamage}, " +
                        $"ActualHealthDamage={result.actualHealthDamage}, " +
                        $"DurationBefore={result.durationBeforeTick}, " +
                        $"DurationAfter={result.durationAfterTick}, " +
                        $"Removed={result.statusRemovedAfterTick}, " +
                        $"TargetDied={result.targetDied}");
                }
            }

            builder.AppendLine("================================");

            Debug.Log(builder.ToString());
        }
    }
}