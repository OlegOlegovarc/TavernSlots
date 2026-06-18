using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Data;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class TurnSpinExecutorDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnSpinExecutor turnSpinExecutor;
        [SerializeField] private MatchConfig matchConfig;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private int testRoundIndex = 1;

        [Header("Players")]
        [SerializeField] private string actingPlayerId = "player";
        [SerializeField] private string opponentPlayerId = "bot";

        [Header("Acting Player")]
        [SerializeField] private int actingPlayerHealth = 30;
        [SerializeField] private int actingPlayerShieldCapacity = 20;
        [SerializeField] private int actingPlayerCrystals = 0;
        [SerializeField] private int allSymbolUpgradeLevel = 0;

        [Header("Opponent")]
        [SerializeField] private int opponentHealth = 30;
        [SerializeField] private int opponentShieldCapacity = 20;
        [SerializeField] private int opponentPhysicalShield = 0;

        [Header("Acting Player Symbol Pool")]
        [SerializeField]
        private List<string> symbolPool =
            new List<string>
            {
                "blade",
                "shield",
                "ore",
                "poison_drop"
            };

        private void Start()
        {
            if (runOnStart)
                RunTurnSpinTest();
        }

        [ContextMenu("Run Turn Spin Test")]
        public void RunTurnSpinTest()
        {
            if (turnSpinExecutor == null)
            {
                Debug.LogWarning(
                    "TurnSpinExecutorDebugTester: TurnSpinExecutor reference is missing.");

                return;
            }

            if (matchConfig == null)
            {
                Debug.LogWarning(
                    "TurnSpinExecutorDebugTester: MatchConfig reference is missing.");

                return;
            }

            MatchState matchState = CreateTestMatchState();

            LogMatchState(
                "BEFORE SPIN",
                matchState);

            TurnSpinExecutionResult executionResult =
                turnSpinExecutor.ExecuteSpin(
                    matchState,
                    actingPlayerId);

            LogExecutionResult(executionResult);

            LogMatchState(
                "AFTER SPIN",
                matchState);
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState actingPlayer =
                new PlayerMatchState(
                    actingPlayerId,
                    "Debug Player",
                    false);

            PlayerMatchState opponent =
                new PlayerMatchState(
                    opponentPlayerId,
                    "Debug Bot",
                    true);

            actingPlayer.currentCrystals = actingPlayerCrystals;

            for (int i = 0; i < symbolPool.Count; i++)
            {
                string symbolId = symbolPool[i];

                actingPlayer.AddSymbol(symbolId);
                actingPlayer.SetSymbolUpgradeLevel(
                    symbolId,
                    allSymbolUpgradeLevel);
            }

            MatchState matchState =
                new MatchState(
                    matchConfig,
                    actingPlayer,
                    opponent);

            matchState.currentRoundIndex = testRoundIndex;

            matchState.playerARound =
                new PlayerRoundState(
                    actingPlayerId,
                    actingPlayerHealth,
                    actingPlayerShieldCapacity);

            matchState.playerBRound =
                new PlayerRoundState(
                    opponentPlayerId,
                    opponentHealth,
                    opponentShieldCapacity);

            matchState.playerBRound.physicalShield =
                opponentPhysicalShield;

            matchState.SetPhase(
                MatchPhase.PreSpinItemPhase);

            matchState.SetActivePlayer(
                actingPlayerId);

            return matchState;
        }

        private void LogExecutionResult(
            TurnSpinExecutionResult result)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(
                "=== TURN SPIN EXECUTION RESULT ===");

            if (result == null)
            {
                builder.AppendLine("Result is null.");
                builder.AppendLine(
                    "==================================");

                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine(
                $"Success: {result.success}");

            builder.AppendLine(
                $"Reason: {result.reason}");

            builder.AppendLine(
                $"Acting Player: {result.actingPlayerId}");

            builder.AppendLine(
                $"Opponent Player: {result.opponentPlayerId}");

            builder.AppendLine(
                $"Phase Before: {result.phaseBefore}");

            builder.AppendLine(
                $"Phase After: {result.phaseAfter}");

            AppendValidationResult(
                builder,
                result.actionValidationResult);

            AppendTurnResultData(
                builder,
                result.turnResultData);

            AppendApplicationResults(
                builder,
                result.applicationResults);

            builder.AppendLine(
                "==================================");

            Debug.Log(builder.ToString());
        }

        private void AppendValidationResult(
            StringBuilder builder,
            TurnActionValidationResult validationResult)
        {
            builder.AppendLine("");
            builder.AppendLine("Action Validation:");

            if (validationResult == null)
            {
                builder.AppendLine("No validation result.");
                return;
            }

            builder.AppendLine(
                $"Valid: {validationResult.isValid}");

            builder.AppendLine(
                $"Reason: {validationResult.reason}");

            builder.AppendLine(
                $"Phase: {validationResult.currentPhase}");

            builder.AppendLine(
                $"Active Player: {validationResult.activePlayerId}");
        }

        private void AppendTurnResultData(
            StringBuilder builder,
            TurnResultData turnResultData)
        {
            builder.AppendLine("");
            builder.AppendLine("Turn Result Data:");

            if (turnResultData == null)
            {
                builder.AppendLine("No TurnResultData.");
                return;
            }

            AppendSpinResult(
                builder,
                turnResultData.spinResult);

            AppendGroups(
                builder,
                turnResultData.resolvedGroups);

            AppendEffectPackets(
                builder,
                turnResultData.effectPackets);

            builder.AppendLine(
                $"Round Ended: {turnResultData.roundEnded}");

            builder.AppendLine(
                $"Round Winner: {turnResultData.roundWinnerPlayerId}");

            builder.AppendLine(
                $"Round Loser: {turnResultData.roundLoserPlayerId}");

            builder.AppendLine(
                $"Match Ended: {turnResultData.matchEnded}");

            builder.AppendLine(
                $"Match Winner: {turnResultData.matchWinnerPlayerId}");

            builder.AppendLine(
                $"Match Loser: {turnResultData.matchLoserPlayerId}");
        }

        private void AppendSpinResult(
            StringBuilder builder,
            SpinResult spinResult)
        {
            builder.AppendLine("");
            builder.AppendLine("Spin Result:");

            if (spinResult == null || !spinResult.IsValid())
            {
                builder.AppendLine("Invalid spin result.");
                return;
            }

            for (int i = 0;
                 i < spinResult.orderedSymbolIds.Count;
                 i++)
            {
                builder.AppendLine(
                    $"Slot {i}: {spinResult.orderedSymbolIds[i]}");
            }
        }

        private void AppendGroups(
            StringBuilder builder,
            List<ResolvedSymbolGroup> groups)
        {
            builder.AppendLine("");
            builder.AppendLine("Resolved Groups:");

            if (groups == null || groups.Count == 0)
            {
                builder.AppendLine("No groups.");
                return;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                ResolvedSymbolGroup group = groups[i];

                builder.AppendLine(
                    $"Group {i}: " +
                    $"{group.symbolId} x{group.length}, " +
                    $"start slot {group.startIndex}");
            }
        }

        private void AppendEffectPackets(
            StringBuilder builder,
            List<EffectPacket> packets)
        {
            builder.AppendLine("");
            builder.AppendLine("Applied Effect Packets:");

            if (packets == null || packets.Count == 0)
            {
                builder.AppendLine("No applied packets.");
                return;
            }

            for (int i = 0; i < packets.Count; i++)
            {
                EffectPacket packet = packets[i];

                builder.AppendLine(
                    $"Packet {i}: " +
                    $"Source={GetPacketSource(packet)}, " +
                    $"Type={packet.effectType}, " +
                    $"Target={packet.targetPlayerId}, " +
                    $"Value={packet.value}, " +
                    $"DamageType={packet.damageType}, " +
                    $"Status={packet.statusId}, " +
                    $"GroupStart={packet.sourceGroupStartIndex}, " +
                    $"GroupLength={packet.sourceGroupLength}");
            }
        }

        private void AppendApplicationResults(
            StringBuilder builder,
            List<EffectApplicationResult> results)
        {
            builder.AppendLine("");
            builder.AppendLine("Application Results:");

            if (results == null || results.Count == 0)
            {
                builder.AppendLine("No application results.");
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
                    $"TargetDied={result.targetDied}");
            }
        }

        private void LogMatchState(
            string label,
            MatchState matchState)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(
                $"=== TURN SPIN STATE {label} ===");

            builder.AppendLine(
                $"Current Round: {matchState.currentRoundIndex}");

            builder.AppendLine(
                $"Current Phase: {matchState.currentPhase}");

            builder.AppendLine(
                $"Active Player Id: {matchState.activePlayerId}");

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
            PlayerMatchState playerMatchState,
            PlayerRoundState playerRoundState)
        {
            builder.AppendLine(
                $"Player: {playerMatchState.playerId}");

            builder.AppendLine(
                $"Round Wins: {playerMatchState.roundWins}");

            builder.AppendLine(
                $"Crystals: {playerMatchState.currentCrystals}");

            builder.AppendLine(
                $"HP: {playerRoundState.currentHealth}/" +
                $"{playerRoundState.maxHealth}");

            builder.AppendLine(
                $"Shields: " +
                $"Physical={playerRoundState.physicalShield}, " +
                $"Magical={playerRoundState.magicalShield}, " +
                $"Other={playerRoundState.otherShield}");

            if (playerRoundState.statuses == null
                || playerRoundState.statuses.Count == 0)
            {
                builder.AppendLine("Statuses: none");
            }
            else
            {
                builder.AppendLine("Statuses:");

                for (int i = 0;
                     i < playerRoundState.statuses.Count;
                     i++)
                {
                    AppliedStatusState status =
                        playerRoundState.statuses[i];

                    builder.AppendLine(
                        $"- {status.statusId}: " +
                        $"power={status.power}, " +
                        $"duration={status.remainingDuration}");
                }
            }

            builder.AppendLine("");
        }

        private string GetPacketSource(
            EffectPacket packet)
        {
            if (packet == null)
                return "null";

            if (!string.IsNullOrWhiteSpace(
                    packet.sourceSymbolId))
            {
                return packet.sourceSymbolId;
            }

            if (!string.IsNullOrWhiteSpace(
                    packet.sourceItemId))
            {
                return packet.sourceItemId;
            }

            return "unknown";
        }
    }
}