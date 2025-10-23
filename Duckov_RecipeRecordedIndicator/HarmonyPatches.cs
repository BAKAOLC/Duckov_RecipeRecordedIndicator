using System;
using System.Linq;
using System.Reflection;
using Duckov.MasterKeys;
using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_RecipeRecordedIndicator
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        private static Tag? _keyItemTag;

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

        private static void UpdateItemDisplay(ItemDisplay instance, Item target)
        {
            try
            {
                if (instance == null) return;
                if (target == null || target.NeedInspection || !CheckShowIndicator(target))
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

        private static bool CheckShowIndicator(Item item)
        {
            return IsRecipeItemAndRecord(item) || IsKeyItemAndRecord(item);
        }

        private static bool IsKeyItemAndRecord(Item item)
        {
            if (item == null) return false;

            _keyItemTag ??= GetTagByName("Key");
            if (_keyItemTag == null) return false;

            return item.Tags.Contains(_keyItemTag) && IsKeyRecorded(item.TypeID);
        }

        private static bool IsKeyRecorded(int typeID)
        {
            return MasterKeysManager.IsActive(typeID);
        }

        private static bool IsRecipeItemAndRecord(Item item)
        {
            if (item == null) return false;

            var formula = item.GetComponent<ItemSetting_Formula>();
            return formula != null && IsFormulaUnlocked(formula.formulaID);
        }

        private static bool IsFormulaUnlocked(string formulaID)
        {
            var craftingManagerType = typeof(CraftingManager);
            var isFormulaUnlockedMethod =
                craftingManagerType.GetMethod("IsFormulaUnlocked", BindingFlags.Static | BindingFlags.NonPublic);
            if (isFormulaUnlockedMethod == null)
            {
                ModLogger.LogError("Could not find IsFormulaUnlocked method via reflection.");
                return false;
            }

            var result = isFormulaUnlockedMethod.Invoke(null, [formulaID]);
            return result is true;
        }

        private static Tag? GetTagByName(string tagName)
        {
            return GameplayDataSettings.Tags.AllTags.FirstOrDefault(t => t.name == tagName);
        }
    }
}