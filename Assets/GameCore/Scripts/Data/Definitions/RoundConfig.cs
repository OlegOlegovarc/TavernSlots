using UnityEngine;

namespace SlotsTavern.Data
{
    [CreateAssetMenu(
        fileName = "RoundConfig_New",
        menuName = "Slots Tavern/Data/Round Config")]
    public class RoundConfig : ScriptableObject
    {
        [Header("Round")]
        public int roundIndex = 1;

        [Header("Slots")]
        public int slotCount = 3;

        [Header("Health")]
        public int playerBaseHealth = 30;

        [Header("Bot Timing")]
        public float botThinkDelayMin = 1f;
        public float botThinkDelayMax = 2f;
    }
}