using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SlotsTavern.Managers;
using SlotsTavern.Runtime;

namespace SlotsTavern.DebugTools
{
    public class SpinDebugTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpinManager spinManager;
        [SerializeField] private GroupResolver groupResolver;

        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private string actingPlayerId = "player";
        [SerializeField] private int slotCount = 5;

        [Header("Test Symbol Pool")]
        [SerializeField]
        private List<string> symbolPoolIds = new List<string>
        {
            "blade",
            "shield",
            "ore",
            "poison_drop"
        };

        private void Start()
        {
            if (runOnStart)
                RunSpinTest();
        }

        [ContextMenu("Run Spin Test")]
        public void RunSpinTest()
        {
            if (spinManager == null)
            {
                Debug.LogWarning("SpinDebugTester: SpinManager reference is missing.");
                return;
            }

            if (groupResolver == null)
            {
                Debug.LogWarning("SpinDebugTester: GroupResolver reference is missing.");
                return;
            }

            SpinResult spinResult = spinManager.CreateSpinResult(
                actingPlayerId,
                symbolPoolIds,
                slotCount);

            List<ResolvedSymbolGroup> groups = groupResolver.ResolveGroups(spinResult);

            LogSpinResult(spinResult, groups);
        }

        private void LogSpinResult(SpinResult spinResult, List<ResolvedSymbolGroup> groups)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("=== SPIN DEBUG TEST ===");

            if (spinResult == null || !spinResult.IsValid())
            {
                builder.AppendLine("Spin result is invalid.");
                Debug.Log(builder.ToString());
                return;
            }

            builder.AppendLine($"Acting Player: {spinResult.actingPlayerId}");
            builder.AppendLine($"Slot Count: {spinResult.slotCount}");
            builder.AppendLine("Spin Result:");

            for (int i = 0; i < spinResult.orderedSymbolIds.Count; i++)
            {
                builder.AppendLine($"Slot {i}: {spinResult.orderedSymbolIds[i]}");
            }

            builder.AppendLine("Resolved Groups:");

            if (groups == null || groups.Count == 0)
            {
                builder.AppendLine("No groups resolved.");
            }
            else
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    ResolvedSymbolGroup group = groups[i];

                    builder.AppendLine(
                        $"Group {i}: {group.symbolId} x{group.length}, start slot {group.startIndex}");
                }
            }

            builder.AppendLine("=======================");

            Debug.Log(builder.ToString());
        }
    }
}