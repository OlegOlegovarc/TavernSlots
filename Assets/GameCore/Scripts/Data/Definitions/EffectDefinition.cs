using System;
using UnityEngine;
using SlotsTavern.Core;

namespace SlotsTavern.Data
{
    [Serializable]
    public class EffectDefinition
    {
        [Header("Effect")]
        public EffectType effectType = EffectType.None;
        public EffectTarget target = EffectTarget.Opponent;

        [Header("Value")]
        [Tooltip("Used if no value curve is assigned.")]
        public int baseValue = 0;

        [Tooltip("Optional. If assigned, the effect value will be taken from this curve by current upgrade level.")]
        public UpgradeCurve valueCurve;

        [Header("Damage")]
        public DamageType damageType = DamageType.Physical;

        [Header("Status")]
        [Tooltip("Used only for ApplyStatus effects. Example: Poison.")]
        public string statusId;

        [Tooltip("Default duration for status effects.")]
        public int statusDuration = 1;

        public int GetValueAtLevel(int level)
        {
            if (valueCurve != null)
                return valueCurve.GetValueAtLevel(level);

            return baseValue;
        }
    }
}