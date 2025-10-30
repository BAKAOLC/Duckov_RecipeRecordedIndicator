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
        [HarmonyPatch(typeof(ItemDisplay), "RefreshWishlistInfo")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void Setup_PostFix(ItemDisplay __instance)
        {
            try
            {
                if (__instance == null) return;
                UpdateItemDisplay(__instance, __instance.Target);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error in PostFix for ItemDisplay.RefreshWishlistInfo in {__instance.name}: {ex}");
            }
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