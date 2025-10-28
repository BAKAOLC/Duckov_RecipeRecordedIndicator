using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov.MasterKeys;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_RecipeRecordedIndicator
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        private static readonly Dictionary<ItemDisplay, Action> RefreshActions = [];

        [HarmonyPatch(typeof(ItemDisplay), "Setup")]
        [HarmonyAfter("KeycardRecordedIndicator")]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void Setup_PostFix(ItemDisplay __instance, Item target)
        {
            try
            {
                if (__instance == null) return;
                UpdateItemDisplay(__instance, target);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error in PostFix for ItemDisplay.Setup in {__instance.name}: {ex}");
            }
        }

        [HarmonyPatch(typeof(ItemDisplay), "Refresh")]
        [HarmonyAfter("KeycardRecordedIndicator")]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void Refresh_PostFix(ItemDisplay __instance)
        {
            try
            {
                if (__instance == null) return;
                UpdateItemDisplay(__instance, __instance.Target);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error in PostFix for ItemDisplay.Refresh in {__instance.name}: {ex}");
            }
        }

        [HarmonyPatch(typeof(ItemDisplay), "OnEnable")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void OnEnable_PostFix(ItemDisplay __instance)
        {
            if (__instance == null) return;
            if (RefreshActions.ContainsKey(__instance)) return;

            StatusRefreshManager.Instance.OnTriggerRefresh += OnTriggerRefresh;
            RefreshActions.Add(__instance, OnTriggerRefresh);

            return;

            void OnTriggerRefresh()
            {
                UpdateItemDisplay(__instance, __instance.Target);
            }
        }

        [HarmonyPatch(typeof(ItemDisplay), "OnDisable")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void OnDisable_PostFix(ItemDisplay __instance)
        {
            if (__instance == null) return;
            if (!RefreshActions.TryGetValue(__instance, out var refreshAction)) return;

            StatusRefreshManager.Instance.OnTriggerRefresh -= refreshAction;
            RefreshActions.Remove(__instance);
        }

        [HarmonyPatch(typeof(ItemDisplay), "OnDestroy")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void OnDestroy_PostFix(ItemDisplay __instance)
        {
            if (__instance == null) return;
            if (!RefreshActions.TryGetValue(__instance, out var refreshAction)) return;

            StatusRefreshManager.Instance.OnTriggerRefresh -= refreshAction;
            RefreshActions.Remove(__instance);
        }

        private static void UpdateItemDisplay(ItemDisplay instance, Item target)
        {
            try
            {
                if (instance == null) return;
                if (target == null || target.NeedInspection || !target.IsRegistered())
                {
                    RecordedIndicatorUI.RemoveIndicator(instance);
                    return;
                }

                RecordedIndicatorUI.AddIndicator(instance);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error updating ItemDisplay {instance.name}: {ex}");
            }
        }
    }
}