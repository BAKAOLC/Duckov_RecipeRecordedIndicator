using System;
using System.Reflection;
using HarmonyLib;

namespace Duckov_RecipeRecordedIndicator
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private Harmony? _harmony;

        private void Awake()
        {
            ModLogger.Log("Recipe Recorded Indicator Mod Loaded");
        }

        private void OnEnable()
        {
            var patched = PatchAll();
            if (!patched) ModLogger.LogError("Failed to apply Harmony patches. Mod functionality may be impaired.");
        }

        private void OnDisable()
        {
            var unpatched = UnpatchAll();
            if (!unpatched) ModLogger.LogError("Failed to remove Harmony patches. Mod unloading may be impaired.");
        }

        private void OnDestroy()
        {
            var unpatched = UnpatchAll();
            if (!unpatched)
                ModLogger.LogError("Failed to remove Harmony patches on destroy. Mod unloading may be impaired.");
        }

        private bool PatchAll()
        {
            try
            {
                _harmony = new("com.ritsukage.recipe_recorded_indicator");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                ModLogger.Log("Harmony Patches Applied Successfully");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Applying Harmony Patches: {ex}");
                return false;
            }
        }

        private bool UnpatchAll()
        {
            try
            {
                if (_harmony == null) return true;
                _harmony.UnpatchAll(_harmony.Id);
                _harmony = null;
                ModLogger.Log("Harmony Patches Removed Successfully");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Removing Harmony Patches: {ex}");
                return false;
            }
        }
    }
}