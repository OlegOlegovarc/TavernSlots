using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class StatusManager : MonoBehaviour
    {
        [Header("Poison")]
        [SerializeField] private string poisonStatusId = "poison";
        [SerializeField] private DamageType poisonDamageType = DamageType.Other;

        public List<StatusTickResult> ProcessTurnReceivedStatuses(
            MatchState matchState,
            string targetPlayerId)
        {
            List<StatusTickResult> results = new List<StatusTickResult>();

            if (matchState == null)
            {
                Debug.LogWarning("StatusManager: MatchState is null.");
                return results;
            }

            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                Debug.LogWarning("StatusManager: Target player id is empty.");
                return results;
            }

            PlayerRoundState targetRoundState = matchState.GetPlayerRoundState(targetPlayerId);

            if (targetRoundState == null)
            {
                Debug.LogWarning($"StatusManager: Target round state not found: {targetPlayerId}");
                return results;
            }

            if (targetRoundState.statuses == null || targetRoundState.statuses.Count == 0)
                return results;

            for (int i = 0; i < targetRoundState.statuses.Count; i++)
            {
                AppliedStatusState status = targetRoundState.statuses[i];

                if (status == null)
                    continue;

                StatusTickResult result = ProcessSingleStatusTick(
                    targetRoundState,
                    status);

                if (result != null)
                    results.Add(result);

                if (status.remainingDuration <= 0)
                {
                    targetRoundState.statuses.RemoveAt(i);
                    i--;

                    if (result != null)
                        result.statusRemovedAfterTick = true;
                }

                if (targetRoundState.IsDead())
                    break;
            }

            return results;
        }

        private StatusTickResult ProcessSingleStatusTick(
            PlayerRoundState targetRoundState,
            AppliedStatusState status)
        {
            if (status.statusId == poisonStatusId)
                return ProcessPoisonTick(targetRoundState, status);

            Debug.LogWarning($"StatusManager: Unsupported status id: {status.statusId}");
            return null;
        }

        private StatusTickResult ProcessPoisonTick(
            PlayerRoundState targetRoundState,
            AppliedStatusState status)
        {
            int durationBeforeTick = status.remainingDuration;
            int poisonPower = Mathf.Max(0, status.power);

            StatusTickResult result = new StatusTickResult(
                targetRoundState.playerId,
                status.statusId,
                poisonPower,
                durationBeforeTick,
                poisonDamageType);

            int actualHealthDamage = targetRoundState.ApplyDamage(
                poisonDamageType,
                poisonPower);

            status.remainingDuration = Mathf.Max(
                0,
                status.remainingDuration - 1);

            result.actualHealthDamage = actualHealthDamage;
            result.durationAfterTick = status.remainingDuration;
            result.targetDied = targetRoundState.IsDead();

            return result;
        }
    }
}