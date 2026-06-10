using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class TurnActionValidatorDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TurnActionValidator turnActionValidator;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;

        [Header("Players")]
        [SerializeField] private string playerId = "player";
        [SerializeField] private string botId = "bot";

        private void Start()
        {
            if (runOnStart)
                RunActionValidationTest();
        }

        [ContextMenu("Run Action Validation Test")]
        public void RunActionValidationTest()
        {
            if (turnActionValidator == null)
            {
                Debug.LogWarning("TurnActionValidatorDebugTester: TurnActionValidator reference is missing.");
                return;
            }

            List<TurnActionValidationResult> results = new List<TurnActionValidationResult>();

            MatchState preSpinState = CreateBaseMatchState();
            preSpinState.SetPhase(MatchPhase.PreSpinItemPhase);
            preSpinState.SetActivePlayer(playerId);

            results.Add(turnActionValidator.ValidateUseItem(preSpinState, playerId));
            results.Add(turnActionValidator.ValidateStartSpin(preSpinState, playerId));
            results.Add(turnActionValidator.ValidateUseItem(preSpinState, botId));
            results.Add(turnActionValidator.ValidateStartSpin(preSpinState, botId));

            MatchState spinState = CreateBaseMatchState();
            spinState.SetPhase(MatchPhase.SpinPhase);
            spinState.SetActivePlayer(playerId);

            results.Add(turnActionValidator.ValidateUseItem(spinState, playerId));
            results.Add(turnActionValidator.ValidateStartSpin(spinState, playerId));

            MatchState buildState = CreateBaseMatchState();
            buildState.SetPhase(MatchPhase.BuildPhase);
            buildState.SetActivePlayer(string.Empty);

            results.Add(turnActionValidator.ValidateReadyBuild(buildState, playerId));
            results.Add(turnActionValidator.ValidateBuySymbol(buildState, playerId));
            results.Add(turnActionValidator.ValidateBuyItem(buildState, playerId));
            results.Add(turnActionValidator.ValidateRemoveSymbol(buildState, playerId));
            results.Add(turnActionValidator.ValidateUpgradeSymbol(buildState, playerId));
            results.Add(turnActionValidator.ValidateUpgradeItem(buildState, playerId));
            results.Add(turnActionValidator.ValidateUpgradePlayerStat(buildState, playerId));

            MatchState roundEndState = CreateBaseMatchState();
            roundEndState.SetPhase(MatchPhase.RoundEnd);
            roundEndState.SetActivePlayer(string.Empty);

            results.Add(turnActionValidator.ValidateUseItem(roundEndState, playerId));
            results.Add(turnActionValidator.ValidateReadyBuild(roundEndState, playerId));

            MatchState endedMatchState = CreateBaseMatchState();
            endedMatchState.SetPhase(MatchPhase.PreSpinItemPhase);
            endedMatchState.SetActivePlayer(playerId);
            endedMatchState.EndMatch();

            results.Add(turnActionValidator.ValidateStartSpin(endedMatchState, playerId));

            results.Add(turnActionValidator.ValidateUseItem(preSpinState, "unknown_player"));

            LogResults(results);
        }

        private MatchState CreateBaseMatchState()
        {
            PlayerMatchState player = new PlayerMatchState(
                playerId,
                "Debug Player",
                false);

            PlayerMatchState bot = new PlayerMatchState(
                botId,
                "Debug Bot",
                true);

            MatchState matchState = new MatchState(null, player, bot);

            matchState.playerARound = new PlayerRoundState(
                playerId,
                30,
                10);

            matchState.playerBRound = new PlayerRoundState(
                botId,
                30,
                10);

            return matchState;
        }

        private void LogResults(List<TurnActionValidationResult> results)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== TURN ACTION VALIDATION TEST ===");

            if (results == null || results.Count == 0)
            {
                builder.AppendLine("No validation results.");
                builder.AppendLine("===================================");

                Debug.Log(builder.ToString());
                return;
            }

            for (int i = 0; i < results.Count; i++)
            {
                TurnActionValidationResult result = results[i];

                builder.AppendLine(
                    $"Result {i}: " +
                    $"Action={result.actionType}, " +
                    $"Requesting={result.requestingPlayerId}, " +
                    $"Active={result.activePlayerId}, " +
                    $"Phase={result.currentPhase}, " +
                    $"Valid={result.isValid}, " +
                    $"Reason={result.reason}");
            }

            builder.AppendLine("===================================");

            Debug.Log(builder.ToString());
        }
    }
}