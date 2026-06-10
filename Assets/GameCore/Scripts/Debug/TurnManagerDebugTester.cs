using System.Text;
using UnityEngine;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class TurnManagerDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnManager turnManager;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private string activePlayerId = "bot";
        [SerializeField] private string opponentPlayerId = "player";

        [Header("Player")]
        [SerializeField] private int playerHealth = 30;
        [SerializeField] private int playerShieldCapacity = 10;

        [Header("Bot")]
        [SerializeField] private int botHealth = 10;
        [SerializeField] private int botShieldCapacity = 10;
        [SerializeField] private int botOtherShield = 0;

        [Header("Poison On Active Player")]
        [SerializeField] private bool addPoisonToActivePlayer = true;
        [SerializeField] private int poisonPower = 4;
        [SerializeField] private int poisonDuration = 2;

        private void Start()
        {
            if (runOnStart)
                RunTurnStartTest();
        }

        [ContextMenu("Run Turn Start Test")]
        public void RunTurnStartTest()
        {
            if (turnManager == null)
            {
                Debug.LogWarning("TurnManagerDebugTester: TurnManager reference is missing.");
                return;
            }

            MatchState matchState = CreateTestMatchState();

            LogMatchState("BEFORE BEGIN TURN", matchState);

            TurnStartResult result = turnManager.BeginTurn(
                matchState,
                activePlayerId);

            LogTurnStartResult(result);
            LogMatchState("AFTER BEGIN TURN", matchState);
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState player = new PlayerMatchState(
                opponentPlayerId,
                "Debug Player",
                false);

            PlayerMatchState bot = new PlayerMatchState(
                activePlayerId,
                "Debug Bot",
                true);

            MatchState matchState = new MatchState(null, player, bot);

            matchState.playerARound = new PlayerRoundState(
                opponentPlayerId,
                playerHealth,
                playerShieldCapacity);

            matchState.playerBRound = new PlayerRoundState(
                activePlayerId,
                botHealth,
                botShieldCapacity);

            matchState.playerBRound.otherShield = botOtherShield;

            matchState.SetPhase(MatchPhase.TurnStart);

            if (addPoisonToActivePlayer)
            {
                PlayerRoundState activeRoundState =
                    matchState.GetPlayerRoundState(activePlayerId);

                if (activeRoundState != null)
                {
                    activeRoundState.statuses.Add(
                        new AppliedStatusState(
                            "poison",
                            poisonPower,
                            poisonDuration));
                }
            }

            return matchState;
        }

        private void LogTurnStartResult(TurnStartResult result)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== TURN START RESULT ===");

            if (result == null)
            {
                builder.AppendLine("Result is null.");
                builder.AppendLine("=========================");

                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine($"Active Player: {result.activePlayerId}");
            builder.AppendLine($"Opponent Player: {result.opponentPlayerId}");
            builder.AppendLine($"Phase Before: {result.phaseBefore}");
            builder.AppendLine($"Phase After: {result.phaseAfter}");
            builder.AppendLine($"Can Active Player Act: {result.canActivePlayerAct}");
            builder.AppendLine($"Died On Turn Start: {result.activePlayerDiedOnTurnStart}");
            builder.AppendLine($"Round Ended: {result.roundEnded}");
            builder.AppendLine($"Round Winner: {result.roundWinnerPlayerId}");
            builder.AppendLine($"Round Loser: {result.roundLoserPlayerId}");

            builder.AppendLine("");
            builder.AppendLine("Status Tick Results:");

            if (result.statusTickResults == null || result.statusTickResults.Count == 0)
            {
                builder.AppendLine("No status ticks.");
            }
            else
            {
                for (int i = 0; i < result.statusTickResults.Count; i++)
                {
                    StatusTickResult tick = result.statusTickResults[i];

                    builder.AppendLine(
                        $"Tick {i}: " +
                        $"Target={tick.targetPlayerId}, " +
                        $"Status={tick.statusId}, " +
                        $"Power={tick.statusPower}, " +
                        $"RequestedDamage={tick.requestedDamage}, " +
                        $"ActualHealthDamage={tick.actualHealthDamage}, " +
                        $"DurationBefore={tick.durationBeforeTick}, " +
                        $"DurationAfter={tick.durationAfterTick}, " +
                        $"Removed={tick.statusRemovedAfterTick}, " +
                        $"TargetDied={tick.targetDied}");
                }
            }

            builder.AppendLine("=========================");

            Debug.Log(builder.ToString());
        }

        private void LogMatchState(string label, MatchState matchState)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"=== TURN MANAGER STATE {label} ===");
            builder.AppendLine($"Current Phase: {matchState.currentPhase}");
            builder.AppendLine($"Active Player Id: {matchState.activePlayerId}");
            builder.AppendLine("");

            AppendPlayerState(builder, matchState.playerA, matchState.playerARound);
            AppendPlayerState(builder, matchState.playerB, matchState.playerBRound);

            builder.AppendLine("===================================");

            Debug.Log(builder.ToString());
        }

        private void AppendPlayerState(
            StringBuilder builder,
            PlayerMatchState matchState,
            PlayerRoundState roundState)
        {
            builder.AppendLine($"Player: {matchState.playerId}");
            builder.AppendLine($"Round Wins: {matchState.roundWins}");
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
    }
}