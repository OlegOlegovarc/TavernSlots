using System.Collections.Generic;
using UnityEngine;
using SlotsTavern.Core;
using SlotsTavern.Runtime;

namespace SlotsTavern.Managers
{
    public class EffectApplier : MonoBehaviour
    {
        public List<EffectApplicationResult> ApplyEffectPackets(
            MatchState matchState,
            List<EffectPacket> packets)
        {
            List<EffectApplicationResult> results = new List<EffectApplicationResult>();

            if (matchState == null)
            {
                Debug.LogWarning("EffectApplier: MatchState is null.");
                return results;
            }

            if (packets == null || packets.Count == 0)
            {
                Debug.LogWarning("EffectApplier: No effect packets to apply.");
                return results;
            }

            for (int i = 0; i < packets.Count; i++)
            {
                EffectPacket packet = packets[i];

                EffectApplicationResult result = ApplyEffectPacket(matchState, packet);

                if (result != null)
                    results.Add(result);

                if (result != null && result.targetDied)
                    break;
            }

            return results;
        }

        public EffectApplicationResult ApplyEffectPacket(
            MatchState matchState,
            EffectPacket packet)
        {
            if (matchState == null)
            {
                Debug.LogWarning("EffectApplier: MatchState is null.");
                return null;
            }

            if (packet == null)
            {
                Debug.LogWarning("EffectApplier: EffectPacket is null.");
                return null;
            }

            EffectApplicationResult result = new EffectApplicationResult(packet);

            switch (packet.effectType)
            {
                case EffectType.GainCrystals:
                    ApplyGainCrystals(matchState, packet, result);
                    break;

                case EffectType.GainShield:
                    ApplyGainShield(matchState, packet, result);
                    break;

                case EffectType.DealDamage:
                    ApplyDealDamage(matchState, packet, result);
                    break;

                case EffectType.Heal:
                    ApplyHeal(matchState, packet, result);
                    break;

                case EffectType.ApplyStatus:
                    ApplyStatus(matchState, packet, result);
                    break;

                case EffectType.Lifesteal:
                    ApplyLifesteal(matchState, packet, result);
                    break;

                default:
                    Debug.LogWarning($"EffectApplier: Unsupported effect type: {packet.effectType}");
                    break;
            }

            CheckDeath(matchState, packet, result);

            return result;
        }

        private void ApplyGainCrystals(
            MatchState matchState,
            EffectPacket packet,
            EffectApplicationResult result)
        {
            PlayerMatchState targetMatchState = matchState.GetPlayerMatchState(packet.targetPlayerId);

            if (targetMatchState == null)
            {
                Debug.LogWarning($"EffectApplier: Target match state not found: {packet.targetPlayerId}");
                return;
            }

            int value = Mathf.Max(0, packet.value);

            targetMatchState.currentCrystals += value;
            result.actualCrystalGain = value;
        }

        private void ApplyGainShield(
            MatchState matchState,
            EffectPacket packet,
            EffectApplicationResult result)
        {
            PlayerRoundState targetRoundState = matchState.GetPlayerRoundState(packet.targetPlayerId);

            if (targetRoundState == null)
            {
                Debug.LogWarning($"EffectApplier: Target round state not found: {packet.targetPlayerId}");
                return;
            }

            int beforeShield = GetShieldValue(targetRoundState, packet.damageType);

            targetRoundState.AddShield(packet.damageType, packet.value);

            int afterShield = GetShieldValue(targetRoundState, packet.damageType);

            result.actualShieldGain = afterShield - beforeShield;
        }

        private void ApplyDealDamage(
            MatchState matchState,
            EffectPacket packet,
            EffectApplicationResult result)
        {
            PlayerRoundState targetRoundState = matchState.GetPlayerRoundState(packet.targetPlayerId);

            if (targetRoundState == null)
            {
                Debug.LogWarning($"EffectApplier: Target round state not found: {packet.targetPlayerId}");
                return;
            }

            int actualHealthDamage = targetRoundState.ApplyDamage(
                packet.damageType,
                packet.value);

            result.actualHealthDamage = actualHealthDamage;
        }

