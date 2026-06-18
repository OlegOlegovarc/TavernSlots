using UnityEngine;

namespace SlotsTavern.Data
{
    [CreateAssetMenu(
        fileName = "RoundConfig_New",
        menuName = "Slots Tavern/Data/Round Config")]
    public class RoundConfig : ScriptableObject
    {
        [Header("Round")]
        [Min(1)]
        public int roundIndex = 1;

        [Header("Slots")]
        [Min(1)]
        public int slotCount = 3;

        [Header("Player Base Stats")]
        [Min(1)]
        public int playerBaseHealth = 30;

        [Min(0)]
        public int playerBaseShieldCapacity = 20;

        [Header("Bot Timing")]
        [Min(0f)]
        public float botThinkDelayMin = 1f;

        [Min(0f)]
        public float botThinkDelayMax = 2f;
    }
}