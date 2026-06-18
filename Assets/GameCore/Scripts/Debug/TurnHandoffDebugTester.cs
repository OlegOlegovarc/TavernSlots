using System.Text;
using UnityEngine;
using SlotsTavern.Data;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class TurnHandoffDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private MatchConfig matchConfig;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;

        [Header("Current Player")]
        [SerializeField] private string playerId = "player";
        [SerializeField] private int playerHealth = 30;
        [SerializeField] private int playerShieldCapacity = 20;
        [SerializeField] private int playerPhysicalShield = 8;
        [SerializeField] private int playerRoundWins = 0;
        [SerializeField] private string usedItemThisTurn = "musket";

        [Header("Next Player")]
        [SerializeField] private string botId = "bot";
        [SerializeField] private int botHealth = 10;
        [SerializeField] private int botShieldCapacity = 20;
        [SerializeField] private int botPhysicalShield = 5;
        [SerializeField] private int botMagicalShield = 3;
        [SerializeField] private int botOtherShield = 2;
        [SerializeField] private int botRoundWins = 0;

        [Header("Poison On Next Player")]
        [SerializeField] private bool addPoisonToBot = true;
        [SerializeField] private int poisonPower = 4;
        [SerializeField] private int poisonDuration = 2;

        private void Start()
        {
            if (runOnStart)
                RunTurnHandoffTest();
        }

        [ContextMenu("Run Turn Handoff Test")]
        public void RunTurnHandoffTest()
        {
            if (turnManager == null)
            {
                Debug.LogWarning(
                    "TurnHandoffDebugTester: TurnManager reference is missing.");

                return;
            }

            if (matchConfig == null)
            {
                Debug.LogWarning(
                    "TurnHandoffDebugTester: MatchConfig reference is missing.");

                return;
            }

            MatchState matchState =
                CreateTestMatchState();

            LogMatchState(
                "BEFORE HANDOFF",
                matchState);

            TurnEndResult result =
                turnManager.EndTurnAndBeginNextTurn(
                    matchState);

            LogTurnEndResult(result);

            LogMatchState(
                "AFTER HANDOFF",
                matchState);
        }

        private MatchState CreateTestMatchState()
        {
            PlayerMatchState player =
                new PlayerMatchState(
                    playerId,
                    "Debug Player",
                    false);

            PlayerMatchState bot =
                new PlayerMatchState(
                    botId,
                    "Debug Bot",
                    true);

            player.roundWins = playerRoundWins;
            bot.roundWins = botRoundWins;

            MatchState matchState =
                new MatchState(
                    matchConfig,
                    player,
                    bot);

            matchState.playerARound =
                new PlayerRoundState(
                    playerId,
                    playerHealth,
                    playerShieldCapacity);

            matchState.playerBRound =
                new PlayerRoundState(
                    botId,
                    botHealth,
                    botShieldCapacity);

            matchState.playerARound.physicalShield =
                playerPhysicalShield;

            matchState.playerBRound.physicalShield =
                botPhysicalShield;

            matchState.playerBRound.magicalShield =
                botMagicalShield;

            matchState.playerBRound.otherShield =
                botOtherShield;

            if (!string.IsNullOrWhiteSpace(usedItemThisTurn))
            {
                matchState.playerARound.MarkItemUsedThisTurn(
                    usedItemThisTurn);
            }

            if (addPoisonToBot)
            {
                matchState.playerBRound.statuses.Add(
                    new AppliedStatusState(
                        "poison",
                        poisonPower,
                        poisonDuration));
            }

            matchState.SetActivePlayer(playerId);
            matchState.SetPhase(MatchPhase.ResolvePhase);

            return matchState;
        }

        private void LogTurnEndResult(
            TurnEndResult result)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                "=== TURN HANDOFF RESULT ===");

            if (result == null)
            {
                builder.AppendLine("Result is null.");
                builder.AppendLine(
                    "===========================");

                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine(
                $"Success: {result.success}");

            builder.AppendLine(
                $"Reason: {result.reason}");

            builder.AppendLine(
                $"Ending Player: {result.endingPlayerId}");

            builder.AppendLine(
                $"Next Player: {result.nextPlayerId}");

            builder.AppendLine(
                $"Phase Before: {result.phaseBefore}");

            builder.AppendLine(
                $"Phase After: {result.phaseAfter}");

            builder.AppendLine("");

            builder.AppendLine(
                "Ending Player Turn Item Usage:");

            builder.AppendLine(
                $"Before: " +
                $"{result.endingPlayerUsedItemsThisTurnBefore}");

            builder.AppendLine(
                $"After: " +
                $"{result.endingPlayerUsedItemsThisTurnAfter}");

            builder.AppendLine("");

            builder.AppendLine(
                "Next Player Shields:");

            builder.AppendLine(
                $"Before: " +
                $"Physical={result.nextPlayerPhysicalShieldBefore}, " +
                $"Magical={result.nextPlayerMagicalShieldBefore}, " +
                $"Other={result.nextPlayerOtherShieldBefore}");

            builder.AppendLine(
                $"After: " +
                $"Physical={result.nextPlayerPhysicalShieldAfter}, " +
                $"Magical={result.nextPlayerMagicalShieldAfter}, " +
                $"Other={result.nextPlayerOtherShieldAfter}");

            builder.AppendLine("");

            builder.AppendLine(
                $"Next Turn Started: {result.nextTurnStarted}");

            builder.AppendLine(
                $"Next Player Can Act: {result.nextPlayerCanAct}");

            builder.AppendLine(
                $"Round Ended During Handoff: " +
                $"{result.roundEndedDuringHandoff}");

            builder.AppendLine(
                $"Match Ended During Handoff: " +
                $"{result.matchEndedDuringHandoff}");

            AppendTurnStartResult(
                builder,
                result.nextTurnStartResult);

            builder.AppendLine(
                "===========================");

            Debug.Log(builder.ToString());
        }

        private void AppendTurnStartResult(
            StringBuilder builder,
            TurnStartResult result)
        {
            builder.AppendLine("");
            builder.AppendLine("Next Turn Start Result:");

            if (result == null)
            {
                builder.AppendLine("No result.");
                return;
            }

            builder.AppendLine(
                $"Active Player: {result.activePlayerId}");

            builder.AppendLine(
                $"Opponent Player: {result.opponentPlayerId}");

            builder.AppendLine(
                $"Phase Before: {result.phaseBefore}");

            builder.AppendLine(
                $"Phase After: {result.phaseAfter}");

            builder.AppendLine(
                $"Can Act: {result.canActivePlayerAct}");

            builder.AppendLine(
                $"Died On Turn Start: " +
                $"{result.activePlayerDiedOnTurnStart}");

            builder.AppendLine(
                $"Round Ended: {result.roundEnded}");

            builder.AppendLine(
                $"Round Winner: {result.roundWinnerPlayerId}");

            builder.AppendLine(
                $"Round Loser: {result.roundLoserPlayerId}");

            builder.AppendLine("Status Ticks:");

            if (result.statusTickResults == null
                || result.statusTickResults.Count == 0)
            {
                builder.AppendLine("No status ticks.");
                return;
            }

            for (int i = 0;
                 i < result.statusTickResults.Count;
                 i++)
            {
                StatusTickResult tick =
                    result.statusTickResults[i];

                builder.AppendLine(
                    $"Tick {i}: " +
                    $"Status={tick.statusId}, " +
                    $"Power={tick.statusPower}, " +
                    $"RequestedDamage={tick.requestedDamage}, " +
                    $"ActualHealthDamage={tick.actualHealthDamage}, " +
                    $"DurationBefore={tick.durationBeforeTick}, " +
                    $"DurationAfter={tick.durationAfterTick}, " +
                    $"TargetDied={tick.targetDied}");
            }
        }

        private void LogMatchState(
            string label,
            MatchState matchState)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                $"=== TURN HANDOFF STATE {label} ===");

            builder.AppendLine(
                $"Current Phase: {matchState.currentPhase}");

            builder.AppendLine(
                $"Active Player: {matchState.activePlayerId}");

            builder.AppendLine(
                $"Match Ended: {matchState.isMatchEnded}");

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
                "==================================");

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
                $"HP: {playerRoundState.currentHealth}/" +
                $"{playerRoundState.maxHealth}");

            builder.AppendLine(
                $"Shields: " +
                $"Physical={playerRoundState.physicalShield}, " +
                $"Magical={playerRoundState.magicalShield}, " +
                $"Other={playerRoundState.otherShield}");

            if (playerRoundState.usedItemsThisTurn == null
                || playerRoundState.usedItemsThisTurn.Count == 0)
            {
                builder.AppendLine(
                    "Used Items This Turn: none");
            }
            else
            {
                builder.AppendLine(
                    $"Used Items This Turn: " +
                    $"{string.Join(", ", playerRoundState.usedItemsThisTurn)}");
            }

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
    }
}