        private void ApplyHeal(
            MatchState matchState,
            EffectPacket packet,
            EffectApplicationResult result)
        {
            PlayerRoundState targetRoundState = matchState.GetPlayerRoundState(packet.targetPlayerId);

            if (targetRoundState == null)
            {
                Debug.LogWarning($"EffectApplier: Target round state not found: {packet.targetPlayerId}");
                return;
            }

            int beforeHealth = targetRoundState.currentHealth;

            targetRoundState.Heal(packet.value);

            int afterHealth = targetRoundState.currentHealth;

            result.actualHeal = afterHealth - beforeHealth;
        }

        private void ApplyStatus(
            MatchState matchState,
            EffectPacket packet,
            EffectApplicationResult result)
        {
            PlayerRoundState targetRoundState = matchState.GetPlayerRoundState(packet.targetPlayerId);

            if (targetRoundState == null)
            {
                Debug.LogWarning($"EffectApplier: Target round state not found: {packet.targetPlayerId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(packet.statusId))
            {
                Debug.LogWarning("EffectApplier: Cannot apply status. Status id is empty.");
                return;
            }

            int power = Mathf.Max(0, packet.value);
            int duration = Mathf.Max(1, packet.statusDuration);

            AppliedStatusState existingStatus = FindStatus(
                targetRoundState,
                packet.statusId);

            if (existingStatus == null)
            {
                targetRoundState.statuses.Add(
                    new AppliedStatusState(packet.statusId, power, duration));
            }
            else
            {
                existingStatus.power += power;
                existingStatus.remainingDuration = Mathf.Max(
                    existingStatus.remainingDuration,
                    duration);
            }

            result.appliedStatusId = packet.statusId;
            result.appliedStatusPower = power;
            result.appliedStatusDuration = duration;
        }

        private void ApplyLifesteal(
            MatchState matchState,
            EffectPacket packet,
            EffectApplicationResult result)
        {
            PlayerRoundState targetRoundState = matchState.GetPlayerRoundState(packet.targetPlayerId);
            PlayerRoundState sourceRoundState = matchState.GetPlayerRoundState(packet.sourcePlayerId);

            if (targetRoundState == null)
            {
                Debug.LogWarning($"EffectApplier: Target round state not found: {packet.targetPlayerId}");
                return;
            }

            if (sourceRoundState == null)
            {
                Debug.LogWarning($"EffectApplier: Source round state not found: {packet.sourcePlayerId}");
                return;
            }

            int actualHealthDamage = targetRoundState.ApplyDamage(
                packet.damageType,
                packet.value);

            result.actualHealthDamage = actualHealthDamage;

            int beforeHealth = sourceRoundState.currentHealth;

            sourceRoundState.Heal(actualHealthDamage);

            int afterHealth = sourceRoundState.currentHealth;

            result.actualHeal = afterHealth - beforeHealth;
        }

        private void CheckDeath(
            MatchState matchState,
            EffectPacket packet,
            EffectApplicationResult result)
        {
            PlayerRoundState targetRoundState = matchState.GetPlayerRoundState(packet.targetPlayerId);

            if (targetRoundState == null)
                return;

            result.targetDied = targetRoundState.IsDead();
        }

        private AppliedStatusState FindStatus(
            PlayerRoundState roundState,
            string statusId)
        {
            if (roundState == null || roundState.statuses == null)
                return null;

            for (int i = 0; i < roundState.statuses.Count; i++)
            {
                AppliedStatusState status = roundState.statuses[i];

                if (status.statusId == statusId)
                    return status;
            }

            return null;
        }

        private int GetShieldValue(
            PlayerRoundState roundState,
            DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Physical:
                    return roundState.physicalShield;

                case DamageType.Magical:
                    return roundState.magicalShield;

                case DamageType.Other:
                    return roundState.otherShield;

                default:
                    return 0;
            }
        }
    }
}