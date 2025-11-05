using System;
using System.Reflection;
using Duckov.BlackMarkets.UI;
using Duckov.MasterKeys;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_RecipeRecordedIndicator
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        private static readonly MethodBase IsFormulaUnlockedMethod =
            AccessTools.Method(typeof(CraftingManager), "IsFormulaUnlocked");

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

                var showIndicator = target != null && !target.NeedInspection && target.IsRegistered();
                RecordedIndicatorUI.AddOrUpdateIndicator(instance, showIndicator);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error updating ItemDisplay {instance.name}: {ex}");
            }
        }

        [HarmonyPatch(typeof(DemandPanel_Entry), "Refresh")]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void DemandPanelEntry_Refresh_PostFix(DemandPanel_Entry __instance)
            // ReSharper restore InconsistentNaming
        {
            if (__instance.Target == null) return;
            var showIndicator = CheckTypeIDIsRecorded(__instance.Target.ItemID);
            RecordedIndicatorUI.AddOrUpdateIndicator(__instance, showIndicator);
        }

        [HarmonyPatch(typeof(SupplyPanel_Entry), "Refresh")]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void SupplyPanelEntry_Refresh_PostFix(SupplyPanel_Entry __instance)
            // ReSharper restore InconsistentNaming
        {
            if (__instance.Target == null) return;
            var showIndicator = CheckTypeIDIsRecorded(__instance.Target.ItemID);
            RecordedIndicatorUI.AddOrUpdateIndicator(__instance, showIndicator);
        }

        private static bool CheckTypeIDIsRecorded(int typeID)
        {
            try
            {
                if (MasterKeysManager.IsActive(typeID)) return true;
                var prefab = ItemAssetsCollection.GetPrefab(typeID);
                if (prefab == null) return false;
                var formulaID = FormulasRegisterView.GetFormulaID(prefab);
                var isFormulaUnlocked = (bool)IsFormulaUnlockedMethod.Invoke(null, [formulaID]);
                return isFormulaUnlocked;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error checking if TypeID is recorded: {ex}");
            }

            return false;
        }
    }
}